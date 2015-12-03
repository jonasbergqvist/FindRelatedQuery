using System.Collections.Generic;
using System.Linq;
using EPiServer.Find;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Api.Querying.Filters;

namespace FindRelatedQuery
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MatchFilter<TQuery, TValue> : RelatedFilterBase<TQuery, TValue>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public MatchFilter(IClient client)
            : base(client)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public override Filter CreateRelatedFilter(IEnumerable<TQuery> instances)
        {
            var filterBuilder = new FilterBuilder<TQuery>(Client);

            return instances
                .Select(instance => CompiledProperty(instance)).Distinct()
                .Aggregate(filterBuilder, (current, valueToMatch) => 
                    current.Or(x => 
                        new TermFilter(GetFieldName(), GetFilterValue(valueToMatch))));
        }
    }
}
