using GTA;
using System;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    //[ParupunteIsono("てんきあめ")]
    internal class WeatherRainy : ParupunteScript
    {
        private string name;
        private Weather weather;

        public WeatherRainy(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
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
                case Weather.Raining:
                    return "雨";

                case Weather.ThunderStorm:
                    return "嵐";

                case Weather.Clearing:
                    return "天気雨";

                default:
                    return "雨";
            }
        }
    }
}