using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Framework.TypeScanner;
using EPiServer.ServiceLocation;

namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// Register for the filters that will be used when creating a related query.
    /// </summary>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class RelatedFilterRegistry
    {
        private readonly ITypeScannerLookup _typeScannerLookup;
        private readonly IServiceLocator _serviceLocator;
        private readonly ConcurrentDictionary<Type, RelatedQueryData> _relatedFilters;
        private readonly ConcurrentDictionary<Type, IModifyExclusionFilterQuery> _filterQueryModifications;
        private readonly ConcurrentDictionary<Type, IModifySearchQuery> _searchQueryModifications;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        public RelatedFilterRegistry(IServiceLocator serviceLocator, ITypeScannerLookup typeScannerLookup)
        {
            _serviceLocator = serviceLocator;
            _typeScannerLookup = typeScannerLookup;
            _relatedFilters = new ConcurrentDictionary<Type, RelatedQueryData>();
            _filterQueryModifications = new ConcurrentDictionary<Type, IModifyExclusionFilterQuery>();
            _searchQueryModifications = new ConcurrentDictionary<Type, IModifySearchQuery>();
        }

        internal void RegisterRelatedQueries()
        {
            var relatedQueryTypes = _typeScannerLookup.AllTypes
                .Where(t => !t.IsAbstract && typeof(IRelatedQuery).IsAssignableFrom(t));

            foreach (var relatedQueryType in relatedQueryTypes)
            {
                var relatedQuery = _serviceLocator.GetInstance(relatedQueryType) as IRelatedQuery;
                if (relatedQuery == null)
                {
                    continue;
                }

                relatedQuery.RegistryQuery(new RelatedQueryRegistration(relatedQueryType, _relatedFilters, _serviceLocator));
            }
        }

        /// <summary>
        /// Gets related filters that has been registered for a specific type.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IRelatedFilter<object>> List<TRelatedQuery>()
            where TRelatedQuery : IRelatedQuery
        {
            var type = typeof (TRelatedQuery);
            if (!_relatedFilters.ContainsKey(type))
            {
                return Enumerable.Empty<IRelatedFilter<object>>();
            }

            return _relatedFilters[type].RelatedFilters;
        }

        public bool TryGetModifiedFilterQueryFunc<TRelatedQuery>(out IModifyExclusionFilterQuery modifyFilterQuery)
        {
            var relatedQueryType = typeof (TRelatedQuery);
            if (_filterQueryModifications.ContainsKey(relatedQueryType))
            {
                modifyFilterQuery = _filterQueryModifications[relatedQueryType];
                return true;
            }

            modifyFilterQuery = null;
            return false;
        }

        public bool TryGetModifiedSearchQueryFunc<TRelatedQuery>(out IModifySearchQuery modifySearchQuery)
        {
            var relatedQueryType = typeof(TRelatedQuery);
            if (_filterQueryModifications.ContainsKey(relatedQueryType))
            {
                modifySearchQuery = _searchQueryModifications[relatedQueryType];
                return true;
            }

            modifySearchQuery = null;
            return false;
        }

        /// <summary>
        /// Gets related filters that has been registered.
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<Type, RelatedQueryData> List()
        {
            return _relatedFilters;
        }
    }
}
