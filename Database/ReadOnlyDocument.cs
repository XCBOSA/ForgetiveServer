using Forgetive.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Forgetive.Database
{
    public class ReadOnlyDocument
    {
        static string basedir = null;
        public string DocumentLocation { get; private set; }
        public string DocumentText { get; private set; }

        public ReadOnlyDocument(string name, Encoding encoder = null)
        {
            if (basedir == null)
                basedir = Path.GetFullPath(Path.GetDirectoryName(Application.ExecutablePath) + "/Document") + "/";
            DocumentLocation = basedir + name;
            if (encoder == null) encoder = Encoding.Default;
            if (!File.Exists(DocumentLocation))
            {
                DocumentText = "";
                Logger.WriteLine("文档{0}不存在。", name);
                return;
            }
            try
            {
                StreamReader sr = new StreamReader(DocumentLocation, encoder);
                DocumentText = sr.ReadToEnd();
                sr.Close();
            }
            catch
            {
                Logger.WriteLine("读取文档{0}时出现问题。", name);
                DocumentText = "";
            }
        }
    }
}
