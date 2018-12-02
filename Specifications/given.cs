using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dolittle.DependencyInversion;
using Dolittle.Execution;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Azure;
using Dolittle.Security;
using Dolittle.Serialization.Json;
using Dolittle.Types;
using Moq;

namespace Dolittle.Runtime.Events.Azure.Specs
{
    public class given
    {
        const string URL = "EVENTSTORE_AZURE_URL";
        const string DATABASE = "EVENTSTORE_AZURE_DATABASE";
        const string AUTHKEY = "EVENTSTORE_AZURE_AUTHKEY";
        static readonly Guid _application = Guid.Parse("3f74981c-d3e9-4819-ab8d-d659300530b0");
        static readonly Guid _bounded_context = Guid.Parse("f2549583-27e8-4dda-9605-c7d876e0f6f0");
        static readonly Guid _tenant = Guid.Parse("eb963893-0dfe-47be-9d1b-79b02e0f5d9d");
        public static ISerializer GetSerializer()
        {
            var container_mock = new Mock<IContainer>();
            var converter_providers = new List<ICanProvideConverters>();
                                    
            var converter_provider_instances = new Mock<IInstancesOf<ICanProvideConverters>>();
            converter_provider_instances.Setup(c => c.GetEnumerator()).Returns(() => converter_providers.GetEnumerator());
            return new Serializer(container_mock.Object, converter_provider_instances.Object);
        }
        
        public static IExecutionContextManager GetExecutionContext()
        {
            var executionContext = new ExecutionContext(_application,_bounded_context,_tenant,"unit tests",Guid.NewGuid(),new Claims(Enumerable.Empty<Claim>()), CultureInfo.CurrentCulture ){};
            var executionContextMock = new Mock<IExecutionContextManager>();
            executionContextMock.SetupGet(_ => _.Current).Returns(executionContext);
            return executionContextMock.Object;
        }

        public static ILogger GetLogger()
        {
            var logger_mock = new Mock<ILogger>();
            logger_mock.Setup(l => l.Error(Moq.It.IsAny<Exception>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(),Moq.It.IsAny<string>()))
                .Callback<Exception,string,string,int,string>((ex,msg,fp,ln,m) => Console.WriteLine(ex.ToString()));
            logger_mock.Setup(l => l.Debug(Moq.It.IsAny<string>(),Moq.It.IsAny<string>(),Moq.It.IsAny<int>(),Moq.It.IsAny<string>()))
                .Callback<string,string,int,string>((msg,fp,ln,m) => Console.WriteLine(msg));
            return logger_mock.Object;
        }

        public static EventStoreAzureDbConfiguration GetEventStoreConfig()
        {
            var config = new EventStoreConfiguration
            {
                EndPointUrl = System.Environment.GetEnvironmentVariable(URL),
                DatabaseId = System.Environment.GetEnvironmentVariable(DATABASE),
                AuthKey = System.Environment.GetEnvironmentVariable(AUTHKEY)
            };
            return new EventStoreAzureDbConfiguration(config,GetLogger(), GetExecutionContext());
        }
    }
}