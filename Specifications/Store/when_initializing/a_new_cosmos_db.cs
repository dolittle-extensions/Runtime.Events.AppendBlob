using Dolittle.Runtime.Events.Azure.Store;
using Machine.Specifications;
using Microsoft.Azure.Documents;

namespace Dolittle.Runtime.Events.Azure.Specs.Store.when_initializing
 {   
     public class a_new_cosmos_db
     {
         static an_azure_client client;
         static Database database;
         static long commit_count;
         
         Establish context = () => 
         {
             client = new an_azure_client();
         };

         private Because of = () =>
         {
             database = client.GetEventStoreDb();
             commit_count = client.GetDocumentCountFor(CosmosConstants.COMMITS_COLLECTION);
         };
 
         It should_have_the_database = () => database.ShouldNotBeNull();
         It should_have_the_commits_collection  = () => client.Commits.ShouldNotBeNull();
         It should_have_an_empty_commits_collection = () => commit_count.ShouldEqual(0);
         It should_have_the_commit_stored_procedure = () => client.HasCommitStoredProcedure().GetAwaiter().GetResult().ShouldBeTrue();

         Cleanup es = () => client.Dispose();
     }
 }