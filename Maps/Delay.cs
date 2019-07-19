using System;

namespace Forgetive.Server.Maps
{
    /// <summary>
    /// 表示一个地图时钟驱动的延时程序
    /// </summary>
    public class Delay
    {
        DateTime nextExecuteTime = DateTime.MaxValue;
        MapBase mp;
        bool thisRemoved = false;
        Action Todo;

        /// <summary>
        /// 从指定地图初始化
        /// </summary>
        /// <param name="map">指定地图</param>
        public Delay(MapBase map)
        {
            mp = map;
            mp.delays.Add(this);
        }

        bool newSet = false;

        /// <summary>
        /// 设置延时程序内容，从这个函数调用时开始计时，如果后续仍调用此函数，则延时程序将被新设置的替换
        /// </summary>
        /// <param name="interval">延时执行的时间（毫秒）</param>
        /// <param name="todo">>延时执行的程序</param>
        public void SetDelay(int interval, Action todo = null)
        {
            newSet = true;
            nextExecuteTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, interval);
            if (todo != null)
                Todo = todo;
        }
        
        internal bool Do()
        {
            if (DateTime.Now >= nextExecuteTime)
            {
                newSet = false;
                try
                {
                    if (Todo != null)
                    {
                        Todo.Invoke();
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
                finally
                {
                    if (!newSet)
                    {
                        nextExecuteTime = DateTime.MaxValue;
                        Todo = null;
                    }
                }
            }
            return thisRemoved;
        }

        /// <summary>
        /// 释放资源（注意：这里不是立即释放，是在下一次地图时钟更新时释放）
        /// </summary>
        public void Dispose()
        {
            thisRemoved = true;
        }
    }
}
