﻿using GTA;
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
    [ParupunteConfigAttribute("東海道五十三次", "　現代　")]
    [ParupunteIsono("ひろしげ")]
    //[ParupunteDebug(true)]
    internal class Tokaido53 : ParupunteScript
    {
        private Random random = new Random();
        private HashSet<int> ninjas = new HashSet<int>();
        private List<Ped> pedList = new List<Ped>();
        private SoundPlayer soundPlayerStart;

        public Tokaido53(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            SetUpSound();
        }


        protected override void OnFinished()
        {
            core.PlayerPed.CanRagdoll = true;
            core.PlayerPed.CanSufferCriticalHits = true;
            core.PlayerPed.CanPlayGestures = true;
            core.PlayerPed.CanBeDraggedOutOfVehicle = true;
            core.PlayerPed.BlockPermanentEvents = true;
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15000);
            AddProgressBar(ReduceCounter);

            core.PlayerPed.CanRagdoll = false;
            core.PlayerPed.CanSufferCriticalHits = false;
            core.PlayerPed.CanPlayGestures = false;
            core.PlayerPed.CanBeDraggedOutOfVehicle = false;
            core.PlayerPed.BlockPermanentEvents = false;

            var ptfxName = "core";
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfxName);

            var animDict = "mini@tennis";
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);

            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(core.PlayerPed.Position, 100) || x.IsDead && x.IsAlive))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                StartCoroutine(PlayCoroutine(ped));
            }

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                foreach (var ped in pedList)
                {
                    if (ped.IsSafeExist())
                    {
                        SetAnimRate(ped, 1);
                        Function.Call(Hash.TASK_FORCE_MOTION_STATE, ped, 0xFFF7E7A4, 0);
                    }
                }

                ParupunteEnd();
            });

            soundPlayerStart?.Play();
        }

        protected override void OnUpdate()
        {
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive
                                                           && x.IsInRangeOf(core.PlayerPed.Position, 100) &&
                                                           !ninjas.Contains(x.Handle)))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                StartCoroutine(PlayCoroutine(ped));
            }

            foreach (var tarped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive
                                   && x.IsInRangeOf(core.PlayerPed.Position, 100) &&
                                   !x.IsInRangeOf(core.PlayerPed.Position, 10) &&
                                   !ninjas.Contains(x.Handle)))
            {
                tarped.Task.ClearAllImmediately();
            }
        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped, (double)rate, 0.0, 0.0);
        }

        private IEnumerable<object> PlayCoroutine(Ped ped)
        {
            while (!ReduceCounter.IsCompleted)
            {
                if (!ped.IsSafeExist()) yield break;

                if (random.Next(100) < 10)
                {
                    ped.Quaternion = Quaternion.RotationAxis(ped.UpVector, (float)(random.NextDouble() - 0.5)) * ped.Quaternion;
                }

                var tar = core.PlayerPed.Position - ped.Position;


                SetAnimRate(ped, 5.0f);
                Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, ped, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0,
                    0, 0);
                Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, ped, true);
                var hp = core.PlayerPed.ForwardVector;
                Function.Call(Hash.APPLY_FORCE_TO_ENTITY, ped, hp.X * 1, hp.Y * 1, hp.Z * 1, 0, 0, 0, 1, false,
                    true, true, true, true);
                Function.Call(Hash.TASK_PLAY_ANIM, ped, "mini@tennis", "dive_bh_long_lo", 1.5, -1.5, -1, 9, 0
                , 0, 0, 0);
                ped.ApplyForce(new Vector3(tar.X * 3.0f, tar.Y * 3.0f, tar.Z * 3.0f));
                StartFire(ped);

                yield return null;
            }
        }

        private void StartFire(Ped ped)
        {
            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var scale = 0.5f;

          //  var offset1 = new Vector3(0.2f, 0.0f, 0.0f);
         //   var rotation1 = new Vector3(80.0f, 10.0f, 0.0f);
          //  var scale1 = 20.0f;

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_petrol",
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.IK_Head, scale, 0, 0, 0);
        }


        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("Hirosige.wav"));
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
