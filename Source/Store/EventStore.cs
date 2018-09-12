using Dolittle.Artifacts;
using Dolittle.Logging;
using Dolittle.Runtime.Events;
using Dolittle.Runtime.Events.Store;
using Dolittle.Serialization.Json;

namespace Dolittle.Runtime.Events.Azure.Storage.Store
{
    /// <summary>
    /// Azure Storage implementation of <see cref="IEventStore"/>
    /// </summary>
    public class EventStore : IEventStore
    {
        static ISerializationOptions _serializationOptions = SerializationOptions.Custom(
            callback: (serializer) => 
            {
                serializer.ContractResolver = new CamelCaseExceptDictionaryKeyResolver();
            }
        );
        object _lock = new object();

        readonly EventStoreConfig _config;
        readonly ILogger _logger;
        readonly ISerializer _serializer;

        /// <summary>
        /// Instantiates an instance of <see cref="EventStore"/>
        /// </summary>
        /// <param name="config"><see cref="EventStoreConfig">The Event Store Configuration</see></param>
        /// <param name="logger"></param>
        /// <param name="serializer"></param>
        public EventStore(EventStoreConfig config, ILogger logger, ISerializer serializer)
        {
            _config = config;
            _logger = logger;
            _serializer = serializer;

        }
        /// <inheritdoc />
        public CommittedEventStream Commit(UncommittedEventStream uncommittedEvents)
        {
            throw new System.NotImplementedException();
        }
        /// <inheritdoc />
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
        /// <inheritdoc />
        public Commits Fetch(EventSourceId eventSourceId)
        {
            throw new System.NotImplementedException();
        }
        /// <inheritdoc />
        public Commits FetchAllCommitsAfter(CommitSequenceNumber commit)
        {
            throw new System.NotImplementedException();
        }
        /// <inheritdoc />
        public SingleEventTypeEventStream FetchAllEventsOfType(ArtifactId eventType)
        {
            throw new System.NotImplementedException();
        }
        /// <inheritdoc />
        public SingleEventTypeEventStream FetchAllEventsOfTypeAfter(ArtifactId eventType, CommitSequenceNumber commit)
        {
            throw new System.NotImplementedException();
        }
        /// <inheritdoc />
        public Commits FetchFrom(EventSourceId eventSourceId, CommitVersion commitVersion)
        {
            throw new System.NotImplementedException();
        }
        /// <inheritdoc />
        public EventSourceVersion GetCurrentVersionFor(EventSourceId eventSource)
        {
            throw new System.NotImplementedException();
        }
        /// <inheritdoc />
        public EventSourceVersion GetNextVersionFor(EventSourceId eventSource)
        {
            throw new System.NotImplementedException();
        }
    }
}