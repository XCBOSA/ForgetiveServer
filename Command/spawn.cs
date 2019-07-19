using Forgetive.Server.Maps;
using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class spawn : IForgetiveCommand
    {
        public spawn()
        {
            Info.CommandNames = new string[]
            {
                "spawn"
            };
            Info.HelpText = "【玩家指令】回到出生点";
            Info.CanPlayerUsed = true;
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (invoker == null) return;
            List<string> data = new List<string>();
            data.Add("updateSelfLocation");
            data.Add(invoker.MapContext.RespawnPoint.ToXYZ());
            data.Add(invoker.MapContext.RespawnEuler.ToXYZ());
            invoker.SendDataPackage(data);
        }
    }
}
