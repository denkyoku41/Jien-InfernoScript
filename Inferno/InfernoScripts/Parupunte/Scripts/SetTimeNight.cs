using GTA.Native;
using System;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("やかん")]
    internal class TimeNight : ParupunteScript
    {
        private int hour;
        private string name;

        public TimeNight(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnSetNames()
        {
            Name = name;
        }

        public override void OnSetUp()
        {
            Random random = new Random();
            hour = random.Next(0, 3);
            name = "夜間";
        }

        public override void OnStart()
        {
            var dayTime = GTA.World.CurrentDayTime;
            Function.Call(Hash.SET_CLOCK_TIME, hour, dayTime.Minutes, dayTime.Seconds);
            ParupunteEnd();
        }
    }
}