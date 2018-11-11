using System;
using System.Threading.Tasks;

namespace Maha.JsonService
{
    /// <summary>
    /// JsonRpc 请求处理器
    /// </summary>
    public class JsonRpcProcessor
    {
        static JsonRpcProcessor()
        {
            //默认加载所有Handler
            JsonRpcRegister.LoadFromConfig();
        }

        /// <summary>
        /// 异步处理请求
        /// </summary>
        /// <param name="jsonRpc"></param>
        /// <param name="callback"></param>
        /// <param name="context"></param>
        public static void AsyncProcess(string jsonRpc, Action<string> callback, object context = null)
        {
            Task.Factory.StartNew(() => { });
        }

        /// <summary>
        /// 基于sessionId异步处理请求
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="jsonRpc"></param>
        /// <param name="callback"></param>
        /// <param name="context"></param>
        public static void AsyncProcess(string sessionId, string jsonRpc, Action<string> callback, object context = null)
        {
            Task.Factory.StartNew(() => { });
        }

        public static void Process(JsonRpcStateAsync stateAsync, object context = null)
        {
            Task.Factory.StartNew((async) =>
            {
                var args = (Tuple<JsonRpcStateAsync, object>)async;
            },
            new Tuple<JsonRpcStateAsync, object>(stateAsync, context));
        }
    }
}
