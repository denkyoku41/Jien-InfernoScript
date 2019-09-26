using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    /// <summary>
    /// プレイヤの近くに飛行機を墜落させる
    /// </summary>
    [ParupunteConfigAttribute("メーデー！メーデー！メーデー！")]
    [ParupunteIsono("めーでー")]
    //[ParupunteDebug(true)]
    internal class Mayday : ParupunteScript
    {
        public Mayday(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(10000);
            AddProgressBar(ReduceCounter);

            StartCoroutine(AirPlaneCoroutine());

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                ParupunteEnd();
            });
        }

        private IEnumerable<object> AirPlaneCoroutine()
        {
            //飛行機生成
            Random random = new Random();

            var model = new Model(VehicleHash.Jet);
            var playerPos = core.PlayerPed.Position + (core.PlayerPed.ForwardVector).Normalized() * 150;

            var plane = GTA.World.CreateVehicle(model, playerPos + new Vector3(0, 0, 100));
            plane.Quaternion = Quaternion.RotationAxis(new Vector3(random.Next(-100, 100), random.Next(-100, 100), random.Next(-10, 10)), random.Next(-100, 100));

            var direction = plane.Position - core.PlayerPed.Position.Around(20);
            direction.Normalize();
            plane.ApplyForce(-direction * 10);
            if (!plane.IsSafeExist()) yield break;
            
            plane.MarkAsNoLongerNeeded();

            //ラマー生成
            var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
            ped.MarkAsNoLongerNeeded();
            ped.Task.ClearAll();

            foreach (var s in WaitForSeconds(5))
            {
                var length = (-core.PlayerPed.Position + plane.Position).Length();
                if (length < 20.0f) break;
                yield return null;
            }

            if (!plane.IsSafeExist() || !ped.IsSafeExist()) yield break;
            plane.EngineHealth = 0;
            plane.EngineRunning = false;

            //飛行機が壊れたら大爆発させる
            foreach (var s in WaitForSeconds(6))
            {
                if (!plane.IsSafeExist()) break;
                if (!plane.IsAlive)
                {
                    foreach (var i in Enumerable.Range(0, 10))
                    {
                        if (!plane.IsSafeExist()) break;
                        var point = plane.Position.Around(10.0f);
                        GTA.World.AddExplosion(point, GTA.ExplosionType.Rocket, 20.0f, 1.5f);
                        yield return WaitForSeconds(0.2f);
                    }
                    break;
                }
                yield return null;
            }
            plane.Delete();
            ParupunteEnd();
        }
    }
}
