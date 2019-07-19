using Forgetive.Server.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Extension.Inner
{
    public class AutoSave : ExecutionContent
    {
        DateTime dtTime;

        public override void MainLoop()
        {
            if ((DateTime.Now - dtTime).TotalMinutes > 10)
            {
                dtTime = DateTime.Now;
                InvokeOnMainThread(() => InvokeCommand("save"));
            }
        }
    }
}
