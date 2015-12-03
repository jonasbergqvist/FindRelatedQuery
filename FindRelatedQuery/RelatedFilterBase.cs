using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Find;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Helpers.Reflection;

namespace FindRelatedQuery
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public abstract class RelatedFilterBase<TQuery, TValue> : IRelatedFilter<TQuery>
    {
        private Func<TQuery, TValue> _compiledProperty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        protected RelatedFilterBase(IClient client)
        {
            Client = client;
        }

        /// <summary>
        /// 
        /// </summary>
        protected IClient Client { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Expression<Func<TQuery, TValue>> Property { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Boost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected Func<TQuery, TValue> CompiledProperty
        {
            get { return _compiledProperty ?? (_compiledProperty = Property.Compile()); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public Filter CreateFilter(IEnumerable<object> instances)
        {
            return CreateRelatedFilter(instances.OfType<TQuery>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public abstract Filter CreateRelatedFilter(IEnumerable<TQuery> instances);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual string GetFieldName()
        {
            var name = Property.GetFieldPath();
            return TypeSuffix.GetSuffixedFieldName(name, typeof(TValue));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual FieldFilterValue GetFilterValue(TValue value)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                return FieldFilterValue.Create(stringValue);
            }

            if (value is int)
            {
                return FieldFilterValue.Create(Convert.ToInt32(value));
            }

            throw new NotSupportedException();
        }
    }
}
