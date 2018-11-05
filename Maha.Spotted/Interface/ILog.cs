using System;
using System.Collections.Generic;
using System.Text;

namespace Maha.Spotted
{
	public interface ILog
	{
		void Init();
		void WriteInfo(object obj);
		void WriteError(object obj);
	}
}
