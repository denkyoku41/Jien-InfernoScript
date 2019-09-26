using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("天候変化：嵐")]
    [ParupunteIsono("てんきあらし")]
    //[ParupunteDebug(true)]
    internal class WeatherStor : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();

        public WeatherStor(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            GTA.World.Weather = Weather.ThunderStorm;
            ParupunteEnd();
        }
    }
}

