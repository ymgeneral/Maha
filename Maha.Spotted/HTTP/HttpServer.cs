using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Maha.Spotted.HTTP
{
	internal class HttpServer
	{
		HttpListener listener;
		public HttpServer()
		{
			listener = new HttpListener();
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
			listener.Start();
			Listen();
		}


		private async void Listen()
		{
			while (true)
			{
				try
				{
					HttpListenerContext context = await listener.GetContextAsync();
				    Task.Factory.StartNew(() => ProcessRequest(context));
				}
				catch (Exception ex)
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
