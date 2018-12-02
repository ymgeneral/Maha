using System;
using System.Collections.Generic;
using System.Web.Routing;
using Maha.JsonService;
using Maha.JsonService.DataAnnotations;

namespace Maha.Microservices.Host
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            JsonRpcRegister.LoadFromConfig();
            JsonRpcConfigManager.SetErrorHandler(OnJsonRpcException);
            JsonRpcConfigManager.SetPreProcessHandler(new PreProcessHandler(PreProcess));
            JsonRpcConfigManager.SetCompletedProcessHandler(new CompletedProcessHandler(CompletedProcess));

            Maha.JsonClient.JsonRpcRequest.SetGlobalContextHandler((collection) =>
            {
                var contextDict = JsonRpcDataContext.GetOrInitCurrentContext();
                foreach (KeyValuePair<string, object> keyValuePair in contextDict)
                    collection[keyValuePair.Key] = (string)keyValuePair.Value;
            });
        }

        private JsonRpcException OnJsonRpcException(JsonRpcRequestContext rpc, JsonRpcException ex)
        {
            // Serivce Call Service 业务异常透传
            if (ex.data is JsonRpcException && ((JsonRpcException)ex.data).code == 32000)
            {
                ex.code = 32000;
                ex.message = ((JsonRpcException)ex.data).message;
                ex.data = null;
                return ex;
            }

            // Service内部产生的异常是BusinessException时，需要抛给调用端，并且指定RPC错误编码为：32000，没有堆栈。
            // 自定义代码，抛出业务异常
            if (ex.data.GetType().Name == "BusinessException")
            {
                ex.code = 32000;
                ex.message = ((Exception)ex.data).Message;
                ex.data = null;
            }
            else if (ex.data is ValidationException)
            {
                ex.code = 32000;
                ex.message = ((ValidationException)ex.data).Message;
                ex.data = null;
            }
            else
            {
                if (ex.data != null)
                    ex.data = ex.data.ToString();
            }

            return ex;
        }

        private JsonRpcException PreProcess(JsonRpcRequestContext rpc, object context)
        {
            return null;
        }

        private void CompletedProcess(JsonRpcRequestContext jsonRequest, JsonRpcResponseContext jsonResponse, object context)
        {
            JsonRpcDataContext.RemoveContext();
        }
    }
}
