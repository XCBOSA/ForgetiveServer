using Forgetive.Server.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Forgetive.Server
{
    /// <summary>
    /// 提供对文件到内存流的加载支持，Forgetive建议使用此类进行所有文件读写操作，仅用于主线程
    /// </summary>
    public static class MemoryManager
    {
        /// <summary>
        /// 加载文件到内存并返回操作流，如果文件已经加载，将从内存中获取该文件操作流
        /// </summary>
        /// <param name="fullloc">文件地址</param>
        /// <returns></returns>
        public static FileStream LoadFile(string fullloc)
        {
            return ItemStorage.__FILE_GETSTREAM(fullloc);
        }

        /// <summary>
        /// 获取指定位置的流
        /// </summary>
        /// <param name="index">流位置</param>
        /// <returns></returns>
        public static FileStream Get(int index)
        {
            return ItemStorage.OpenedFiles[index];
        }

        /// <summary>
        /// 获取已加载文件的总数
        /// </summary>
        public static int Length
        {
            get => ItemStorage.OpenedFiles.Count;
        }

        /// <summary>
        /// 关闭指定流
        /// </summary>
        /// <param name="fs">指定流</param>
        public static void Close(int index)
        {
            FileStream fs = Get(index);
            fs.Flush();
            fs.Close();
            ItemStorage.OpenedFiles.RemoveAt(index);
        }
    }
}
