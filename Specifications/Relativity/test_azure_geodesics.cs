namespace Dolittle.Runtime.Events.Relativity.Azure.Specs
{
    using Dolittle.Runtime.Events.Azure.Specs;
    using Dolittle.Runtime.Events.Relativity.Azure;
    using Dolittle.Runtime.Events.Relativity.Specs;

    public class test_azure_geodesics : Geodesics
    {
        private readonly an_azure_client an_azure_client;

        public test_azure_geodesics(an_azure_client client): base(client.Config,given.GetLogger())
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