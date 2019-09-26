using GTA;
using System;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    //[ParupunteIsono("てんきそのた")]
    internal class WeatherOthers : ParupunteScript
    {
        private string name;
        private Weather weather;

        public WeatherOthers(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            Random random = new Random();

            weather = Enum.GetValues(typeof(Weather))
                .Cast<Weather>()
                .Where(x => x != Weather.Unknown)
                .OrderBy(x => random.Next())
                .FirstOrDefault();

            var weatherName = GetWeatherName(weather);
            name = "天候変化" + "：" + weatherName;
        }

        public override void OnSetNames()
        {
            Name = name;
        }

        public override void OnStart()
        {
            GTA.World.Weather = weather;
            ParupunteEnd();
        }

        private string GetWeatherName(Weather weather)
        {
            switch (weather)
            {

                case Weather.Smog:
                    return "スモッグ";

                case Weather.Foggy:
                    return "霧";

                case Weather.Neutral:
                    return "奇妙";

                default:
                    return "わからん";
            }
        }
    }
}
