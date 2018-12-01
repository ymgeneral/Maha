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
		HttpServer httpServer;
		/// <summary>
		/// 日志接口
		/// </summary>
		public ILog Log
		{
			get
			{
				if (General.LogManage == null)
					General.LogManage = new DefaultLog();
				return General.LogManage;
			}
			set { General.LogManage = value; }
		}
		public SpottedService()
		{
			httpServer = new HttpServer();
		}
		/// <summary>
		/// 开启服务
		/// </summary>
		/// <param name="prefixes"></param>
		public void Start(params string[] prefixes)
		{
			httpServer.Start(prefixes);
		}
		/// <summary>
		/// 关闭服务
		/// </summary>
		public void Stop()
		{

		}
	}
}
