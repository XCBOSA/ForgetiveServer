using Forgetive.Server.Maps.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Command
{
    public class NPCCmd : IForgetiveCommand
    {
        public NPCCmd()
        {
            Info.HelpText = "与可移动NPC相关的操作。用法 npc <list>";
            Info.CommandNames = new string[]
            {
                "npc"
            };
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length < 1)
            {
                Print(Info.HelpText);
                return;
            }
            string command = (string)obj[0].value;
            switch (command)
            {
                case "list":
                    for (int i = 0; i < NPCManager.NPCs.Count; i++)
                        Print(NPCManager.NPCs[i].ToString());
                    break;
                default:
                    Print(Info.HelpText);
                    break;
            }
        }
    }
}
