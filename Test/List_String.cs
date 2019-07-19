using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Forgetive.Server.Test
{
    public class List_String
    {
        List<string> srcList;

        public List_String()
        {
            srcList = new List<string>();
            //Logger.WriteLine("Into .ctor");
            //StackTrace st = new StackTrace();
            //StackFrame[] sfs = st.GetFrames();
            //for (int u = 0; u < sfs.Length; ++u)
            //{
            //    System.Reflection.MethodBase mb = sfs[u].GetMethod();
            //    Logger.WriteLine("[CALL STACK][{0}]: {1}.{2}", u, mb.DeclaringType.FullName, mb.Name);
            //}
            //Logger.WriteLine("End .ctor");
        }

        public void ShowCallStack()
        {
            StackTrace st = new StackTrace();
            StackFrame[] sfs = st.GetFrames();
            for (int u = 0; u < sfs.Length; ++u)
            {
                MethodBase mb = sfs[u].GetMethod();
                Logger.WriteLine("[CALL STACK][{0}]: {1}.{2}", u, mb.DeclaringType.FullName, mb.Name);
            }
        }

        public List_String(List<string> copy)
        {
            srcList = copy;
            //Logger.WriteLine("Into .ctor_copy");
            //StackTrace st = new StackTrace();
            //StackFrame[] sfs = st.GetFrames();
            //for (int u = 0; u < sfs.Length; ++u)
            //{
            //    System.Reflection.MethodBase mb = sfs[u].GetMethod();
            //    Logger.WriteLine("[CALL STACK][{0}]: {1}.{2}", u, mb.DeclaringType.FullName, mb.Name);
            //}
            //Logger.WriteLine("End .ctor_copy");
        }

        public void Add(string str)
        {
            srcList.Add(str);
        }

        public void AddRange(IEnumerable<string> collection)
        {
            srcList.AddRange(collection);
        }

        public void Remove(string str)
        {
            srcList.Remove(str);
        }

        public void RemoveAt(int index)
        {
            srcList.RemoveAt(index);
        }

        public void Clear()
        {
            srcList.Clear();
        }

        public int Count
        {
            get { return srcList.Count; }
        }

        public void RemoveRange(int index, int size)
        {
            srcList.RemoveRange(index, size);
        }

        public string GetId(int i)
        {
            return srcList[i];
        }

        public List<string> GetList()
        {
            return srcList;
        }
    }
}
