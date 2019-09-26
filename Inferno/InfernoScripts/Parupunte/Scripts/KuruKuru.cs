using GTA;
using GTA.Math;
using GTA.Native;
using System.Media;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("くるくる", "おわり")]
    [ParupunteIsono("くるくる")]
    //[ParupunteDebug(true)]
    internal class KuruKuru : ParupunteScript
    {
        private IDisposable mainStream;
        private Random random = new Random();
        private HashSet<int> ninjas = new HashSet<int>();
        private List<Ped> pedList = new List<Ped>();
        private SoundPlayer soundPlayerStart;

        public KuruKuru(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            ReduceCounter = new ReduceCounter(10 * 1000);
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            AddProgressBar(ReduceCounter);

            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(core.PlayerPed.Position, 100) && x.IsAlive))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
              //  ped.Task.ClearAllImmediately();
                StartCoroutine(DashCoroutine(ped));
            }

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                var player = core.PlayerPed;
                var targets = core.CachedVehicles
                    .Where(x => x.IsSafeExist()
                                && x.IsInRangeOf(player.Position, 80.0f)
                                && x != player.CurrentVehicle

                    );
                foreach (var veh in targets)
                {
                    veh.Speed = 200;
                }
                ParupunteEnd();
            });

            //TODO 別の場所にHookする
            mainStream =
            DrawingCore.OnDrawingTickAsObservable
                .TakeUntil(ReduceCounter.OnFinishedAsync)
                .Subscribe(_ =>
                {
                    var player = core.PlayerPed;
                    var targets = core.CachedVehicles
                        .Where(x => x.IsSafeExist()
                                    && x.IsInRangeOf(player.Position, 80.0f)
                                    && x != player.CurrentVehicle

                        );
                    var rate = (1.0f - ReduceCounter.Rate);
                    foreach (var veh in targets)
                    {
                        if (!veh.IsSafeExist()) continue;
                        veh.Quaternion = Quaternion.RotationAxis(Vector3.WorldUp, 1.0f * rate) * veh.Quaternion;
                        if (rate > 0.5f)
                        {
                            veh.ApplyForce(Vector3.WorldUp * 2.0f * rate);
                            veh.Speed = 40.0f * 2.0f * (rate - 0.5f);
                        }
                    }
                });
        }

        protected override void OnUpdate()
        {
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive
                                                           && x.IsInRangeOf(core.PlayerPed.Position, 100) &&
                                                           !ninjas.Contains(x.Handle)))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                ped.Task.ClearAllImmediately();
                StartCoroutine(DashCoroutine(ped));
                

            }
        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped, (double)rate, 0.0, 0.0);
        }

        private IEnumerable<object> DashCoroutine(Ped ped)
        {
            if (!core.PlayerPed.IsInVehicle() || !core.PlayerPed.CurrentVehicle.IsSafeExist())
            {
                var animDict = "move_f@generic";
                var animPlay = "idle_turn_l_-180";
                Function.Call(Hash.REQUEST_ANIM_DICT, animDict);

                while (!ReduceCounter.IsCompleted)
                {
                    if (!ped.IsSafeExist()) yield break;
                    // if (ped.IsDead)
                    //{
                    //     GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Rocket, 1.0f, 1.0f);
                    //  yield break;
                    // }

                    if (random.Next(100) < 100)
                    {
                        ped.Quaternion = Quaternion.RotationAxis(ped.Position, (float)(random.NextDouble() - 0.5)) * ped.Quaternion;
                    }

                    var posX = ped.Position.X;
                    var posY = ped.Position.Y;
                    var posZ = ped.Position.Z;
                    var tarX = core.PlayerPed.Position.X - ped.Position.X;
                    var tarY = core.PlayerPed.Position.Y - ped.Position.Y;
                    var tarZ = core.PlayerPed.Position.Z - ped.Position.Z;

                    SetAnimRate(ped, 5.0f);
                    Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, ped, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0,
                        0, 0);
                    Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, ped, true);
                    var hp = core.PlayerPed.Position;
                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY, ped, hp.X * 1, hp.Y * 1, hp.Z * 1, hp.X, hp.Y, hp.Z, 1, false,
                        true, true, true, true);
                    Function.Call(Hash.TASK_PLAY_ANIM, ped, animDict, animPlay, 4.0, -4.0, -1, 0, 0
                    , 0, 0, 0);

                    //  ped.ApplyForce(new Vector3(0, 0, posZ * -8.0f));

                    // StartFire(ped);

                    yield return null;
                }
            }
        }

        protected override void OnFinished()
        {
            mainStream?.Dispose();
        }
    }
}
