using GTA;
using GTA.Math;
using GTA.Native;
using System.Media;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("異常気象", "　おわり　")]
    [ParupunteIsono("いじょうきしょう")]
    //[ParupunteDebug(true)]
    internal class AbnormalWeather : ParupunteScript
    {
        private Random random = new Random();
        private int hour;

        public AbnormalWeather(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {

        }




        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(10000);
            AddProgressBar(ReduceCounter);
            StartCoroutine(WeatherCoroutine());

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                Random random = new Random();

                switch (random.Next(0, 1) % 2)
                {
                    case 0:
                        GTA.World.Weather = Weather.Blizzard;
                        break;
                    case 1:
                        break;

                }

                ParupunteEnd();
            });


        }

        private IEnumerable<object> WeatherCoroutine()
        {
           //  hour = 9;

            while (!ReduceCounter.IsCompleted)
            {
                Random random = new Random();
                //     hour = random.Next(0, 23);


                switch (random.Next(0, 3) % 5)
                {
                    case 0:
                        GTA.World.Weather = Weather.ExtraSunny;
                        break;
                    case 1:
                        GTA.World.Weather = Weather.ThunderStorm;
                        break;
                    case 2:
                        GTA.World.Weather = Weather.Overcast;
                        break;

                }

                if (hour == 23)
                {
                    hour = 0;
                }
                else
                {
                    hour = 1 + hour;
                }
                

                var dayTime = GTA.World.CurrentDayTime;
                Function.Call(Hash.SET_CLOCK_TIME, hour, dayTime.Minutes, dayTime.Seconds);

                yield return null;
            }
        }



    }
}