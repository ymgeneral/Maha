using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Maha.JsonClient
{
    /// <summary>
    /// 配置管理器（jsonrpc.config）
    /// </summary>
    public static class Configmanager
    {
        /// <summary>
        /// 域名对应客户Id或者Url
        /// </summary>
        private static Dictionary<string, string> DomainToClientOrUrl = new Dictionary<string, string>();

        /// <summary>
        /// 根据请求方法名获取服务端Id
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string GetClientIdFromConfig(string method)
        {
            var items = method.Split('.');
            var domainName = items.Length >= 2 ? "JsonRpcServiceClientId." + items[0] : null;
            if (!string.IsNullOrWhiteSpace(domainName))
            {
                var clientId = DomainToClientOrUrl[domainName];
                if (!string.IsNullOrWhiteSpace(clientId))
                    return clientId;
            }
            return DomainToClientOrUrl["JsonRpcServiceClientId"];
        }

        /// <summary>
        /// 根据请求方法名获取服务端Url
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string GetServiceUrlFromConfig(string method)
        {
            var items = method.Split('.');
            var domainName = items.Length >= 2 ? "JsonRpcServiceUrl." + items[0] : null;
            if (!string.IsNullOrWhiteSpace(domainName))
            {
                var clientId = DomainToClientOrUrl[domainName];
                if (!string.IsNullOrWhiteSpace(clientId))
                    return clientId;
            }
            return DomainToClientOrUrl["JsonRpcServiceUrl"];
        }

        /// <summary>
        /// 获取节点信息
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="keyName">key名称</param>
        /// <returns></returns>
        private static Dictionary<string, string> GetNodeFromConfig(string nodeName, string keyName = "")
        {
            string jsonRpcConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jsonrpc.config");
            Dictionary<string, string> nodes = new Dictionary<string, string>();

            if (!File.Exists(jsonRpcConfigPath))
                return nodes;

            XmlDocument jsonRpcDoc = new XmlDocument();
            jsonRpcDoc.Load(jsonRpcConfigPath);
            foreach (XmlNode node in jsonRpcDoc["jsonrpc"][nodeName].ChildNodes)
            {
                if (!node.Name.Equals("add", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var key = node.Attributes["key"]?.Value;
                var val = node.Attributes["value"]?.Value;
                nodes.Add(key, val);

                if (!string.IsNullOrWhiteSpace(keyName) && keyName.Equals(key))
                    return nodes;
            }
            return nodes;
        }

        /// <summary>
        /// 配置管理器构造器
        /// </summary>
        static Configmanager()
        {
            DomainToClientOrUrl = GetNodeFromConfig("handlers");
        }
    }
}
