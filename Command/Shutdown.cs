using Forgetive.Database;
using Forgetive.Server.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Forgetive.Server.Command
{
    public class ShutDown : IForgetiveCommand
    {
        public ShutDown()
        {
            Info.CommandNames = new string[]
            {
                "shutdown", "exit", "stop"
            };
            Info.HelpText = "保存数据然后安全的关闭服务器";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            ItemStorage.Save();
            Print("正在等待数据库刷新...");
            Data.timer.Enabled = false;
            ForgetiveServer.WaitExecute(1500, () =>
            {
                Print("正在将临时数据写入到文件...");
                Data.SaveAll();
                Print("正在结束进程...");
                Process.GetCurrentProcess().Kill();
            });
        }
    }
}
