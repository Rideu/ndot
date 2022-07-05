using System;
using System.Collections.Generic;

namespace ndot
{
    public static class Args
    {
        public static Dictionary<string, string> ArgsKeysValues = new Dictionary<string, string>();

        static Args()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i][0] == '-')
                {
                    string val = "";
                    if (i + 1 < args.Length && args[i + 1][0] != '-')
                        val = args[i + 1];

                    ArgsKeysValues.Add(args[i], val);
                }
            }
        }
    }
}
