using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Maps
{
    public class Island : MapBase
    {
        public override void Init()
        {
            MapName = "Island";
            Rect2 hillPoint = new Rect2(0f, 0f, 3000f, 3000f);
            Area hillArea = new Area("Forgetive", hillPoint);
            hillArea.EnablePVP = true;
            hillArea.EnableSelfDamage = true;
            hillArea.EnablePVE = true;
            AddSafeArea(hillArea);
        }
    }
}
