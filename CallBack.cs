using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server
{
    public class CallBack<T> where T : Delegate
    {
        List<T> reg;

        public CallBack()
        {
            reg = new List<T>();
        }

        public void Reg(T method)
        {
            reg.Add(method);
        }

        public void Del(T method)
        {
            reg.Remove(method);
        }

        internal Exception[] Call(params object[] args)
        {
            List<Exception> excs = new List<Exception>();
            for (int i = 0; i < reg.Count; i++)
            {
                try
                {
                    reg[i].DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    excs.Add(e);
                }
            }
            return excs.ToArray();
        }
    }
}
