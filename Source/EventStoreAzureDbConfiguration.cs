/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dolittle.Execution;
using Dolittle.Lifecycle;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Azure.Store;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dolittle.Runtime.Events.Azure
{
    /// <summary>
    /// 
    /// </summary>
    [SingletonPerTenant]
    public class EventStoreAzureDbConfiguration 
    {  
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string DatabaseId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string EndpointUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string AuthorizationKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DocumentCollection> Collections =>_collections?.AsEnumerable() ?? Enumerable.Empty<DocumentCollection>();
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ILogger Logger { get; private set;}
        private readonly IExecutionContextManager _executionContextManager;

        /// <summary>
        /// 
        /// </summary>
        public DocumentClient Client { get; }

        readonly IList<DocumentCollection> _collections;

        bool _isConfigured;

        /// <summary>
        /// 
        /// </summary>
        public Uri CommitsUri { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Uri OffsetsUri { get; private set; }
                /// <summary>
        /// 
        /// </summary>
        public Uri GeodesicsOffsetsUri { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Uri CommitStoredProcedure { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string BasePartitionKey { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpointUrl"></param>
        /// <param name="databaseId"></param>
        /// <param name="authKey"></param>
        /// <param name="logger"></param>
        /// <param name="executionContextManager"></param>
        public EventStoreAzureDbConfiguration(string endpointUrl, string databaseId, string authKey, ILogger logger, IExecutionContextManager executionContextManager)
        {
            Logger = logger;
            _executionContextManager = executionContextManager;
            EndpointUrl = endpointUrl;
            DatabaseId = databaseId;
            AuthorizationKey = authKey;
            _collections = new List<DocumentCollection>();
            Client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);
            BasePartitionKey =
                $"{executionContextManager.Current.Application}_{executionContextManager.Current.BoundedContext}_{executionContextManager.Current.Tenant}";
            Bootstrap();
        }

        void Bootstrap()
        {
            if (_isConfigured) 
                return;
            CommitsUri = UriFactory.CreateDocumentCollectionUri(DatabaseId,CosmosConstants.COMMITS_COLLECTION);
            OffsetsUri = UriFactory.CreateDocumentCollectionUri(DatabaseId,CosmosConstants.OFFSETS_COLLECTION);
            CommitStoredProcedure = UriFactory.CreateStoredProcedureUri(DatabaseId, CosmosConstants.COMMITS_COLLECTION, CosmosConstants.COMMIT_STORED_PROCEDURE);

            var tasks = new List<Task>
            {
                EnsureDatabaseExists(DatabaseId),
                EnsureCollectionsExist(),
                EnsureStoredProceduresExist()
            };
            Task.WhenAll(tasks).Wait();
            _isConfigured = true;

        }

        private async Task EnsureDatabaseExists(string dbName)
        {
            await Client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseId });
        }

        private async Task EnsureCollectionsExist()
        {
            var dbUri = UriFactory.CreateDatabaseUri(DatabaseId);
            var ensureCollections = new[]
            {
                Client.CreateDocumentCollectionIfNotExistsAsync(dbUri, CreateCommitsCollection())
                    .ContinueWith(t => _collections.Add(t.Result.Resource)),
                Client.CreateDocumentCollectionIfNotExistsAsync(dbUri, CreateOffsetsCollection())
                    .ContinueWith(t => _collections.Add(t.Result.Resource)),    
            };
            await Task.WhenAll(ensureCollections);
        }

        private DocumentCollection CreateCommitsCollection()
        {
            var commits = new DocumentCollection
            {
                Id = CosmosConstants.COMMITS_COLLECTION,
                DefaultTimeToLive = CosmosConstants.NEVER_EXPIRE
            };
            
            commits.PartitionKey.Paths.Add($"/{CosmosConstants.PARTITON_KEY}");

            commits.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*"});
            commits.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = $"/{CosmosConstants.ID}/?", Indexes = new System.Collections.ObjectModel.Collection<Index> { new RangeIndex(DataType.Number, -1) }});
            commits.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/commit/?", Indexes = new System.Collections.ObjectModel.Collection<Index> { new RangeIndex(DataType.Number, -1) }});
            commits.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/events/*" });
            
            //Unique index so we cannot have a duplicate 
            commits.UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys = new Collection<UniqueKey>
                {
                    new UniqueKey { Paths = new Collection<string> { "/eventsource_id" , "/commit", "/event_source_artifact" }}
                }
            };
            return commits;
        }

        private DocumentCollection CreateOffsetsCollection()
        {
            var offsets = new DocumentCollection
            {
                Id = CosmosConstants.OFFSETS_COLLECTION,
                DefaultTimeToLive = CosmosConstants.NEVER_EXPIRE
            };
            
            offsets.PartitionKey.Paths.Add($"/{CosmosConstants.PARTITON_KEY}");
            return offsets;
        }        
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task EnsureStoredProceduresExist()
        {
            await EnsureStoredProcedureExists(CosmosConstants.COMMIT_STORED_PROCEDURE, "commit.js", CommitsUri);
        }
        
        private async Task EnsureStoredProcedureExists(string procedureName, string resourceName, Uri collection)
        {
            if (!(await HasStoredProcedure(collection, procedureName)))
            {
                var result = await Client.CreateStoredProcedureAsync(collection, new StoredProcedure
                {
                    Id = procedureName,
                    Body = Resources.GetString(resourceName)
                });
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        public async Task<bool> HasStoredProcedure(Uri collection, string storedProcedure)
        {

            var query = Client.CreateStoredProcedureQuery(collection)
                .Where(x => x.Id == storedProcedure)
                .AsDocumentQuery();
            var results = await query.ExecuteNextAsync<StoredProcedure>();
            return results.Any();
        }
    }
    
    internal static class Resources
    {
        public static string GetString(string resourceName)
        {
            using (var reader = new StreamReader(GetStream(resourceName)))
            {
                var str = reader.ReadToEnd();
                return str;
            }
        }

        private static Stream GetStream(string resource)
        {
            var resourceName = $"{typeof(EventStore).Namespace}.{resource}";
            Console.WriteLine($"trying to get {resourceName}");
            return typeof(Resources).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName);
        }
    }
}