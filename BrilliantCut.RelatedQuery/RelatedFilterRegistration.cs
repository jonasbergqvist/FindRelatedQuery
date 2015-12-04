using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using EPiServer.ServiceLocation;

namespace BrilliantCut.RelatedQuery
{
    public class RelatedFilterRegistration
    {
        private readonly Type _type;
        private readonly ConcurrentDictionary<Type, List<IRelatedFilter<object>>> _relatedFilters;
        private readonly IServiceLocator _serviceLocator;

        public RelatedFilterRegistration(Type type, ConcurrentDictionary<Type, List<IRelatedFilter<object>>> relatedFilters, IServiceLocator serviceLocator)
        {
            _type = type;
            _relatedFilters = relatedFilters;
            _serviceLocator = serviceLocator;
        }

        /// <summary>
        /// Adds a filter to the registry.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        /// <returns></returns>
        public RelatedFilterRegistration AddFilter(IRelatedFilter<object> filter)
        {
            _relatedFilters.AddOrUpdate(_type,
                add => new List<IRelatedFilter<object>> {filter},
                (t, list) =>
                {
                    list.Add(filter);
                    return list;
                });

            return this;
        }

        /// <summary>
        /// Adds a filter, which will match content against the property for an exact match.
        /// </summary>
        /// <param name="property">The property to use</param>
        /// <param name="boost">The importance of the filter. Higher boost makes the filter more important.</param>
        /// <returns>The match filter.</returns>
        public RelatedFilterRegistration AddMatchFilter<TQuery, TValue>(Expression<Func<TQuery, TValue>> property, double boost)
        {
            AddFilter<RelatedMatchFilter<TQuery, TValue>, TQuery, TValue>(property, boost);
            return this;
        }

        /// <summary>
        /// Adds a filter, which will perform get the min and max value from the content, and make a range filtering for the property between the min and max value.
        /// </summary>
        /// <param name="property">The property to use</param>
        /// <param name="boost">The importance of the filter. Higher boost makes the filter more important.</param>
        /// <returns>The range filter.</returns>
        public RelatedFilterRegistration AddRangeFilter<TQuery, TValue>(Expression<Func<TQuery, TValue>> property, double boost)
        {
            AddFilter<RelatedRangeFilter<TQuery, TValue>, TQuery, TValue>(property, boost);
            return this;
        }

        /// <summary>
        /// Adds a filter that inherits from RelatedFilterBase
        /// </summary>
        /// <typeparam name="TRelatedFilterBase">The type inheriting from RelatedFilterBase</typeparam>
        /// <typeparam name="TQuery">The query type</typeparam>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <param name="property">The property</param>
        /// <param name="boost">The importance of the filter. Higher number makes the filter more important.</param>
        protected virtual void AddFilter<TRelatedFilterBase, TQuery, TValue>(Expression<Func<TQuery, TValue>> property, double boost)
            where TRelatedFilterBase : RelatedFilterBase<TQuery, TValue>
        {
            var filter = _serviceLocator.GetInstance<TRelatedFilterBase>();
            SetProperties(filter, property, boost);

            AddFilter((IRelatedFilter<object>)filter);
        }

        private static void SetProperties<TQuery, TValue>(RelatedFilterBase<TQuery, TValue> filter, Expression<Func<TQuery, TValue>> property, double boost)
        {
            filter.Property = property;
            filter.Boost = boost;
        }
    }
}
