
using System;

namespace ndot
{
    public static class Log
    {

        public static event Action<string> OnLog;

        static object locker = 0;

        public static void LogMsg(string text, bool newline = false, bool timestamp = false)
        {
            //lock (locker)
            {
                var msg = $"{(timestamp ? $"[{DateTime.Now}]: " : "")}{text}{(newline ? "\n" : "")}";
                Console.Write(msg);
                OnLog?.Invoke(msg);
            }
        }

#pragma warning disable CS1066
        static void LogMsg(string text, System.Drawing.Color c, bool newline = false, bool timestamp = false)
        {
            var msg = $"{(timestamp ? $"[{DateTime.Now}]: " : "")}{text}{(newline ? '\n' : ' ')}";
            Console.Write(msg);
            OnLog?.Invoke(msg);
        }

        public static void LogMsg(string text, ConsoleColor c, ConsoleColor b = ConsoleColor.Black, bool newline = false, bool timestamp = false)
        {
            //lock (locker)
            {
                Console.ForegroundColor = c;
                Console.BackgroundColor = b;
                var msg = $"{(timestamp ? $"[{DateTime.Now}]: " : "")}{text}{(newline ? "\n" : "")}";
                Console.Write(msg);
                Console.ResetColor();
                OnLog?.Invoke(msg);
            }
        }

        public static void LogMsgTag(string tag, string text, System.Drawing.Color tagc, System.Drawing.Color textc)
        {
            var msg = $"[{DateTime.Now}]: [{tag}] {text}\n";
            Console.Write(msg);
            OnLog?.Invoke(msg);
        }

        public static void LogMsgTag(string tag, string text, ConsoleColor tagc = ConsoleColor.White, ConsoleColor textc = ConsoleColor.White, ConsoleColor tagb = ConsoleColor.Black, ConsoleColor textb = ConsoleColor.Black, bool newline = true)
        {
            lock (locker)
            {

                //if (tagc != ConsoleColor.White)
                LogMsg(tag, tagc, tagb, false, true);
                LogMsg(" ", false, false);
                //else
                //    LogMsg(tag + " ", false, true);

                //if (textc != ConsoleColor.White)
                LogMsg(text, textc, textb, newline);
                //else
                //    LogMsg(text, true);
            }
        }

        public static void LogMsgAt(string text, int x, int y)
        {

            lock (locker)
            {
                var cu = Console.GetCursorPosition();
                Console.Write(text);
                Console.MoveBufferArea(cu.Left, cu.Top, Console.BufferWidth, 1, x, y);
                Console.SetCursorPosition(cu.Left, cu.Top);
            }
        }
    }
}
