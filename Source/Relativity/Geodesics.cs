/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/

namespace Dolittle.Runtime.Events.Relativity.Azure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dolittle.Logging;
    using Dolittle.Runtime.Events.Azure;
    using Dolittle.Runtime.Events.Relativity;
    using Dolittle.Runtime.Events.Store;
    using Dolittle.Serialization.Json;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    /// <summary>
    /// An Azure (CosmosDB) implementation of <see cref="IGeodesics" />
    /// </summary>
    public class Geodesics : IGeodesics
    {
        private ILogger _logger;
        private EventStoreAzureDbConfiguration _config;

        /// <summary>
        /// Instantiates an instance of <see cref="IGeodesics" />
        /// </summary>
        /// <param name="config">The connection for the <see cref="EventStoreAzureDbConfiguration"/></param>
        /// <param name="logger">A logger instance</param>
        public Geodesics(EventStoreAzureDbConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <inheritdoc />
        public ulong GetOffset(EventHorizonKey eventHorizon)
        {
            var offset = GetOffsetDoc(eventHorizon);
            return offset?.Value ?? 0;
        }

        /// <inheritdoc />
        public void SetOffset(EventHorizonKey key, ulong offset)
        {
           SetAsync(key,offset).GetAwaiter().GetResult();
        }

        Offset GetOffsetDoc(EventHorizonKey key)
        {
            return _config.Client.CreateDocumentQuery<Offset>(_config.OffsetsUri, GetFeedOptions() )
                .Where(_ => _.Id == key.AsId()).Take(1).AsEnumerable().SingleOrDefault();
        }

        async Task SetAsync(EventHorizonKey eventHorizon, ulong offset)
        {
            var o = Offset.From(eventHorizon, offset, _config.BasePartitionKey);
            try
            {
                var result = await _config.Client.UpsertDocumentAsync(_config.OffsetsUri, o, new RequestOptions{ PartitionKey = new PartitionKey(_config.BasePartitionKey)});
                _logger.Debug(ResponseMetadata.FromOffset("Setting Geodesics Offset", result)?.ToString());
            }
            catch (DocumentClientException ex)
            {
                throw new EventStorePersistenceError("Error", ex);
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

        FeedOptions GetFeedOptions()
        {
            return new FeedOptions { PartitionKey = new PartitionKey(_config.BasePartitionKey), MaxItemCount = -1 };
        }
    }
}