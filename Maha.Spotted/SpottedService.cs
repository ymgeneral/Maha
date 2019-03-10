using Maha.Spotted.Connect;
using Maha.Spotted.HTTP;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maha.Spotted
{
    /// <summary>
    /// 发现服务类
    /// </summary>
    public class SpottedService
    {
        HttpServer httpServer;//http服务
        TCPServer tCPServer;//TCP服务
                            /// <summary>
                            /// 日志接口
                            /// </summary>
        public ILog Log
        {
            get
            {
                if (General.LogManage == null)
                {
                    General.LogManage = new DefaultLog();
                    General.LogManage.Init();
                }
                return General.LogManage;
            }
            set { General.LogManage = value; }
        }
        public SpottedService()
        {
            httpServer = new HttpServer();
            tCPServer = new TCPServer();
            if (General.LogManage == null)
            {
                General.LogManage = new DefaultLog();
                General.LogManage.Init();
            }
        }
        /// <summary>
        /// 开启服务
        /// </summary>
        /// <param name="prefixes"></param>
        public void Start(params string[] prefixes)
        {
            httpServer.Start("http://+:" + General.HttpPort);
            tCPServer.Init(Guid.NewGuid().ToString());
            tCPServer.Start(General.TCPPort, 10000);
            General.LogManage.WriteInfo($"http服务开监听列表:http://+:{ General.HttpPort}");
            General.LogManage.WriteInfo($"tcp服务开监听端口:{ General.TCPPort}");
        }
        /// <summary>
        /// 关闭服务
        /// </summary>
        public void Stop()
        {
            tCPServer.Dispose();
            httpServer.Close();
        }
    }
}
