using System.Collections.Generic;
using EPiServer.Find.Api.Querying;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRelatedFilter<out TQuery>
    {
        /// <summary>
        /// 
        /// </summary>
        double Boost { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Filter CreateFilter(IEnumerable<object> instances);
    }
}
