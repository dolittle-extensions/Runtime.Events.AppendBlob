/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

using Dolittle.Concepts;
using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Store;
using Newtonsoft.Json;

namespace Dolittle.Runtime.Events.Azure.Processing
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Offset : Value<Offset>
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(CosmosConstants.ID)]
        public string Id { get; set; }  

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [JsonProperty("major")]
        public ulong Major { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [JsonProperty("minor")]
        public ulong Minor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [JsonProperty("revision")]
        public uint Revision { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [JsonProperty(CosmosConstants.PARTITON_KEY)]
        public string PartitionKey { get; set;}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventProcessorId"></param>
        /// <param name="committedEventVersion"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public static Offset From(EventProcessorId eventProcessorId, CommittedEventVersion committedEventVersion, string partitionKey)
        {
            return new Offset { Id = eventProcessorId.ToString(), Major = committedEventVersion.Major, Minor = committedEventVersion.Minor, 
                                    Revision = committedEventVersion.Revision, PartitionKey = partitionKey  };
        }   

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CommittedEventVersion ToCommittedEventVersion() 
        {
            return new CommittedEventVersion(Major,Minor,Revision);
        }
    }
}