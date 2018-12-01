using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Maha.JsonService
{
    /// <summary>
    /// 委托管理器
    /// </summary>
    public class Handler
    {
        #region Memers
        /// <summary>
        /// 默认SessionId
        /// </summary>
        private static string _defaultSessionId;

        /// <summary>
        /// sessionId->Handler集合
        /// </summary>
        private static ConcurrentDictionary<string, Handler> _sessionHandlers;

        /// <summary>
        /// Rpc上下文集合,线程Id->上下文集合
        /// </summary>
        private static ConcurrentDictionary<int, object> rpcContexts = new ConcurrentDictionary<int, object>();

        /// <summary>
        /// Rpc异常集合,线程Id->异常集合
        /// </summary>
        private static ConcurrentDictionary<int, JsonRpcException> rpcExceptions = new ConcurrentDictionary<int, JsonRpcException>();

        /// <summary>
        /// 外部预处理器委托
        /// </summary>
        private PreProcessHandler externalPreProcessingHandler = null;

        /// <summary>
        /// 外部完成处理委托
        /// </summary>
        private CompletedProcessHandler externalCompletedProcessingHandler = null;

        /// <summary>
        /// 外部错误委托
        /// </summary>
        private Func<JsonRpcRequestContext, JsonRpcException, JsonRpcException> externalErrorHandler = null;

        /// <summary>
        /// 转换错误委托
        /// </summary>
        private Func<string, JsonRpcException, JsonRpcException> parseErrorHandler = null;

        /// <summary>
        /// 线程回调标识名
        /// </summary>
        private const string THREAD_CALLBACK_SLOT_NAME = "Callback";
        #endregion

        #region Properties
        /// <summary>
        /// 当前Handler对应SessionId
        /// </summary>
        public string SessionId { get; private set; }

        /// <summary>
        /// Meta 源数据
        /// </summary>
        public SMD MetaData { get; set; }

        /// <summary>
        /// 所有委托集
        /// </summary>
        public Dictionary<string, Delegate> Handlers { get; set; }

        /// <summary>
        /// 默认的SessionId
        /// </summary>
        public static string DefaultSessionId
        {
            get
            {
                return _defaultSessionId;
            }
        }

        /// <summary>
        /// 默认的Handler
        /// </summary>
        public static Handler DefaultHandler
        {
            get
            {
                return GetSessionHandler(_defaultSessionId);
            }
        }
        #endregion

        #region Methods
        #region Register & UnRegister & Other
        /// <summary>
        /// 转换参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="metaData"></param>
        /// <remarks>避免在JValue在具有显式转换器调用DeserializeObject,故尝试对常见类型进行优化</remarks>
        /// <returns></returns>
        private object ConvetParameter(object param, SMDAdditionalParameters metaData)
        {
            var jsonVal = param as JValue;
            if (jsonVal != null && (jsonVal.Value == null))
            {
                return jsonVal.Value;
            }

            if (jsonVal != null)
            {
                if (metaData.ObjectType == typeof(string)) return (string)jsonVal;
                if (metaData.ObjectType == typeof(int)) return (int)jsonVal;
                if (metaData.ObjectType == typeof(double)) return (double)jsonVal;
                if (metaData.ObjectType == typeof(float)) return (float)jsonVal;
                if (metaData.ObjectType == typeof(decimal)) return (decimal)jsonVal;

                if (metaData.ObjectType.IsAssignableFrom(typeof(JValue)))
                    return jsonVal;
            }
            else
            {
                try
                {
                    if (param is string)
                    {
                        return JsonConvert.DeserializeObject((string)param, metaData.ObjectType);
                    }
                    return JsonConvert.DeserializeObject(param.ToString(), metaData.ObjectType);
                }
                catch { }
            }
            return param;
        }

        /// <summary>
        /// 注册委托
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="handle">委托</param>
        /// <returns></returns>
        public bool Register(string key, Delegate handle)
        {
            var result = false;
            if (!this.Handlers.ContainsKey(key))
            {
                this.Handlers.Add(key, handle);
            }
            return result;
        }

        /// <summary>
        /// 注销委托
        /// </summary>
        /// <param name="key">键值</param>
        public void UnRegister(string key)
        {
            this.Handlers.Remove(key);
            MetaData.Services.Remove(key);
        }

        /// <summary>
        /// 调用处理RpcRequest的方法
        /// </summary>
        /// <param name="requestContext">待处理的请求方法</param>
        /// <param name="rpcContext">Rpc方法中获得的可选上下文</param>
        /// <param name="callback">回调方法</param>
        /// <returns></returns>
        public JsonRpcResponseContext Handle(JsonRpcRequestContext requestContext, Object rpcContext = null, Action<JsonRpcResponseContext> callback = null)
        {
            //回调函数初始
            if (callback == null) callback = delegate (JsonRpcResponseContext item) { };

            //添加到线程Id->上下文集合中
            AddRpcContext(rpcContext);
            //初始化当前线程数据上下文
            //JsonRpcDataContext.GetOrInitCurrentContext();
            //将请求上下文Tags赋值到线程上下文中
            foreach (var key in requestContext.Tags.Keys)
            {
                JsonRpcDataContext.SetContextItem(key, requestContext.Tags[key]);
            }

            #region 预处理&出错则回调直接返回
            var preProcessingException = PreProcess(requestContext, rpcContext);
            if (preProcessingException != null)
            {
                var response = new JsonRpcResponseContext()
                {
                    Error = preProcessingException,
                    Id = requestContext.Id
                };
                callback.Invoke(response);
                CompletedProcess(requestContext, response, rpcContext);
                return response;
            }
            #endregion

            #region 获取和验证请求的委托与meta信息
            //当前请求方法对应的meta信息
            SMDService metaData = null;
            //当前请求方法对应的反射委托
            Delegate handle = null;
            //根据请求方法，获取方法对应的委托
            var haveDelegate = Handlers.TryGetValue(requestContext.Method, out handle);
            //根据请求方法，获取方法对应的meta信息
            var haveMetadata = MetaData.Services.TryGetValue(requestContext.Method, out metaData);
            if (haveDelegate == false || haveMetadata == false || metaData == null || handle == null)
            {
                var response = new JsonRpcResponseContext()
                {
                    Result = null,
                    Error = new JsonRpcException(-32601, "方法未找到", "此方法 " + requestContext.Method + " 不存在或未部署", requestContext),
                    Id = requestContext.Id
                };
                callback.Invoke(response);
                CompletedProcess(requestContext, response, rpcContext);
            }
            #endregion

            #region 参数获取与校验
            bool isJObject = requestContext.Params is JObject;
            bool isJArray = requestContext.Params is JArray;

            //请求方法参数
            object[] parameters = null;

            //metaData是否包含异常
            bool expectsRefException = false;

            //metaData 参数数量
            var metaDataParamCount = metaData.parameters.Count(p => p != null);

            //获取当前请求上下问参数集合
            var paramCollection = requestContext.Params as ICollection;
            int paramCount = 0, loopCount = 0;
            if (paramCollection != null)
            {
                paramCount = loopCount = paramCollection.Count;
            }
            //在请求参数与metaData参数相差一情况下，判断metaData最后参数是否为异常
            if (paramCount == metaDataParamCount - 1 && metaData.parameters[metaDataParamCount - 1].ObjectType.Name.Contains(typeof(JsonRpcException).Name))
            {
                paramCount++;
                expectsRefException = true;
            }
            //初始化请求参数集合大小
            parameters = new object[paramCount];

            //根据请求参数不同类型，填充参数集合
            if (isJArray)
            {
                var jArray = requestContext.Params as JArray;
                for (int i = 0; i < loopCount; i++)
                {
                    parameters[i] = ConvetParameter(jArray[i], metaData.parameters[i]);
                }
            }
            else if (isJObject)
            {
                var jObject = requestContext.Params as JObject;
                var tokenDict = jObject as IDictionary<string, JToken>;
                parameters = new object[metaData.parameters.Length];

                for (int i = 0; i < metaData.parameters.Length; i++)
                {
                    if (tokenDict.ContainsKey(metaData.parameters[i].Name) == false)
                    {
                        var response = new JsonRpcResponseContext()
                        {
                            Error = ProcessException(requestContext,
                            new JsonRpcException(-32602, "无效参数", string.Format("参数 '{0}' 未定义", metaData.parameters[i].Name), requestContext)),
                            Id = requestContext.Id
                        };
                    }
                }
            }

            //请求参数小于metaData参数，可能包含可选参数
            if (parameters.Length < metaDataParamCount && metaData.defaultValues.Length > 0)
            {
                //请求参数提供的数量
                var suppliedParamsCount = parameters.Length;

                //非Rpc设置的可选参数数量，缺失的参数数量
                var missingParamsCount = metaDataParamCount - parameters.Length;

                //调整可选参数数组大小，用于包含所有可选参数
                Array.Resize(ref parameters, parameters.Length + missingParamsCount);

                //将所有参数填充到parameters中
                for (int paramIndex = parameters.Length - 1, defaultIndex = metaData.defaultValues.Length - 1;
                    paramIndex >= suppliedParamsCount && defaultIndex >= 0; paramIndex--, defaultIndex--)
                {
                    parameters[paramIndex] = metaData.defaultValues[defaultIndex].Value;
                }

                if (missingParamsCount > metaData.defaultValues.Length)
                {
                    var response = new JsonRpcResponseContext()
                    {
                        Error = ProcessException(requestContext,
                        new JsonRpcException(-32602, "无效参数", string.Format("缺省参数数量 {0} 不足以填充所缺失的参数数量 {1}",
                        metaData.defaultValues.Length, missingParamsCount), requestContext)),
                        Id = requestContext.Id
                    };
                    callback.Invoke(response);
                    CompletedProcess(requestContext, response, rpcContext);
                    return response;
                }
            }
            if (parameters.Length != metaDataParamCount)
            {
                var response = new JsonRpcResponseContext()
                {
                    Error = ProcessException(requestContext,
                    new JsonRpcException(-32602, "无效参数", string.Format("应接收 {0} 个参数,实际接收到 {1}",
                    metaData.parameters.Length, parameters.Length), requestContext)),
                    Id = requestContext.Id
                };
                callback.Invoke(response);
                CompletedProcess(requestContext, response, rpcContext);
                return response;
            }
            #endregion

            #region 调用
            try
            {
                //将回调存储到当前线程中，，便于从GetAsyncCallback中获得以进行回调
                Thread.SetData(Thread.GetNamedDataSlot(THREAD_CALLBACK_SLOT_NAME), callback);
                //调用请求方法对应委托并传入参数
                var results = handle.DynamicInvoke(parameters);

                //当前线程包含异常
                if (Task.CurrentId.HasValue && rpcExceptions.TryRemove(Task.CurrentId.Value, out JsonRpcException contextException))
                {
                    var response = new JsonRpcResponseContext() { Error = ProcessException(requestContext, contextException), Id = requestContext.Id };
                    callback.Invoke(response);
                    CompletedProcess(requestContext, response, rpcContext);
                    return response;
                }

                var lastParam = parameters.LastOrDefault();
                if (expectsRefException && lastParam != null && lastParam is JsonRpcException)
                {
                    var response = new JsonRpcResponseContext()
                    {
                        Error = ProcessException(requestContext, lastParam as JsonRpcException),
                        Id = requestContext.Id
                    };
                    callback.Invoke(response);
                    CompletedProcess(requestContext, response, rpcContext);
                    return response;
                }

                //返回结果
                var reponse = new JsonRpcResponseContext() { Result = results };
                CompletedProcess(requestContext, reponse, requestContext);
                return reponse;
            }
            catch (Exception ex)
            {
                JsonRpcResponseContext response = null;
                //系统委托调用异常(参数不匹配)
                if (ex is TargetParameterCountException)
                {
                    response = new JsonRpcResponseContext()
                    {
                        Error = ProcessException(requestContext, new JsonRpcException(-32602, "参数不匹配", ex, rpcContext))
                    };
                    callback.Invoke(response);
                    CompletedProcess(requestContext, response, rpcContext);
                    return response;
                }
                //业务异常
                if (ex is JsonRpcException)
                {
                    response = new JsonRpcResponseContext()
                    {
                        Error = ProcessException(requestContext, new JsonRpcException(-32603, "内部错误", ex.InnerException, rpcContext)),
                        Id = requestContext.Id
                    };
                    callback.Invoke(response);
                    CompletedProcess(requestContext, response, rpcContext);
                    return response;
                }
                //内部异常为业务异常
                if (ex.InnerException != null && ex.InnerException is JsonRpcException)
                {
                    response = new JsonRpcResponseContext()
                    {
                        Error = ProcessException(requestContext, ex.InnerException as JsonRpcException)
                    };
                    callback.Invoke(response);
                    CompletedProcess(requestContext, response, rpcContext);
                    return response;
                }
                //内部异常
                else if (ex.InnerException != null)
                {
                    response = new JsonRpcResponseContext()
                    {
                        Error = ProcessException(requestContext, new JsonRpcException(-32603, "内部错误", ex.InnerException, rpcContext))
                    };
                    callback.Invoke(response);
                    CompletedProcess(requestContext, response, rpcContext);
                    return response;
                }
                //其他异常，封装返回
                else
                {
                    response = new JsonRpcResponseContext()
                    {
                        Error = ProcessException(requestContext, new JsonRpcException(-32603, "内部错误", ex, rpcContext))
                    };
                    callback.Invoke(response);
                    CompletedProcess(requestContext, response, rpcContext);
                    return response;
                }
            }
            finally
            {
                RemoveRpcContext();
            }
            #endregion
        }

        /// <summary>
        /// 获取异步回调
        /// </summary>
        /// <remarks>返回的回调方法将设置为当前线程的调用方法中（本线程）</remarks>
        /// <returns></returns>
        internal Action<JsonRpcResponseContext> GetAsyncCallback()
        {
            object data = Thread.GetData(Thread.GetNamedDataSlot(THREAD_CALLBACK_SLOT_NAME));
            Action<JsonRpcResponseContext> callback = null;
            if (data is Action<JsonRpcResponseContext>)
            {
                callback = data as Action<JsonRpcResponseContext>;
            }
            else
            {
                callback = delegate (JsonRpcResponseContext item) { };
            }
            return callback;
        }
        #endregion

        #region Session
        /// <summary>
        /// 根据Session获取对应的Handler
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static Handler GetSessionHandler(string sessionId)
        {
            return _sessionHandlers.GetOrAdd(sessionId, new Handler(sessionId));
        }

        /// <summary>
        /// 根据SessionId销毁Session
        /// </summary>
        /// <param name="sessionId"></param>
        public static void DestroySession(string sessionId)
        {
            Handler handler = null;
            _sessionHandlers.TryRemove(sessionId, out handler);
            handler.Handlers.Clear();
            handler.MetaData.Services.Clear();
        }

        /// <summary>
        /// 从处理程序注册表中销毁指定Session
        /// </summary>
        public void Destroy()
        {
            DestroySession(SessionId);
        }
        #endregion

        #region RpcContext
        /// <summary>
        /// 返回当前线程上下文
        /// </summary>
        /// <returns></returns>
        public static object RpcContext()
        {
            if (Task.CurrentId == null)
                return null;
            if (rpcContexts.ContainsKey(Task.CurrentId.Value) == false)
                return null;
            return rpcContexts[Task.CurrentId.Value];
        }

        /// <summary>
        /// 添加Rpc上下文
        /// </summary>
        /// <param name="rpcContext"></param>
        private void AddRpcContext(object rpcContext)
        {
            if (Task.CurrentId != null)
            {
                rpcContexts[Task.CurrentId.Value] = rpcContext;
            }
        }

        /// <summary>
        /// 删除Rpc上下文
        /// </summary>
        private void RemoveRpcContext()
        {
            if (Task.CurrentId != null)
            {
                int taskId = Task.CurrentId.Value;
                rpcContexts[taskId] = null;
                rpcContexts.TryRemove(taskId, out object val);
            }
        }
        #endregion

        #region RpcException
        /// <summary>
        /// 设置Rpc响应中使用的异常
        /// </summary>
        /// <param name="exception"></param>
        public static void SetRpcException(JsonRpcException exception)
        {
            if (Task.CurrentId != null)
                rpcExceptions[Task.CurrentId.Value] = exception;
            else
                throw new InvalidOperationException("此方法仅在标记为Json Rpc方法的上下文中使用时才有效，并且该方法必须由JsonRpc Handler调用。");
        }

        /// <summary>
        /// 删除Rpc相应使用的异常
        /// </summary>
        private void RemoveRpcException()
        {
            if (Task.CurrentId != null)
            {
                var id = Task.CurrentId.Value;
                rpcExceptions[id] = null;
                rpcExceptions.TryRemove(id, out JsonRpcException jsonRpcException);
            }
        }
        #endregion

        #region SetHander & Process Handler
        /// <summary>
        /// 设置预处理器
        /// </summary>
        /// <param name="handler"></param>
        internal void SetPreProcessHandler(PreProcessHandler handler)
        {
            externalPreProcessingHandler += handler;
        }
        /// <summary>
        /// 预处理调用
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private JsonRpcException PreProcess(JsonRpcRequestContext request, object context)
        {
            if (externalPreProcessingHandler == null)
                return null;
            return externalPreProcessingHandler(request, context);
        }

        /// <summary>
        /// 设置完成处理器
        /// </summary>
        /// <param name="handler"></param>
        internal void SetCompletedProcessHandler(CompletedProcessHandler handler)
        {
            externalCompletedProcessingHandler += handler;
        }
        /// <summary>
        /// 完成后调用
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="context"></param>
        internal void CompletedProcess(JsonRpcRequestContext request, JsonRpcResponseContext response, object context)
        {
            JsonRpcDataContext.RemoveContext();
            if (externalCompletedProcessingHandler == null)
                return;
            externalCompletedProcessingHandler(request, response, context);
        }

        /// <summary>
        /// 设置异常处理器
        /// </summary>
        /// <param name="handler"></param>
        internal void SetErrorHandler(Func<JsonRpcRequestContext, JsonRpcException, JsonRpcException> handler)
        {
            externalErrorHandler = handler;
        }
        /// <summary>
        /// 处理异常
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private JsonRpcException ProcessException(JsonRpcRequestContext request, JsonRpcException ex)
        {
            if (externalErrorHandler != null)
                return externalErrorHandler(request, ex);
            return ex;
        }

        /// <summary>
        /// 设置转换处理器
        /// </summary>
        /// <param name="handler"></param>
        internal void SetParseErrorHandler(Func<string, JsonRpcException, JsonRpcException> handler)
        {
            parseErrorHandler = handler;
        }
        /// <summary>
        /// 调用转换异常
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        internal JsonRpcException ProcessParseException(string request, JsonRpcException ex)
        {
            if (parseErrorHandler != null)
                return parseErrorHandler(request, ex);
            return ex;
        }

        #endregion
        #endregion

        #region Constructors
        static Handler()
        {
            _defaultSessionId = Guid.NewGuid().ToString();
            _sessionHandlers = new ConcurrentDictionary<string, Handler>();
            _sessionHandlers[_defaultSessionId] = new Handler(_defaultSessionId);
        }

        private Handler(string sessionId)
        {
            SessionId = sessionId;
            this.MetaData = new SMD();
            this.Handlers = new Dictionary<string, Delegate>();
        }
        #endregion
    }
}
