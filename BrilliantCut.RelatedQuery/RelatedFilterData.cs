using System;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// Class containing a related filter together with the type it has been registered for.
    /// </summary>
    public class RelatedFilterData
    {
        /// <summary>
        /// The registered type.
        /// </summary>
        public Type ContentType { get; set; }

        /// <summary>
        /// The related filter.
        /// </summary>
        public IRelatedFilter<object> RelatedFilter { get; set; }
    }
}
