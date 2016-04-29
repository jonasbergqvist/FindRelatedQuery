using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using BrilliantCut.Filters.RelatedQuery;
using EPiServer.ServiceLocation;

namespace BrilliantCut.RelatedQuery
{
    public class RelatedQueryRegistration : RelatedFilterRegistration
    {
        public RelatedQueryRegistration(Type type, ConcurrentDictionary<Type, RelatedQueryData> relatedFilters, IServiceLocator serviceLocator)
            : base(type, relatedFilters, serviceLocator)
        {
        }

        public ModifiedFilterRegistration ModifyExclusionFilterQuery<TModifyFilterQiery>()
            where TModifyFilterQiery : IModifyExclusionFilterQuery
        {
            InternalModifyFilterQuery<TModifyFilterQiery>();

            return new ModifiedFilterRegistration(FilterType, RelatedFilters, ServiceLocator); ;
        }

        public ModifiedSearchRegistration ModifySearchQuery<TModifySearchQuery>()
            where TModifySearchQuery : IModifySearchQuery
        {
            InternalModifySearchQuery<TModifySearchQuery>();

            return new ModifiedSearchRegistration(FilterType, RelatedFilters, ServiceLocator);
        }
    }

    public class ModifiedSearchRegistration : RelatedFilterRegistration
    {
        public ModifiedSearchRegistration(Type type, ConcurrentDictionary<Type, RelatedQueryData> relatedFilters, IServiceLocator serviceLocator)
            : base(type, relatedFilters, serviceLocator)
        {
        }

        public RelatedFilterRegistration ModifyExclusionFilterQuery<TModifyFilterQiery>()
            where TModifyFilterQiery : IModifyExclusionFilterQuery
        {
            InternalModifyFilterQuery<TModifyFilterQiery>();

            return this;
        }
    }

    public class ModifiedFilterRegistration : RelatedFilterRegistration
    {
        public ModifiedFilterRegistration(Type type, ConcurrentDictionary<Type, RelatedQueryData> relatedFilters, IServiceLocator serviceLocator)
            : base(type, relatedFilters, serviceLocator)
        {
        }

        public RelatedFilterRegistration ModifySearchQuery<TModifySearchQuery>()
            where TModifySearchQuery : IModifySearchQuery
        {
            InternalModifySearchQuery<TModifySearchQuery>();

            return this;
        }
    }

    public class RelatedFilterRegistration
    {
        public RelatedFilterRegistration(Type type, ConcurrentDictionary<Type, RelatedQueryData> relatedFilters, IServiceLocator serviceLocator)
        {
            FilterType = type;
            RelatedFilters = relatedFilters;
            ServiceLocator = serviceLocator;
        }

        protected Type FilterType { get; private set; }

        protected ConcurrentDictionary<Type, RelatedQueryData> RelatedFilters { get; private set; }

        protected IServiceLocator ServiceLocator { get; private set; }

        /// <summary>
        /// Adds a filter to the registry.
        /// </summary>
        /// <returns></returns>
        public RelatedFilterRegistration AddFilter<T>()
            where T : IRelatedFilter<object>
        {
            var filter = ServiceLocator.GetInstance<T>();
            return AddFilter(filter);
        }

        private RelatedFilterRegistration AddFilter(IRelatedFilter<object> filter)
        {
            RelatedFilters.AddOrUpdate(FilterType,
                key => new RelatedQueryData
                {
                    RelatedFilters = new List<IRelatedFilter<object>> {filter}
                },
                (key, relatedQueryData) =>
                {
                    relatedQueryData.RelatedFilters.Add(filter);
                    return relatedQueryData;
                });

            return this;
        }

        /// <summary>
        /// Adds a filter, which will match content against the property for an exact match.
        /// </summary>
        /// <param name="property">The property to use</param>
        /// <param name="boost">The importance of the filter. Higher boost makes the filter more important.</param>
        /// <returns>The match filter.</returns>
        public RelatedFilterRegistration AddMatchFilter<TQuery>(Expression<Func<TQuery, object>> property, double boost)
        {
            AddFilter<RelatedMatchFilter<TQuery>, TQuery>(property, boost);
            return this;
        }

        /// <summary>
        /// Adds a filter, which will perform get the min and max value from the content, and make a range filtering for the property between the min and max value.
        /// </summary>
        /// <param name="property">The property to use</param>
        /// <param name="boost">The importance of the filter. Higher boost makes the filter more important.</param>
        /// <returns>The range filter.</returns>
        public RelatedFilterRegistration AddRangeFilter<TQuery>(Expression<Func<TQuery, object>> property, double boost)
        {
            AddFilter<RelatedRangeFilter<TQuery>, TQuery>(property, boost);
            return this;
        }

        protected void InternalModifyFilterQuery<TModifyFilterQiery>()
            where TModifyFilterQiery : IModifyExclusionFilterQuery
        {
            var modifyFilterQuery = ServiceLocator.GetInstance<TModifyFilterQiery>();

            RelatedFilters.AddOrUpdate(FilterType,
                key => new RelatedQueryData
                {
                    RelatedFilters = new List<IRelatedFilter<object>>(),
                    ModifyFilterQuery = modifyFilterQuery
                },
                (key, relatedQueryData) =>
                {
                    relatedQueryData.ModifyFilterQuery = modifyFilterQuery;
                    return relatedQueryData;
                });
        }

        protected void InternalModifySearchQuery<TModifySearchQuery>()
            where TModifySearchQuery : IModifySearchQuery
        {
            var modifySearchQuery = ServiceLocator.GetInstance<TModifySearchQuery>();

            RelatedFilters.AddOrUpdate(FilterType,
                key => new RelatedQueryData
                {
                    RelatedFilters = new List<IRelatedFilter<object>>(),
                    ModifySearchQuery = modifySearchQuery
                },
                (key, relatedQueryData) =>
                {
                    relatedQueryData.ModifySearchQuery = modifySearchQuery;
                    return relatedQueryData;
                });
        }

        /// <summary>
        /// Adds a filter that inherits from RelatedFilterBase
        /// </summary>
        /// <typeparam name="TRelatedFilterBase">The type inheriting from RelatedFilterBase</typeparam>
        /// <typeparam name="TQuery">The query type</typeparam>
        /// <param name="property">The property</param>
        /// <param name="boost">The importance of the filter. Higher number makes the filter more important.</param>
        protected virtual void AddFilter<TRelatedFilterBase, TQuery>(Expression<Func<TQuery, object>> property, double boost)
            where TRelatedFilterBase : RelatedFilterBase<TQuery>
        {
            var filter = ServiceLocator.GetInstance<TRelatedFilterBase>();
            SetProperties(filter, property, boost);

            AddFilter((IRelatedFilter<object>)filter);
        }

        private static void SetProperties<TQuery>(RelatedFilterBase<TQuery> filter, Expression<Func<TQuery, object>> property, double boost)
        {
            filter.Property = property;
            filter.Boost = boost;
        }
    }
}
