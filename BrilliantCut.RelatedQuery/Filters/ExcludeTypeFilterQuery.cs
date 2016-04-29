using System.Collections.Generic;
using System.Linq;
using BrilliantCut.RelatedQuery;
using EPiServer;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.ServiceLocation;

namespace BrilliantCut.Filters.RelatedQuery
{
    [ServiceConfiguration(typeof(IModifyExclusionFilterQuery), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TypeFilterQuery : IModifyExclusionFilterQuery
    {
        public ITypeSearch<TQuery> Filter<TQuery>(IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostQuery, IEnumerable<object> content)
        {
            var distinctTypes = content
                .Select(x => x.GetOriginalType())
                .Distinct();

            var typeBuilder = new FilterBuilder<object>(boostQuery.Client);
            typeBuilder = distinctTypes.Aggregate(typeBuilder, (current, distinctType) => current.And(x => !x.MatchType(distinctType)));

            return boostQuery.Filter(typeBuilder);
        }
    }
}
