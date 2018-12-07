using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dolittle.PropertyBags;
using Dolittle.Serialization.Json;
using Dolittle.Artifacts;
using Dolittle.Collections;

namespace Dolittle.Runtime.Events.Azure.Store.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Event
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.ID)]
        public Guid Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.CORRELATION_ID)]
        public Guid CorrelationId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.EVENT_ARTIFACT)]
        public Guid EventArtifact { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Constants.GENERATION)]
        public int Generation { get; set; }
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
        public ulong Commit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(VersionConstants.SEQUENCE)]
        public uint Sequence { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.OCCURRED)]
        public long Occurred { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.ORIGINAL_CONTEXT)]
        public OriginalContext OriginalContext { get; set; }  
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.EVENT)]
        public dynamic EventData { get; set; }  

        /// <summary>
        ///
        /// </summary>
        /// <returns>An <see cref="EventEnvelope" /> instance corresponding to the <see cref="Event" /> representation</returns>
        public EventEnvelope ToEventEnvelope(ISerializer serializer)
        {
           return new EventEnvelope(ToEventMetadata(),ToPropertyBag(serializer));
        }

        EventMetadata ToEventMetadata()
        {
            return new EventMetadata(this.Id,ToVersionedEventSource(),this.CorrelationId,new Artifact(this.EventArtifact,this.Generation),DateTimeOffset.FromUnixTimeMilliseconds(Occurred), this.OriginalContext.ToOriginalContext());
        }

        PropertyBag ToPropertyBag(ISerializer serializer)
        {
            return PropertyBagSerializer.From(EventData, serializer);
        }

        VersionedEventSource ToVersionedEventSource()
        {
            return new VersionedEventSource(new EventSourceVersion(this.Commit,this.Sequence), new EventSourceKey(this.EventSourceId, this.EventSourceArtifact));
        }
    }
}