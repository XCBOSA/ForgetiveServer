using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Extension
{
    public sealed class SudoContent
    {
        ExecutionContent cont;

        public SudoContent(ExecutionContent content)
        {
            cont = content;
        }
        
        public string GetExtHash()
        {
            Logger.WriteLine(LogLevel.Info, cont.mdHash);
            return cont.mdHash;
        }
    }
}
