using System.Collections.Generic;
using System.Linq;
using UniRx;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("天候変化：曇り")]
    [ParupunteIsono("てんきくもり")]
    //[ParupunteDebug(true)]
    internal class WeatherCloudy : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();

        public WeatherCloudy(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnStart()
        {
            GTA.World.Weather = Weather.Foggy;
            ParupunteEnd();
        }
    }
}
