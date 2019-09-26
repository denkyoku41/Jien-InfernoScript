using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("へりついか")]
    //[ParupunteDebug(true)]
    internal class SpawnHelis : ParupunteScript
    {
        private string name;

        public SpawnHelis(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            name = "ブラックホーク・ダウン";
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

            foreach (var s in WaitForSeconds(1.5f))
            {
                var heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Annihilator, player.Position.AroundRandom2D(25));

                if (heli.IsSafeExist())
                {
                    heli.MarkAsNoLongerNeeded();
                    var ped = heli.CreateRandomPedAsDriver();
                    if (ped.IsSafeExist()) { ped.MarkAsNoLongerNeeded(); }
                }
                yield return null;
            }
            ParupunteEnd();
        }
    }
}
