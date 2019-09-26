using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    //[ParupunteIsono("くうちゅうよが")]
    //[ParupunteDebug(true)]
    internal class AirYoga : ParupunteScript
    {
        private string name;
        private readonly List<VehicleSeat> vehicleSeat = new List<VehicleSeat> { VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear };

        public AirYoga(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            name = "空中ヨガ";
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

            foreach (var s in WaitForSeconds(4.0f))
            {
                var heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Annihilator, player.Position.AroundRandom2D(50));

                if (heli.IsSafeExist())
                {
                    heli.MarkAsNoLongerNeeded();
                    heli.IsVisible = false;
                    heli.MaxHealth = 3000;
                    heli.Health = 3000;
                    foreach (var seat in vehicleSeat)
                    {
                        var ped = heli.CreateRandomPedAsDriver();
                        ped.IsVisible = true;
                        if (ped.IsSafeExist()) { ped.MarkAsNoLongerNeeded(); }
                    }

                }
                yield return null;
            }


            ParupunteEnd();
        }
    }
}

