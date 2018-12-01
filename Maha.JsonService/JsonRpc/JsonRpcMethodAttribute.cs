using System;

namespace Maha.JsonService.JsonRpc
{
    /// <summary>
    /// 公开JsonRpc服务方法的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class JsonRpcMethodAttribute : Attribute
    {
        private readonly string jsonMethodName;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="jsonMethodName">指定方法名</param>
        public JsonRpcMethodAttribute(string jsonMethodName = "")
        {
            this.jsonMethodName = jsonMethodName;
        }

        public string JsonMethodName
        {
            get { return jsonMethodName; }
        }
    }
}
