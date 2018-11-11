using Maha.JsonClient.Invoker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Maha.JsonClient
{
    /// <summary>
    /// 注入Rpc上下文的委托
    /// </summary>
    /// <param name="collection"></param>
    public delegate void GlobalContextHandler(IDictionary<string, object> collection);

    /// <summary>
    /// Rpc客户端请求
    /// </summary>
    public static class JsonRpcRequest
    {
        /// <summary>
        /// 注入上下文委托
        /// </summary>
        internal static GlobalContextHandler Handler = null;

        /// <summary>
        /// 设置上下文触发事件，当调用JSON-RPC服务之前触发，从而注入上下文键值。
        /// </summary>
        /// <param name="contextHandler"></param>
        public static void SetGlobalContextHandler(GlobalContextHandler contextHandler)
        {
            Handler += contextHandler;
        }

        /// <summary>
        /// 调用JSON-RPC服务(同步)
        /// </summary>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为可变数组</param>
        /// <remarks>服务地址需要配置在Web.config中的Setting下的JsonRpcServiceUrl Key的值</remarks>
        public static void Call(string method, params object[] args)
        {
            Call<object>(method, args);
        }

        /// <summary>
        /// 调用JSON-RPC服务(异步)
        /// </summary>
        /// <param name="successedAction">执行成功后调用的函数</param>
        /// <param name="failedAction">执行失败后调用的函数</param>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为可变数组</param>
        public static void CallAsync(Action successedAction, Action<Exception> failedAction, string method, params object[] args)
        {
            Action<object> successedActionEx = null;
            if (successedAction != null)
            {
                successedActionEx = (item) => successedAction();
            }
            CallAsync<object>(successedActionEx, failedAction, method, args);
        }

        /// <summary>
        /// 调用JSON-RPC服务(同步),泛型并返回值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为可变数组</param>
        /// <returns>T</returns>
        public static T Call<T>(string method, params object[] args)
        {
            var rpcInvoker = GetRpcInvoker(method, null);
            var responseContext = rpcInvoker.Invoke<T>(method, args);

            if (responseContext.Error != null)
            {
                throw responseContext.Error;
            }
            return responseContext.Result;
        }

        /// <summary>
        /// 调用JSON-RPC服务(异步),泛型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="successedAction">执行成功后调用的函数</param>
        /// <param name="failedAction">执行失败后调用的函数</param>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为可变数组</param>
        public static void CallAsync<T>(Action<T> successedAction, Action<Exception> failedAction, string method, params object[] args)
        {
            Task.Factory.StartNew(() =>
            {
                var asyncState = new CallAsyncResult<T>()
                {
                    Method = method,
                    Args = args,
                    SuccessedCallBack = successedAction,
                    FailedCallBack = failedAction
                };
                ProcessCallAsync<T>(asyncState);
            });
        }

        /// <summary>
        /// 调用JSON-RPC服务(同步)
        /// </summary>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="option">自定义选项</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为可变数组</param>
        public static void CallWithOption(string method, RpcOption option, params object[] args)
        {
            CallWithOption<object>(method, option, args);
        }

        /// <summary>
        /// 调用JSON-RPC服务(同步),泛型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="option">自定义选项</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为可变数组</param>
        /// <returns></returns>
        public static T CallWithOption<T>(string method, RpcOption option, params object[] args)
        {
            var rpcInvoker = GetRpcInvoker(method, option);
            var responseContext = rpcInvoker.Invoke<T>(method, args);

            if (responseContext.Error != null)
            {
                throw responseContext.Error;
            }
            return responseContext.Result;
        }

        /// <summary>
        /// 调用JSON-RPC服务(同步)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">JSON-RPC服务地址</param>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为可变数组</param>
        /// <returns></returns>
        public static T CallUrl<T>(string url, string method, params object[] args)
        {
            var rpcInvoker = GetRpcInvoker(method, new RpcOption() { ServiceAddress = url });
            var responseContext = rpcInvoker.Invoke<T>(method, args);

            if (responseContext.Error != null)
            {
                throw responseContext.Error;
            }
            return responseContext.Result;
        }

        /// <summary>
        /// 调用JSON-RPC服务(同步)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为将参数数组对象化的写法，如new { param1 = "A", param2 = "B"  }</param>
        /// <returns></returns>
        public static T CallWithDeclaredParams<T>(string method, object args)
        {
            var rpcInvoker = GetRpcInvoker(method, null);
            var responseContext = rpcInvoker.InvokeWithDeclaredParams<T>(method, args);

            if (responseContext.Error != null)
            {
                throw responseContext.Error;
            }
            return responseContext.Result;
        }

        /// <summary>
        /// 调用JSON-RPC服务(异步)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="successedAction">执行成功后调用的函数</param>
        /// <param name="failedAction">执行失败后调用的函数</param>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为将参数数组对象化的写法，如new { param1 = "A", param2 = "B"  }</param>
        public static void CallWithDeclaredParamsAsync<T>(Action<T> successedAction, Action<Exception> failedAction, string method, object args)
        {
            Task.Factory.StartNew(() =>
            {
                var asyncState = new CallAsyncResult<T>()
                {
                    IsDeclaredParams = true,
                    Method = method,
                    DeclaredArgs = args,
                    SuccessedCallBack = successedAction,
                    FailedCallBack = failedAction
                };
                ProcessCallAsync<T>(asyncState);
            });
        }

        /// <summary>
        /// 批量调用JSON-RPC服务
        /// </summary>
        /// <param name="jsonRpcRequests">批量JSON-RPC请求列表</param>
        /// <param name="option">参数列表</param>
        /// <returns></returns>
        public static List<JsonRpcResponseContext> BatchCall(List<JsonRpcRequestContext> jsonRpcRequests, RpcOption option = null)
        {
            if (jsonRpcRequests == null || jsonRpcRequests.Count == 0)
                throw new ArgumentNullException("jsonRpcRequests", "jsonRpcRequests is null or empty");

            var rpcInvoker = GetRpcInvoker(jsonRpcRequests[0].Method, option);
            return rpcInvoker.BatchInvoke(jsonRpcRequests);
        }

        #region Private Methods
        /// <summary>
        /// 处理异步调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        private static void ProcessCallAsync<T>(object result)
        {
            Task.Factory.StartNew(() =>
            {
                if (result is CallAsyncResult<T> ar)
                {
                    try
                    {
                        var rpcInvoker = GetRpcInvoker(ar.Method, null);
                        JsonRpcResponseContext<T> requestContext = null;
                        if (ar.IsDeclaredParams)
                        {
                            requestContext = rpcInvoker.InvokeWithDeclaredParams<T>(ar.Method, ar.DeclaredArgs);
                        }
                        else
                        {
                            requestContext = rpcInvoker.Invoke<T>(ar.Method, ar.Args);
                        }
                        ar.Error = requestContext?.Error;
                        ar.Result = requestContext.Result;
                    }
                    catch (Exception ex)
                    {
                        ar.Error = ex;
                    }
                    ar.SetCompleted();
                }
            });
        }

        /// <summary>
        /// 获取Rpc调用器
        /// </summary>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="option">自定义选项</param>
        private static RpcInvoker GetRpcInvoker(string method, RpcOption option)
        {
            string serviceAddress = (option != null && !string.IsNullOrWhiteSpace(option.ServiceAddress)) ? option.ServiceAddress : Configmanager.GetServiceUrlFromConfig
                (method);

            RpcInvoker invoker = null;
            if (string.Equals(serviceAddress, "local", StringComparison.InvariantCultureIgnoreCase))
            {
                invoker = new LocalRpcInvoker();
            }
            else if (serviceAddress.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                || serviceAddress.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                invoker = new HttpRpcInvoker();
            }
            else
            {
                throw new NotSupportedException("不支持的Rpc服务地址类型:" + serviceAddress);
            }

            invoker.ServiceAddress = serviceAddress;
            invoker.Option = option;
            return invoker;
        }
        #endregion
    }
}
