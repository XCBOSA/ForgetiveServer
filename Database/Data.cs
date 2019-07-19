using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Forgetive.Database
{
    public static class Data
    {
        public static string RootPath { get; internal set; }

        public static int AutoSaveInterval { get; internal set; }

        static List<Table> created;
        internal static Timer timer;

        internal static void InitData(string dataRoot)
        {
            AutoSaveInterval = 1000;
            RootPath = dataRoot;
            created = new List<Table>();
            timer = new Timer
            {
                Interval = AutoSaveInterval,
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += UpdateFiles;
        }

        public static void SaveAll()
        {
            UpdateFiles(null, null);
        }

        static void UpdateFiles(object sender, ElapsedEventArgs e)
        {
            for (int i = 0; i < created.Count; i++)
            {
                if (created[i].AutoSave)
                    created[i].Save();
            }
        }

        public static Table GetTable(string xName, string yName, bool autoSave = true)
        {
            for (int i = 0; i < created.Count; i++)
            {
                if (created[i].XName == xName && created[i].YName == yName)
                {
                    created[i].AutoSave = autoSave;
                    return created[i];
                }
            }
            string tablePath = RootPath + "/" + xName + "-" + yName;
            Table table = new Table(tablePath);
            table.AutoSave = autoSave;
            created.Add(table);
            return table;
        }
    }
}
