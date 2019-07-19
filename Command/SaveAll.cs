using Forgetive.Database;
using Forgetive.Server.Items;
using Forgetive.Server.TreeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Forgetive.Server.Command
{
    public class SaveAll : IForgetiveCommand
    {
        public SaveAll()
        {
            Info.CommandNames = new string[]
            {
                "save", "saveall"
            };
            Info.HelpText = "保存数据";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            ItemStorage.Save();
            Data.SaveAll();
            TreeGen.instance.WriteTrees();
            Print("保存过程完成。");
        }
    }
}
