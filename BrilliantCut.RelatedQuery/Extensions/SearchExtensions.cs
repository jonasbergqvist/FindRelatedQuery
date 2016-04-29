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
        public static RelatedSearchResults<TResult> GetRelatedResult<TResult>(this ISearch<TResult> query)
        {
            return new RelatedSearchResults<TResult>(query.GetResult());
        }
    }
}
