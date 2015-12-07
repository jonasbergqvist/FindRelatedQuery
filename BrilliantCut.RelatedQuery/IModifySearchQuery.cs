using System.Collections.Generic;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Queries;

namespace BrilliantCut.RelatedQuery
{
    public interface IModifySearchQuery
    {
        IQueriedSearch<TQuery, QueryStringQuery> CreateSearchQuery<TQuery>(IClient client, IEnumerable<object> content);
    }
}
