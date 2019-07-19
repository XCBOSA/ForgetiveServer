using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Command
{
    public class tppos : IForgetiveCommand
    {
        public tppos()
        {
            Info.CommandNames = new string[]
            {
                "tppos"
            };
            Info.HelpText = "【玩家指令-权限】 强制转移到指定位置。用法 tppos <location:Vector3>";
        }

        public override void OnInit()
        {
            
        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (invoker == null) return;
            if (obj.Length != 1)
            {
                Print(Info.HelpText);
                return;
            }
            if (!(obj[0].value is Vector3))
            {
                Print(Info.HelpText);
                return;
            }
            Vector3 vec = (Vector3)obj[0].value;
            List<string> data = new List<string>();
            data.Add("updateSelfLocation");
            data.Add(vec.ToXYZ());
            data.Add(new Vector3(0f, 0f, 0f).ToXYZ());
            invoker.SendDataPackage(data);
        }
    }
}
