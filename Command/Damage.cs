using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Damage : IForgetiveCommand
    {
        public Damage()
        {
            Info.CommandNames = new string[]
            {
                "damage"
            };
            Info.HelpText = "伤害玩家。用法 damage <玩家> <伤害值>";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length != 2)
            {
                Print("伤害玩家。用法 damage <玩家> <伤害值>");
                return;
            }
            if (obj[0].type == Program.engine.STRING && !obj[0].IsNull &&
                obj[1].type == Program.engine.STRING && !obj[1].IsNull)
            {
                string name = (string)obj[0].value;
                int damage;
                if (!int.TryParse((string)obj[1].value, out damage))
                {
                    Print("伤害玩家。用法 damage <玩家> <伤害值>");
                    return;
                }
                for (int i = 0; i < GlobalServer.Maps.Count; i++)
                {
                    Player player = GlobalServer.Maps[i].FindPlayer(name);
                    if (player != null)
                    {
                        player.LastDamageSourceStr = "HIT0";
                        player.Health -= damage;
                        List<string> data = new List<string>();
                        data.Add("damage");
                        data.Add(player.LastDamageSourceStr);
                        data.Add(player.Health.ToString());
                        player.SendDataPackage(data);
                        return;
                    }
                }
                Print("玩家不存在");
            }
        }
    }
}
