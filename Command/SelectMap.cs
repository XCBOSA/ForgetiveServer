using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class SelectMap : IForgetiveCommand
    {
        public SelectMap()
        {
            Info.CommandNames = new string[]
            {
                "cd"
            };
            Info.HelpText = "选定Map，如果有操作需要指定Map，则需要先使用此指令定义默认Map。用法: cd <Map名称>";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length != 1)
            {
                Print(Info.HelpText);
                return;
            }
            string arg = (string)obj[0].value;
            if (GlobalServer.FindMap(arg) == null)
            {
                Print("指定地图不存在");
                return;
            }
            ForgetiveServer.DefaultMapName = arg;
            Logger.WriteLine(LogLevel.Default, "已选择地图 {0}", arg);
        }
    }
}
