using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Extension.Inner
{
    public class Vehicle : ExecutionContent
    {
        ClientCommand handler;

        public override void Init()
        {
            GetSudo();
            if (!Sudo)
            {
                SudoContent sudoContent = new SudoContent(this);
                sudoContent.GetExtHash();
                throw new CannotLoadExecutionContentException();
            }
            handler = GetClientCommandHandler();
        }
    }
}
