using System;

namespace Forgetive.Server
{
    /// <summary>
    /// 总感觉这么写会出错
    /// TODO: 错误备查
    /// </summary>
    public class KeyGen
    {
        public string Result { get; private set; }

        public bool Gen()
        {
            if (Result != null)
                return false;
            bool isSuccess = false;
            lock (Program.key_device)
            {
                string[][] keys = Program.key_device.List();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i][1] == "{noplayer}")
                    {
                        Program.key_device.SetXToY(keys[i][0], "{using}");
                        Result = keys[i][0];
                        isSuccess = true;
                        break;
                    }
                }
            }
            if (isSuccess)
            {
                Logger.WriteLine("已占用激活码{0}", Result);
            }
            else
            {
                Logger.WriteLine("获取激活码失败，激活码数量不足");
            }
            return isSuccess;
        }
        
        ~KeyGen()
        {
            Dispose();
        }

        /// <summary>
        /// 手动释放，不调用也没关系
        /// </summary>
        public void Dispose()
        {
            if (Result == null)
                return;
            if (Program.key_device.GetFirstYWithX(Result) == "{using}")
            {
                lock (Program.key_device)
                {
                    Program.key_device.SetXToY(Result, "{noplayer}");
                }
                Logger.WriteLine("已释放激活码{0}", Result);
            }
            Result = null;
        }
    }
}
