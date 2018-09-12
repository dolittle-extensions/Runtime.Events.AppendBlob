using System;
using System.IO;
using Dolittle.Execution;
using Dolittle.Lifecycle;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace Dolittle.Runtime.Events.Azure.Storage.Store
{
    public class StorageConfig
    {
        public string ConnectionString {get; set;}
        public string BlobContainer {get; set;}
    }
    /// <summary>
    /// The configuration for <see cref="Dolittle.Runtime.Events.Azure.Storage.Store.EventStore"/>
    /// </summary>
    [Dolittle.Lifecycle.SingletonPerTenant]
    public class EventStoreConfig
    {
        /// <summary>
        /// Name of the Commits collection
        /// </summary>
        public const string COMMITS = "commits"; 
        /// <summary>
        /// Name of the Versions collection
        /// </summary>
        public const string VERSIONS = "versions";
        /// <summary>
        /// Name of the Snapshots collection
        /// </summary>
        public const string SNAPSHOTS = "shapshots";

        /// <summary>
        /// Gets the <see cref="CloudAppendBlob"/> representing the Commits
        public CloudAppendBlob Commits {get; }
        /// <summary>
        /// Gets the <see cref="CloudAppendBlob"/> representing the Versions
        /// </summary>
        public CloudAppendBlob Versions {get; }

        // public CloudAppendBlob Snapshots {get; }
        readonly CloudBlobClient _cloudBlobClient;
        readonly CloudBlobContainer _container;
        
        bool _isConfigured = false;

        /// <summary>
        /// Instantiates an instance of <see cref="EventStoreConfig"/>
        /// </summary>
        public EventStoreConfig(ExecutionContext executionContext)
        {
            
            if (File.Exists(Constants.LOCAL_STORAGE_CONFIG_PATH))
            {
                // Temporary solution for local testing
                var storageConfig = LoadLocalStorageConfig();

                var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
                _cloudBlobClient = storageAccount.CreateCloudBlobClient();
                _container = _cloudBlobClient.GetContainerReference(storageConfig.BlobContainer);
                Commits = _container.GetAppendBlobReference(COMMITS);
                Versions = _container.GetAppendBlobReference(VERSIONS);

            }
            else 
            {
                var storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable(Constants.CONNECTION_STRING_ENV_VARIABLE));
                _cloudBlobClient = storageAccount.CreateCloudBlobClient();
                // We propbably want to have a constant container name that we create if it doesn't exist
                _container = _cloudBlobClient.GetContainerReference(System.Environment.GetEnvironmentVariable(Constants.BLOB_CONTAINER_ENV_VARIABLE));
                Commits = _container.GetAppendBlobReference(COMMITS);
                Versions = _container.GetAppendBlobReference(VERSIONS);
            }
        }

        StorageConfig LoadLocalStorageConfig()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<StorageConfig>(File.ReadAllText(Constants.LOCAL_STORAGE_CONFIG_PATH));
        }
    }
}