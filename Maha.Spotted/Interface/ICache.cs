using System;
using System.Collections.Generic;
using System.Text;

namespace Maha.Spotted.Interface
{
	public interface ICache
	{
		void Add(string key, object value);
		object GetObject(string key);
		T Get<T>(string key);
		bool Delete(string key);
		bool Clear();
		void Set(string key, object newvalue);
	}
}
