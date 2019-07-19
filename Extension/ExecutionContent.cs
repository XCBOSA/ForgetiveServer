using Forgetive.Server.VersionControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;

namespace Forgetive.Server.Extension
{
    public class Key<T0, T1>
    {
        public Key(T0 t0, T1 t1)
        {
            Value0 = t0;
            Value1 = t1;
        }

        public T0 Value0;
        public T1 Value1;
    }

    /// <summary>
    /// 在Init中抛出这个异常允许友好的不初始化这个执行体，而不是输出错误细节
    /// </summary>
    public sealed class CannotLoadExecutionContentException : Exception
    {
        /// <summary>
        /// 在Init中抛出这个异常允许友好的不初始化这个执行体，而不是输出错误细节
        /// </summary>
        public CannotLoadExecutionContentException() { }
    }

    /// <summary>
    /// 执行体交互状态
    /// </summary>
    public enum MutualExecuteContentState
    {
        /// <summary>
        /// 没有定义执行体交互初始化函数
        /// </summary>
        Undef = 0,
        /// <summary>
        /// 没有加载到指定执行体交互初始化函数
        /// </summary>
        Unknown = 1,
        /// <summary>
        /// 定义了执行体交互初始化函数并且已经加载完成
        /// </summary>
        Inited = 2
    }

    /// <summary>
    /// 前台执行体
    /// </summary>
    public abstract class ExecutionContent
    {
        public ExecutionContent()
        {
            MutualState = MutualExecuteContentState.Unknown;
            NeedCallEveryOneTick = false;
            Sudo = false;
        }

        /// <summary>
        /// 初始化此执行体，不应包括与其它执行体的交互
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// 执行体交互状态
        /// </summary>
        public MutualExecuteContentState MutualState { get; private set; }

        bool useLoop = true;

        /// <summary>
        /// 执行体的Ext文件名称
        /// </summary>
        public string ExtName { get; private set; }

        bool canSudo = false;

        /// <summary>
        /// 是否具有Sudo权限
        /// </summary>
        public bool Sudo { get; private set; }

        /// <summary>
        /// 初始化执行体与其它执行体的交互
        /// </summary>
        public virtual void InitMutual()
        {
            MutualState = MutualExecuteContentState.Undef;
        }

        /// <summary>
        /// 执行体入口点
        /// </summary>
        public virtual void Main() { }

        /// <summary>
        /// 在执行体线程上的循环入口点，注意引用非线程安全类型时使用 <see cref="InvokeOnMainThread(Action)"/>
        /// </summary>
        public virtual void MainLoop()
        {
            useLoop = false;
        }

        /// <summary>
        /// 是否需要在每一次 Tick 时主线程调用 Main 函数
        /// </summary>
        public bool NeedCallEveryOneTick { get; set; }

