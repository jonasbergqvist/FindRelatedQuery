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
        private readonly ConcurrentDictionary<Type, List<IRelatedFilter<object>>> _relatedFilters;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        public RelatedFilterRegistry(IServiceLocator serviceLocator, ITypeScannerLookup typeScannerLookup)
        {
            _serviceLocator = serviceLocator;
            _typeScannerLookup = typeScannerLookup;
            _relatedFilters = new ConcurrentDictionary<Type, List<IRelatedFilter<object>>>();
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

                relatedQuery.RegistryQuery(new RelatedFilterRegistration(relatedQueryType, _relatedFilters, _serviceLocator));
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

            return _relatedFilters[type];
        }

        /// <summary>
        /// Gets related filters that has been registered.
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<Type, List<IRelatedFilter<object>>> List()
        {
            return _relatedFilters;
        }
    }
}
