using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Help : IForgetiveCommand
    {
        public Help()
        {
            Info.CommandNames = new string[]
            {
                "help"
            };
            Info.HelpText = "显示帮助";
            Info.CanPlayerUsed = true;
        }

        public override void OnInit()
        {
            
        }

        public override void OnInvoke(CommandObject[] obj)
        {
            Print("指令帮助");
            Print("指令调用命令具有如下的基本结构");
            Print("<指令名称> [存储的参数名称] [\"字符串参数\"]");
            Print("可用的指令列表");
            for (int i = 0; i < ForgetiveCommandCenter.cmds.Count; i++)
            {
                IForgetiveCommand desc = ForgetiveCommandCenter.cmds[i];
                for (int j = 0; j < desc.Info.CommandNames.Length; j++)
                {
                    Print(desc.Info.CommandNames[j] + "; " + desc.Info.HelpText);
                }
            }
        }
    }
}