        /// <summary>
        /// 执行体的完全限定名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取执行体
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ExecutionContent[] GetExecutionContent(string name)
        {
            List<ExecutionContent> con = new List<ExecutionContent>();
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i].Name == name)
                    con.Add(contents[i]);
            }
            return con.ToArray();
        }

        /// <summary>
        /// 获取执行体列表，需要Sudo权限
        /// </summary>
        /// <returns></returns>
        public ExecutionContent[] GetExecutionContents()
        {
            if (!Sudo)
                return null;
            return contents.ToArray();
        }

        /// <summary>
        /// 获取客户端命令执行器，需要Sudo权限
        /// </summary>
        /// <returns></returns>
        public ClientCommand GetClientCommandHandler()
        {
            if (!Sudo)
                return null;
            return clicmd;
        }

        static List<ExecutionContent> contents;
        static DateTime firstTicker, lastTicker;
        static List<Key<Action, ExecutionContent>> mainThreadActions;
        static internal ClientCommand clicmd;

        internal string mdHash = "";

        internal static void Load(Assembly[] assembly)
        {
            clicmd = new ClientCommand();
            contents = new List<ExecutionContent>();
            mainThreadActions = new List<Key<Action, ExecutionContent>>();

            for (int i = 0; i < assembly.Length; i++)
            {
                Type[] types = assembly[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    if (types[j].IsSubclassOf(typeof(ExecutionContent)))
                    {
                        ExecutionContent content = (ExecutionContent)Activator.CreateInstance(types[j]);
                        content.Name = types[j].FullName;
                        content.ExtName = Path.GetFileName(assembly[i].Location);
                        content.mdHash = Program.assembliesMd5[i];
                        if (content.Name == Program.sudoExecutionContent)
                            content.canSudo = true;
                        else
                        {
                            for (int k = 0; k < ServerVersion.AllowedEXTs.Count; k++)
                            {
                                if (Program.assembliesMd5[i] == ServerVersion.AllowedEXTs[k])
                                {
                                    content.canSudo = true;
                                    break;
                                }
                            }
                        }
                        contents.Add(content);
                    }
                }
            }

            for (int i = 0; i < contents.Count; i++)
            {
                try
                {
                    contents[i].Init();
                    contents[i].canSudo = false;
                }
                catch (Exception e)
                {
                    if (e is CannotLoadExecutionContentException)
                    {
                        Logger.WriteLine(LogLevel.Default, "已禁用执行体 {0}", contents[i].Name);
                    }
                    else
                    {
                        Logger.WriteLine(LogLevel.Warning, "在初始化执行体 {0} 时引发异常，需要禁用这个执行体", contents[i].Name);
                        Logger.WriteLine(LogLevel.Warning, e.ToString());
                    }
                    contents.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < contents.Count; i++)
            {
                try
                {
                    contents[i].InitMutual();
                    if (contents[i].MutualState != MutualExecuteContentState.Undef)
                        contents[i].MutualState = MutualExecuteContentState.Inited;
                }
                catch (Exception e)
                {
                    Logger.WriteLine(LogLevel.Warning, "在初始化执行体交互 {0} 时引发异常", contents[i].Name);
                    Logger.WriteLine(LogLevel.Warning, e.ToString());
                }
            }

            for (int i = 0; i < contents.Count; i++)
            {
                ExecutionContent execution = contents[i];
                Thread thread = new Thread(() =>
                {
                    Exception exception = null;
                    while (true)
                    {
                        try
                        {
                            execution.MainLoop();
                            if (!execution.useLoop)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                            break;
                        }
                        Thread.Sleep(33);
                    }
                    if (exception == null)
                        return;
                    Logger.WriteLine(LogLevel.Warning, "在执行体 {0} 执行 MainLoop 时中断", execution.Name);
                    Logger.WriteLine(LogLevel.Warning, exception.ToString());
                });
                thread.IsBackground = true;
                thread.Start();
            }

            firstTicker = DateTime.Now;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 100;
            timer.Elapsed += OnTick;
            timer.Start();
        }

        /// <summary>
        /// 尝试获取Sudo权限，仅在重写 <see cref="Init"/> 函数中使用有效
        /// </summary>
        public void GetSudo()
        {
            if (canSudo)
            {
                if (Sudo == false)
                {
                    Sudo = true;
                    Logger.WriteLine(LogLevel.Default, "{0}({1}) 已获取执行体最高权限", Name, ExtName);
                }
            }
        }

        /// <summary>
        /// 在主线程上调用指定函数方法
        /// </summary>
        /// <param name="action"></param>
        public void InvokeOnMainThread(Action action)
        {
            Key<Action, ExecutionContent> key = new Key<Action, ExecutionContent>(action, this);
            mainThreadActions.Add(key);
        }

        /// <summary>
        /// 调用命令行命令
        /// </summary>
        /// <param name="cmd"></param>
        public void InvokeCommand(string cmd)
        {
            Program.engine.Execute(cmd, true);
        }

        static void OnTick(object sender, ElapsedEventArgs e)
        {
            lastTicker = DateTime.Now;
            if ((lastTicker - firstTicker).TotalSeconds > 1)
            {
                firstTicker = lastTicker;
                for (int i = 0; i < contents.Count; i++)
                {
                    if (contents[i].NeedCallEveryOneTick)
                    {
                        try
                        {
                            contents[i].Main();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLine(LogLevel.Warning, "在执行执行体 {0} 时引发异常", contents[i].Name);
                            Logger.WriteLine(LogLevel.Warning, ex.ToString());
                        }
                    }
                }
            }

            for (int i = 0; i < mainThreadActions.Count; i++)
            {
                Key<Action, ExecutionContent> key = null;
                try
                {
                    key = mainThreadActions[i];
                    mainThreadActions.RemoveAt(i);
                    i--;
                    key.Value0?.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(LogLevel.Warning, "在执行来自 {0} 的主线程调用时引发异常", key.Value1.Name);
                    Logger.WriteLine(LogLevel.Warning, ex.ToString());
                }
            }
        }
    }
}
