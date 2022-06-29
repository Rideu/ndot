using System;
using System.Threading;

namespace ndot
{
    class Program
    {
        static void Main(string[] args)
        {
            DoTConverter dc = new DoTConverter();
            dc.Open();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
