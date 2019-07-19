using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Clear : IForgetiveCommand
    {
        public Clear()
        {
            Info.CommandNames = new string[]
            {
                "clear", "cls"
            };
            Info.HelpText = "清空屏幕";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            Console.Clear();
        }
    }
}
