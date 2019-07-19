using Forgetive.Server.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class StorageInfo : IForgetiveCommand
    {
        public StorageInfo()
        {
            Info.CommandNames = new string[]
            {
                "storageinfo", "fileinfo"
            };
            Info.HelpText = "显示当前服务器实例已打开的文件";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            Print("打开的文件 : ");
            long total = 0;
            for (int i = 0; i < ItemStorage.OpenedFiles.Count; i++)
            {
                total += ItemStorage.OpenedFiles[i].Length;
                Print(i + "; " + ItemStorage.OpenedFiles[i].Name + "; " + ItemStorage.OpenedFiles[i].Length + "B");
            }
            Print("总计 内存中的" + total + "B");
        }
    }
}
