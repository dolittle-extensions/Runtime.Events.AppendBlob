namespace Dolittle.Runtime.Events.Processing.Azure.Specs
{
    using Dolittle.Runtime.Events.Processing;
    using Dolittle.Runtime.Events.Azure.Processing;
    using Dolittle.Runtime.Events.Processing.InMemory.Specs;
    using Dolittle.Runtime.Events.Azure.Specs;

    public class SUTProvider : IProvideTheOffsetRepository
    {
        public IEventProcessorOffsetRepository Build() => new test_event_processor_offset_repository(new an_azure_client());
    }
}