using Newtonsoft.Json;
using System.Collections.Generic;

namespace Maha.JsonClient
{
    /// <summary>
    /// Rpc客户端请求上下文
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcRequestContext
    {
        /// <summary>
        /// 请求上下文Rpc版本
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "jsonrpc")]
        public string JsonRpc { get { return "2.0"; } }

        /// <summary>
        /// 请求上下文方法
        /// </summary>
        [JsonProperty("method")]
        public string Method { get; set; }

        /// <summary>
        /// 请求上下文参数
        /// </summary>
        [JsonProperty("params")]
        public object Params { get; set; }

        /// <summary>
        /// 请求上下文Id
        /// </summary>
        [JsonProperty("id")]
        public object Id { get; set; }

        /// <summary>
        /// 请求上下文键、值
        /// </summary>
        [JsonProperty("tags")]
        public Dictionary<string, object> Tags { get; set; }

        /// <summary>
        /// 请求上下文构造器
        /// </summary>
        public JsonRpcRequestContext()
        {
            Tags = new Dictionary<string, object>();
        }
    }
}
