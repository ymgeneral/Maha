using System;
using System.Collections.Generic;
using System.Text;

namespace Maha.Spotted
{
	internal class DefaultLog : ILog
	{

		public void Init(string logPath="")
		{
			if (string.IsNullOrWhiteSpace(logPath))
			{
				logPath = "";
			}
		}

		public void WriteError(object obj)
		{

		}

		public void WriteInfo(object obj)
		{

		}
	}
}
