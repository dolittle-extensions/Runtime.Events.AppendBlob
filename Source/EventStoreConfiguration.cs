using Dolittle.Lifecycle;

namespace Dolittle.Runtime.Events.Azure
{
    /// <summary>
    /// Defines the config values for the Azure CosmosDB instance
    /// </summary>
    [SingletonPerTenant]
    public class EventStoreConfiguration
    {
        /// <summary>
        /// Gets or set the EndPoint url for the CosmosDB instance
        /// </summary>
        public string EndPointUrl { get; set; }
        /// <summary>
        /// Gets or sets the database id for the configuration
        /// </summary>
        public string DatabaseId { get; set; }
        /// <summary>
        /// Gets or sets the Auth Key for the CosmosDB instance
        /// </summary>
        public string AuthKey { get; set; }     
    }
}