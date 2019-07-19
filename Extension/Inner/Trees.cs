using Forgetive.Server.Maps;
using Forgetive.Server.Math;
using Forgetive.Server.TreeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Extension.Inner
{
    public class Trees : ExecutionContent
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
            handler.Add("damageTree", OnDamageTree);
        }

        void OnDamageTree(Player player, string[] data)
        {
            Vector3 vec3 = new Vector3(data[0]);
            InvokeOnMainThread(() => TreeGen.instance.RemTree(vec3));
        }
    }
}
