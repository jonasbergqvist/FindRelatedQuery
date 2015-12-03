using System.Collections.Generic;
using System.Linq;
using EPiServer.Find;

namespace BrilliantCut.RelatedQuery.Extensions
{
    /// <summary>
    /// Search extensions
    /// </summary>
    public static class SearchExtensions
    {
        /// <summary>
        /// Executes the query, and performs a post filter, to exclude content that isn't related.
        /// </summary>
        /// <typeparam name="TResult">The search type.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>Related items.</returns>
        public static IEnumerable<TResult> GetRelatedResult<TResult>(this ISearch<TResult> query)
        {
            return query.GetResult().Hits.Where(x => x.Score > 1).Select(x => x.Document);
        }
    }
}
