using Maha.JsonService;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;

namespace Maha.Microservices.Host
{
    /// <summary>
    /// JsonRpc Http管线处理程序
    /// HttpRequest-->inetinfo.exe-->ASPNET_ISAPI.dll-->ASPNET_WP.exe-->HttpRuntime-->HttpApplication Factory-->HttpApplication-->HttpModule-->HttpHandler Factory-->HttpHandler-->HttpHandler.BeginProcessRequest()
    /// </summary>
    public class RequestHandler : IHttpAsyncHandler
    {
        /// <summary>
        /// 使用低位在前的编码方式
        /// </summary>
        private static Encoding utf8Encoding = new UTF8Encoding(false);

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
        /// 获取请求字符串
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetRequestContext(HttpRequest request)
        {
            string requestContext = string.Empty;
            if (request.RequestType == "GET")
            {
                requestContext = request.Params["jsonrpc"] ?? HttpUtility.UrlDecode(request.QueryString.ToString(), utf8Encoding);
            }
            else if (request.RequestType == "POST")
            {
                requestContext = request.Params["jsonrpc"] ?? new StreamReader(request.InputStream).ReadToEnd();
            }
            return requestContext;
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
            if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
            {
                context.Response.ContentType = context.Request.ContentType;
            }
            else
            {
                //默认采用json格式返回
                context.Response.ContentType = "application/json";
            }

            //此处将HttpContext作为附加数据传入，由End时取出得到Request返回
            var stateAsync = new JsonRpcStateAsync(cb, context);
            stateAsync.JsonRpc = GetRequestContext(context.Request);
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