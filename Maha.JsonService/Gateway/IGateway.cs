using System;
using System.Collections.Generic;
using System.Text;

namespace Maha.JsonService.Gateway
{
    /// <summary>
    /// 网关接口
    /// </summary>
    public interface IGateway
    {
        /// <summary>
        /// 预调用
        /// </summary>
        /// <param name="jsonRpc"></param>
        void PreProcess(string jsonRpc);

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="jsonRpcResponse"></param>
        void CompletedProcess(string jsonRpcResponse);

        /// <summary>
        /// 属性参数
        /// </summary>
        Dictionary<string, string> Attributes { get; set; }
    }
}
