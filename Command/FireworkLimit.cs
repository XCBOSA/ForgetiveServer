using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class FireworkLimit : IForgetiveCommand
    {
        public FireworkLimit()
        {
            Info.CommandNames = new string[]
            {
                "firelimit"
            };
            Info.HelpText = "设置烟花燃放上限。用法 give <地图> <上限数量>";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length != 2)
            {
                Print(Info.HelpText);
                return;
            }
            if (obj[0].type == Program.engine.STRING && !obj[0].IsNull &&
                obj[1].type == Program.engine.STRING && !obj[1].IsNull)
            {
                string name = (string)obj[0].value;
                int count;
                if (!int.TryParse((string)obj[1].value, out count))
                {
                    Print(Info.HelpText);
                    return;
                }
                MapBase map = GlobalServer.FindMap(name);
                if (map == null)
                {
                    Print("指定地图不存在");
                    return;
                }
                map.MaxBlockCount = count;
                Print("操作成功");
            }
        }
    }
}
