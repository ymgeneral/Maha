using System;

namespace Maha.Microservices.DemoService.Base
{
    /// <summary>
    /// Rpc自定义异常
    /// </summary>
    internal class RpcException : Exception
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int Code { get; private set; }

        /// <summary>
        /// 异常构造器
        /// </summary>
        public RpcException() : base() { }

        /// <summary>
        /// 异常构造器（多参）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        public RpcException(string message, int code = 0) : base(message)
        {
            this.Code = code;
        }
    }
}
