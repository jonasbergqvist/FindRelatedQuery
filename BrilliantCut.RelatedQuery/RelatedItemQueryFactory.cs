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
    public class RelatedItemQueryFactory
    {
        private readonly IClient _client;
        private readonly RelatedFilterRegistry _relatedFilterRepository;
        private readonly IContentLoader _contentLoader;

        private readonly Lazy<IEnumerable<RelatedFilterData>> _relatedFilters;

        /// <summary>
        /// Creates an instance of the <see cref="RelatedItemQueryFactory"/>.
        /// </summary>
        /// <param name="client">The EPiServer Find client.</param>
        /// <param name="contentLoader">The content loader.</param>
        /// <param name="relatedFilterRepository">The repository containing the filters, which will be used when creating the related query.</param>
        public RelatedItemQueryFactory(IClient client, IContentLoader contentLoader, RelatedFilterRegistry relatedFilterRepository)
        {
            _client = client;
            _contentLoader = contentLoader;
            _relatedFilterRepository = relatedFilterRepository;

            _relatedFilters = new Lazy<IEnumerable<RelatedFilterData>>(GetRelatedFilters);
        }

        /// <summary>
        /// Gets the related filters from the <see cref="RelatedFilterRegistry"/>
        /// </summary>
        /// <returns>The registered filters together with the type it's registered for.</returns>
        protected virtual IEnumerable<RelatedFilterData> GetRelatedFilters()
        {
            return _relatedFilterRepository.List().Select(relatedFilter => new RelatedFilterData
            {
                RelatedFilter = relatedFilter,
                ContentType = relatedFilter.GetOriginalType().GetGenericArguments().First()
            });
        }

        /// <summary>
        /// Creates the query using content references.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="contentLinks">References to the content, which will be used for creating the related query.</param>
        /// <returns>Related query.</returns>
        public ITypeSearch<TQuery> CreateQuery<TQuery>(
            IEnumerable<ContentReference> contentLinks)
            where TQuery : class
        {
            return CreateQuery<TQuery>(contentLinks, Int32.MaxValue);
        }

        /// <summary>
        /// Creates the query using content references.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="contentLinks">References to the content, which will be used for creating the related query.</param>
        /// <param name="basedOnContentItems">Number of item's in the <paramref name="contentLinks"/> that should be used when creating the query.</param>
        /// <returns>Related query.</returns>
        public ITypeSearch<TQuery> CreateQuery<TQuery>(
            IEnumerable<ContentReference> contentLinks,
            int basedOnContentItems)
            where TQuery : class
        {
            return CreateQuery<TQuery>(_contentLoader.GetItems(contentLinks, CultureInfo.InvariantCulture), basedOnContentItems);
        }

        /// <summary>
        /// Creates the query using content references.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="content">The content, which will be used for creating the related query.</param>
        /// <returns>Related query.</returns>
        public ITypeSearch<TQuery> CreateQuery<TQuery>(
            IEnumerable<object> content)
            where TQuery : class
        {
            return CreateQuery<TQuery>(content, Int32.MaxValue);
        }

        /// <summary>
        /// Creates the query using content references.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="content">The content, which will be used for creating the related query.</param>
        /// <param name="basedOnContentItems">Number of item's in the <paramref name="contentLinks"/> that should be used when creating the query.</param>
        /// <returns>Related query.</returns>
        public virtual ITypeSearch<TQuery> CreateQuery<TQuery>(
            IEnumerable<object> content, 
            int basedOnContentItems)
            where TQuery : class
        {
            var contentArray = content.Take(basedOnContentItems).ToArray();
            var freeTextSearch = CreateSearchQuery<TQuery>();
            if (!_relatedFilters.Value.Any())
            {
                return freeTextSearch;
            }

            var boostedQuery = CreateBoostQuery(freeTextSearch, contentArray);
            if (boostedQuery == null)
            {
                return freeTextSearch;
            }

            return CreateFilterQuery(boostedQuery, contentArray);
        }

        /// <summary>
        /// Creates the search query, which will be the base of the search.
        /// </summary>
        /// <typeparam name="TQuery">The search type.</typeparam>
        /// <returns>Search query.</returns>
        protected virtual IQueriedSearch<TQuery, QueryStringQuery> CreateSearchQuery<TQuery>()
        {
            return _client.Search<TQuery>()
                .For("");
        }

        /// <summary>
        /// Creates a boost query based on a search query.
        /// </summary>
        /// <typeparam name="TQuery">The search type.</typeparam>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="contentArray">Items to base the boost query on.</param>
        /// <returns>Boost query.</returns>
        protected virtual IQueriedSearch<TQuery, CustomFiltersScoreQuery> CreateBoostQuery<TQuery>(
            IQueriedSearch<TQuery, QueryStringQuery> searchQuery,
            object[] contentArray)
        {
            IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostedQuery = null;
            foreach (var relatedFilter in _relatedFilters.Value)
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
        /// <param name="boostQuery">The boost query</param>
        /// <param name="contentArray">The content to use when creating the query.</param>
        /// <returns>Filter query.</returns>
        protected virtual ITypeSearch<TQuery> CreateFilterQuery<TQuery>(
            IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostQuery,
            object[] contentArray)
            where TQuery : class
        {
            var distinctTypes = contentArray
                .Select(x => x.GetOriginalType())
                .Distinct();

            ITypeSearch<TQuery> filterQuery = boostQuery;
            return distinctTypes
                .Aggregate(filterQuery, (current, type) =>
                    current.Filter(x => !x.MatchType(type)));
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