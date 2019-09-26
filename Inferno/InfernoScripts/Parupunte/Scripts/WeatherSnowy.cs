using System.Collections.Generic;
using System.Linq;
using UniRx;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("天候変化：雪")]
    [ParupunteIsono("てんきゆき")]
   // [ParupunteDebug(true)]
    internal class WeatherSnowy : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();

        public WeatherSnowy(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnStart()
        {
            GTA.World.Weather = Weather.Blizzard;
            ParupunteEnd();
        }
    }
}
