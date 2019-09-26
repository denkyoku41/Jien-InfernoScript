using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("大量車両ハッキング")]
    [ParupunteIsono("はっきんぐ")]
    //[ParupunteDebug(true)]
    class MassiveVehicleHack : ParupunteScript
    {
        public MassiveVehicleHack(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        //ハック済み車両
        private List<Vehicle> hacksList = new List<Vehicle>();

        //演出用の線を引くリスト
        private List<System.Tuple<Entity, Entity>> drawLineList = new List<System.Tuple<Entity, Entity>>();

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {

                hacksList = null;
                if (core.PlayerPed.IsInVehicle())
                {
                    core.PlayerPed.CurrentVehicle.IsInvincible = false;
                }
                ParupunteEnd();
            });

            core.OnDrawingTickAsObservable
                .TakeUntil(OnFinishedAsObservable)
                .Subscribe(_ =>
                {
                    foreach (var tuple in drawLineList)
                    {
                        var t1 = tuple.Item1;
                        var t2 = tuple.Item2;
                        if (!t1.IsSafeExist() || !t2.IsSafeExist()) continue;
                        DrawLine(t1.Position, t2.Position, Color.White);
                    }
                });

            if (core.PlayerPed.IsInVehicle())
            {
                hacksList.Add(core.PlayerPed.CurrentVehicle);
                core.PlayerPed.CurrentVehicle.IsInvincible = true;
            }
            StartCoroutine(HackCoroutine(core.PlayerPed));
            StartCoroutine(HackCoroutine(core.PlayerPed));
            StartCoroutine(HackCoroutine(core.PlayerPed));
        }

        IEnumerable<object> HackCoroutine(Entity root)
        {
            if (!root.IsSafeExist()) yield break;

            //プレイヤから離れすぎてたら対象外
            if (!root.IsInRangeOf(core.PlayerPed.Position, 40)) yield break;

            //ターゲットを探す
            var targetsList = core.CachedVehicles
                .Where(x => x.IsSafeExist() && x.IsAlive && x.IsInRangeOf(root.Position, 25))
                .Except(hacksList).ToArray();

            Vehicle target = null;

            if (targetsList.Length == 1)
            {
                target = targetsList.FirstOrDefault();
            }
            else if (targetsList.Length > 1)
            {
                var rootPos = root.Position;
                target = targetsList.Aggregate((p, c) =>
                    p.Position.DistanceTo(rootPos) > c.Position.DistanceTo(rootPos) ? c : p);
            }

            if (target == null) yield break;

            hacksList.Add(target);

            //追加
            drawLineList.Add(new System.Tuple<Entity, Entity>(root, target));
            StartCoroutine(ControlleCoroutine(target));

            //伝播させる
            for (var i = 0; i < 10; i++)
            {
                StartCoroutine(HackCoroutine(target));
                yield return WaitForSeconds(1.0f);
            }
        }

        //車両を暴走させるコルーチン
        IEnumerable<object> ControlleCoroutine(Vehicle target)
        {

            target.EngineRunning = true;

            while (target.IsSafeExist() && target.IsAlive)
            {
                target.CanWheelsBreak = false;
                target.HandbrakeOn = false;
                if (target.IsOnAllWheels)
                {
                    //メイン処理
                    this.OnUpdateAsObservable
                    .Where(_ => core.IsGamePadPressed(GameKey.VehicleAccelerate))
                    .Subscribe(_ =>
                    {
                        var targetPos = (core.PlayerPed.ForwardVector).Normalized();
                        var targetPosition = core.PlayerPed.Position + targetPos * 20;
                        var direction = targetPosition - core.PlayerPed.Position;

                        target.ApplyForce(direction * 0.1f + new Vector3(0, 0, 0));

                    });

                    this.OnUpdateAsObservable
                    .Where(_ => core.IsGamePadPressed(GameKey.VehicleBrake))
                    .Subscribe(_ =>
                    {
                        var targetPos = (core.PlayerPed.ForwardVector).Normalized();
                        var targetPosition = core.PlayerPed.Position + targetPos * 20;
                        var direction = targetPosition - core.PlayerPed.Position;
                        target.ApplyForce(-direction  * 0.4f + new Vector3(0, 0, 0));

                    });

                    this.OnUpdateAsObservable
                       .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                        .Subscribe(_ =>
                        {
                            target.ApplyForce(new Vector3(0, 0, 2));

                        });
                }
                yield return null;
            }
        }

        private void DrawLine(Vector3 from, Vector3 to, Color col)
        {
            Function.Call(Hash.DRAW_LINE, from.X, from.Y, from.Z, to.X, to.Y, to.Z, col.R, col.G, col.B, col.A);
        }
    }
}
