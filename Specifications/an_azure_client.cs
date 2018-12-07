using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Dolittle.Runtime.Events.Azure.Store;
using Dolittle.Runtime.Events.Azure.Specs;

namespace Dolittle.Runtime.Events.Azure.Specs
{
    public class an_azure_client : IDisposable
    {
        public EventStoreAzureDbConfiguration Config { get; private set; }  
        public DocumentCollection Commits { get; private set; } 

        public async Task<bool> HasCommitStoredProcedure() => await Config.HasStoredProcedure(Config.CommitsUri,CosmosConstants.COMMIT_STORED_PROCEDURE);
        
        public an_azure_client()
        {
            Config = given.GetEventStoreConfig();
            ClearDatabase();
        }

        public Database GetEventStoreDb()
        {
           return Config.Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(Config.DatabaseId)).Result;
        }

        public void Dispose()
        {
            ClearDatabase();
        }

        void ClearDatabase()
        {
            try
            {
                var tidyUpTasks = new[]
                {
                    DeleteAllDocuments(CosmosConstants.COMMITS_COLLECTION),
                    DeleteAllDocuments(CosmosConstants.OFFSETS_COLLECTION),
                    ClearStoredProceduresAndTriggers()
                };

                Task.WhenAll(tidyUpTasks).GetAwaiter().GetResult();
            } 
            catch(Exception ex)
            {
                Config.Logger.Error((ex as AggregateException)?.Flatten().InnerException ?? ex,ex.Message);
            }

            Commits = Config.Collections.SingleOrDefault(c => c.Id == CosmosConstants.COMMITS_COLLECTION);
        }

        public long GetDocumentCountFor(string collectionId)
        {
            IQueryable<dynamic> query = Config.Client.CreateDocumentQuery<dynamic>(
                UriFactory.CreateDocumentCollectionUri(Config.DatabaseId, collectionId),
                "SELECT VALUE COUNT(1) FROM c",new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true});
            return query.ToList().FirstOrDefault() ?? 0;
        }
        
        Task DeleteAllDocuments(string collectionName)
        {
            var docs = GetDocs(collectionName);
            var deletes = new List<Task>();
            foreach (var doc in docs)
            {
                var requestOptions = new RequestOptions() {PartitionKey = new PartitionKey(doc.partitionKey)};
                deletes.Add(Config.Client.DeleteDocumentAsync(doc._self, requestOptions));
            }
            return Task.WhenAll(deletes);
        }

        Task ClearStoredProceduresAndTriggers()
        {
            var tasks = new List<Task>
            {
                //Config.Client.DeleteStoredProcedureAsync(Config.CommitStoredProcedure)
            };
            return Task.WhenAll(tasks).ContinueWith(_ => Config.EnsureStoredProceduresExist());
        }

        List<dynamic> GetDocs(string collectionName)
        {
            return Config.Client.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri(Config.DatabaseId, collectionName), "select c._self, c.partitionKey from c", new FeedOptions() {EnableCrossPartitionQuery = true}).ToList();
        }
    }
}