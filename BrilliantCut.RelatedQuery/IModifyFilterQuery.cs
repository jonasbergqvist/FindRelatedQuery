using System.Collections.Generic;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Queries;

namespace BrilliantCut.RelatedQuery
{
    public interface IModifyFilterQuery
    {
        ITypeSearch<TQuery> Filter<TQuery>(IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostQuery, IEnumerable<object> content);
    }
}
