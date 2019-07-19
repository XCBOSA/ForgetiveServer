using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Copys : IForgetiveCommand
    {
        public Copys()
        {
            Info.CommandNames = new string[]
            {
                "copys", "cps"
            };
            Info.HelpText = "将常量拷贝到变量。用法copys <dest(名称)> <src(字符串)>";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length == 2)
            {
                string dest = obj[0].GetValue<string>();
                string src = obj[1].GetValue<string>();
                CommandObject ptrDest = new CommandObject();
                ptrDest.type = Program.engine.STRING;
                ptrDest.value = src;
                ptrDest.IsNull = false;
                ptrDest.ShowName = dest;
                Program.engine.SetObject(ptrDest);
            }
            else
            {
                Print("用法copys <dest(名称)> <src(名称)>");
            }
        }
    }
}
