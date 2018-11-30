using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Dolittle.Runtime.Events.Azure.Store;
using Dolittle.Runtime.Events.Azure.Specs;

namespace Dolittle.Runtime.Events.Azure.Specs.Store
{

    public class test_azure_event_store : EventStore
    {
        private readonly an_azure_client an_azure_client;
        public test_azure_event_store(an_azure_client client): base(client.Config,given.GetSerializer(),given.GetLogger())
        {
            an_azure_client = client;
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                an_azure_client.Dispose();
                disposedValue = true;
            }
        }
    }
}