using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Maha.JsonService.Gateway
{
    /// <summary>
    /// 网关工厂
    /// </summary>
    public class GatewayFactory
    {
        //当前网关集
        private static IGateway[] _gateways = null;
        //锁定器
        private static object locker = new object();

        /// <summary>
        /// 从xml节点加载网关
        /// </summary>
        /// <param name="gatewayNode"></param>
        /// <param name="gateways"></param>
        private static void LoadGateway(XmlNode gatewayNode, List<IGateway> gateways)
        {
            //配置前缀必须为add
            if (gatewayNode.Name.Equals("add", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            if (gatewayNode.Attributes != null)
            {
                string gatewayClassName = gatewayNode.Attributes["className"].Value;
                var gatewayType = Type.GetType(gatewayClassName);
                if (gatewayType == null)
                    throw new TypeLoadException(string.Format("JsonRpc网关类型'{0}'加载失败，可能不包含该类", gatewayClassName));
                var instance = (IGateway)Activator.CreateInstance(gatewayType);
                instance.Attributes = new Dictionary<string, string>();
                foreach (XmlAttribute item in gatewayNode.Attributes)
                {
                    if (item.Name.Equals("className")) continue;
                    instance.Attributes[item.Name] = item.Value;
                }
                gateways.Add(instance);
            }
        }

        /// <summary>
        /// 获取本地配置网关（jsonrpc.config）
        /// </summary>
        /// <returns></returns>
        public static IGateway[] GetGateways()
        {
            lock (locker)
            {
                if (_gateways != null) return _gateways;

                List<IGateway> gateways = new List<IGateway>();

                string jsonRpcConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jsonrpc.config");
                if (!File.Exists(jsonRpcConfigPath))
                {
                    return gateways.ToArray();
                }

                XmlDocument jsonrpcDoc = new XmlDocument();
                jsonrpcDoc.Load(jsonRpcConfigPath);
                if (jsonrpcDoc["jsonrpc"] != null && jsonrpcDoc["jsonrpc"]["gateways"] != null)
                {
                    foreach (XmlNode item in jsonrpcDoc["jsonrpc"]["gateways"].ChildNodes)
                    {
                        try
                        {
                            LoadGateway(item, gateways);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                    }
                }
                _gateways = gateways.ToArray();
                return _gateways;
            }
        }
    }
}
