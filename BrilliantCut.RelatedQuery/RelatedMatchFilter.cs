using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Find;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Api.Querying.Filters;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// A filter, which will match content against the property for an exact match.
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class RelatedMatchFilter<TQuery> : RelatedFilterBase<TQuery>
    {
        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="client">The Episerver Find client.</param>
        public RelatedMatchFilter(IClient client)
            : base(client)
        {
        }
        /// <summary>
        /// Creates a filter, which will match content against the property for an exact match.
        /// </summary>
        /// <param name="content">The content to use when creating the query.</param>
        /// <returns>Match filter.</returns>
        public override Filter CreateRelatedFilter(IEnumerable<TQuery> content)
        {
            var filterBuilder = new FilterBuilder<TQuery>(Client);

            return content
                .Select(instance => CompiledProperty(instance)).Distinct()
                .Aggregate(filterBuilder, (current, valueToMatch) => 
                    current.Or(x =>
                        new TermFilter(GetFieldName(valueToMatch.GetOriginalType()), GetFilterValue(valueToMatch))));
        }
    }
}
