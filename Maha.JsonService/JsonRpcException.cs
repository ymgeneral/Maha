using Newtonsoft.Json;
using System;

namespace Maha.JsonService
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcException : ApplicationException
    {
        #region Properties
        /// <summary>
        /// rpc json请求信息
        /// </summary>
        [JsonProperty]
        public JsonRequest rpcRequest { get; set; }

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

        public JsonRpcException(int code, string message, object data, object request)
        {
            this.code = code;
            this.message = message;
            this.data = data;
            if (request != null)
            {
                var jsonRequest = request as JsonRequest;
                if (jsonRequest != null)
                    rpcRequest = jsonRequest;

                var requestRaw = request as string;
                if (requestRaw != null)
                    rpcRequestRaw = requestRaw;
            }
        }
    }
}
