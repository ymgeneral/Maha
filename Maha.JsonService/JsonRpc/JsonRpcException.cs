using Newtonsoft.Json;
using System;

namespace Maha.JsonService
{
    /// <summary>
    /// JsonRpc异常
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcException : ApplicationException
    {
        #region Properties
        /// <summary>
        /// rpc json请求信息
        /// </summary>
        [JsonProperty]
        public JsonRpcRequestContext rpcRequest { get; set; }

        /// <summary>
        /// rpc 原始字符串
        /// </summary>
        [JsonProperty]
        public string rpcRequestRaw { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        [JsonProperty]
        public int code { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        [JsonProperty]
        public string message { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        [JsonProperty]
        public object data { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public override string Message
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(message))
                    return message;
                else
                    return base.Message;
            }
        }
        #endregion

        /// <summary>
        /// JsonRpc异常构造器
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <param name="request"></param>
        public JsonRpcException(int code, string message, object data, object request)
        {
            this.code = code;
            this.message = message;
            this.data = data;
            if (request != null)
            {
                var jsonRequest = request as JsonRpcRequestContext;
                if (jsonRequest != null)
                    rpcRequest = jsonRequest;

                var requestRaw = request as string;
                if (requestRaw != null)
                    rpcRequestRaw = requestRaw;
            }
        }
    }
}
