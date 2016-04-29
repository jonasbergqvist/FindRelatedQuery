using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.ServiceLocation;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// Creates a related query that should be executed using GetRelatedResult
    /// </summary>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class RelatedQueryFactory
    {
        private readonly IClient _client;
        private readonly RelatedFilterRegistry _relatedFilterRepository;
        private readonly IModifyExclusionFilterQuery _defaultModifyFilterQuery;
        private readonly IModifySearchQuery _defaultModifySearchQuery;
        private readonly IContentLoader _contentLoader;

        private readonly Lazy<Dictionary<Type, IEnumerable<RelatedFilterData>>> _relatedFilters;

        /// <summary>
        /// Creates an instance of the <see cref="RelatedQueryFactory"/>.
        /// </summary>
        /// <param name="client">The EPiServer Find client.</param>
        /// <param name="contentLoader">The content loader.</param>
        /// <param name="relatedFilterRepository">The repository containing the filters, which will be used when creating the related query.</param>
        /// <param name="defaultModifyFilterQuery">The default modify filter query.</param>
        /// <param name="defaultModifySearchQuery">The default modify search query.</param>
        public RelatedQueryFactory(IClient client, IContentLoader contentLoader, RelatedFilterRegistry relatedFilterRepository, IModifyExclusionFilterQuery defaultModifyFilterQuery, IModifySearchQuery defaultModifySearchQuery)
        {
            _client = client;
            _contentLoader = contentLoader;
            _relatedFilterRepository = relatedFilterRepository;
            _defaultModifyFilterQuery = defaultModifyFilterQuery;
            _defaultModifySearchQuery = defaultModifySearchQuery;

            _relatedFilters = new Lazy<Dictionary<Type, IEnumerable<RelatedFilterData>>>(GetRelatedFilters);
        }

        /// <summary>
        /// Gets the related filters from the <see cref="RelatedFilterRegistry"/>
        /// </summary>
        /// <returns>The registered filters together with the type it's registered for.</returns>
        protected virtual Dictionary<Type, IEnumerable<RelatedFilterData>> GetRelatedFilters()
        {
            return _relatedFilterRepository
                .List()
                .ToDictionary(x => 
                    x.Key, 
                    x => (IEnumerable<RelatedFilterData>)new List<RelatedFilterData>(x.Value.RelatedFilters.Select(relatedFilter => 
                        new RelatedFilterData
                        {
                            RelatedFilter = relatedFilter,
                            ContentType = relatedFilter.GetOriginalType().GetGenericArguments().FirstOrDefault() ?? relatedFilter.GetOriginalType()
                        })));
        }

        /// <summary>
        /// Creates the query using content references.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <typeparam name="TRelatedQuery">The registered query to use</typeparam>
        /// <param name="contentLinks">References to the content, which will be used for creating the related query.</param>
        /// <returns>Related query.</returns>
        public ITypeSearch<TQuery> CreateQuery<TRelatedQuery, TQuery>(
            IEnumerable<ContentReference> contentLinks)
            where TRelatedQuery : IRelatedQuery
            where TQuery : class
        {
            return CreateQuery<TRelatedQuery, TQuery>(contentLinks, 20);
        }

        /// <summary>
        /// Creates the query using content references.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <typeparam name="TRelatedQuery">The registered query to use</typeparam>
        /// <param name="contentLinks">References to the content, which will be used for creating the related query.</param>
        /// <param name="basedOnContentItems">Number of item's in the <paramref name="contentLinks"/> that should be used when creating the query.</param>
        /// <returns>Related query.</returns>
        public ITypeSearch<TQuery> CreateQuery<TRelatedQuery, TQuery>(
            IEnumerable<ContentReference> contentLinks,
            int basedOnContentItems)
            where TRelatedQuery : IRelatedQuery
            where TQuery : class
        {
            return CreateQuery<TRelatedQuery, TQuery>(_contentLoader.GetItems(contentLinks, CultureInfo.InvariantCulture), basedOnContentItems);
        }

        /// <summary>
        /// Creates the query using content references.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <typeparam name="TRelatedQuery">The registered query to use</typeparam>
        /// <param name="content">The content, which will be used for creating the related query.</param>
        /// <returns>Related query.</returns>
        public ITypeSearch<TQuery> CreateQuery<TRelatedQuery, TQuery>(
            IEnumerable<object> content)
            where TRelatedQuery : IRelatedQuery
            where TQuery : class
        {
            return CreateQuery<TRelatedQuery, TQuery>(content, 20);
        }

        /// <summary>
        /// Creates the query using content references.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <typeparam name="TRelatedQuery">The registered query to use</typeparam>
        /// <param name="content">The content, which will be used for creating the related query.</param>
        /// <param name="basedOnContentItems">Number of item's in the <paramref name="content"/> that should be used when creating the query.</param>
        /// <returns>Related query.</returns>
        public virtual ITypeSearch<TQuery> CreateQuery<TRelatedQuery, TQuery>(
            IEnumerable<object> content, 
            int basedOnContentItems)
            where TRelatedQuery : IRelatedQuery
            where TQuery : class
        {
            var contentArray = content.Take(basedOnContentItems).ToArray();
            var freeTextSearch = CreateSearchQuery<TRelatedQuery, TQuery>(contentArray);
            if (!_relatedFilters.Value.Any())
            {
                return freeTextSearch;
            }

            var boostedQuery = CreateBoostQuery<TRelatedQuery, TQuery>(freeTextSearch, contentArray);
            if (boostedQuery == null)
            {
                return freeTextSearch;
            }

            return CreateFilterQuery<TRelatedQuery, TQuery>(boostedQuery, contentArray);
        }

        /// <summary>
        /// Creates the search query, which will be the base of the search.
        /// </summary>
        /// <typeparam name="TQuery">The search type.</typeparam>
        /// <typeparam name="TRelatedQuery">The related query type.</typeparam>
        /// <returns>Search query.</returns>
        protected virtual IQueriedSearch<TQuery, QueryStringQuery> CreateSearchQuery<TRelatedQuery, TQuery>(IEnumerable<object> content)
            where TRelatedQuery : IRelatedQuery
        {
            IModifySearchQuery modifySearchQuery;
            if (_relatedFilterRepository.TryGetModifiedSearchQueryFunc<TRelatedQuery>(out modifySearchQuery))
            {
                return modifySearchQuery.CreateSearchQuery<TQuery>(_client, content);
            }

            return _defaultModifySearchQuery.CreateSearchQuery<TQuery>(_client, content);
        }

        /// <summary>
        /// Creates a boost query based on a search query.
        /// </summary>
        /// <typeparam name="TQuery">The search type.</typeparam>
        /// <typeparam name="TRelatedQuery">The registered query to use</typeparam>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="contentArray">Items to base the boost query on.</param>
        /// <returns>Boost query.</returns>
        protected virtual IQueriedSearch<TQuery, CustomFiltersScoreQuery> CreateBoostQuery<TRelatedQuery, TQuery>(
            IQueriedSearch<TQuery, QueryStringQuery> searchQuery,
            object[] contentArray)
            where TRelatedQuery : IRelatedQuery
        {
            var relatedQueryType = typeof (TRelatedQuery);
            if (!_relatedFilters.Value.ContainsKey(relatedQueryType))
            {
                return null;
            }

            var relatedQuery = _relatedFilters.Value[relatedQueryType];

            IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostedQuery = null;
            foreach (var relatedFilter in relatedQuery)
            {
                var filter = relatedFilter;

                var contentForRelatedFilter = GetContentForRelatedFilter(contentArray, filter).ToArray();
                if (!contentForRelatedFilter.Any())
                {
                    continue;
                }

                var createdFilter = filter.RelatedFilter.CreateFilter(contentForRelatedFilter);
                if (createdFilter == null)
                {
                    continue;
                }

                var boost = filter.RelatedFilter.Boost;
                if (boostedQuery == null)
                {
                    boostedQuery = searchQuery.BoostMatching(f => createdFilter, boost);
                    continue;
                }

                boostedQuery = boostedQuery.BoostMatching(f => createdFilter, boost);
            }

            return boostedQuery;
        }

        /// <summary>
        /// Creates a filter query based on a boost query.
        /// </summary>
        /// <typeparam name="TQuery">The search type.</typeparam>
        /// <typeparam name="TRelatedQuery">The related query type</typeparam>
        /// <param name="boostQuery">The boost query</param>
        /// <param name="contentArray">The content to use when creating the query.</param>
        /// <returns>Filter query.</returns>
        protected virtual ITypeSearch<TQuery> CreateFilterQuery<TRelatedQuery, TQuery>(
            IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostQuery,
            object[] contentArray)
            where TRelatedQuery : IRelatedQuery
            where TQuery : class
        {
            IModifyExclusionFilterQuery filterQueryFunc;
            if (_relatedFilterRepository.TryGetModifiedFilterQueryFunc<TRelatedQuery>(out filterQueryFunc))
            {
                return filterQueryFunc.Filter(boostQuery, contentArray);
            }

            return _defaultModifyFilterQuery.Filter(boostQuery, contentArray);
        }

        /// <summary>
        /// Filters content, so only content which can be used by the filters are used.
        /// </summary>
        /// <param name="contentArray">The content to use in the query.</param>
        /// <param name="filter">The filters.</param>
        /// <returns>Content, which will be used by at least one filter in the query.</returns>
        protected virtual IEnumerable<object> GetContentForRelatedFilter(IEnumerable<object> contentArray, RelatedFilterData filter)
        {
            return contentArray.Where(x => filter.ContentType.IsAssignableFrom(x.GetOriginalType()));
        }
    }
}