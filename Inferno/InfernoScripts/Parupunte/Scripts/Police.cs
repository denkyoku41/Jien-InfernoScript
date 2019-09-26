using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("けいさつ")]
    //[ParupunteDebug(true)]
    internal class Police : ParupunteScript
    {
        private string name;

        public Police(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            name = "もしもし？ポリスメン？";
        }

        public override void OnSetNames()
        {
            Name = name;
        }

        public override void OnStart()
        {
            StartCoroutine(SpawnHeli());
        }

        private IEnumerable<object> SpawnHeli()
        {
            var player = core.PlayerPed;

            foreach (var s in WaitForSeconds(1.0f))
            {
                var heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Police, player.Position.AroundRandom2D(12));

                if (heli.IsSafeExist())
                {
                    heli.MarkAsNoLongerNeeded();
                    var ped = heli.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.Cop01SMY));
                    if (ped.IsSafeExist()) { ped.MarkAsNoLongerNeeded(); }
                }
                yield return null;
            }
            ParupunteEnd();
        }
    }
}
