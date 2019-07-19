using Forgetive.Server.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class CloseFile : IForgetiveCommand
    {
        static bool accept = false;

        public CloseFile()
        {
            Info.CommandNames = new string[]
            {
                "closefile"
            };
            Info.HelpText = "释放内存向导";
        }

        public override void OnInit()
        {

        }

        void Start()
        {
            lock (ItemStorage.OpenedFiles)
            {
                Print("打开的文件 : ");
                long total = 0;
                for (int i = 0; i < ItemStorage.OpenedFiles.Count; i++)
                {
                    total += ItemStorage.OpenedFiles[i].Length;
                    Print(i + "; " + ItemStorage.OpenedFiles[i].Name + "; " + ItemStorage.OpenedFiles[i].Length + "B");
                }
                Print("总计 内存中的" + total + "B");
                Logger.WriteLine(LogLevel.Default, "输入要关闭的文件序号: (序号0-{0}/取消C)", MemoryManager.Length - 1);
                string input = Console.ReadLine();
                if (input.ToUpper() == "C")
                {
                    Logger.WriteLine(LogLevel.Default, "操作已被取消");
                    return;
                }
                if (!int.TryParse(input, out int ic))
                {
                    Logger.WriteLine(LogLevel.Default, "序号错误，操作已被取消");
                    return;
                }
                if (ic >= MemoryManager.Length)
                {
                    Logger.WriteLine(LogLevel.Default, "序号不存在，操作已被取消");
                    return;
                }
                MemoryManager.Close(ic);
                Logger.WriteLine(LogLevel.Default, "操作成功完成");
            }
        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (!accept)
            {
                Logger.WriteLine(LogLevel.Warning, "任何关闭文件的行为都是极其危险的，可能会影响数据存储甚至破坏数据，尤其是不能关闭由Forgetive.Database打开的文件，如果您不知道您要关闭的文件是否源自Forgetive.Database，请中止此操作。");
                Logger.WriteLine(LogLevel.Warning, "要继续操作吗? (继续Y/继续并不再提醒A/取消C)");
                string recv = Console.ReadLine().ToUpper();
                if (recv == "Y")
                {
                    Start();
                }
                else if (recv == "A")
                {
                    accept = true;
                    Start();
                }
                else
                {
                    Logger.WriteLine(LogLevel.Default, "操作已被取消");
                }
            }
            else Start();
        }
    }
}
