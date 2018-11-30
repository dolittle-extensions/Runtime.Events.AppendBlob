using System.Collections.Specialized;
using Microsoft.Azure.Documents.Client;
using Dolittle.Concepts;
using Dolittle.Runtime.Events.Azure.Store.Persistence;
using Microsoft.Azure.Documents;

namespace Dolittle.Runtime.Events.Azure
{
    /// <summary>
    /// 
    /// </summary>
    public class ResponseMetadata : Value<ResponseMetadata>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string RequestIdentifier { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string CurrentResourceQuotaUsage { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string MaxResourceQuota { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double RequestCharge { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestIdentifier"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ResponseMetadata FromCommit(string requestIdentifier, IStoredProcedureResponse<dynamic> response)
        {
            return new ResponseMetadata
            {
                RequestIdentifier = requestIdentifier,
                CurrentResourceQuotaUsage = response.CurrentResourceQuotaUsage,
                MaxResourceQuota = response.MaxResourceQuota,
                RequestCharge = response.RequestCharge
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestIdentifier"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ResponseMetadata FromQuery(string requestIdentifier, IResourceResponse<Document> response)
        {
            return new ResponseMetadata
            {
                RequestIdentifier = requestIdentifier,
                CurrentResourceQuotaUsage = response.CurrentResourceQuotaUsage,
                MaxResourceQuota = response.MaxResourceQuota,
                RequestCharge = response.RequestCharge
            };
        }   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestIdentifier"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ResponseMetadata FromOffset(string requestIdentifier, IResourceResponse<Document> response)
        {
            return new ResponseMetadata
            {
                RequestIdentifier = requestIdentifier,
                CurrentResourceQuotaUsage = response.CurrentResourceQuotaUsage,
                MaxResourceQuota = response.MaxResourceQuota,
                RequestCharge = response.RequestCharge
            };
        }       
    }
}