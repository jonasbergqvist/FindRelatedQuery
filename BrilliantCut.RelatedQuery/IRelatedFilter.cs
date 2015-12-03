using System.Collections.Generic;
using EPiServer.Find.Api.Querying;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// Related filter
    /// </summary>
    public interface IRelatedFilter<out TQuery>
    {
        /// <summary>
        /// How much the filter should be boost. Higher number will make the filter more important.
        /// </summary>
        double Boost { get; }

        /// <summary>
        /// Creates a filter based on content.
        /// </summary>
        /// <returns></returns>
        Filter CreateFilter(IEnumerable<object> content);
    }
}
