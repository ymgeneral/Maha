using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Maha.JsonClient.Invoker
{
    /// <summary>
    /// 基于本地Rpc 调用器
    /// </summary>
    internal class LocalRpcInvoker : RpcInvoker
    {
        /// <summary>
        /// 调用本地Rpc服务
        /// </summary>
        /// <param name="jsonRequestStr">请求字符串</param>
        /// <returns></returns>
        private static string InvokeLocalRpc(string jsonRequestStr)
        {
            var jsonRpcProcessor = Type.GetType("Maha.JsonService.JsonRpcProcessor,Maha.JsonService");
            if (jsonRpcProcessor == null)
            {
                throw new TypeUnloadedException("找不到 Maha.JsonService.JsonRpcProcessor, 是否 OF.JsonRpc.dll 没有被引用");
            }

            //调用Process方法并获取返回Task
            var task = jsonRpcProcessor.GetMethod("Process", new Type[] { }).Invoke(null, new object[] { (object)jsonRequestStr, null }) as Task<string>;
            return task.Result;
        }

        /// <summary>
        /// 基于请求上下文调用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestContext">请求上下文</param>
        protected override JsonRpcResponseContext<T> Invoke<T>(JsonRpcRequestContext requestContext)
        {
            int currId = 0;
            if (requestContext.Id == null)
            {
                lock (idLocker)
                {
                    currId = ++id;
                }
                requestContext.Id = currId.ToString();
            }

            var requestStr = JsonConvert.SerializeObject(requestContext);
            SetReqContextTags(requestContext);//TODO 序列化之后再设置上下文?
            var responseStr = InvokeLocalRpc(requestStr);
            var responseContext = JsonConvert.DeserializeObject<JsonRpcResponseContext<T>>(responseStr);
            if (responseContext == null)
            {
                if (!string.IsNullOrWhiteSpace(responseStr))
                {
                    JObject jObject = JsonConvert.DeserializeObject(responseStr) as JObject;
                    throw new Exception(jObject["Error"].ToString());
                }
                else
                {
                    throw new Exception("the response is null");
                }
            }
            return responseContext;
        }

        /// <summary>
        /// 基于请求上下文批量调用方法
        /// </summary>
        /// <param name="requestContexts">请求上下文集合</param>
        /// <returns></returns>
        internal override List<JsonRpcResponseContext> BatchInvoke(List<JsonRpcRequestContext> requestContexts)
        {
            foreach (var item in requestContexts)
            {
                int currId = 0;
                if (item.Id == null)
                {
                    lock (idLocker)
                    {
                        currId = ++id;
                    }
                    item.Id = currId.ToString();
                }
                SetReqContextTags(item);//TODO 每次均需要设置请求上下文？
            }
            var requestStr = JsonConvert.SerializeObject(requestContexts);
            var responseStr = InvokeLocalRpc(requestStr);
            return JsonConvert.DeserializeObject<List<JsonRpcResponseContext>>(responseStr);
        }
    }
}
