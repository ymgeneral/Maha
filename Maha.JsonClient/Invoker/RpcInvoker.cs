using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Maha.JsonClient.Invoker
{
    /// <summary>
    /// Rpc调用器
    /// </summary>
    internal abstract class RpcInvoker
    {
        #region Fields
        /// <summary>
        /// 使用低位在前的编码方式
        /// </summary>
        protected static Encoding utf8Encoding = new UTF8Encoding(false);

        /// <summary>
        /// Id锁定器
        /// </summary>
        protected static object idLocker = new object();

        /// <summary>
        /// 调用器Id
        /// </summary>
        protected static int id = 0;

        /// <summary>
        /// 服务地址
        /// </summary>
        internal string ServiceAddress { get; set; }

        /// <summary>
        /// 调用选项
        /// </summary>
        public RpcOption Option { get; set; }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// 调用，返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        protected abstract JsonRpcResponseContext<T> Invoke<T>(JsonRpcRequestContext requestContext);

        /// <summary>
        /// 批量调用，批量返回
        /// </summary>
        /// <param name="requestContexts"></param>
        /// <returns></returns>
        internal abstract List<JsonRpcResponseContext> BatchInvoke(List<JsonRpcRequestContext> requestContexts);
        #endregion

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal JsonRpcResponseContext<T> Invoke<T>(string method, params object[] args)
        {
            var reqContext = new JsonRpcRequestContext()
            {
                Method = method,
                Params = args
            };

            int currId = 0;
            if (reqContext.Id == null)
            {
                lock (idLocker)
                {
                    currId = ++id;
                }
                reqContext.Id = currId.ToString();
            }
            SetReqContextTags(reqContext);
            SetReqContextAuth(reqContext);
            return Invoke<T>(reqContext);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">JSON-RPC方法名</param>
        /// <param name="args">JSON-RPC方法接收的参数，此参数为将参数数组对象化的写法，如new { param1 = "A", param2 = "B"  }</param>
        /// <returns></returns>
        internal JsonRpcResponseContext<T> InvokeWithDeclaredParams<T>(string method, object args)
        {
            var reqContext = new JsonRpcRequestContext()
            {
                Method = method,
                Params = args,
                Id = 1
            };
            SetReqContextTags(reqContext);
            SetReqContextAuth(reqContext);
            return Invoke<T>(reqContext);
        }

        /// <summary>
        /// 设置请求上下文的Tags
        /// </summary>
        /// <param name="reqContext">请求上下文</param>
        protected void SetReqContextTags(JsonRpcRequestContext reqContext)
        {
            if (reqContext.Tags == null)
                reqContext = new JsonRpcRequestContext();

            //引用全局事件中的上下文注入函数，从而将其加入到JsonRpcRequestContext.Tags
            if (JsonRpcRequest.Handler != null)
                JsonRpcRequest.Handler(reqContext.Tags);

            //合并当前Invoker Option到JsonRpcRequestContext Tag中
            if (Option != null && Option.ContextValues != null)
            {
                foreach (var key in Option.ContextValues.Keys)
                {
                    reqContext.Tags[key] = Option.ContextValues[key];
                }
            }

            if (Option != null && Option.ForceWriteDB)
                reqContext.Tags["x-rpc-forcewritedb"] = "true";
        }

        /// <summary>
        /// 设置请求上下文的授权
        /// </summary>
        /// <param name="reqContext"></param>
        protected void SetReqContextAuth(JsonRpcRequestContext reqContext)
        {
            if (reqContext.Tags.ContainsKey("auth_client_id") && !string.IsNullOrWhiteSpace(reqContext.Tags["auth_token"].ToString()))
                return;

            //上下文不包含授权信息则使用配置文件
            if (!reqContext.Tags.ContainsKey("auth_client_id") || string.IsNullOrWhiteSpace(reqContext.Tags["auth_token"].ToString()))
            {
                var clientId = Configmanager.GetClientIdFromConfig(reqContext.Method);
                if (clientId == null)
                    return;
                else
                    reqContext.Tags["auth_client_id"] = clientId;
            }

            //使用上下文中的授权信息
            var client_id = reqContext.Tags["auth_client_id"].ToString();
            //私钥文件路径
            var privateKeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keys\\private_key_" + client_id + ".pem");
            if (!File.Exists(privateKeyFilePath))
            {
                System.Diagnostics.Debug.WriteLine("Rpc私钥证书文件不存在: " + privateKeyFilePath);
                return;
            }
            //私钥内容
            var privateKeyContent = File.ReadAllText(privateKeyFilePath);
            //请求上下文文本
            var reqContextText = JsonConvert.SerializeObject(reqContext);
            try
            {
                var auth_token = string.Empty;
                reqContext.Tags["auth_token"] = auth_token;
            }
            catch (Exception ex)
            {
                throw new Exception("构建服务请求签名失败", ex);
            }
        }
    }
}
