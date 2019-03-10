using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Maha.Spotted
{
    public delegate bool ControlCtrlDelegate(int CtrlType);
    public class Program
    {
        internal static TimeOutManager<string> ClientNode { get; set; }
        static SpottedService spottedService;
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);

        static ControlCtrlDelegate newDelegate = new ControlCtrlDelegate(HandlerRoutine);

        public static bool HandlerRoutine(int CtrlType)
        {
            switch (CtrlType)
            {
                case 0:
                    if(spottedService!=null)
                    {
                        spottedService.Log.Close();
                        spottedService.Stop();
                    }
                    Console.WriteLine("程序关闭");

                    break;
                case 2:
                    if (spottedService != null)
                    {
                        spottedService.Log.Close();
                        spottedService.Stop();
                    }
                    Console.WriteLine("程序关闭");

                    break;
            }
            return false;
        }
        public static void Main(string[] args)
        {
            bool bRet = SetConsoleCtrlHandler(newDelegate, true);
            ClientNode = new TimeOutManager<string>();
            ClientNode.Start();
            spottedService = new SpottedService();
            try
            {
                spottedService.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
                return;
            }
            string comm = Console.ReadLine();
            while(!comm.ToUpper().Equals("EXIT"))
            {
                Console.WriteLine(comm);
                comm = Console.ReadLine();
            }
            spottedService.Log.Close();
            spottedService.Stop();
            Console.WriteLine("程序关闭");
        }
    }
}
