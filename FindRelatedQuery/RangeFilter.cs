using System;
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
    public class RangeFilter<TQuery, TValue> : RelatedFilterBase<TQuery, TValue>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public RangeFilter(IClient client)
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
            var values = instances.Select(instance => CompiledProperty(instance)).ToArray();
            return CreateRangeFilter(GetFieldName(), values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        protected virtual Filter CreateRangeFilter(string fieldName, TValue[] values)
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

                return RangeFilter.Create(fieldName, min, max);
            }

            if (firstValue is double)
            {
                var min = Convert.ToDouble(values.Min());
                var max = Convert.ToDouble(values.Max());

                return RangeFilter.Create(fieldName, min, max);
            }

            if (firstValue is decimal)
            {
                var min = Convert.ToDecimal(values.Min());
                var max = Convert.ToDecimal(values.Max());

                return RangeFilter.Create(fieldName, min, max);
            }

            if (firstValue is DateTime)
            {
                var min = Convert.ToDateTime(values.Min());
                var max = Convert.ToDateTime(values.Max());

                return RangeFilter.Create(fieldName, min, max);
            }

            throw new NotSupportedException();
        }
    }
}
