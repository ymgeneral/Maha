using System;
using System.Collections.Generic;

namespace Maha.JsonClient
{
    /// <summary>
    /// 注入Rpc上下文的委托
    /// </summary>
    /// <param name="collection"></param>
    public delegate void GlobalContextHandler(IDictionary<string, object> collection);

    /// <summary>
    /// JsonRpc请求
    /// </summary>
    public class JsonRpcRequest
    {
        internal static GlobalContextHandler handler = null;

        /// <summary>
        /// 设置上下文触发事件，当调用JSON-RPC服务之前触发，从而注入上下文键值。
        /// </summary>
        /// <param name="contextHandler"></param>
        public static void SetGlobalContextHandler(GlobalContextHandler contextHandler)
        {
            handler = contextHandler;
        }
    }
}
