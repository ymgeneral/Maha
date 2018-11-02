using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Maha.JsonService
{
    public class Handler
    {
        #region Memers
        private static string _defaultSessionId;
        private static ConcurrentDictionary<string, Handler> _sessionHandlers;
        private static ConcurrentDictionary<int, object> RpcContexts = new ConcurrentDictionary<int, object>();
        private static ConcurrentDictionary<int, JsonRpcException> RpcExceptions = new ConcurrentDictionary<int, JsonRpcException>();
        private PreProcessHandler externalPreProcessingHandler;
        private CompletedProcessHandler externalCompletedProcessingHandler;
        private Func<JsonRequest, JsonRpcException, JsonRpcException> externalErrorHandler;
        private Func<string, JsonRpcException, JsonRpcException> parseErrorHandler;
        public Dictionary<string, Delegate> Handlers { get; set; }

        /// <summary>
        /// Meta 源数据
        /// </summary>
        public SMD MetaData { get; set; }

        private const string THREAD_CALLBACK_SLOT_NAME = "Callback";
        #endregion

        #region Properties
        public string SessionId { get; private set; }

        public static string DefaultSessionId
        {
            get
            {
                return _defaultSessionId;
            }
        }

        public static Handler GetSessionHandler(string sessionId)
        {
            return _sessionHandlers.GetOrAdd(sessionId,new Handler(sessionId));
        }


        public static Handler DefaultHandler
        {
            get
            {
                return GetSessionHandler(_defaultSessionId);
            }
        }


        public static void DestroySession(string sessionId)
        {
            Handler handler;
            _sessionHandlers.TryRemove(sessionId, out handler);
            handler.Handlers.Clear();
            handler.MetaData.Services.Clear();
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
        private object CleanUpParameter()
        {
            return null;
        }

        private JsonRpcException PreProcess(JsonRequest request,object context)
        {
            return null;
        }

        internal void CompletedProcess(JsonRequest request, JsonResponse response, object context)
        {

        }

        internal void SetPreProcessHandler(PreProcessHandler handler)
        {

        }

        internal void SetCompletedProcessHandler(CompletedProcessHandler handler)
        {

        }
        #endregion
    }
}
