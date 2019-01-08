using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Maha.Spotted.Interface;
namespace Maha.Spotted.HTTP
{
    public class CommandManager
    {

        private IHttpCommand rPCCommand;
        private IHttpCommand httpCommand;
        private IHttpCommand registerCommand;

        internal IHttpCommand RPCCommand
        {
            get
            {
                return rPCCommand;
            }

            set
            {
                rPCCommand = value;
            }
        }

        internal IHttpCommand HttpCommand
        {
            get
            {
                return httpCommand;
            }

            set
            {
                httpCommand = value;
            }
        }

        internal IHttpCommand RegisterCommand
        {
            get
            {
                return registerCommand;
            }

            set
            {
                registerCommand = value;
            }
        }

        public void Command(HttpListenerContext context)
        {

        }
        public void Command(byte[] buffer)
        {

        }
    }
}
