using System.Collections.Generic;

namespace BrilliantCut.RelatedQuery
{
    public class RelatedQueryData
    {
        public List<IRelatedFilter<object>> RelatedFilters { get; set; }
        public IModifyFilterQuery ModifyFilterQuery { get; set; }
        public IModifySearchQuery ModifySearchQuery { get; set; }
    }
}
