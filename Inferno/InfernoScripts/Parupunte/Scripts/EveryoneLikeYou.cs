using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Media;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("いっぱいちゅき", "よく見たらクソむかつく")]
    [ParupunteIsono("いっぱいちゅき")]
    //[ParupunteDebug(true)]
    class EveryoneLikeYou : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();
        private SoundPlayer soundPlayerStart;

        public EveryoneLikeYou(ParupunteCore core, ParupunteConfigElement config) : base(core, config)
        {
            SetUpSound();
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            core.PlayerPed.IsInvincible = true;
            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                if (core.PlayerPed.IsInVehicle())
                {
                    core.PlayerPed.CurrentVehicle.IsCollisionProof = false;
                    core.PlayerPed.CurrentVehicle.Repair();
                }

                core.PlayerPed.IsInvincible = false;
                ParupunteEnd();
            });

            soundPlayerStart?.Play();
        }

        protected override void OnUpdate()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist()
                                                           && x.IsInRangeOf(playerPos, 30)
                                                           && !entityList.Contains(x)
                                                           && !x.IsCutsceneOnlyPed()))
            {

                entityList.Add(ped);
                StartCoroutine(MoveCoroutine(ped));


            }
            foreach (var veh in core.CachedVehicles.Where(x => x.IsSafeExist()
                                                               && x.IsInRangeOf(playerPos, 60)
                                                               && !entityList.Contains(x)
            ))
            {
                if (!entityList.Contains(veh))
                {
                    entityList.Add(veh);
                    StartCoroutine(MoveCoroutine(veh));
                }

            }
        }

        private IEnumerable<object> MoveCoroutine(Entity entity)
        {
            Random random = new Random();
            while (!ReduceCounter.IsCompleted)
            {
                if (!entity.IsSafeExist()) yield break;
                var playerPos = entity is Ped ? core.PlayerPed.Position + (core.PlayerPed.ForwardVector).Normalized() * 0
                    : core.PlayerPed.Position + (core.PlayerPed.ForwardVector).Normalized() * 10;

                if (core.PlayerPed.IsInVehicle())
                {
                    core.PlayerPed.CurrentVehicle.IsCollisionProof = true;
                    if (core.PlayerPed.CurrentVehicle.IsDamaged)
                    {
                        core.PlayerPed.CurrentVehicle.Repair();
                    }
                }
                if (entity is Ped)
                {
                    var p = entity as Ped;
                    p.IsInvincible = true;
                    if (p.IsDead) yield break;
                    p.SetToRagdoll();
                }


                //プレイヤに向かうベクトル
                var gotoPlayerVector = playerPos - entity.Position;
                gotoPlayerVector.Normalize();

                var mainPower = entity is Ped ? Random.Next(5, 6) : Random.Next(1, 3);
                var upPower = entity is Ped ? Random.Next(0, 2) : Random.Next(1, 5);
                var offset = !entity.IsInRangeOf(playerPos, 10) ? 3 : 0;

                if (entity.IsInRangeOf(playerPos, 5))
                {
                    mainPower = 0;
                    offset = 0;
                    upPower = entity is Ped ? Random.Next(0, 2) : Random.Next(1, 5);
                }


                entity.Quaternion = Quaternion.RotationAxis(new Vector3(random.Next(-100, 100), random.Next(-100, 100), random.Next(-10, 10)), random.Next(-100, 100));

                entity.ApplyForce(gotoPlayerVector * (mainPower + offset) + Vector3.WorldUp * upPower);

                entity.Quaternion = Quaternion.RotationAxis(new Vector3(random.Next(-100, 100), random.Next(-100, 100), random.Next(-10, 10)), random.Next(-100, 100));

                yield return WaitForSeconds(0.05f);
            }

            if (entity is Ped)
            {
                var p = entity as Ped;
                p.IsInvincible = false;
            }
        }

        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("ippaityuki.wav"));
            if (setupWav != null)
            {
                soundPlayerStart = new SoundPlayer(setupWav);
            }
        }

        private string[] LoadWavFiles(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                return new string[0];
            }

            return Directory.GetFiles(targetPath).Where(x => Path.GetExtension(x) == ".wav").ToArray();
        }

    }
}
