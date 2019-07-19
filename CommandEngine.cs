using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;

namespace Forgetive.Server
{
    internal class CommandEngine
    {
        internal List<CommandObject> objs;
        internal readonly Type STRING = typeof(string);

        internal CommandEngine()
        {
            objs = new List<CommandObject>();
        }

        internal CommandObject GetObject(string str)
        {
            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i].ShowName == str)
                    return objs[i];
            }
            CommandObject @object = new CommandObject(typeof(string), str);
            @object.ShowName = str;
            return @object;
        }

        public void SetObject(CommandObject obj)
        {
            bool isDefined = false;
            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i].ShowName == obj.ShowName)
                {
                    objs[i] = obj;
                    isDefined = true;
                    break;
                }
            }
            if (!isDefined)
                objs.Add(obj);
        }

        internal CommandObject[] SplitCommand(string str)
        {
            List<CommandObject> strs = new List<CommandObject>();
            string current = "";
            bool thisIsString = false;
            bool isInQuote = false;
            bool isInSkipFlag = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ' ' && !isInQuote)
                {
                    if (thisIsString)
                        strs.Add(new CommandObject(STRING, current));
                    else strs.Add(GetObject(current));
                    thisIsString = false;
                    current = "";
                }
                else if (str[i] == '\"' && !isInSkipFlag)
                {
                    thisIsString = true;
                    isInQuote = !isInQuote;
                }
                else if (str[i] == '\\')
                {
                    isInSkipFlag = true;
                }
                else
                {
                    current += str[i];
                    if (isInSkipFlag)
                        isInSkipFlag = false;
                }
            }
            if (thisIsString)
                strs.Add(new CommandObject(STRING, current));
            else strs.Add(GetObject(current));
            return strs.ToArray();
        }

        internal void Execute(string cmd, bool needRepeatCommand = false)
        {
            string command = "";
            if (needRepeatCommand)
                Logger.WriteLine(LogLevel.Info, "Server> " + cmd);
            CommandObject[] args = new CommandObject[0];
            if (cmd.Contains(" "))
            {
                command = cmd.Substring(0, cmd.IndexOf(' '));
                args = SplitCommand(cmd.Substring(cmd.IndexOf(' ') + 1));
            }
            else command = cmd;
            IForgetiveCommand desc = ForgetiveCommandCenter.PreInvoke(command);
            if (desc == null)
            {
                Logger.WriteLine(LogLevel.Warning, "<{0}> {1}", command, "找不到指令。输入help查看更多帮助");
                return;
            }
            desc.invoker = null;
#if !DEBUG
            try
            {
                desc.OnInvoke(args);
            }
            catch
            {
                Logger.WriteLine(LogLevel.Warning, "<{0}> {1}", command, "执行命令时出现内部错误");
            }
#else
            desc.OnInvoke(args);
#endif
        }

        internal void ExecuteByPlayer(string cmd, Player player)
        {
            string command = "";
            CommandObject[] args = new CommandObject[0];
            if (cmd.Contains(" "))
            {
                command = cmd.Substring(0, cmd.IndexOf(' '));
                args = SplitCommand(cmd.Substring(cmd.IndexOf(' ') + 1));
            }
            else command = cmd;
            IForgetiveCommand desc = ForgetiveCommandCenter.PreInvoke(command, !player.IsOp);
            if (desc == null)
            {
                player.SendChat("找不到指令或没有权限调用该指令。输入@help查看更多帮助", "Forgetive");
                return;
            }
            desc.invoker = player;
            try
            {
                desc.OnInvoke(args);
            }
            catch
            {
                player.SendChat("<" + command + "> 执行命令时出现内部错误", "Forgetive");
            }
        }
    }

    [Serializable]
    public struct CommandObject
    {
        internal Type type;
        public object value;
        internal bool IsNull;

        public string ShowName;

        //public CommandObject()
        //{
        //    IsNull = true;
        //    type = null;
        //    value = null;
        //    ShowName = "unnamed_obj";
        //}

        public CommandObject(Type tp, object obj)
        {
            type = tp;
            value = obj;
            IsNull = false;
            ShowName = "unnamed_obj";
        }

        public new Type GetType()
        {
            return type;
        }

        public T GetValue<T>()
        {
            return (T)value;
        }
    }
}
