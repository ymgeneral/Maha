using System;
using System.Collections.Generic;
using System.Text;

namespace Maha.Spotted
{
	/// <summary>
	/// 通用工具类
	/// </summary>
	public class General
	{
		public static ILog LogManage { get; set; }
        public static readonly uint TCPPort = 19863;
        public static readonly uint HttpPort = 19862;
	}
}
