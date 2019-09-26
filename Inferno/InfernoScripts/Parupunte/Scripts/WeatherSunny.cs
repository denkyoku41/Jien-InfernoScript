using System.Collections.Generic;
using System.Linq;
using UniRx;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("天候変化：快晴")]
    [ParupunteIsono("てんきはれ")]
    //[ParupunteDebug(true)]
    internal class WeatherSunny : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();

        public WeatherSunny(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnStart()
        {
            GTA.World.Weather = Weather.ExtraSunny;
            ParupunteEnd();
        }
    }
}
