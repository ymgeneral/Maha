using System;
using System.IO;
namespace Maha.Spotted
{
    public class Program
    {
       internal static TimeOutManager<string> ClientNode { get; set; }
        public static void Main(string[] args)
        {
            ClientNode = new TimeOutManager<string>();
            ClientNode.Start();
            Console.WriteLine("OK");
            string comm = Console.ReadLine();
            while(!comm.ToUpper().Equals("EXIT"))
            {
                Console.WriteLine(comm);
                comm = Console.ReadLine();
            }
        }
    }
}
