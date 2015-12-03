using System;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// 
    /// </summary>
    public class RelatedFilterContext
    {
        /// <summary>
        /// 
        /// </summary>
        public Type ContentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IRelatedFilter<object> RelatedFilter { get; set; }
    }
}
