using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.IO;
using System.Media;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("もっと！熱くなれよ！", "　北京だってがんばってるんだから！　")]
    [ParupunteIsono("しゅうぞう")]
   // [ParupunteDebug(true)]
    internal class Shuzo : ParupunteScript
    {
        private SoundPlayer soundPlayerStart;


        private bool AffectAllPed = false;

        private List<Ped> targetPeds = new List<Ped>();

        public Shuzo(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            SetUpSound();
        }

        private uint coroutineId = 0;

        public override void OnSetUp()
        {
            var r = new Random();

            //たまに全員に対して発動させる
            AffectAllPed = r.Next(0, 1) <= 2;
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(8000);
            AddProgressBar(ReduceCounter);


            if (AffectAllPed)
            {
                targetPeds = core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive).ToList();
            }
            targetPeds.Add(core.PlayerPed);

            foreach (var ped in targetPeds)
            {
                //コルーチン起動
                coroutineId = StartCoroutine(MagicFireCoroutine(ped));
            }
            soundPlayerStart?.Play();
        }

        protected override void OnFinished()
        {
            StopCoroutine(coroutineId);
           // core.PlayerPed.IsVisible = true;
            //終了時に炎耐性解除
            WaitForSeconds(2.0f);
        }

        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("Shuzo.wav"));
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

        private IEnumerable<object> MagicFireCoroutine(Ped ped)
        {
            var target = core.PlayerPed.IsInVehicle() ? (Entity)core.PlayerPed.CurrentVehicle : (Entity)core.PlayerPed;
            target.IsInvincible = true;
            var ptfxName = "core";

          //  core.PlayerPed.IsVisible = false;

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfxName);

            while (!ReduceCounter.IsCompleted)
            {
                ped.IsFireProof = true;
                StartFire(ped);
                yield return WaitForSeconds(2.5f);
            }

            //まだ炎が残っているのでロスタイム
           // yield return WaitForSeconds(0.0f);

            ParupunteEnd();
            ped.IsFireProof = false;
            target.IsInvincible = false;
        }

        private int StartFire(Ped ped)
        {
            //var player = core.PlayerPed;
            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 2.0f;
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");

            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_petrol_fire",
                    ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale, 0, 0, 0);
        }
    }
}