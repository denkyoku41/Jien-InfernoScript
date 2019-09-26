using GTA.Native;
using System;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("あさがた")]
    internal class TimeMorning : ParupunteScript
    {
        private int hour;
        private string name;

        public TimeMorning(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnSetNames()
        {
            Name = name;
        }

        public override void OnSetUp()
        {
            Random random = new Random();
            hour = random.Next(6, 8);
            name = "朝方";
        }

        public override void OnStart()
        {
            var dayTime = GTA.World.CurrentDayTime;
            Function.Call(Hash.SET_CLOCK_TIME, hour, dayTime.Minutes, dayTime.Seconds);
            ParupunteEnd();
        }
    }
}
