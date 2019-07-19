using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Maps
{
    [Serializable]
    public class Area
    {
        public Rect2 Rect { get; private set; }
        public string Name { get; private set; }
        public string Owner { get; set; }
        public bool EnablePVP { get; set; }
        public bool EnableSelfDamage { get; set; }
        public bool EnablePVE { get; set; }

        public Area(string name, Rect2 areaRect)
        {
            Name = name;
            Rect = areaRect;
        }

        public bool Contains(Vector2 vec)
        {
            return Rect.Contains(vec);
        }

        public string JoinMessage
        {
            get
            {
                string dest = Player.Keyword;
                if (Owner == null)
                    dest += "已进入" + Name;
                else dest += "已进入" + Owner + "的封地" + Name;
                return dest;
            }
        }

        public string LeaveMessage
        {
            get
            {
                string dest = Player.Keyword;
                if (Owner == null)
                    dest += "已离开" + Name;
                else dest += "已离开" + Owner + "的封地" + Name;
                return dest;
            }
        }
    }
}
