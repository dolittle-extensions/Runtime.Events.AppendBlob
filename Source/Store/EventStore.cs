using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dolittle.Artifacts;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Azure.Store.Persistence;
using Dolittle.Runtime.Events.Store;
using Dolittle.Serialization.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dolittle.Runtime.Events.Azure.Store
{
    /// <summary>
    /// 
    /// </summary>
    public class EventStore : IEventStore
    {   
        private readonly DocumentClient _client;
        private readonly EventStoreAzureDbConfiguration _config;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        private const string CONCURRENCY_CONFLICT_DUPLICATE_KEY = "Exception = Duplicate";
        private const string CONCURRENCY_CONFLICT_PREVIOUS_VERSION_KEY = "Exception = Stale";

        /// <summary>
        /// git submodule update --init --recursive
        /// </summary>
        /// <param name="config"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public EventStore(EventStoreAzureDbConfiguration config, ISerializer serializer, ILogger logger)
        {
            _client = new DocumentClient(new Uri(config.EndpointUrl), config.AuthorizationKey);;
            _config = config;
            _serializer = serializer;
            _logger = logger;
        }

        /// <inheritdoc />
        public CommittedEventStream Commit(UncommittedEventStream uncommittedEvents)
        {
            return CommitAsync(uncommittedEvents).GetAwaiter().GetResult();
        }

        async Task<CommittedEventStream> CommitAsync(UncommittedEventStream uncommittedEvents)
        {
            var commit = Persistence.Commit.From(uncommittedEvents, _serializer, _config.BasePartitionKey);
            try
            {
                var result = await _config.Client.ExecuteStoredProcedureAsync<dynamic>(
                    _config.CommitStoredProcedure,
                    new RequestOptions 
                    { 
                        PartitionKey = new PartitionKey(_config.BasePartitionKey), 
                        ConsistencyLevel = ConsistencyLevel.Session, 
                        //EnableScriptLogging = true
                    },
                    commit);
                //_logger.Debug(result.ScriptLog);
                ulong commitSequenceNumber = Convert.ToUInt64(result.Response);
                
                //_logger.Debug(ResponseMetadata.FromCommit("Commit", result)?.ToString());
                return uncommittedEvents.ToCommitted(commitSequenceNumber);
            }
            catch (DocumentClientException ex)
            {
                //_logger.Debug(ex.ScriptLog);
                if (ex.Message.Contains(CONCURRENCY_CONFLICT_DUPLICATE_KEY))
                {
                    throw new CommitIsADuplicate();
                }
                if(ex.Message.Contains(CONCURRENCY_CONFLICT_PREVIOUS_VERSION_KEY))
                {
                    throw new EventSourceConcurrencyConflict(ex.Error.Message, ex);
                }

                throw new EventStorePersistenceError("Unknown error", ex);
            }
        }

        #region IDisposable Support
        /// <summary>
        /// Disposed flag to detect redundant calls
        /// </summary>
        protected bool disposedValue = false; 

        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EventStore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Disposes of the EventStore
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        /// <inheritdoc />
        public Commits Fetch(EventSourceKey eventSource)
        {
            var commitsQuery = GetBasicCommitQuery()
                .Where(_ => _.EventSourceId == eventSource.Id.Value && _.EventSourceArtifact == eventSource.Artifact.Value)
                .OrderBy(_ => _.Id2)
                .AsDocumentQuery();

            return GetCommits(commitsQuery,$"Fetching {eventSource.Id.Value } of artitfact {eventSource.Artifact.Value}");
        }

        /// <inheritdoc />
        public Commits FetchAllCommitsAfter(CommitSequenceNumber commit)
        {
            return GetCommits(QueryCommitsFor(_ => _.Id2 > commit.Value),$"Fetching commits after {commit.Value}");
        }

        /// <inheritdoc />
        public SingleEventTypeEventStream FetchAllEventsOfType(ArtifactId eventType)
        {
            var sqlQuery = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.events, { \"event_artifact\": \"" + eventType.Value + "\"}, true)";
            var query =  QueryCommitsFor(sqlQuery);
            var commits = GetCommits(query,$"Fetching events of type: {eventType}");
            return GetEventsFromCommits(commits,eventType);
        }

        /// <inheritdoc />
        public SingleEventTypeEventStream FetchAllEventsOfTypeAfter(ArtifactId eventType, CommitSequenceNumber commit)
        {
            var sqlQuery = $"SELECT * FROM c WHERE ARRAY_CONTAINS(c.events, {{ \"event_artifact\": \"{ eventType.Value }\" }}, true) AND c.commit > {commit.Value}";
            var query =  QueryCommitsFor(sqlQuery);
            var commits = GetCommits(query,$"Fetching events of type: {eventType} after { commit.Value }");
            return GetEventsFromCommits(commits,eventType);
        }

        /// <inheritdoc />
        public Commits FetchFrom(EventSourceKey eventSource, CommitVersion commitVersion)
        {
            return GetCommits(QueryCommitsFor(_ => _.EventSourceId == eventSource.Id.Value && _.EventSourceArtifact == eventSource.Artifact.Value && _.CommitNumber >= commitVersion.Value),
                                $"Fetching commits on for event source { eventSource.Id.Value }  of artifact:{ eventSource.Artifact.Value} after {commitVersion.ToString()}");
        }

        /// <inheritdoc />
        public EventSourceVersion GetCurrentVersionFor(EventSourceKey eventSource)
        {
            var queryString = $"SELECT TOP 1 c.commit, c.sequence FROM c where c.eventsource_id='{eventSource.Id.Value}' and c.event_source_artifact = '{eventSource.Artifact.Value}' order by c.commit desc";
            var result = _config.Client.CreateDocumentQuery<dynamic>(_config.CommitsUri, queryString, GetCommitFeedOptions()).ToList().FirstOrDefault();
            if(result != null)
            {
                return new EventSourceVersion((ulong)result.commit,(uint)result.sequence);
            } 
            else 
            {
                return EventSourceVersion.NoVersion;
            }
        }

        /// <inheritdoc />
        public EventSourceVersion GetNextVersionFor(EventSourceKey eventSource)
        {
            return GetCurrentVersionFor(eventSource).NextCommit();
        }

        Commits GetCommits(IDocumentQuery<Commit> commitsQuery, string responseLabel)
        {
            var commits = new List<CommittedEventStream>();
            while (commitsQuery.HasMoreResults)
            {
                var result = commitsQuery.ExecuteNextAsync<Commit>().GetAwaiter().GetResult();
                //_logger.Debug(ResponseMetadata.FromQuery(responseLabel, result)?.ToString());

                foreach (var commit in result)
                {
                    commits.Add(commit.ToCommittedEventStream(_serializer));
                }
            }
            return new Commits(commits);
        }
        IDocumentQuery<Commit> QueryCommitsFor(Expression<Func<Commit, bool>> predicate)
        {
            return GetBasicCommitQuery()
                    .Where(predicate)
                    .OrderBy(_ => _.Id2)
                    .AsDocumentQuery();
        }

        IDocumentQuery<Commit> QueryCommitsFor(string queryString)
        {
            return _config.Client.CreateDocumentQuery<Commit>(_config.CommitsUri, queryString, GetCommitFeedOptions()).AsDocumentQuery();
        }

        IOrderedQueryable<Commit> GetBasicCommitQuery()
        {
            return _config.Client.CreateDocumentQuery<Commit>(_config.CommitsUri, GetCommitFeedOptions() );
        }

        FeedOptions GetCommitFeedOptions()
        {
            return new FeedOptions { PartitionKey = new PartitionKey(_config.BasePartitionKey), MaxItemCount = -1 };
        }

        SingleEventTypeEventStream GetEventsFromCommits(IEnumerable<CommittedEventStream> commits, ArtifactId eventType)
        {
            var events = new List<CommittedEventEnvelope>();
            foreach(var commit in commits)
            {
                events.AddRange(commit.Events.FilteredByEventType(eventType).Select(e => new CommittedEventEnvelope(commit.Sequence,e.Metadata,e.Event)));
            }
            return new SingleEventTypeEventStream(events);
        }
    }
}