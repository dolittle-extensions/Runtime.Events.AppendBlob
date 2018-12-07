using System;
using System.Linq;
using Dolittle.Runtime.Events.Store;
using Dolittle.Serialization.Json;
using Newtonsoft.Json;

namespace Dolittle.Runtime.Events.Azure.Store.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Commit
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(CosmosConstants.ID)]
        public string Id { get; set; }  
        /// <summary>
        /// We need a duplicate of the Primary Key (Id) as you cannot currently order by the Primary Key.false  You need a separate index on a duplicate property.
        /// </summary>
        [JsonProperty(Constants.ID)]
        public ulong Id2 { get; set; }  
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.CORRELATION_ID)]
        public Guid CorrelationId { get; set; }  
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(CommitConstants.COMMIT_ID)]
        public Guid CommitId { get; set; }     
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(CommitConstants.TIMESTAMP)]
        public long Timestamp { get; set; }    
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.EVENTSOURCE_ID)]
        public Guid EventSourceId { get; set; }    
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.EVENT_SOURCE_ARTIFACT)]
        public Guid EventSourceArtifact { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(VersionConstants.COMMIT)]
        public ulong CommitNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(VersionConstants.SEQUENCE)]
        public uint Sequence { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(CommitConstants.EVENTS)]
        public Event[] Events { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [JsonProperty(CosmosConstants.PARTITON_KEY)]
        public string PartitionKey { get; set;}
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [JsonProperty(CosmosConstants.IS_METADATA)]
        public bool isMetadata => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <param name="jsonSerializer"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public static Commit From(UncommittedEventStream events, ISerializer jsonSerializer, string partitionKey)
        {
            var eventDocs = events.Events.Select(e => new Event
            {
                Id = e.Id,
                CorrelationId = e.Metadata.CorrelationId,
                EventArtifact = e.Metadata.Artifact.Id,
                EventSourceId = e.Metadata.EventSourceId,
                Generation = e.Metadata.Artifact.Generation,
                EventSourceArtifact = events.Source.Artifact,
                Commit = e.Metadata.VersionedEventSource.Version.Commit,
                Sequence = e.Metadata.VersionedEventSource.Version.Sequence,
                Occurred =  e.Metadata.Occurred.ToUnixTimeMilliseconds(),
                OriginalContext = OriginalContext.From(e.Metadata.OriginalContext),
                EventData = PropertyBagSerializer.Serialize(e.Event,jsonSerializer) 
            });

            var commit = new Commit
            {
                Id = "0",
                CorrelationId = events.CorrelationId,
                CommitId =  events.Id,
                Timestamp = events.Timestamp.ToUnixTimeMilliseconds(),
                EventSourceId = events.Source.EventSource,
                EventSourceArtifact = events.Source.Artifact,
                CommitNumber = events.Source.Version.Commit,
                Sequence = events.Source.Version.Sequence,
                Events = eventDocs.ToArray(),
                PartitionKey = partitionKey
            };

            return commit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public  Dolittle.Runtime.Events.Store.CommittedEventStream ToCommittedEventStream(ISerializer serializer)
        {
            return new CommittedEventStream(
                Id2,
                new VersionedEventSource(new EventSourceVersion(this.CommitNumber,this.Sequence),new EventSourceKey(EventSourceId,EventSourceArtifact)),
                CommitId,
                CorrelationId,
                DateTimeOffset.FromUnixTimeMilliseconds(Timestamp),
                new EventStream(Events.Select(e => e.ToEventEnvelope(serializer)))
             );
        } 
    }   
}