using Forgetive.Database;
using Forgetive.Server.Extension;
using Forgetive.Server.Items;
using Forgetive.Server.Maps;
using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Forgetive.Server.TreeSystem
{
    public class TreeGen
    {
        readonly string filemap = Data.RootPath + "/Trees/ExportTreeMap";
        readonly string filetrees = Data.RootPath + "/Trees/Trees";
        const int size = 4000;
        const int length = 4048;

        public MapBase Map;
        public static TreeGen instance;
        public bool[,] TreeMap;
        public List<Vector2L> Trees;

        public TreeGen(MapBase map)
        {
            instance = this;
            Map = map;
            Trees = new List<Vector2L>();
            ReadTrees();
        }

        public void Init()
        {
            if (!File.Exists(filemap))
                throw new CannotLoadExecutionContentException();
            Logger.WriteLine(LogLevel.Info, "正在加载ExportTreeMap...");
            StreamReader reader = new StreamReader(filemap);
            string t = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            TreeMap = new bool[length, length];
            int currX = 0;
            for (int i = 0; i < t.Length; i++)
            {
                int c = int.Parse(t[i].ToString());
                TreeMap[currX, i - length * currX] = c > 0;
                if (i - length * currX == length - 1)
                    currX++;
            }
            GC.Collect();
            Logger.WriteLine(LogLevel.Info, "ExportTreeMap加载完成");
            if (Trees.Count == 0)
                GenAll();
        }

        public void GenAll()
        {
            Trees.Clear();
            Logger.WriteLine(LogLevel.Info, "开始生成全地形的树木");
            for (int i = 0; i < TreeMap.GetLength(0); i++)
            {
                for (int j = 0; j < TreeMap.GetLength(1); j++)
                {
                    if (TreeMap[i, j])
                    {
                        Random rd = new Random(GetRandomSeed());
                        if (rd.Next(0, 20) == 5)
                        {
                            Vector2L vec2 = GetRealLocationForDetails(new Vector2Int(i, j));
                            Trees.Add(vec2);
                        }
                    }
                }
            }
            WriteTrees();
            Logger.WriteLine(LogLevel.Info, "全地形树木生成成功");
        }

        int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public void RemTree(Vector3 v3)
        {
            Vector2L tree = new Vector2L(v3.X, v3.Z);
            bool found = false;
            int foundId = 0;
            for (int i = 0; i < Trees.Count; i++)
            {
                if (Trees[i] == tree)
                {
                    foundId = Trees[i].ID;
                    Trees.RemoveAt(i);
                    found = true;
                    break;
                }
            }
            if (!found)
                return;
            List<string> list = new List<string>();
            list.Add("damageTreeCallback");
            list.Add(foundId.ToString());
            Map.SendAll(list);
            for (int i = 0; i < 4; i++)
            {
                Map.thrownOutItems.ThrowNewObject(
                    116, "砍掉的树干", "", v3.ADD(new Vector3(0, i + 3, 0)));
            }
        }

        public void UpdateForPlayer(Player p)
        {
            List<string> list = new List<string>();
            list.Add("addTreeAll");
            for (int i = 0; i < Trees.Count; i++)
                list.Add(Trees[i].ToXY());
            p.SendDataPackage(list);
        }

        Vector2L GetRealLocationForDetails(Vector2Int index)
        {
            float ptz = index.X * size / length;
            float ptx = index.Y * size / length;
            return new Vector2L(ptx, ptz);
        }

        void ReadTrees()
        {
            try
            {
                if (Trees != null)
                    Trees.Clear();
                Trees = new List<Vector2L>();
                if (!File.Exists(filetrees))
                    return;
                Stream file = ItemStorage.__FILE_GETSTREAM(filetrees);
                BinaryFormatter binFormat = new BinaryFormatter();
                Trees = (List<Vector2L>)binFormat.Deserialize(file);
            }
            catch
            {
                throw new CannotLoadExecutionContentException();
            }
        }

        public void WriteTrees()
        {
            try
            {
                Stream file = ItemStorage.__FILE_GETSTREAM(filetrees);
                BinaryFormatter binFormat = new BinaryFormatter();
                binFormat.Serialize(file, Trees);
            }
            catch
            {
                throw new CannotLoadExecutionContentException();
            }
        }
    }
}
