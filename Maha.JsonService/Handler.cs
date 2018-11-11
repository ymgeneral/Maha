using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maha.JsonService
{
    public class Handler
    {
        #region Memers
        /// <summary>
        /// 默认SessionId
        /// </summary>
        private static string _defaultSessionId;

        /// <summary>
        /// SessionId与Handler对应集合
        /// </summary>
        private static ConcurrentDictionary<string, Handler> _sessionHandlers;

        private static ConcurrentDictionary<int, object> rpcContexts = new ConcurrentDictionary<int, object>();

        private static ConcurrentDictionary<int, JsonRpcException> rpcExceptions = new ConcurrentDictionary<int, JsonRpcException>();

        private PreProcessHandler externalPreProcessingHandler;

        private CompletedProcessHandler externalCompletedProcessingHandler;

        private Func<JsonRpcRequestContext, JsonRpcException, JsonRpcException> externalErrorHandler;

        private Func<string, JsonRpcException, JsonRpcException> parseErrorHandler;

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

        #region Methods
        #region Private&Internal Methods
        private object CleanUpParameter()
        {
            return null;
        }

        private JsonRpcException PreProcess(JsonRpcRequestContext request, object context)
        {
            return null;
        }

        internal void CompletedProcess(JsonRpcRequestContext request, JsonRpcResponseContext response, object context)
        {

        }

        internal void SetPreProcessHandler(PreProcessHandler handler)
        {

        }

        internal void SetCompletedProcessHandler(CompletedProcessHandler handler)
        {

        }
        #endregion

        #region Public Methods
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
            Handler handler;
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

        /// <summary>
        /// 提供对每个JSON RPC方法调用特定的上下文的访问。
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

        public static void RpcSetException(JsonRpcException exception)
        {
            if (Task.CurrentId != null)
                rpcExceptions[Task.CurrentId.Value] = exception;
            else
                throw new InvalidOperationException("此方法仅在标记为JSONRPC方法的上下文中使用时才有效，并且该方法必须由JsonRpc Handler调用。");

        }

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
        #endregion
    }
}
