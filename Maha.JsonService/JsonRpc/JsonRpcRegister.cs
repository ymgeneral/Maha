using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Maha.JsonService
{
    /// <summary>
    /// 服务注册器
    /// </summary>
    public class JsonRpcRegister
    {
        /// <summary>
        /// jsonRpc 实例集合
        /// </summary>
        private static readonly List<object> jsonRpcInstances = new List<object>();

        /// <summary>
        /// 加载锁定器
        /// </summary>
        private static readonly object loadLocker = new object();

        /// <summary>
        /// jsonRpc配置文件路径
        /// </summary>
        private static readonly string jsonRpcConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jsonrpc.config");

        /// <summary>
        /// 是否加载完成配置
        /// </summary>
        private static bool isLoadedConfig = false;

        /// <summary>
        /// 锁定加载程序集
        /// </summary>
        public static void LoadFromConfig()
        {
            lock (loadLocker)
            {
                if (isLoadedConfig)
                    return;
                if (File.Exists(jsonRpcConfigPath))
                {
                    DoLoadFromConfig();
                }
                isLoadedConfig = true;
            }
        }

        /// <summary>
        /// 遍历config加载程序集
        /// </summary>
        private static void DoLoadFromConfig()
        {
            XmlDocument jsonRpcDoc = new XmlDocument();
            jsonRpcDoc.Load(jsonRpcConfigPath);

            foreach (XmlNode node in jsonRpcDoc["jsonrpc"]["serviceAssemblies"].ChildNodes)
            {
                if (!node.Name.Equals("add", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                try
                {
                    RegisterAssembly(node);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("注册程序集 " + node.Attributes["assembly"].Value + " 错误并跳过, " + ex.ToString());
                }
                isLoadedConfig = true;
            }
        }

        /// <summary>
        /// 注册程序集
        /// </summary>
        /// <param name="node"></param>
        private static void RegisterAssembly(XmlNode node)
        {
            var assemblyName = node.Attributes["assembly"]?.Value;
            var assemblyNameSpace = node.Attributes["namespace"]?.Value;
            var domain = node.Attributes["domain"]?.Value;
            var methodMode = node.Attributes["methodMode"]?.Value;
            var subdomainTrimChars = node.Attributes["subdomainTrimChars"]?.Value;

            var assem = Assembly.Load(assemblyName);
            var typesWithHandlers = assem.GetTypes().Where(p => p.IsPublic && p.IsClass);
            foreach (Type handler in typesWithHandlers)
            {
                try
                {
                    //在当前程序集中，匹配方法域
                    if (!string.IsNullOrWhiteSpace(assemblyNameSpace) && !string.Equals(handler.Namespace, assemblyNameSpace) && !handler.Namespace.StartsWith(assemblyNameSpace + "."))
                    {
                        continue;
                    }
                    RegisterType(handler, assem, methodMode, domain, subdomainTrimChars);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("注册类型 " + handler.FullName + " 错误并跳过, " + ex.ToString());
                }
            }
        }

        private static void RegisterType(Type handler, Assembly assem, string methodMode, string domain, string subdomainTrimChars = null)
        {

        }
    }
}
