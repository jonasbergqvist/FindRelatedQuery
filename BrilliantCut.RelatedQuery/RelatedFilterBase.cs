using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Find;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Helpers.Reflection;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// Abstract class for related filters.
    /// </summary>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public abstract class RelatedFilterBase<TQuery> : IRelatedFilter<TQuery>
    {
        private Func<TQuery, object> _compiledProperty;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="client"></param>
        protected RelatedFilterBase(IClient client)
        {
            Client = client;
        }

        /// <summary>
        /// The Episerver Find client.
        /// </summary>
        protected IClient Client { get; set; }

        /// <summary>
        /// The property to use in the query.
        /// </summary>
        public Expression<Func<TQuery, object>> Property { get; set; }

        /// <summary>
        /// The importance of the filter. Higher number will make the filter more important.
        /// </summary>
        public double Boost { get; set; }

        /// <summary>
        /// Lazy created compiled property.
        /// </summary>
        protected Func<TQuery, object> CompiledProperty
        {
            get { return _compiledProperty ?? (_compiledProperty = Property.Compile()); }
        }

        /// <summary>
        /// Creates a filter using CreateRelatedFilter.
        /// </summary>
        /// <param name="content">The content to use when creating the query.</param>
        /// <returns>The related filter.</returns>
        public Filter CreateFilter(IEnumerable<object> content)
        {
            return CreateRelatedFilter(content.OfType<TQuery>());
        }

        /// <summary>
        /// When implemented, a filter will be created that will be used when a related query are created.
        /// </summary>
        /// <param name="content">The content to use when creating the query.</param>
        /// <returns>The related filter</returns>
        public abstract Filter CreateRelatedFilter(IEnumerable<TQuery> content);

        /// <summary>
        /// Gets the field name for the property
        /// </summary>
        /// <returns></returns>
        protected virtual string GetFieldName(Type type)
        {
            var name = Property.GetFieldPath();
            return TypeSuffix.GetSuffixedFieldName(name, type);
        }

        /// <summary>
        /// Gets the <see cref="FieldFilterValue"/> value
        /// </summary>
        /// <param name="value">The value</param>
        /// <exception cref="NotSupportedException">If the type isn't supported.</exception>
        /// <returns><see cref="FieldFilterValue"/> instance for the value.</returns>
        protected virtual FieldFilterValue GetFilterValue(object value)
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

            if (value is decimal)
            {
                return FieldFilterValue.Create(Convert.ToDecimal(value));
            }

            if (value is double)
            {
                return FieldFilterValue.Create(Convert.ToDouble(value));
            }

            if (value is DateTime)
            {
                return FieldFilterValue.Create(Convert.ToDateTime(value));
            }

            throw new NotSupportedException();
        }
    }
}
