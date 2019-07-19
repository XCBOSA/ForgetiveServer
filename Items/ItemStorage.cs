using Forgetive.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Forgetive.Server.Items
{
    public static class ItemStorage
    {
        public static List<Item> ServerItems;
        static int currentServerId = 0;
        public static int ItemPerFile = 16384000;

        public static List<FileStream> OpenedFiles;

        public static FileStream __FILE_GETSTREAM(string fullloc)
        {
            for (int i = 0; i < OpenedFiles.Count; i++)
            {
                if (OpenedFiles[i].Name == Path.GetFullPath(fullloc))
                {
                    FileStream stream = OpenedFiles[i];
                    stream.Position = 0;
                    return stream;
                }
            }
            FileStream fs = new FileStream(Path.GetFullPath(fullloc), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            OpenedFiles.Add(fs);
            fs.Position = 0;
            return fs;
        }

        public static void Init()
        {
            ServerItems = new List<Item>();
            DirectoryInfo dir = new DirectoryInfo(Data.RootPath + "/UserItems");
            if (!dir.Exists)
                dir.Create();
            FileInfo[] files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                ItemPage page = ReadPage(files[i].FullName);
                for (int j = 0; j < page.Items.Length; j++)
                {
                    if (page.Items[j].ServerId >= currentServerId)
                    {
                        currentServerId = page.Items[j].ServerId;
                        ServerItems.Add(page.Items[j]);
                    }
                }
                Logger.WriteLine(LogLevel.Info, "加载 ItemStorage {0} mId={1}", files[i].FullName, currentServerId);
            }
            SayUsedState();
        }

        static void SayUsedState()
        {
            double usedPerMax = currentServerId / ItemPerFile;
            Logger.WriteLine(LogLevel.Info, "物品ID占用比 " + usedPerMax.ToString("P"));
            if (usedPerMax >= 60d)
                Logger.WriteLine(LogLevel.Warning, "物品ID占用超过60%，可能已经影响服务器效率，需要优化物品数据");
        }

        public static void Save()
        {
            Logger.WriteLine(LogLevel.Info, "[ItemStorage] 保存中，在保存结束前不要关闭服务器");
            Directory.Delete(Data.RootPath + "/UserItems", true);
            Directory.CreateDirectory(Data.RootPath + "/UserItems");

            ItemPage page = new ItemPage();
            List<Item> aitem = new List<Item>();
            for (int i = 0; i < ServerItems.Count; i++)
            {
                aitem.Add(ServerItems[i]);
                if (aitem.Count >= ItemPerFile || i == ServerItems.Count - 1)
                {
                    page.Items = aitem.ToArray();
                    string root = Path.GetFullPath(Data.RootPath + "/UserItems/items." + ServerItems[i].ServerId + ".bin");
                    Stream file = new FileStream(root, FileMode.Create, FileAccess.Write);
                    BinaryFormatter binFormat = new BinaryFormatter();
                    binFormat.Serialize(file, page);
                    file.Close();
                    file.Dispose();
                    Logger.WriteLine(LogLevel.Info, "存储 {0}", root);
                    page = new ItemPage();
                    aitem = new List<Item>();
                }
            }

            for (int i = 0; i < OpenedFiles.Count; i++)
                OpenedFiles[i].Flush();

            SayUsedState();
            Logger.WriteLine(LogLevel.Info, "[ItemStorage] 保存完成");
        }

        public static ItemPage ReadPage(string pageLocation)
        {
            string root = pageLocation;
            if (File.Exists(root))
            {
                Stream file = new FileStream(root, FileMode.Open, FileAccess.Read);
                BinaryFormatter binFormat = new BinaryFormatter();
                ItemPage item = (ItemPage)binFormat.Deserialize(file);
                file.Close();
                file.Dispose();
                return item;
            }
            else
            {
                return null;
            }
        }

        public static PlayerOwnServerId ReadPlayer(string playerName, ref bool isNewUser)
        {
            try
            {
                string root = Data.RootPath + "/Players";
                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);
                root += "/" + playerName + ".bin";
                if (!File.Exists(root))
                    isNewUser = true;
                Stream file = __FILE_GETSTREAM(root);
                BinaryFormatter binFormat = new BinaryFormatter();
                PlayerOwnServerId llc = (PlayerOwnServerId)binFormat.Deserialize(file);
                return llc;
            }
            catch
            {
                isNewUser = true;
                return new PlayerOwnServerId();
            }
        }

        public static void WritePlayer(string playerName, PlayerOwnServerId playerData)
        {
            try
            {
                string root = Data.RootPath + "/Players/" + playerName + ".bin";
                Stream file = __FILE_GETSTREAM(root);
                BinaryFormatter binFormat = new BinaryFormatter();
                binFormat.Serialize(file, playerData);
            }
            catch
            {
                Logger.WriteLine(LogLevel.Info, "存储玩家 " + playerName + " 的存档时出现问题，但这可能并不严重。如果短时间内多次出现关于此玩家的类似消息，需要强制登出玩家。");
            }
        }

        public static int NewItem(int staticId, string descMsg, string extra)
        {
            Item item = new Item();
            item.ItemStaticId = staticId;
            item.DescribeMessage = descMsg;
            item.ExtraInfo = extra;
            currentServerId += 1;
            item.ServerId = currentServerId;
            ServerItems.Add(item);
            return item.ServerId;
        }

        static int errorTimes = 0;
        const int maxTimes = 100;

        public static Item GetItemDesc(int serverId)
        {
            for (int i = 0; i < ServerItems.Count; i++)
            {
                if (ServerItems[i].ServerId == serverId)
                    return ServerItems[i];
            }
            errorTimes++;
            Logger.WriteLine(LogLevel.Warning,
                "尝试在物品池中寻找ID=" + serverId + "时失败，这可能源于一次保存异常。累计错误 " + errorTimes);
            if (errorTimes == maxTimes)
            {
                Logger.WriteLine(LogLevel.Error, "服务器即将被强制关闭，因为触发的物品异常次数大于最大值。这通常说明服务器的数据出现了严重问题。请询问XCBOSA获取数据修复帮助并修复服务器的数据再重试。");
                Program.engine.Execute("shutdown", true);
                ForgetiveServer.FillThread();
            }
            return null;
        }

        public static void SetChanged(int serverId, int staticId, string descMsg, string extra)
        {
            for (int i = 0; i < ServerItems.Count; i++)
            {
                if (ServerItems[i].ServerId == serverId)
                {
                    ServerItems[i].DescribeMessage = descMsg;
                    ServerItems[i].ExtraInfo = extra;
                    ServerItems[i].ItemStaticId = staticId;
                }
            }
        }
    }
}
