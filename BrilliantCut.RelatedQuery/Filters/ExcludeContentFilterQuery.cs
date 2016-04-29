using System.Collections.Generic;
using System.Linq;
using BrilliantCut.RelatedQuery;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.Find.Cms;
using EPiServer.ServiceLocation;

namespace BrilliantCut.Filters.RelatedQuery
{
    [ServiceConfiguration(typeof(IModifyExclusionFilterQuery), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ExcludeContentFilterQuery : IModifyExclusionFilterQuery
    {
        public ITypeSearch<TQuery> Filter<TQuery>(IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostQuery, IEnumerable<object> content)
        {
            var contentItems = content.OfType<IContent>();

            var typeBuilder = new FilterBuilder<IContent>(boostQuery.Client);
            typeBuilder = contentItems.Aggregate(typeBuilder, (current, contentItem) => current.And(x => !x.ContentLink.Match(contentItem.ContentLink.ToReferenceWithoutVersion())));

            return boostQuery.Filter(typeBuilder);
        }
    }
}
