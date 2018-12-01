using Maha.JsonService.Gateway;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Maha.JsonService
{
    /// <summary>
    /// JsonRpc 请求处理器
    /// </summary>
    public class JsonRpcProcessor
    {
        /// <summary>
        /// 静态构建器，默认加载所有Handler
        /// </summary>
        static JsonRpcProcessor()
        {
            JsonRpcRegister.LoadFromConfig();
        }

        /// <summary>
        /// 执行调用，回调（不设置异步状态）
        /// </summary>
        /// <param name="jsonRpc">请求内容</param>
        /// <param name="callback">回调方法</param>
        /// <param name="context">请求上下文</param>
        public static void ProcessAsync(string jsonRpc, Action<string> callback, object context = null)
        {
            Task.Factory.StartNew(() => ExecuteAsync(Handler.DefaultSessionId, jsonRpc, context, callback));
        }
        /// <summary>
        /// 执行调用，回调（不设置异步状态）
        /// </summary>
        /// <param name="sessionId">sessionId</param>
        /// <param name="jsonRpc">请求内容</param>
        /// <param name="callback">回调方法</param>
        /// <param name="context">请求上下文</param>
        public static void ProcessAsync(string sessionId, string jsonRpc, Action<string> callback, object context = null)
        {
            Task.Factory.StartNew(() => ExecuteAsync(sessionId, jsonRpc, context, callback));
        }

        /// <summary>
        ///  执行调用（设置异步状态）
        /// </summary>
        /// <param name="stateAsync">异步状态</param>
        /// <param name="context">请求上下文</param>
        public static void Process(JsonRpcStateAsync stateAsync, object context = null)
        {
            Task.Factory.StartNew((async) =>
            {
                var args = (Tuple<JsonRpcStateAsync, object>)async;
                ProcessJsonRpcState(args.Item1, args.Item2);
            },
            new Tuple<JsonRpcStateAsync, object>(stateAsync, context));
        }
        /// <summary>
        /// 执行调用（设置异步状态）
        /// </summary>
        /// <param name="sessionId">sessionId</param>
        /// <param name="stateAsync">异步状态</param>
        /// <param name="context">请求上下文</param>
        public static void Process(string sessionId, JsonRpcStateAsync stateAsync, object context = null)
        {
            Task.Factory.StartNew((async) =>
            {
                var args = (Tuple<string, JsonRpcStateAsync, object>)async;
                ProcessJsonRpcState(args.Item1, args.Item2, args.Item3);
            },
            new Tuple<string, JsonRpcStateAsync, object>(sessionId, stateAsync, context));
        }

        /// <summary>
        /// 执行调用（不设置异步状态）
        /// </summary>
        /// <param name="jsonRpc">请求内容</param>
        /// <param name="context">请求上下文</param>
        /// <returns>执行结果（空则无错误信息）</returns>
        public static Task<string> Process(string jsonRpc, object context = null)
        {
            return Process(Handler.DefaultSessionId, jsonRpc, context);
        }
        /// <summary>
        /// 执行调用（不设置异步状态）
        /// </summary>
        /// <param name="sessionId">sessionId</param>
        /// <param name="jsonRpc">请求内容</param>
        /// <param name="context">请求上下文</param>
        /// <returns></returns>
        public static Task<string> Process(string sessionId, string jsonRpc, object context = null)
        {
            return Task<string>.Factory.StartNew((async) =>
            {
                var args = (Tuple<string, string, object>)async;
                return Execute(args.Item1, args.Item2, args.Item3);
            },
            new Tuple<string, string, object>(sessionId, jsonRpc, context));
        }

        /// <summary>
        /// 获取当前sessionId对应Handler的回调方法
        /// </summary>
        /// <param name="sessionId">sessionId</param>
        /// <returns></returns>
        public static Action<JsonRpcResponseContext> GetProcessAsyncCallback(string sessionId = "")
        {
            Handler handler = null;
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                handler = Handler.GetSessionHandler(Handler.DefaultSessionId);
            }
            else
            {
                handler = Handler.GetSessionHandler(sessionId);
            }
            return handler.GetAsyncCallback();
        }

        /// <summary>
        /// 执行调用，（设置异步状态）
        /// </summary>
        /// <param name="stateAsync">异步状态</param>
        /// <param name="context">请求上下文</param>
        internal static void ProcessJsonRpcState(JsonRpcStateAsync stateAsync, object context = null)
        {
            ProcessJsonRpcState(Handler.DefaultSessionId, stateAsync, context);
        }
        /// <summary>
        /// 执行调用，（设置异步状态）
        /// </summary>
        /// <param name="sessionId">sessionId</param>
        /// <param name="stateAsync">异步状态</param>
        /// <param name="context">请求上下文</param>
        internal static void ProcessJsonRpcState(string sessionId, JsonRpcStateAsync stateAsync, object context = null)
        {
            stateAsync.Result = Execute(sessionId, stateAsync.JsonRpc, context);
            stateAsync.SetCompleted();
        }

        /// <summary>
        /// 基本执行调用方法
        /// </summary>
        /// <param name="sessionId">sessionId</param>
        /// <param name="jsonRpc">请求内容</param>
        /// <param name="context">请求上下文</param>
        /// <returns>执行结果（空则无错误信息）</returns>
        private static string Execute(string sessionId, string jsonRpc, object context)
        {
            var handler = Handler.GetSessionHandler(sessionId);
            try
            {
                //从请求内容中构建请求与响应上下文，转换请求上下文
                Tuple<JsonRpcRequestContext, JsonRpcResponseContext>[] batch = null;
                if (IsSingle(jsonRpc))
                {
                    batch = new[] { Tuple.Create(JsonConvert.DeserializeObject<JsonRpcRequestContext>(jsonRpc), new JsonRpcResponseContext()) };
                }
                else
                {
                    batch = JsonConvert.DeserializeObject<JsonRpcRequestContext[]>(jsonRpc)
                        .Select(request => new Tuple<JsonRpcRequestContext, JsonRpcResponseContext>(request, new JsonRpcResponseContext()))
                        .ToArray();

                    if (batch.Length > 1)
                    {
                        //将所有请求上下文Tags都赋值为第一个请求上下文的Tags
                        Dictionary<string, object> firstTags = batch[0].Item1.Tags;
                        for (int i = 1; i < batch.Length; i++)
                            batch[i].Item1.Tags = firstTags;
                    }
                }

                //基本验证
                if (batch.Length == 0)
                {
                    return JsonConvert.SerializeObject(new JsonRpcResponseContext
                    {
                        Error = handler.ProcessParseException(jsonRpc,
                            new JsonRpcException(-32600, "无效请求", "请求调用为空", jsonRpc))
                    });
                }

                //网关预处理
                try
                {
                    var gateways = GatewayFactory.GetGateways();
                    foreach (var gateway in gateways)
                        gateway.PreProcess(jsonRpc);
                }
                catch (AuthenticationException ex)
                {
                    return JsonConvert.SerializeObject(new JsonRpcResponseContext
                    {
                        Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32600, ex.Message, ex.ToString(), jsonRpc))
                    });
                }
                catch (JsonRpcException ex)
                {
                    ex.rpcRequestRaw = jsonRpc;
                    return JsonConvert.SerializeObject(new JsonRpcResponseContext
                    {
                        Error = handler.ProcessParseException(jsonRpc, ex)
                    });
                }
                catch (Exception ex)
                {
                    return JsonConvert.SerializeObject(new JsonRpcResponseContext
                    {
                        Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32603, "内部错误", ex.Message, jsonRpc))
                    });
                }

                //循环请求上下文，并调用Handler处理请求
                foreach (var tuple in batch)
                {
                    var requestContext = tuple.Item1;
                    var responseContext = tuple.Item2;
                    if (requestContext == null)
                    {
                        responseContext.Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32700, "转换错误", "服务器接收到无效的json文本，解析json文本时服务器出现错误", jsonRpc));
                    }
                    else
                    {
                        responseContext.Id = requestContext.Id;
                        if (string.IsNullOrWhiteSpace(requestContext.Method))
                        {
                            responseContext.Error = handler.ProcessParseException(jsonRpc,
                                new JsonRpcException(-32600, "无效请求", "未检测到'Method'", jsonRpc));
                        }
                        else
                        {
                            var response = handler.Handle(requestContext, context);
                            if (response == null) continue;
                            responseContext.Error = response.Error;
                            responseContext.Result = response.Result;
                        }
                    }
                }

                //提取所有错误并返回
                var responses = new string[0];
                var batchError = batch.Where(x => x.Item2.Id != null || x.Item2.Error != null);
                if (batchError != null)
                {
                    var index = 0;
                    responses = new string[batchError.Count()];
                    foreach (var item in batchError)
                    {
                        responses[index++] = JsonConvert.SerializeObject(item.Item2);
                    }
                }
                return responses.Length == 0 ? string.Empty : responses.Length == 1 ? responses[0] : string.Format("[{0}]", string.Join(",", responses));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new JsonRpcResponseContext
                {
                    Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32700, "转换错误", ex, jsonRpc))
                });
            }
        }
        /// <summary>
        /// 基本执行调用方法(异步)
        /// </summary>
        /// <param name="sessionId">sessionId</param>
        /// <param name="jsonRpc">请求内容</param>
        /// <param name="context">请求上下文</param>
        /// <param name="callback">回调，包含执行结果（空则无错误信息）</param>
        private static void ExecuteAsync(string sessionId, string jsonRpc, object context, Action<string> callback)
        {
            var handler = Handler.GetSessionHandler(sessionId);
            try
            {
                //从请求内容中构建请求上下文
                Tuple<JsonRpcRequestContext>[] batch = null;
                if (IsSingle(jsonRpc))
                {
                    batch = new[] { Tuple.Create(JsonConvert.DeserializeObject<JsonRpcRequestContext>(jsonRpc)) };
                }
                else
                {
                    batch = JsonConvert.DeserializeObject<JsonRpcRequestContext[]>(jsonRpc)
                        .Select(request => new Tuple<JsonRpcRequestContext>(request))
                        .ToArray();

                    if (batch.Length > 1)
                    {
                        //将所有请求上下文Tags都赋值为第一个请求上下文的Tags
                        Dictionary<string, object> firstTags = batch[0].Item1.Tags;
                        for (int i = 1; i < batch.Length; i++)
                            batch[i].Item1.Tags = firstTags;
                    }
                }

                //基本验证
                if (batch.Length == 0)
                {
                    callback.Invoke(JsonConvert.SerializeObject(new JsonRpcResponseContext
                    {
                        Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32600, "无效请求", "请求调用为空", jsonRpc))
                    }));
                }

                //网关预处理
                try
                {
                    var gateways = GatewayFactory.GetGateways();
                    foreach (var gateway in gateways)
                        gateway.PreProcess(jsonRpc);
                }
                catch (AuthenticationException ex)
                {
                    callback.Invoke(JsonConvert.SerializeObject(new JsonRpcResponseContext
                    {
                        Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32600, ex.Message, ex.ToString(), jsonRpc))
                    }));
                }
                catch (JsonRpcException ex)
                {
                    ex.rpcRequestRaw = jsonRpc;
                    callback.Invoke(JsonConvert.SerializeObject(new JsonRpcResponseContext
                    {
                        Error = handler.ProcessParseException(jsonRpc, ex)
                    }));
                }
                catch (Exception ex)
                {
                    callback.Invoke(JsonConvert.SerializeObject(new JsonRpcResponseContext
                    {
                        Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32603, "内部错误", ex.Message, jsonRpc))
                    }));
                }

                //循环请求上下文，并调用Handler处理请求
                foreach (var tuple in batch)
                {
                    var requestContext = tuple.Item1;
                    var responseContext = new JsonRpcResponseContext();
                    if (requestContext == null)
                    {
                        responseContext.Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32700, "转换错误", "服务器接收到无效的json文本，解析json文本时服务器出现错误", jsonRpc));
                    }
                    else
                    {
                        responseContext.Id = requestContext.Id;
                        if (string.IsNullOrWhiteSpace(requestContext.Method))
                        {
                            responseContext.Error = handler.ProcessParseException(jsonRpc,
                                new JsonRpcException(-32600, "无效请求", "未检测到'Method'", jsonRpc));
                        }
                        else
                        {
                            handler.Handle(requestContext, context, (resContext) =>
                            {
                                resContext.Id = requestContext.Id;
                                if (resContext.Id != null || resContext.Error != null)
                                {
                                    callback.Invoke(JsonConvert.SerializeObject(resContext));
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                callback.Invoke(JsonConvert.SerializeObject(new JsonRpcResponseContext
                {
                    Error = handler.ProcessParseException(jsonRpc, new JsonRpcException(-32700, "转换错误", ex, jsonRpc))
                }));
            }
        }

        /// <summary>
        /// 判断是否为简单Rpc调用
        /// </summary>
        /// <param name="jsonRpc"></param>
        /// <returns></returns>
        private static bool IsSingle(string jsonRpc)
        {
            for (int i = 0; i < jsonRpc.Length; i++)
            {
                if (jsonRpc[i] == '{') return true;
                else if (jsonRpc[i] == '[') return false;
            }
            return true;
        }
    }
}
