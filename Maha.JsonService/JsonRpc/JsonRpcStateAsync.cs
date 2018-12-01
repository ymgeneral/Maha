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
        /// <summary>
        /// 请求内容
        /// </summary>
        public string JsonRpc { get; set; }

        /// <summary>
        /// 返回结果
        /// </summary>
        public string Result { get; set; }

        private object _asyncState = null;
        /// <summary>
        /// 异步状态对象
        /// </summary>
        public object AsyncState
        {
            get
            {
                return _asyncState;
            }
        }

        /// <summary>
        /// 线程资源等待对象
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// 同步完成标识位
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        private bool _isCompleted = false;
        /// <summary>
        /// 完成标识位
        /// </summary>
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
