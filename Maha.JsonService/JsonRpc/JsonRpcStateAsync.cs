using System;
using System.Threading;

namespace Maha.JsonService 
{
    /// <summary>
    /// JsonRpc状态（异步）
    /// 用于处理JsonRpcHandler请求的状态
    /// </summary>
    public class JsonRpcStateAsync : IAsyncResult
    {
        public JsonRpcStateAsync(AsyncCallback callback, object asyncState)
        {
            _callback = callback;
            _asyncState = asyncState;
        }

        #region Properties
        public string JsonRpc { get; set; }
        public string Result { get; set; }


        private object _asyncState = null;
        public object AsyncState
        {
            get
            {
                return _asyncState;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return null;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        private bool _isCompleted = false;
        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
        }

        private AsyncCallback _callback = null;
        internal void SetCompleted()
        {
            _isCompleted = true;
            if (_callback != null)
            {
                _callback(this);
            }
        }
        #endregion
    }
}
