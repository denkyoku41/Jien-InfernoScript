using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("可視化")]
    [ParupunteIsono("かしか")]
    //[ParupunteDebug(true)]
    internal class VisibleVehicles : ParupunteScript
    {
        public VisibleVehicles(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            SetVehiclesInvisible();
            ParupunteEnd();
        }

        private void SetVehiclesInvisible()
        {
            var radius = 100f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where
                (x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var vehicle in vehicles)
            {
                vehicle.IsVisible = true;
            }

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsVisible = true;
               // player.IsVisible = true;
            }
        }
    }
}

