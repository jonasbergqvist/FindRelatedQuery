using System;
using System.Collections.Generic;
using System.Linq;
using BrilliantCut.RelatedQuery;
using EPiServer;
using EPiServer.Find;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Api.Querying.Filters;

namespace BrilliantCut.Filters.RelatedQuery
{
    /// <summary>
    /// A filter, which will perform get the min and max value from the content, and make a range filtering for the property between the min and max value.
    /// </summary>
    public class RelatedRangeFilter<TQuery> : RelatedFilterBase<TQuery>
    {
        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="client"></param>
        public RelatedRangeFilter(IClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Creates a filter, which will perform get the min and max value from the content, and make a range filtering for the property between the min and max value.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public override Filter CreateRelatedFilter(IEnumerable<TQuery> content)
        {
            var values = content.Select(instance => CompiledProperty(instance)).ToArray();
            return CreateRangeFilter(GetFieldName(values.Any() ? values.First().GetOriginalType() : typeof(object)), values);
        }

        /// <summary>
        /// Creates a filter, which will perform get the min and max value from the content, and make a range filtering for the property between the min and max value.
        /// </summary>
        /// <param name="property">The property to use.</param>
        /// <param name="values">The values to use when creating the query.</param>
        /// <returns>The range filter.</returns>
        protected virtual Filter CreateRangeFilter(string property, object[] values)
        {
            if (!values.Any())
            {
                return null;
            }
            
            var firstValue = values.FirstOrDefault();

            if (firstValue is int)
            {
                var min = Convert.ToInt32(values.Min());
                var max = Convert.ToInt32(values.Max());

                return RangeFilter.Create(property, min, max);
            }

            if (firstValue is double)
            {
                var min = Convert.ToDouble(values.Min());
                var max = Convert.ToDouble(values.Max());

                return RangeFilter.Create(property, min, max);
            }

            if (firstValue is decimal)
            {
                var min = Convert.ToDecimal(values.Min());
                var max = Convert.ToDecimal(values.Max());

                return RangeFilter.Create(property, min, max);
            }

            if (firstValue is DateTime)
            {
                var min = Convert.ToDateTime(values.Min());
                var max = Convert.ToDateTime(values.Max());

                return RangeFilter.Create(property, min, max);
            }

            throw new NotSupportedException();
        }
    }
}
