using System;
using System.Collections.Generic;
using System.Text;

namespace Maha.Spotted
{
	public interface ILog
	{
		string LogPath { get; set; }
		void Init();
		void WriteInfo(object obj);
		void WriteError(object obj);
	}
}
