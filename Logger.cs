using Forgetive.Database;
using Forgetive.Server.VersionControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Forgetive.Server
{
    /// <summary>
    /// 表示输出内容的等级
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 提示
        /// </summary>
        Info,
        /// <summary>
        /// 正常
        /// </summary>
        Default,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 错误
        /// </summary>
        Error
    }

    internal class LogLine
    {
        internal string str;
        internal LogLevel lvl;
    }

    public static class Logger
    {
        public static bool ShowInfo = true;
        public static bool ShowDefault = true;
        public static bool ShowWarning = true;
        static bool mainThread = false;
        public static bool RunInMainThread
        {
            get
            {
                return mainThread;
            }
            set
            {
                if (ServerVersion.IsRunInDebug)
                {
                    mainThread = value;
                    if (value)
                        WriteLine(LogLevel.Warning, "已强制Logger使用单线程，这将影响服务器执行效率。");
                    else WriteLine(LogLevel.Warning, "已关闭强制Logger使用单线程。");
                }
            }
        }
        static StreamWriter writer;
        static ConsoleColor srcColor;
        static List<LogLine> lines;

        internal static void Init()
        {
            string file = Data.RootPath + "/log.txt";
            writer = new StreamWriter(file, true, Encoding.UTF8);
            srcColor = Console.ForegroundColor;
            lines = new List<LogLine>();
            Thread thread = new Thread(Loop);
            thread.Start();
        }

        static void Loop()
        {
            while (true)
            {
                lock (lines)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (i < 0) continue;
                        if (lines[i] != null)
                            writeInternal(lines[i].lvl, lines[i].str);
                        lines.RemoveAt(i);
                        i--;
                    }
                }
                Thread.Sleep(1);
            }
        }

        static void writeInternal(LogLevel level, string str)
        {
            string timeStr = "[" + DateTime.Now.ToString() + "] ";
            if (Program.isDebugMode)
                timeStr += "[Debug] ";
            string logStr = timeStr;
            switch (level)
            {
                case LogLevel.Default:
                    logStr += "[Default] ";
                    break;
                case LogLevel.Error:
                    logStr += "[Error] ";
                    break;
                case LogLevel.Info:
                    logStr += "[Info] ";
                    break;
                case LogLevel.Warning:
                    logStr += "[Warning] ";
                    break;
            }
            logStr += str;
            writer?.WriteLine(logStr);
            writer?.Flush();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(timeStr);
            string writeStr = str;
            switch (level)
            {
                case LogLevel.Default:
                    if (!ShowDefault) return;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Info:
                    if (!ShowInfo) return;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogLevel.Warning:
                    if (!ShowWarning) return;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }
            Console.WriteLine(writeStr);
            Console.ForegroundColor = srcColor;
        }

        public static void WriteLine(LogLevel level, string str)
        {
            if (mainThread)
            {
                writeInternal(level, str);
            }
            else
            {
                LogLine ll = new LogLine();
                ll.lvl = level;
                ll.str = str;
                lines.Add(ll);
            }
        }

        public static void WriteLine(string str)
        {
            WriteLine(LogLevel.Default, str);
        }

        public static void WriteLine(string str, params object[] cs)
        {
            string c = str;
            for (int i = 0; i < cs.Length; i++)
                c = c.Replace("{" + i + "}", cs[i].ToString());
            WriteLine(c);
        }

        public static void WriteLine(LogLevel level, string str, params object[] cs)
        {
            string c = str;
            for (int i = 0; i < cs.Length; i++)
                c = c.Replace("{" + i + "}", cs[i].ToString());
            WriteLine(level, c);
        }

        public static void LogError(Exception e)
        {
            WriteLine(LogLevel.Error, e.ToString());
        }
    }
}
