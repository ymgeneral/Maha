using System.Collections.Generic;

namespace Maha.JsonClient
{
    /// <summary>
    /// Rpc请求选项
    /// </summary>
    public class RpcOption
    {
        /// <summary>
        /// 时间长度，单位毫秒，默认为100000毫秒(即100秒)
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 强制使用WriteDB;
        /// 设置为true时,JsonRpc服务端执行时将在读取数据时覆盖原先的ReadDB数据库连接，改使用对应的WriteDB数据库连接; 
        /// 设置为false时,将不做额外的处理，服务端配置使用ReadDB还是WriteDB自定
        /// </summary>
        public bool ForceWriteDB { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string ServiceAddress { get; set; }

        /// <summary>
        /// 上下文值集
        /// </summary>
        public Dictionary<string, object> ContextValues { get; set; }
    }
}
