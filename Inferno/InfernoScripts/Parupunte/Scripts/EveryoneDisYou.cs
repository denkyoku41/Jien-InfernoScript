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
    [ParupunteConfigAttribute("さてはアンチだなおめー", "竹書房ゥァア゛ーッ")]
    [ParupunteIsono("あんちだな")]
    //[ParupunteDebug(true)]
    class EveryoneDisYou : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();
        private SoundPlayer soundPlayerStart;

        public EveryoneDisYou(ParupunteCore core, ParupunteConfigElement config) : base(core, config)
        {
            SetUpSound();
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {

                ParupunteEnd();
            });

            soundPlayerStart?.Play();
        }

        protected override void OnUpdate()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist()
                                                           && x.IsInRangeOf(playerPos, 20)
                                                           && !entityList.Contains(x)
                                                           && !x.IsCutsceneOnlyPed()))
            {

                entityList.Add(ped);
                StartCoroutine(MoveCoroutine(ped));

            }
            
            foreach (var veh in core.CachedVehicles.Where(x => x.IsSafeExist()
                                                               && x.IsInRangeOf(playerPos, 20)
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

            while (!ReduceCounter.IsCompleted)
            {
                if (!entity.IsSafeExist()) yield break;
                var playerPos = core.PlayerPed.Position;

                if (entity is Ped)
                {
                    var p = entity as Ped;
                    p.Health += 40;
                    p.SetToRagdoll();
                }

                //プレイヤに向かうベクトル
                var gotoPlayerVector = -(playerPos - entity.Position);
                gotoPlayerVector.Normalize();

                var mainPower = 30;

                entity.ApplyForce(gotoPlayerVector * mainPower);

                yield return WaitForSeconds(0.2f);
            }

        }

        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("antidanaome.wav"));
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
