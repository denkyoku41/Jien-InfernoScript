using GTA.Math;
using GTA.Native;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("エンジンパワーアップ")]
    //[ParupunteDebug(true)]
    internal class VehicleEnginePowerUp : ParupunteScript
    {
        public VehicleEnginePowerUp(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            var v = core.PlayerPed.CurrentVehicle;
                v.EnginePowerMultiplier = 10000.0f;
                v.EngineTorqueMultiplier =50000.0f;
                v.Health = 300;

             //   v.Speed = 100.0f;
            

            ParupunteEnd();
        }
    }
}
