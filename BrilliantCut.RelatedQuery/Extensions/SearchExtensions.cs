using System.Collections.Generic;
using System.Linq;
using EPiServer.Find;

namespace BrilliantCut.RelatedQuery.Extensions
{
    public static class SearchExtensions
    {
        public static IEnumerable<TResult> GetRelatedResult<TResult>(this ISearch<TResult> search)
        {
            return search.GetResult().Hits.Where(x => x.Score > 1).Select(x => x.Document);
        }
    }
}
