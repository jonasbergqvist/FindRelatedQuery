using System.Collections.Generic;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.ServiceLocation;

namespace BrilliantCut.RelatedQuery
{
    [ServiceConfiguration(typeof(IModifySearchQuery), Lifecycle = ServiceInstanceScope.Singleton)]
    public class DefaultSearchQuery : IModifySearchQuery
    {
        public IQueriedSearch<TQuery, QueryStringQuery> CreateSearchQuery<TQuery>(IClient client, IEnumerable<object> content)
        {
            return client.Search<TQuery>()
                .For("");
        }
    }
}
