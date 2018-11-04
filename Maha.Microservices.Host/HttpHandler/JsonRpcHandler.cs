using Maha.JsonService;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;

namespace Maha.Microservices.Host.HttpHandler
{
    /// <summary>
    /// JsonRpc Http管线处理程序
    /// HttpRequest-->inetinfo.exe-->ASPNET_ISAPI.dll-->ASPNET_WP.exe-->HttpRuntime-->HttpApplication Factory-->HttpApplication-->HttpModule-->HttpHandler Factory-->HttpHandler-->HttpHandler.BeginProcessRequest()
    /// </summary>
    public class JsonRpcHandler : IHttpAsyncHandler
    {
        /// <summary>
        /// 使用低位在前的编码方式
        /// </summary>
        private static Encoding utf8Encoding = new UTF32Encoding(false, false);

        /// <summary>
        /// 压缩并返回
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private static void CompressResponseEnd(HttpRequest request, HttpResponse response, string result, Encoding encoding)
        {
            string acceptEncoding = request.Headers["Accept-Encoding"];
            if (string.IsNullOrWhiteSpace(acceptEncoding) && acceptEncoding.Contains("gzip"))
            {
                response.AddHeader("Content-Encoding", "gzip");
                using (var gstream = new GZipStream(response.OutputStream, CompressionMode.Compress))
                using (var writer = new StreamWriter(gstream, encoding))
                {
                    writer.Write(result);
                    writer.Flush();
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(response.OutputStream, encoding))
                {
                    writer.Write(result);
                    writer.Flush();
                }
            }
            response.End();
        }

        /// <summary>
        /// 获取请求JsonRpc字符串
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetJsonRpcString(HttpRequest request)
        {
            string json = string.Empty;
            if (request.RequestType == "GET")
            {
                json = request.Params["jsonrpc"] ?? string.Empty;
            }
            else if (request.RequestType == "POST")
            {
                if (request.ContentType == "application/x-www-form-urlencoded")
                {
                    json = request.Params["jsonrpc"] ?? string.Empty;
                }
                else
                {
                    json = new StreamReader(request.InputStream).ReadToEnd();
                }
            }
            return json;
        }

        /// <summary>
        /// 开始异步请求
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            context.Response.ContentType = "application/json";

            //此处将HttpContext作为附加数据传入，由End时取出得到Request返回
            var stateAsync = new JsonRpcStateAsync(cb, context);
            stateAsync.JsonRpc = GetJsonRpcString(context.Request);
            JsonRpcProcessor.Process(stateAsync, context);
            return stateAsync;
        }

        /// <summary>
        /// 结束异步请求(异步请求返回的本质是基于Json 包装call的方式)
        /// </summary>
        /// <param name="result"></param>
        public void EndProcessRequest(IAsyncResult result)
        {
            var state = result as JsonRpcStateAsync;
            if (state != null)
            {
                var rpcResult = state.Result;
                var callback = ((HttpContext)state.AsyncState).Request.Params["callback"];
                if (!string.IsNullOrWhiteSpace(callback))
                {
                    rpcResult = string.Format("{0}({1})", callback, rpcResult);
                }

                var context = (HttpContext)state.AsyncState;
                CompressResponseEnd(context.Request, context.Response, rpcResult, utf8Encoding);
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            //not use
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}