using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace Maha.JsonClient.Invoker
{
    /// <summary>
    /// 基于Http Rpc 调用器
    /// </summary>
    internal class HttpRpcInvoker : RpcInvoker
    {
        /// <summary>
        /// 开启GZip压缩
        /// </summary>
        public static bool EnabledGZip = true;

        /// <summary>
        /// 默认超时时间5分钟
        /// </summary>
        public const int DefaultTimeout = 1000 * 60 * 5;

        /// <summary>
        /// 读取字节流数据
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        private static Stream ReadStream(Stream inputStream)
        {
            const int readSize = 4096;
            byte[] buffer = new byte[readSize];
            MemoryStream stream = new MemoryStream();
            int count = inputStream.Read(buffer, 0, readSize);
            while (count > 0)
            {
                stream.Write(buffer, 0, count);
                count = inputStream.Read(buffer, 0, readSize);
            }
            stream.Position = 0;
            inputStream.Close();
            return stream;
        }

        /// <summary>
        /// 基于请求上下文调用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        protected override JsonRpcResponseContext<T> Invoke<T>(JsonRpcRequestContext requestContext)
        {
            HttpWebRequest request = WebRequest.Create(new Uri(ServiceAddress + "?callid=" + requestContext.Id.ToString() + "&method=" + requestContext.Method)) as HttpWebRequest;
            request.KeepAlive = false;
            request.Proxy = null;
            request.Method = "Post";
            request.ContentType = "application/json-rpc";
            if (this.Option != null && this.Option.Timeout > 0)
            {
                request.Timeout = this.Option.Timeout;
            }
            else
            {
                request.Timeout = DefaultTimeout;
            }
            request.ReadWriteTimeout = request.Timeout;

            if (EnabledGZip)
                request.Headers["Accept-Encoding"] = "gzip";

            //将请求数据写入请求流
            var stream = new StreamWriter(request.GetRequestStream());
            var requestStr = JsonConvert.SerializeObject(requestContext);
            stream.Write(requestStr);
            stream.Close();

            //获取返回对象
            var response = request.GetResponse();
            string responseStr = string.Empty;
            string responseContentEncoding = response.Headers["Content-Encoding"];

            //使用gzip方式读书返回数据流
            if (!string.IsNullOrWhiteSpace(responseContentEncoding) && responseContentEncoding.ToLower().Contains("gzip"))
            {
                var responseStream = ReadStream(response.GetResponseStream());
                using (var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(gzipStream, utf8Encoding))
                    {
                        responseStr = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                using (var reader = new StreamReader(ReadStream(response.GetResponseStream())))
                {
                    responseStr = reader.ReadToEnd();
                }
            }
            response.Close();

            //Json反序列化获取返回数据上下文
            var responseContext = JsonConvert.DeserializeObject<JsonRpcResponseContext<T>>(responseStr);
            if (responseContext == null)
            {
                if (!string.IsNullOrWhiteSpace(responseStr))
                {
                    JObject jObject = JsonConvert.DeserializeObject(responseStr) as JObject;
                    throw new Exception(jObject["Error"].ToString());
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
            bool isFirstRequest = true;
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

                if (isFirstRequest)
                {
                    SetReqContextTags(item);
                    SetReqContextAuth(item);
                }
                isFirstRequest = false;
            }

            HttpWebRequest request = WebRequest.Create(new Uri(ServiceAddress)) as HttpWebRequest;
            request.KeepAlive = false;
            request.Proxy = null;
            request.Method = "Post";
            request.ContentType = "application/json-rpc";
            if (this.Option != null && this.Option.Timeout > 0)
            {
                request.Timeout = this.Option.Timeout;
            }
            else
            {
                request.Timeout = DefaultTimeout;
            }
            request.ReadWriteTimeout = request.Timeout;

            if (EnabledGZip)
                request.Headers["Accept-Encoding"] = "gzip";

            //将请求数据写入请求流
            var stream = new StreamWriter(request.GetRequestStream());
            var requestStr = JsonConvert.SerializeObject(requestContexts);
            stream.Write(requestStr);
            stream.Close();

            //获取返回对象
            var response = request.GetResponse();
            string responseStr = string.Empty;
            string responseContentEncoding = response.Headers["Content-Encoding"];

            //使用gzip方式读书返回数据流
            if (!string.IsNullOrWhiteSpace(responseContentEncoding) && responseContentEncoding.ToLower().Contains("gzip"))
            {
                var responseStream = ReadStream(response.GetResponseStream());
                using (var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(gzipStream, utf8Encoding))
                    {
                        responseStr = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                using (var reader = new StreamReader(ReadStream(response.GetResponseStream())))
                {
                    responseStr = reader.ReadToEnd();
                }
            }
            response.Close();

            //Json反序列化获取返回数据上下文
            var responseContexts = JsonConvert.DeserializeObject<List<JsonRpcResponseContext>>(responseStr);
            if (responseContexts == null)
            {
                if (!string.IsNullOrWhiteSpace(responseStr))
                {
                    JObject jObject = JsonConvert.DeserializeObject(responseStr) as JObject;
                    throw new Exception(jObject["Error"].ToString());
                }
            }
            return responseContexts;
        }
    }
}
