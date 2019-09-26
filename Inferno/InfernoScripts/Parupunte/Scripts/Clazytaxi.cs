using System;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts;
using Inferno.InfernoScripts.Parupunte;
using UniRx;

namespace Inferno
{
    [ParupunteConfigAttribute("タミフル化", "おわり")]
    [ParupunteIsono("くれたく")]
    //[ParupunteDebug(true)]
    internal class Clazytaxi : ParupunteScript
    {
        public Clazytaxi(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {

        }

        protected override void OnFinished()
        {
            if (core.PlayerPed.IsInVehicle() && core.PlayerPed.CurrentVehicle.IsSafeExist())
            {
                core.PlayerPed.CurrentVehicle.Repair();
            }
            core.PlayerPed.IsInvincible = false;
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(25000);
            AddProgressBar(ReduceCounter);

            var v = core.PlayerPed.CurrentVehicle;
            v.EnginePowerMultiplier = 10000.0f;
            v.EngineTorqueMultiplier = 50000.0f;
            v.Health = 5000;

                if (core.PlayerPed.IsInVehicle() && v.IsSafeExist())
                {


                    //メイン処理
                    this.OnUpdateAsObservable
                    .Where(_ => core.IsGamePadPressed(GameKey.VehicleAccelerate))
                    .Subscribe(_ =>
                    {
                        var targetPos = (core.PlayerPed.ForwardVector).Normalized();
                        var targetPosition = core.PlayerPed.Position + targetPos * 10;
                        var direction = targetPosition - core.PlayerPed.Position;

                        v.ApplyForce(direction + new Vector3(0, 0, -5));

                    });

                    this.OnUpdateAsObservable
                    .Where(_ => core.IsGamePadPressed(GameKey.VehicleBrake))
                    .Subscribe(_ =>
                    {
                        var targetPos = (core.PlayerPed.ForwardVector).Normalized();
                        var targetPosition = core.PlayerPed.Position + targetPos * 10;
                        var direction = targetPosition - core.PlayerPed.Position;
                        v.ApplyForce(-direction * 0.7f + new Vector3(0, 0, -15));

                    });

                this.OnUpdateAsObservable
                   .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                    .Subscribe(_ =>
                    {
                       v.ApplyForce(new Vector3(0, 0, 2));

                    });


            }
                else {

                ParupunteEnd();

                }
               ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
        }


    }
}
