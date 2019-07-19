using System;
using System.Timers;

namespace Forgetive.Server.Maps
{
    public class SkySystem
    {
        public float Rotation = 0f;
        public float RotateSpeed = 0.5f;
        public float FogLevel = 0.002f;
        public long TimeStamp = 0;
        const int TimeGranularity = 2500;
        const int fastDelay = 100;

        MapBase map;
        Timer timer;
        Delay fastTimer;
        float fogto;

        public SkySystem(MapBase mapInfo)
        {
            timer = new Timer();
            timer.Interval = TimeGranularity;
            timer.AutoReset = true;
            timer.Elapsed += CalcSkyRotation;
            timer.Enabled = true;
            map = mapInfo;
            fogto = FogLevel;
            fastTimer = new Delay(map);
            fastTimer.SetDelay(fastDelay, FastDelay);
        }

        void FastDelay()
        {
            float delta = fastDelay / 5000f;
            if (FogLevel != fogto)
                FogLevel = FogLevel - ((FogLevel - fogto) * delta);
            fastTimer.SetDelay(fastDelay, FastDelay);
        }

        private void CalcSkyRotation(object sender, ElapsedEventArgs e)
        {
            float sec = TimeGranularity / 1000f;
            float tempRotation = Rotation;
            tempRotation += sec * RotateSpeed;
            if (tempRotation >= 360f)
            {
                tempRotation -= 360f;
                Logger.WriteLine("[" + map.MapName + "] 开始新的一天");
            }
            Rotation = tempRotation;
            TimeStamp = DateTime.Now.ToBinary();
            Random rd = new Random();
            int c = rd.Next(0, 10);
            if (c == 1)
            {
                float d = rd.Next(2, 20) / 1000f;
                SetFogLevel(d);
                Logger.WriteLine(LogLevel.Info,
                    "[{0}] 雾等级切换到 {1}",
                    map.MapName, d);
            }
        }

        public void SetFogLevel(float fog)
        {
            fogto = fog;
        }

        public bool IsDay
        {
            get
            {
                return Rotation >= 90f && Rotation < 270f;
            }
        }

        ~SkySystem()
        {
            timer.Enabled = false;
            timer.Dispose();
        }
    }
}
