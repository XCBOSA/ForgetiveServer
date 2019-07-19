using Forgetive.Server.Maps;
using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class MakeEffect : IForgetiveCommand
    {
        public MakeEffect()
        {
            Info.CommandNames = new string[]
            {
                "effect"
            };
            Info.HelpText = "制造效果。用法 [Map] effect <效果类型ID> <位置(Vector3类型)>";
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
                obj[1].type == typeof(Vector3) && !obj[1].IsNull)
            {
                if (ForgetiveServer.DefaultMap == null)
                {
                    Print("需要先指定操作的地图");
                    return;
                }
                ForgetiveServer.DefaultMap.PlayEffect(int.Parse((string)obj[0].value), (Vector3)obj[1].value);
            }
            else Print(Info.HelpText);
        }
    }
}
