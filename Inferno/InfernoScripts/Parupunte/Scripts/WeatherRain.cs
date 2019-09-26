using System.Collections.Generic;
using System.Linq;
using UniRx;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("天候変化：雨")]
    [ParupunteIsono("てんきあめ")]
    //[ParupunteDebug(true)]
    internal class WeatherRain : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();

        public WeatherRain(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnStart()
        {
            GTA.World.Weather = Weather.Raining;
            ParupunteEnd();
        }
    }
}
