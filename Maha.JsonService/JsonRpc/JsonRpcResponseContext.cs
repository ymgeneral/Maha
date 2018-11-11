using Newtonsoft.Json;

namespace Maha.JsonService
{
    /// <summary>
    /// JsonRpc返回上下文
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcResponseContext
    {
        /// <summary>
        /// Rpc版本
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "jsonrpc")]
        public string JsonRpc { get { return "2.0"; } }

        /// <summary>
        /// 返回结果
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "result")]
        public object Result { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "error")]
        public JsonRpcException Error { get; set; }

        /// <summary>
        /// 上下文Id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public object Id { get; set; }
    }

    /// <summary>
    /// JsonRpc返回上下文(泛型)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcResponseContext<T>
    {
        /// <summary>
        /// Rpc版本
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "jsonrpc")]
        public string JsonRpc { get { return "2.0"; } }

        /// <summary>
        /// 返回结果(T)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "result")]
        public T Result { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "error")]
        public JsonRpcException Error { get; set; }

        /// <summary>
        /// 上下文Id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public object Id { get; set; }
    }
}
