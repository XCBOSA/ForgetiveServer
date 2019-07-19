using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Vector3Make : IForgetiveCommand
    {
        public Vector3Make()
        {
            Info.CommandNames = new string[]
            {
                "vector3make"
            };
            Info.HelpText = "制造Vector3(单精度)变量。用法 vector3make <变量名> <X> <Y> <Z>";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length != 4)
            {
                Print(Info.HelpText);
                return;
            }
            if (obj[0].type == Program.engine.STRING && !obj[0].IsNull &&
                obj[1].type == Program.engine.STRING && !obj[1].IsNull &&
                obj[2].type == Program.engine.STRING && !obj[1].IsNull &&
                obj[3].type == Program.engine.STRING && !obj[1].IsNull)
            {
                float x = 0f, y = 0f, z = 0f;
                if (!float.TryParse((string)obj[1].value, out x))
                {
                    Print("指定的x,y,z参数必须都是表示单精度数字的字符串");
                    return;
                }
                if (!float.TryParse((string)obj[2].value, out y))
                {
                    Print("指定的x,y,z参数必须都是表示单精度数字的字符串");
                    return;
                }
                if (!float.TryParse((string)obj[3].value, out z))
                {
                    Print("指定的x,y,z参数必须都是表示单精度数字的字符串");
                    return;
                }
                Vector3 vec = new Vector3(x, y, z);
                CommandObject co = new CommandObject();
                co.type = typeof(Vector3);
                co.ShowName = (string)obj[0].value;
                co.IsNull = false;
                co.value = vec;
                Program.engine.SetObject(co);
            }
            else Print(Info.HelpText);
        }
    }
}
