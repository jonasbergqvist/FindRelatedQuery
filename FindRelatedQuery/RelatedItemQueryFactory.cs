using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.ServiceLocation;

namespace FindRelatedQuery
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class RelatedItemQueryFactory
    {
        private readonly IClient _client;
        private readonly RelatedFilterRepository _relatedFilterRepository;
        private readonly IContentLoader _contentLoader;

        private readonly Lazy<IEnumerable<RelatedFilterContext>> _relatedFilters;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="contentLoader"></param>
        /// <param name="relatedFilterRepository"></param>
        public RelatedItemQueryFactory(IClient client, IContentLoader contentLoader, RelatedFilterRepository relatedFilterRepository)
        {
            _client = client;
            _contentLoader = contentLoader;
            _relatedFilterRepository = relatedFilterRepository;

            _relatedFilters = new Lazy<IEnumerable<RelatedFilterContext>>(GetRelatedFilters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<RelatedFilterContext> GetRelatedFilters()
        {
            return _relatedFilterRepository.List().Select(relatedFilter => new RelatedFilterContext
            {
                RelatedFilter = relatedFilter,
                ContentType = relatedFilter.GetOriginalType().GetGenericArguments().First()
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentLinks"></param>
        /// <returns></returns>
        public ITypeSearch<TContent> CreateRelatedItemsQuery<TContent>(
            IEnumerable<ContentReference> contentLinks)
            where TContent : IContent
        {
            return CreateRelatedItemsQuery<TContent>(contentLinks, Int32.MaxValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentLinks"></param>
        /// <param name="basedOnContentItems"></param>
        /// <returns></returns>
        public ITypeSearch<TContent> CreateRelatedItemsQuery<TContent>(
            IEnumerable<ContentReference> contentLinks,
            int basedOnContentItems)
            where TContent : IContent
        {
            return CreateRelatedItemsQuery<TContent>(_contentLoader.GetItems(contentLinks, CultureInfo.InvariantCulture), basedOnContentItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public ITypeSearch<TContent> CreateRelatedItemsQuery<TContent>(
            IEnumerable<IContent> content)
            where TContent : IContent
        {
            return CreateRelatedItemsQuery<TContent>(content, Int32.MaxValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="content"></param>
        /// <param name="basedOnContentItems"></param>
        /// <returns></returns>
        public virtual ITypeSearch<TContent> CreateRelatedItemsQuery<TContent>(
            IEnumerable<IContent> content, 
            int basedOnContentItems)
            where TContent : IContent
        {
            if (!_relatedFilters.Value.Any())
            {
                return null;
            }

            var contentArray = content.Take(basedOnContentItems).ToArray();
            var freeTextSearch = CreateSearchQuery<TContent>();

            var boostedQuery = CreateBoostQuery(freeTextSearch, contentArray);
            if (boostedQuery == null)
            {
                return null;
            }

            return CreateFilterQuery(boostedQuery, contentArray);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <returns></returns>
        protected virtual IQueriedSearch<TContent, QueryStringQuery> CreateSearchQuery<TContent>()
        {
            return _client.Search<TContent>()
                .For("");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <returns></returns>
        protected virtual IQueriedSearch<TContent, CustomFiltersScoreQuery> CreateBoostQuery<TContent>(
            IQueriedSearch<TContent, QueryStringQuery> searchQuery, 
            IContent[] contentArray)
        {
            IQueriedSearch<TContent, CustomFiltersScoreQuery> boostedQuery = null;
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
        /// 
        /// </summary>
        /// <param name="contentArray"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IContent> GetContentForRelatedFilter(IEnumerable<IContent> contentArray, RelatedFilterContext filter)
        {
            return contentArray.Where(x => filter.ContentType.IsAssignableFrom(x.GetOriginalType()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <returns></returns>
        protected virtual ITypeSearch<TContent> CreateFilterQuery<TContent>(
            IQueriedSearch<TContent, CustomFiltersScoreQuery> boostQuery, 
            IContent[] contentArray)
            where TContent : IContent
        {
            var distinctContentTypes = contentArray
                .Select(x => x.ContentTypeID)
                .Distinct();

            ITypeSearch<TContent> filterQuery = boostQuery;
            return distinctContentTypes
                .Aggregate(filterQuery, (current, contentType) =>
                    current.Filter(x => !x.ContentTypeID.Match(contentType)));
        }
    }
}