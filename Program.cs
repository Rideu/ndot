using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;

namespace ndot
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Console.Title = "ndot-dbg";
#else
            Console.Title = "ndot";
#endif  
            var imf = NetworkInterface.GetAllNetworkInterfaces()[0].GetIPProperties().DnsAddresses[0].ToString();

             
            DoTConverter dc = new DoTConverter();
            dc.Open();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
