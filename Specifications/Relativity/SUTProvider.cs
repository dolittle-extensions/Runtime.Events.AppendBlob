using Dolittle.Runtime.Events.Azure.Specs;

namespace Dolittle.Runtime.Events.Relativity.Azure.Specs
{
    using Dolittle.Runtime.Events.Relativity.Azure;
    using Dolittle.Runtime.Events.Relativity.Specs;
    public class SUTProvider : IProvideGeodesics
    {
        public IGeodesics Build() => new test_azure_geodesics(new an_azure_client());
    }
}