using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.VersionControl
{
    /// <summary>
    /// 服务器和支持的客户端版本信息
    /// </summary>
    public static class ServerVersion
    {
        /// <summary>
        /// 服务器版本
        /// </summary>
        public static readonly Version Version = new Version(0, 27, 35);

        /// <summary>
        /// 允许的客户端版本
        /// </summary>
        public static List<Version> AllowedVersionList;

        /// <summary>
        /// 已知的客户端版本
        /// </summary>
        public static List<Version> ClientVersionList;
        
        internal static List<string> AllowedEXTs;

#if DEBUG
        /// <summary>
        /// 指示当前 Forgetive Server 是否运行在测试模式
        /// </summary>
        public static readonly bool IsRunInDebug = true;
#else
        /// <summary>
        /// 指示当前 Forgetive Server 是否运行在测试模式
        /// </summary>
        public static readonly bool IsRunInDebug = false;
#endif

        internal static void Init()
        {
            ClientVersionList = new List<Version>();
            AllowedVersionList = new List<Version>();
            AllowedEXTs = new List<string>();

            ClientVersionList.Add(new Version(1, 0, 1));
            ClientVersionList.Add(new Version(1, 0, 2));
            ClientVersionList.Add(new Version(1, 0, 3));
            ClientVersionList.Add(new Version(1, 0, 4));
            ClientVersionList.Add(new Version(1, 0, 5));
            ClientVersionList.Add(new Version(1, 0, 6));
            ClientVersionList.Add(new Version(1, 0, 7));
            ClientVersionList.Add(new Version(1, 0, 8));
            ClientVersionList.Add(new Version(1, 0, 9));
            ClientVersionList.Add(new Version(1, 0, 10));
            ClientVersionList.Add(new Version(1, 0, 11));
            ClientVersionList.Add(new Version(1, 0, 12));
            ClientVersionList.Add(new Version(1, 0, 13));
            ClientVersionList.Add(new Version(1, 0, 14));
            ClientVersionList.Add(new Version(1, 0, 15));
            ClientVersionList.Add(new Version(1, 0, 16));
            ClientVersionList.Add(new Version(1, 0, 17));
            ClientVersionList.Add(new Version(1, 0, 18));
            ClientVersionList.Add(new Version(1, 0, 19));
            ClientVersionList.Add(new Version(1, 0, 21));
            ClientVersionList.Add(new Version(1, 0, 22));
            ClientVersionList.Add(new Version(1, 0, 23));
            ClientVersionList.Add(new Version(1, 0, 24));
            ClientVersionList.Add(new Version(1, 0, 25));
            ClientVersionList.Add(new Version(1, 0, 26));
            ClientVersionList.Add(new Version(1, 0, 27));
            ClientVersionList.Add(new Version(1, 0, 28));

            AllowedVersionList.Add(new Version(1, 0, 28));

            AllowedEXTs.Add("INNEREXT");

            Logger.WriteLine(LogLevel.Info, "Forgetive Server {0}", Version);
            Logger.WriteLine(LogLevel.Info, "XCBOSA build");
            Logger.WriteLine(LogLevel.Info, "可识别的Forgetive客户端版本：");
            for (int i = 0; i < ClientVersionList.Count; i++)
                Logger.WriteLine(LogLevel.Info, "    Forgetive v{0}", ClientVersionList[i]);
            Logger.WriteLine(LogLevel.Info, "支持的Forgetive客户端版本：");
            for (int i = 0; i < AllowedVersionList.Count; i++)
                Logger.WriteLine(LogLevel.Info, "    Forgetive v{0}", AllowedVersionList[i]);
        }

        /// <summary>
        /// 检查客户端版本支持
        /// </summary>
        /// <param name="build">版本号</param>
        /// <returns></returns>
        public static bool CheckVersion(int build)
        {
            for (int i = 0; i < AllowedVersionList.Count; i++)
            {
                if (AllowedVersionList[i].Build == build)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取客户端版本信息
        /// </summary>
        /// <param name="build">版本号</param>
        /// <returns></returns>
        public static Version GetVersion(int build)
        {
            for (int i = 0; i < ClientVersionList.Count; i++)
            {
                if (ClientVersionList[i].Build == build)
                    return ClientVersionList[i];
            }
            Logger.WriteLine(LogLevel.Warning, "未识别的版本Build{0}，请联系XCBOSA更新服务端来支持更新版本客户端。", build);
            return new Version(build);
        }
    }
}
