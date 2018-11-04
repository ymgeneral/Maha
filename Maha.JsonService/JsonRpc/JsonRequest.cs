using Newtonsoft.Json;
using System.Collections.Generic;

namespace Maha.JsonService
{
    /// <summary>
    /// Json请求信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRequest
    {
        public JsonRequest()
        {
            Tags = new Dictionary<string, object>();
        }

        #region Properties
        /// <summary>
        /// JsonRpc请求版本
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "jsonrpc")]
        public string JsonRpc { get { return "2.0"; } }

        /// <summary>
        /// 上下文中的方法名
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 上下文中的参数
        /// </summary>
        [JsonProperty("params")]
        public object Params { get; set; }

        /// <summary>
        /// 上下文中的Id
        /// </summary>
        [JsonProperty("id")]
        public object Id { get; set; }

        /// <summary>
        /// 上下文中定义的键、值
        /// </summary>
        [JsonProperty("tags")]
        public Dictionary<string, object> Tags { get; set; }
        #endregion
    }
}
