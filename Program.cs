#pragma warning disable CS0649

using Forgetive.Database;
using Forgetive.Server.Items;
using Forgetive.Server.Extension;
using Forgetive.Server.VersionControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Management;
using System.Windows.Forms;
using Forgetive.Server.Maps.NPCs;

namespace Forgetive.Server
{
    class Program
    {
        public static void ShutDown(string reason)
        {
            Logger.WriteLine(reason);
            Data.SaveAll();
            Process.GetCurrentProcess().Kill();
        }

        public static Table key_device, key_banned, key_nick;

        public static Table nick_xcoin, nick_bp, nick_isop, nick_newplayerstep;

        public static ServerResult LogDevice(string machine)
        {
            ServerResult ret = new ServerResult(false, "");
            string key = key_device.GetFirstXWithY(machine);
            if (key == null)
            {
                ret.IsSuccess = false;
                ret.Note = "nomac";
            }
            else if (key_banned.GetFirstYWithX(key) == "{notbanned}")
            {
                for (int i = 0; i < ForgetiveServer.Users.Count; i++)
                {
                    UserSocketState sock = ForgetiveServer.Users[i];
                    if (sock == null)
                    {
                        ForgetiveServer.Users.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (sock.UsedKey == key)
                    {
                        ForgetiveServer.Close(sock, null, false);
                        ForgetiveServer.Users.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                ret.IsSuccess = true;
                ret.InternalEx = key;
                ret.Note = ForgetiveServer.Base64Encode(key_nick.GetFirstYWithX(key));
                Logger.WriteLine("设备ID:{0}已登陆密匙:{1}", machine, key);
            }
            else
            {
                ret.IsSuccess = false;
                ret.Note = "banned";
            }
            return ret;
        }

        public static ServerResult RegDevice(string key, string machine)
        {
            ServerResult ret = new ServerResult(false, "");
            string r = key_device.GetFirstYWithX(key);
            if (r == null)
            {
                ret.IsSuccess = false;
                ret.Note = "nokey";
            }
            else if (r != "{noplayer}" && r != "{using}")
            {
                ret.IsSuccess = false;
                ret.Note = "nokey";
            }
            else
            {
                key_device.SetXToY(key, machine);
                key_banned.SetXToY(key, "{notbanned}");
                ret.IsSuccess = true;
                ret.Note = "succ";
                Logger.WriteLine("设备ID:{0}已绑定密匙:{1}", machine, key);
                Logger.WriteLine("设备ID:{0}已登陆密匙:{1}", machine, key);
            }
            return ret;
        }

        public static CommandEngine engine;
        internal static string sudoExecutionContent;
        static List<Assembly> assemblies;
        internal static List<string> assembliesMd5;
        static DateTime StartTime;
        internal static bool isDebugMode = false;

        const int MinCores = 1;
        const int MinMemory = 1;

        static void CheckHardware()
        {
            bool hasError = false;
            string cpuName = "DefaultProcessor", currentClock = "DefaultClock";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                try
                {
                    cpuName = obj.GetPropertyValue("Name").ToString();
                    currentClock = obj.GetPropertyValue("CurrentClockSpeed").ToString();
                }
                catch
                {
                    continue;
                }
            }
            Logger.WriteLine(LogLevel.Info, "CPU 可用数量 : 1");
            Logger.WriteLine(LogLevel.Info, "CPU0 : {0} 在 {1} MHz", cpuName, currentClock);
            Logger.WriteLine(LogLevel.Info, "核心线程数 {0}", Environment.ProcessorCount);
            double GB = 0.0d;
            ManagementClass cimobject1 = new ManagementClass("Win32_PhysicalMemory");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo1 in moc1)
            {
                GB += ((System.Math.Round(long.Parse(mo1.Properties["Capacity"].Value.ToString()) / 1024 / 1024 / 1024.0d, 1)));
            }
            moc1.Dispose();
            cimobject1.Dispose();
            Logger.WriteLine(LogLevel.Info, "物理内存 : {0}GB", GB);
            if (Environment.ProcessorCount < MinCores)
            {
                Logger.WriteLine(LogLevel.Error, "ForgetiveServer需要拥有至少" + MinCores + "个线程的CPU");
                hasError = true;
            }
            if (GB < MinMemory)
            {
                Logger.WriteLine(LogLevel.Error, "ForgetiveServer需要至少" + MinMemory + "GB物理内存容量");
                hasError = true;
            }
            if (hasError)
            {
                Logger.WriteLine(LogLevel.Warning, "此服务器的硬件无法运行ForgetiveServer，要跳过硬件检查并继续吗 (Y:跳过/N:取消)");
                if (Console.ReadLine().ToUpper() == "Y") return;
                Process.GetCurrentProcess().Kill();
            }
        }

        static void __InitExceptionEventer()
        {
            if (!Directory.Exists("./Exception"))
            {
                Directory.CreateDirectory("./Exception");
            }
            AppDomain.CurrentDomain.UnhandledException += (a, b) =>
            {
                string name = Path.GetFullPath("./Exception/Exception_" + DateTime.Now.ToBinary() + ".log");
                StreamWriter sw = new StreamWriter(name, false, Encoding.UTF8);
                sw.WriteLine("Forgetive Server Exception Report");
                sw.WriteLine("Time : {0}", DateTime.Now.ToString());
                sw.WriteLine("System : {0}", Environment.OSVersion.Platform.ToString());
                if (Environment.Is64BitProcess)
                    sw.WriteLine("Process : 64-bit ForgetiveServer");
                else sw.WriteLine("Process : 32-bit ForgetiveServer\n32-bit process may cause many strange exceptions.");
                sw.WriteLine("--------------------------------------");
                sw.WriteLine("UnhandledException");
                sw.WriteLine("ExceptionObject : {0}", b.ExceptionObject.ToString());
                sw.WriteLine("IsTerminating : {0}", b.IsTerminating);
                sw.Flush();
                sw.Close();
                Logger.WriteLine(LogLevel.Error, "未处理的UnhandledException，已输出详细信息到文件{0}", name);
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (a, b) =>
            {
                string name = Path.GetFullPath("./Exception/Exception_" + DateTime.Now.ToBinary() + ".log");
                StreamWriter sw = new StreamWriter(name, false, Encoding.UTF8);
                sw.WriteLine("Forgetive Server Exception Report");
                sw.WriteLine("Time : {0}", DateTime.Now.ToString());
                sw.WriteLine("System : {0}", Environment.OSVersion.Platform.ToString());
                if (Environment.Is64BitProcess)
                    sw.WriteLine("Process : 64-bit ForgetiveServer");
                else sw.WriteLine("Process : 32-bit ForgetiveServer\n32-bit process may cause many strange exceptions.");
                sw.WriteLine("--------------------------------------");
                sw.WriteLine("ThreadException");
                sw.WriteLine("Exception : {0}", b.Exception.ToString());
                sw.WriteLine("IsTerminating : {0}", false);
                sw.Flush();
                sw.Close();
                Logger.WriteLine(LogLevel.Error, "未处理的ThreadException，已输出详细信息到文件{0}", name);
            };
        }

        static void Main(string[] args)
        {
            __InitExceptionEventer();

            #region 主要环境初始化和硬件检查
            StartTime = DateTime.Now;
            Console.Title = "XCWorld Server - Forgetive";
            ItemStorage.OpenedFiles = new List<FileStream>();
            if (args.Length != 0)
                Data.InitData(args[0]);
            else return;
            Logger.Init();

#if !__LINUX_ARM
            CheckHardware();
#endif

            #endregion

            #region 参数处理
            if (args.Length == 1)
            {

            }
            else if (args.Length == 2)
            {
#if DEBUG
                sudoExecutionContent = args[1];
                isDebugMode = true;
                Logger.WriteLine(LogLevel.Info, "已启用Debug模式");
#else
                Logger.WriteLine(LogLevel.Error, "正式版不允许手动设置权限");
                return;
#endif
            }
            else
            {
                Logger.WriteLine(LogLevel.Error, "仅允许添加一个具有完全权限执行体的名称");
                return;
            }

#if DEBUG
            Logger.WriteLine(LogLevel.Warning, "Forgetive Server SDK版");
            Logger.WriteLine(LogLevel.Warning, "测试版支持对应版本的 Forgetive Developer For Win10 客户端");
#endif
            if (!Environment.Is64BitProcess)
                Logger.WriteLine(LogLevel.Error, "Forgetive Server 运行在32位进程下，进程仅允许最高1.5GB内存，这将导致不可恢复的错误。");
            #endregion

            Logger.WriteLine("Forgetive Server 初始化网络...");

            key_device = Data.GetTable("key", "device");
            key_banned = Data.GetTable("key", "banned");
            key_nick = Data.GetTable("key", "nick");
            nick_xcoin = Data.GetTable("nick", "xcoin");
            nick_bp = Data.GetTable("nick", "bp");
            nick_isop = Data.GetTable("nick", "isop");
            nick_newplayerstep = Data.GetTable("nick", "newplayerstep");

            ForgetiveServer.Init();
            ItemUsage.Init();
            ItemStorage.Init();
            ServerVersion.Init();
            engine = new CommandEngine();

            assemblies = new List<Assembly>();
            assemblies.Add(Assembly.GetAssembly(typeof(Program)));
            assembliesMd5 = new List<string>();
            assembliesMd5.Add("INNEREXT");
            Logger.WriteLine(LogLevel.Info, "加载 Forgetive EXT {0}", "ForgetiveServer [Inner]");

            DirectoryInfo info = new DirectoryInfo(Data.RootPath + "/Extension");
            if (!info.Exists)
                info.Create();
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Extension == ".dll")
                {
                    try
                    {
                        assembliesMd5.Add(Hash(files[i].FullName));
                        Assembly assembly = Assembly.LoadFile(files[i].FullName);
                        assemblies.Add(assembly);
                    }
                    catch
                    {
                        Logger.WriteLine(LogLevel.Warning, "无法加载 {0} : 无效的 Forgetive EXT 文件", files[i].Name);
                        assembliesMd5.RemoveAt(assembliesMd5.Count - 1);
                    }
                    Logger.WriteLine(LogLevel.Info, "加载 Forgetive EXT {0}", files[i].Name);
                }
            }
            ExecutionContent.Load(assemblies.ToArray());

            List<Assembly> ass = new List<Assembly>(assemblies);
            ass.Add(Assembly.GetEntryAssembly());
            GlobalServer.Init(ass.ToArray());
            NPCManager.Init();

            TimeSpan total = DateTime.Now - StartTime;
            Logger.WriteLine(LogLevel.Default, "初始化完成({0}s)，使用help查看指令列表", total.TotalSeconds);

            while (true)
            {
                string cmd = Console.ReadLine();
                engine.Execute(cmd);
            }
        }

        static string Hash(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(LogLevel.Warning, "打开EXT时出现问题");
                Logger.WriteLine(LogLevel.Warning, ex.ToString());
                return "::::ERROR_CANNOT_OPEN_FILE";
            }
        }
    }
}
