using System;
using Newtonsoft.Json;
using System.Linq;
namespace Dolittle.Runtime.Events.Azure.Store.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class OriginalContext
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.APPLICATION)]
        public Guid Application { get; set; }  
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.BOUNDED_CONTEXT)]
        public Guid BoundedContext { get; set; }  
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.TENANT)]
        public Guid Tenant { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.ENVIRONMENT)]
        public string Environment { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.CLAIMS)]
        public Claim[] Claims { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static OriginalContext From(Dolittle.Runtime.Events.OriginalContext source)
        {
            return new OriginalContext
            {
                Application = source.Application,
                BoundedContext = source.BoundedContext,
                Tenant = source.Tenant,
                Environment = source.Environment,
                Claims = source.Claims.Select(c => Claim.From(c)).ToArray()
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dolittle.Runtime.Events.OriginalContext ToOriginalContext()
        {
            return new Dolittle.Runtime.Events.OriginalContext(this.Application,this.BoundedContext,this.Tenant,this.Environment,ToClaims());
        }

        Dolittle.Security.Claims ToClaims()
        {
            return new Security.Claims(Claims.Select(c => c.ToClaim()).ToList());
        }
    }
}