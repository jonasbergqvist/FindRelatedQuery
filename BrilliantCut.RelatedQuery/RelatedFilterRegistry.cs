using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EPiServer.ServiceLocation;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// Register for the filters that will be used when creating a related query.
    /// </summary>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class RelatedFilterRegistry
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly List<IRelatedFilter<object>> _relatedFilters;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        public RelatedFilterRegistry(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _relatedFilters = new List<IRelatedFilter<object>>();
        }

        /// <summary>
        /// Adds a filter to the registry.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        /// <returns></returns>
        public RelatedFilterRegistry AddFilter(IRelatedFilter<object> filter)
        {
            _relatedFilters.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds a filter, which will match content against the property for an exact match.
        /// </summary>
        /// <param name="property">The property to use</param>
        /// <param name="boost">The importance of the filter. Higher boost makes the filter more important.</param>
        /// <returns>The match filter.</returns>
        public RelatedFilterRegistry AddMatchFilter<TQuery, TValue>(Expression<Func<TQuery, TValue>> property, double boost)
        {
            var filter = _serviceLocator.GetInstance<RelatedMatchFilter<TQuery, TValue>>();
            SetProperties(filter, property, boost);

            _relatedFilters.Add((IRelatedFilter<object>)filter);
            return this;
        }

        /// <summary>
        /// Adds a filter, which will perform get the min and max value from the content, and make a range filtering for the property between the min and max value.
        /// </summary>
        /// <param name="property">The property to use</param>
        /// <param name="boost">The importance of the filter. Higher boost makes the filter more important.</param>
        /// <returns>The range filter.</returns>
        public RelatedFilterRegistry AddRangeFilter<TQuery, TValue>(Expression<Func<TQuery, TValue>> property, double boost)
        {
            var filter = _serviceLocator.GetInstance<RelatedRangeFilter<TQuery, TValue>>();
            SetProperties(filter, property, boost);

            _relatedFilters.Add((IRelatedFilter<object>)filter);
            return this;
        }

        /// <summary>
        /// Gets all related filters that has been registered.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IRelatedFilter<object>> List()
        {
            return _relatedFilters;
        }

        private static void SetProperties<TQuery, TValue>(RelatedFilterBase<TQuery, TValue> filter, Expression<Func<TQuery, TValue>> property, double boost)
        {
            filter.Property = property;
            filter.Boost = boost;
        }
    }
}
