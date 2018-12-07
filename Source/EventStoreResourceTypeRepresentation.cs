namespace Dolittle.Runtime.Events.Azure
{
    using Dolittle.Runtime.Events;
    using Dolittle.Resources;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the Azure Cosmos DB implemenation of EventStore as a Resource
    /// </summary>
    public class EventStoreResourceTypeRepresentation : IRepresentAResourceType
    {
        static IDictionary<Type, Type> _bindings = new Dictionary<Type, Type>
        {
            {typeof(Dolittle.Runtime.Events.Store.IEventStore), typeof(Dolittle.Runtime.Events.Azure.Store.EventStore)},
            {typeof(Dolittle.Runtime.Events.Relativity.IGeodesics), typeof(Dolittle.Runtime.Events.Relativity.Azure.Geodesics)},
            {typeof(Dolittle.Runtime.Events.Processing.IEventProcessorOffsetRepository), typeof(Dolittle.Runtime.Events.Azure.Processing.EventProcessorOffsetRepository)}
        };
        
        /// <inheritdoc/>
        public ResourceType Type => "eventStore";
        /// <inheritdoc/>
        public ResourceTypeImplementation ImplementationName => "Azure";
        /// <inheritdoc/>
        public Type ConfigurationObjectType => typeof(EventStoreConfiguration);
        /// <inheritdoc/>
        public IDictionary<Type, Type> Bindings => _bindings;
    }
}