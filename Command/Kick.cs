using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Kick : IForgetiveCommand
    {
        public Kick()
        {
            Info.CommandNames = new string[]
            {
                "kick"
            };
            Info.HelpText = "踢出玩家。用法 kick <玩家1> [玩家2] [玩家3] ...";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            lock (ForgetiveServer.Users)
            {
                List<int> deleteId = new List<int>();
                for (int i = 0; i < obj.Length; i++)
                {
                    if (obj[i].GetType() == typeof(string))
                    {
                        for (int j = 0; j < ForgetiveServer.UserCount; j++)
                        {
                            if (ForgetiveServer.Users[j].NickName == obj[i].GetValue<string>())
                            {
                                ForgetiveServer.Users[j].Close();
                                Print("已踢出玩家" + obj[i].GetValue<string>());
                                deleteId.Add(j);
                            }
                        }
                    }
                }
                for (int i = 0; i < deleteId.Count; i++)
                    ForgetiveServer.Users.RemoveAt(i);
            }
        }
    }
}
