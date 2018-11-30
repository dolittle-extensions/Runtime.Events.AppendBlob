namespace Dolittle.Runtime.Events.Processing.Azure.Specs
{
    using Dolittle.Runtime.Events.Azure.Processing;
    using Dolittle.Runtime.Events.Azure.Specs;

    public class test_event_processor_offset_repository : EventProcessorOffsetRepository
    {
        private readonly an_azure_client an_azure_client;

        public test_event_processor_offset_repository(an_azure_client client): base(client.Config,given.GetLogger())
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