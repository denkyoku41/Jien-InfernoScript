using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("くるま")]
    //[ParupunteDebug(true)]
    class SpawnVheicle2 : ParupunteScript
    {
        private VehicleHash vehicleHash;
        private String _name;
        public SpawnVheicle2(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {

        }

        public override void OnSetUp()
        {
            vehicleHash = Enum
                 .GetValues(typeof(VehicleHash))
                 .Cast<VehicleHash>()
                 .OrderBy(x => Random.Next())
                 .FirstOrDefault();

            var vehicleGxtEntry = Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, (int)vehicleHash);
            _name = Game.GetGXTEntry(vehicleGxtEntry);
        }

        public override void OnSetNames()
        {
            Name = $"{_name}だッ！";
        }

        public override void OnStart()
        {
            StartCoroutine(SpawnCoroutine());
        }

        private IEnumerable<object> SpawnCoroutine()
        {
            core.PlayerPed.IsInvincible = true;
            var v1 = GTA.World.CreateVehicle(new Model(vehicleHash), core.PlayerPed.Position + new Vector3(0, 0, 30));

            if (v1.IsSafeExist())
            {
                //召喚中はプレイヤ無敵
                v1.MarkAsNoLongerNeeded();
                v1.FreezePosition = false;
                v1.IsCollisionProof = true;
                var p = v1.CreateRandomPedAsDriver();
                if (p.IsSafeExist()) p.MarkAsNoLongerNeeded();
                foreach (var s in WaitForSeconds(3.0f))
                {
                    v1.ApplyForce(Vector3.WorldDown * 50.0f);
                    yield return null;
                }
            }
            foreach (var s in WaitForSeconds(1.5f))
            {
                yield return null;
            }
        　　　　//パルプンテ終了後は車自動修理
                core.PlayerPed.IsInvincible = false;
                v1.IsCollisionProof = false;
                v1.Repair();
                ParupunteEnd();
               yield return null;
        }
    }
}
