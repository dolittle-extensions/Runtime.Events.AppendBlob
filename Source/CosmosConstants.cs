/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/
using Dolittle.Runtime.Events.Store;

namespace Dolittle.Runtime.Events.Azure
 {
     /// <summary>
     /// 
     /// </summary>
     public class CosmosConstants
     {
         
        /// <summary>
        /// 
        /// </summary>
        public const string COMMITS_COLLECTION = "commits";
        /// <summary>
        /// 
        /// </summary>
        public const string OFFSETS_COLLECTION = "offsets";
        /// <summary>
        /// 
        /// </summary>
        public const string EVENTS_BY_ARTIFACT_COLLECTION = "events";
        /// <summary>
        /// 
        /// </summary>
        public const string EVENTS_BY_EVENT_SOURCE_COLLECTION = "event_sources";
        /// <summary>
        /// 
        /// </summary>
        public const int NEVER_EXPIRE = -1;
        /// <summary>
        /// 
        /// </summary>
        public const string COMMIT_TRIGGER = "generateCommitSequenceNumber.js";
        /// <summary>
        /// 
        /// </summary>
        public const string COMMIT_STORED_PROCEDURE = "commit";
        /// <summary>
        /// 
        /// </summary>
         public const string PARTITON_KEY = "partitionKey";
        /// <summary>
        /// 
        /// </summary>
        public const string IS_METADATA = "isMetadata";

        /// <summary>
        /// We need the id in this format so that Cosmos recognises it is the primary key
        /// </summary>
        public const string ID = "id";
     }
     
     //The constants were copied from the MongoDB implementation.  Looks like we need to generalize them...
     
     
     /// <summary>
     /// Generic Constants for use within the <see cref="IEventStore" />
     /// </summary>
     public class Constants 
     {
#pragma warning disable 1591
        public const string EVENTSOURCE_ID = "eventsource_id";     
        public const string VERSION = "version"; 
        public const string MAJOR_VERSION = "major"; 
        public const string MINOR_VERSION = "minor"; 
        public const string REVISION = "revision"; 
        public const string GENERATION = "generation";
        public const string EVENT_SOURCE_ARTIFACT = "event_source_artifact";
        public const string ID = "_id";
        public const string CORRELATION_ID = "correlation_id";
        public const string ERROR = "err";
        public const string QUERY_EVENT_ARTIFACT = "events.event_artifact";
        public const string OFFSET = "offset";
#pragma warning restore 1591
     }
     
     /// <summary>
     /// Constants related to the Version of the Event
     /// </summary>
     public class VersionConstants
     {
#pragma warning disable 1591
        public const string COMMIT = "commit";
        public const string SEQUENCE = "sequence";
        public const string SNAPSHOT = "shapshot";
        public const string EVENT_COUNT = "total_events";
#pragma warning disable 1591
     } 

     
     /// <summary>
     /// Constants related to the Event Metadata
     /// </summary>
     public class EventConstants 
     {
#pragma warning disable 1591
        public const string ORIGINAL_CONTEXT = "original_context";
        public const string OCCURRED = "occurred";
        public const string EVENT = "event";
        public const string SHA = "SHA";
        public const string EVENT_ARTIFACT = "event_artifact";
        public const string APPLICATION = "application";
        public const string BOUNDED_CONTEXT = "bounded_context";
        public const string TENANT = "tenant";
        public const string ENVIRONMENT = "environment";
        public const string CLAIMS = "claims";
        public const string CLAIM_NAME = "value";
        public const string CLAIM_VALUE = "type";
        public const string CLAIM_VALUE_TYPE = "value_type";
#pragma warning disable 1591
     }
     
     /// <summary>
     /// Constants related to the Commit
     /// </summary>
     public class CommitConstants 
     {
#pragma warning disable 1591
        public const string COMMIT_ID = "commit_id";
        public const string TIMESTAMP = "timestamp";
        public const string VERSION = "version";
        public const string EVENTS = "events";
#pragma warning disable 1591
     }
 }