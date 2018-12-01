using System;
using System.Threading;

namespace Maha.JsonClient
{
    /// <summary>
    /// 异步调用结果 
    /// </summary>
    public class CallAsyncResult<T> : IAsyncResult
    {
        #region Properties
        /// <summary>
        /// 是否为申明式参数
        /// </summary>
        internal bool IsDeclaredParams { get; set; }

        /// <summary>
        /// 申明式参数
        /// </summary>
        internal object DeclaredParam { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// 请求选项
        /// </summary>
        public RpcOption RpcOption { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        internal object[] Args { get; set; }

        /// <summary>
        /// 申明式参数值
        /// </summary>
        internal object DeclaredArgs { get; set; }

        /// <summary>
        /// 成功回调函数
        /// </summary>
        internal Action<T> SuccessedCallBack { get; set; }

        /// <summary>
        /// 失败回调函数
        /// </summary>
        internal Action<Exception> FailedCallBack { get; set; }

        /// <summary>
        /// 异步状态
        /// </summary>
        public object AsyncState
        {
            get;
        }

        /// <summary>
        /// 是否完成同步
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 异步等待句柄
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                //不支持
                return null;
            }
        }

        private bool isCompleted = false;
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
        }

        /// <summary>
        /// 请求方法名
        /// </summary>
        public string Method { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 设置请求完成，调用回调函数
        /// </summary>
        internal void SetCompleted()
        {
            isCompleted = true;
            if (SuccessedCallBack != null)
            {
                if (this.Error == null)
                    SuccessedCallBack(this.Result);
            }
            if (FailedCallBack != null)
            {
                if (this.Error != null)
                    FailedCallBack(this.Error);
            }
        }
        #endregion
    }
}
