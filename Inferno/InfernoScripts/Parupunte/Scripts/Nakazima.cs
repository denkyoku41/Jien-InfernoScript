
using UniRx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Media;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("N A K A Z I M A", "おわり")]
    [ParupunteIsono("なかじま")]
    //[ParupunteDebug(true)]
    internal class Nakazima : ParupunteScript
    {
        private SoundPlayer soundPlayerStart;

        public Nakazima(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            SetUpSound();
        }
        private HashSet<Entity> entityList = new HashSet<Entity>();


        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(ReduceCounter);
            soundPlayerStart?.Play();

            GameplayCamera.StopShaking();

        }

        protected override void OnUpdate()
        {
            var radius = 20.0f;
            var player = core.PlayerPed;


                foreach (var vec in core.CachedVehicles.Where(
                    x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius)
                    ))
                {
                    vec.Speed = vec.Handle % 10 == 0 ? -50 : 50;
                }

                foreach (var veh in core.CachedVehicles.Where(x => x.IsSafeExist()
                                                       && x.IsInRangeOf(player.Position, 300)
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
            var target = core.PlayerPed.IsInVehicle() ? (Entity)core.PlayerPed.CurrentVehicle : (Entity)core.PlayerPed;
            target.IsInvincible = true;
            

            while (!ReduceCounter.IsCompleted)
            {
                if (!entity.IsSafeExist()) yield break;
                var playerPos = core.PlayerPed.Position;
                var mainPower = 100;
                //プレイヤに向かうベクトル

                    var gotoPlayerVector = (playerPos + new Vector3(0, 0, 10)) - entity.Position;
                    gotoPlayerVector.Normalize();

                    entity.ApplyForce(gotoPlayerVector * mainPower);
                    yield return WaitForSeconds(0.3f);

            }

            
            core.PlayerPed.CurrentVehicle.Repair();

            WaitForSeconds(3.0f);
            target.IsInvincible = false;

        }

        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("Nakazima.wav"));
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
