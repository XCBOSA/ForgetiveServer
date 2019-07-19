using Forgetive.Server.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace Forgetive.Database
{
    public class Table
    {
        readonly string file;
        public bool IsNewCreated { get; private set; }
        List<string> tempData;
        public bool AutoSave { get; set; }

        FileStream handler;

        string __READ_TO_END()
        {
            handler.Position = 0;
            int fsLen = (int)handler.Length;
            byte[] heByte = new byte[fsLen];
            int r = handler.Read(heByte, 0, heByte.Length);
            string result = Encoding.UTF8.GetString(heByte);
            handler.Position = 0;
            return result;
        }

        void __WRITE_TO_END(string data)
        {
            handler.Position = 0;
            byte[] bytc = Encoding.UTF8.GetBytes(data);
            handler.Write(bytc, 0, bytc.Length);
            handler.Position = 0;
        }

        internal Table(string fullPath)
        {
            AutoSave = true;
            file = fullPath;
            IsNewCreated = false;
            if (!File.Exists(file))
            {
                IsNewCreated = true;
                File.Create(file).Close();
            }
            handler = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            ItemStorage.OpenedFiles.Add(handler);
            IgnoreChangeAndReload();
        }

        public void IgnoreChangeAndReload()
        {
            handler.Position = 0;
            tempData = new List<string>();
            string result = __READ_TO_END();
            if (result != "")
                tempData.AddRange(result.Split('\n'));
        }

        public string XName
        {
            get
            {
                return file.Split('-')[0];
            }
        }

        public string YName
        {
            get
            {
                return file.Split('-')[1];
            }
        }

        /// <summary>
        /// 0809 加入自动修正检测功能
        /// </summary>
        public string[][] List()
        {
            List<string[]> vs = new List<string[]>();
            List<int> deleteIndex = new List<int>();
            lock (tempData)
            {
                for (int i = 0; i < tempData.Count; i++)
                {
                    string[] lg = tempData[i].Split(' ');
                    if (lg.Length != 2)
                    {
                        deleteIndex.Add(i);
                        continue;
                    }
                    vs.Add(new string[2]
                    {
                        Decode(lg[0]).Trim(),
                        Decode(lg[1]).Trim()
                    });
                }
                for (int i = 0; i < deleteIndex.Count; i++)
                {
                    tempData.RemoveAt(deleteIndex[i]);
                }
            }
            return vs.ToArray();
        }

        string Decode(string input)
        {
            char c = (char)65279;
            return HttpUtility.UrlDecode(input, Encoding.UTF8).Replace(c.ToString(), "");
        }

        string Encode(string input)
        {
            return HttpUtility.UrlEncode(input, Encoding.UTF8);
        }

        public string GetFirstYWithX(string x, string Default = null)
        {
            if (x == null) throw new NullReferenceException();
            string[][] vs = List();
            for (int i = 0; i < vs.Length; i++)
            {
                if (vs[i][0].Trim() == x)
                    return vs[i][1].Trim();
            }
            if (Default == null)
                return null;
            else return Default;
        }

        public string GetFirstXWithY(string y, string Default = null)
        {
            if (y == null) throw new NullReferenceException();
            string[][] vs = List();
            for (int i = 0; i < vs.Length; i++)
            {
                if (vs[i][1] == y)
                    return vs[i][0].Trim();
            }
            if (Default == null)
                return null;
            else return Default;
        }

        public bool Exists(string x)
        {
            if (x == null) throw new NullReferenceException();
            string[][] vs = List();
            for (int i = 0; i < vs.Length; i++)
            {
                if (vs[i][0] == x)
                    return true;
            }
            return false;
        }

        public bool DeleteX(string x)
        {
            if (x == null) throw new NullReferenceException();
            for (int i = 0; i < tempData.Count; i++)
            {
                if (Decode(tempData[i].Split(' ')[0]) == x)
                {
                    tempData.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void SetXToY(string x, string y)
        {
            if (x == null || y == null) throw new NullReferenceException();
            bool setd = false;
            for (int i = 0; i < tempData.Count; i++)
            {
                if (Decode(tempData[i].Split(' ')[0]) == x)
                {
                    tempData[i] = (Encode(x) + " " + Encode(y)).Trim();
                    setd = true;
                }
            }
            if (!setd)
                tempData.Add((Encode(x) + " " + Encode(y)).Trim());
        }

        public void Save()
        {
            try
            {
                string rStr = "";
                for (int i = 0; i < tempData.Count; i++)
                {
                    if (i != tempData.Count - 1)
                        rStr += tempData[i].Trim() + "\n";
                    else rStr += tempData[i].Trim();
                }
                __WRITE_TO_END(rStr);
            }
            catch { }
        }
    }
}
