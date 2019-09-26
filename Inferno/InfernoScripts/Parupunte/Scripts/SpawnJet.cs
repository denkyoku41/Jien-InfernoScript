using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("じぇっとついか")]
    //[ParupunteDebug(true)]
    internal class SpawnJet : ParupunteScript
    {
        private string name;

        public SpawnJet(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            name = "カオスダヨ！スターリング集合！";
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

            foreach (var s in WaitForSeconds(0.5f))
            {
                var heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Starling, player.Position.AroundRandom2D(12));

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
