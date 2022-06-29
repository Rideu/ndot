using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ndot
{
    public static class Helper
    {

        public static string GetExceptionRecur(this Exception e)
        {
            var msg = e;
            string buf = msg.Message + " ";
            while (msg != null)
            {
                msg = msg.InnerException;
                if (msg != null)
                    buf += msg.Message + " ";
            }
            return buf;
        }

        public static byte[] Rev(this byte[] src)
        {
            Array.Reverse(src);
            return src;
        }

        static Encoding urf8encoder = Encoding.UTF8; 
        public static byte[] UTF8Encode(this string s) => urf8encoder.GetBytes(s);
        public static string UTF8Decode(this byte[] s) => urf8encoder.GetString(s);


        public static string Print<T>(this ICollection<T> collection, string div)
        {
            if (collection == null || collection.Count == 0)
                return String.Empty;

            string buf = collection.First().ToString();

            for (int i = 1; i < collection.Count; i++)
            {
                var el = collection.ElementAt(i);
                buf += div + el;
            }

            return buf;
        }
    }
}
