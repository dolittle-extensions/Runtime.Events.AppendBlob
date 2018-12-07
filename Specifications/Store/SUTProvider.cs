using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dolittle.DependencyInversion;
using Dolittle.Execution;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Specs;
using Dolittle.Runtime.Events.Azure.Store;
using Dolittle.Security;
using Dolittle.Serialization.Json;
using Dolittle.Types;
using Machine.Specifications;
using Moq;

namespace Dolittle.Runtime.Events.Azure.Specs.Store
{
    public class SUTProvider : IProvideTheEventStore
    {
        public IEventStore Build() => new test_azure_event_store(new an_azure_client());
    }
}