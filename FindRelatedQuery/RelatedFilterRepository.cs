using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EPiServer.ServiceLocation;

namespace FindRelatedQuery
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class RelatedFilterRepository
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly List<IRelatedFilter<object>> _relatedFilters;

        /// <summary>
        /// 
        /// </summary>
        public RelatedFilterRepository(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _relatedFilters = new List<IRelatedFilter<object>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public RelatedFilterRepository AddFilter(IRelatedFilter<object> filter)
        {
            _relatedFilters.Add(filter);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="boost"></param>
        /// <returns></returns>
        public RelatedFilterRepository AddMatchFilter<TQuery, TValue>(Expression<Func<TQuery, TValue>> property, double boost)
        {
            var filter = _serviceLocator.GetInstance<MatchFilter<TQuery, TValue>>();
            SetProperties(filter, property, boost);

            _relatedFilters.Add((IRelatedFilter<object>)filter);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="boost"></param>
        /// <returns></returns>
        public RelatedFilterRepository AddRangeFilter<TQuery, TValue>(Expression<Func<TQuery, TValue>> property, double boost)
        {
            var filter = _serviceLocator.GetInstance<RangeFilter<TQuery, TValue>>();
            SetProperties(filter, property, boost);

            _relatedFilters.Add((IRelatedFilter<object>)filter);
            return this;
        }

        /// <summary>
        /// 
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
