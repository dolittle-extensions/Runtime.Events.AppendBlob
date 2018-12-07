using Newtonsoft.Json;

namespace Dolittle.Runtime.Events.Azure.Store.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Claim
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.CLAIM_NAME)]
        public string Name { get; set; }  
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.CLAIM_VALUE)]
        public string Value { get; set; }  
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(EventConstants.CLAIM_VALUE_TYPE)]
        public string ValueType { get; set; }   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public static Claim From(Dolittle.Security.Claim claim)
        {
            return new Claim { Name = claim.Name, Value = claim.Value, ValueType = claim.Value };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dolittle.Security.Claim ToClaim()
        {
            return new Dolittle.Security.Claim(this.Name, this.Value, this.ValueType);
        }      
    }
}