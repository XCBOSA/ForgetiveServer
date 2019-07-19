using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Forgetive.Server
{
    public abstract class IForgetiveCommand
    {
        public abstract void OnInit();
        public abstract void OnInvoke(CommandObject[] obj);
        public ForgetiveCommandDescription Info;

        public Player invoker;

        public void InvokeAnotherCommandUseEngine(string command)
        {
            Program.engine.Execute(command);
        }

        public void Print(string str)
        {
            if (invoker == null)
                ForgetiveCommandCenter.Log(Info.CommandNames[0], str);
            else invoker.SendChat(str, "Forgetive");
        }
    }

    public struct ForgetiveCommandDescription
    {
        public string[] CommandNames;
        public string HelpText;
        public bool CanPlayerUsed;
    }

    public static class ForgetiveCommandCenter
    {
        internal static List<IForgetiveCommand> cmds;

        internal static void FindCommands(Assembly[] asses)
        {
            Type basicType = typeof(IForgetiveCommand);
            for (int i = 0; i < asses.Length; i++)
            {
                Assembly current = asses[i];
                Type[] ltype = current.GetTypes();
                for (int j = 0; j < ltype.Length; j++)
                {
                    if (ltype[j].IsSubclassOf(basicType))
                    {
                        IForgetiveCommand command = null;
                        try
                        {
                            command = (IForgetiveCommand)Activator.CreateInstance(ltype[j]);
                        }
                        catch
                        {
                            Logger.WriteLine("尝试初始化指令时出现错误。类型{0}", ltype[j].FullName);
                        }
                        if (command.Info.CommandNames != null && command.Info.HelpText != null)
                        {
                            if (command != null)
                                AddCommand(command);
                        }
                        else
                        {
                            Logger.WriteLine("类型{0}已正确加载，但是它并不包含完整的信息表", ltype[j].FullName);
                        }
                    }
                }
            }
        }

        static void AddCommand(IForgetiveCommand body)
        {
            body.OnInit();
            cmds.Add(body);
        }

        internal static void Log(string cmd, string txt)
        {
            Logger.WriteLine("<{0}> {1}", cmd, txt);
        }

        public static IForgetiveCommand PreInvoke(string cmd, bool playerInvoke = false)
        {
            for (int i = 0; i < cmds.Count; i++)
            {
                for (int j = 0; j < cmds[i].Info.CommandNames.Length; j++)
                {
                    if (cmds[i].Info.CommandNames[j] == cmd)
                    {
                        if (playerInvoke)
                        {
                            if (cmds[i].Info.CanPlayerUsed)
                            {
                                return cmds[i];
                            }
                        }
                        else return cmds[i];
                    }
                }
            }
            return null;
        }
    }
}
