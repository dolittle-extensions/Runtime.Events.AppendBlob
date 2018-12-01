/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

using Dolittle.Concepts;
using Dolittle.Runtime.Events.Azure;
using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Relativity;
using Dolittle.Runtime.Events.Store;
using Newtonsoft.Json;

namespace Dolittle.Runtime.Events.Relativity.Azure
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
        [JsonProperty("offset")]
        public ulong Value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [JsonProperty(CosmosConstants.PARTITON_KEY)]
        public string PartitionKey { get; set;}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public static Offset From(EventHorizonKey key, ulong offset, string partitionKey)
        {
            return new Offset { Id = key.AsId(), Value = offset, PartitionKey = partitionKey  };
        }   
    }
}