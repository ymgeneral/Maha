using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
namespace Maha.Spotted.HTTP
{
	internal class HttpServer
	{
		HttpListener listener;
		AutoResetEvent shopEvent;
		public HttpServer()
		{
			listener = new HttpListener();
			shopEvent = new AutoResetEvent(false);
		}
		public void Start(params string[] prefixes)
		{
			foreach (string s in prefixes)
			{
                if (!s.EndsWith("/"))
				{
					listener.Prefixes.Add(s + "/");
				}
				else
				{
					listener.Prefixes.Add(s);
				}

			}
			shopEvent.Reset();
			listener.Start();
			Listen();
		}

		public void Close()
		{
			shopEvent.Set();
			listener.Close();
		}
		private async void Listen()
		{
			while (true)
			{
				try
				{
					HttpListenerContext context = await listener.GetContextAsync();
					if (shopEvent.WaitOne(10))
					{
						break;
					}
				    await Task.Factory.StartNew(() => ProcessRequest(context));
				}
				catch  
				{
					break;
				}
			}
			listener.Close();
		}

		private void ProcessRequest(HttpListenerContext context)
		{

		}
	}
}
