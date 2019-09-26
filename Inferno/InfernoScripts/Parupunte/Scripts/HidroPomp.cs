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
    [ParupunteConfigAttribute("ハイドロポンプ", "　おわり　")]
    [ParupunteIsono("はいどろ")]
    //[ParupunteDebug(true)]
    internal class HidroPomp : ParupunteScript
    {
        private Random random = new Random();
        private HashSet<int> ninjas = new HashSet<int>();
        private List<Ped> pedList = new List<Ped>();
        private SoundPlayer soundPlayerStart;

        public HidroPomp(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            SetUpSound();
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15000);
            AddProgressBar(ReduceCounter);

            var ptfxName = "core";

            core.PlayerPed.CanRagdoll = false;
            core.PlayerPed.CanSufferCriticalHits = false;
            core.PlayerPed.CanPlayGestures = false;
            core.PlayerPed.CanBeDraggedOutOfVehicle = false;
            core.PlayerPed.BlockPermanentEvents = false;

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfxName);

            var animDict = "move_crouch_proto";
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);

            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(core.PlayerPed.Position, 100) && x.IsAlive))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                StartCoroutine(HidroCoroutine(ped));
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
                                                           && x.IsInRangeOf(core.PlayerPed.Position, 100)
                                                           &&
                                                           !ninjas.Contains(x.Handle)))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                StartCoroutine(HidroCoroutine(ped));
            }
            foreach (var tarped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive
                                   && x.IsInRangeOf(core.PlayerPed.Position, 100) &&
                                   !x.IsInRangeOf(core.PlayerPed.Position, 5) &&
                                   !ninjas.Contains(x.Handle)))
            {
                tarped.Task.ClearAllImmediately();
            }
        }

        protected override void OnFinished()
        {
            core.PlayerPed.CanRagdoll = true;
            core.PlayerPed.CanSufferCriticalHits = true;
            core.PlayerPed.CanPlayGestures = true;
            core.PlayerPed.CanBeDraggedOutOfVehicle = true;
            core.PlayerPed.BlockPermanentEvents = true;
        }


        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped, (double)rate, 0.0, 0.0);
        }

        private IEnumerable<object> HidroCoroutine(Ped ped)
        {
            while (!ReduceCounter.IsCompleted)
            {
                if (!ped.IsSafeExist()) yield break;

                if (random.Next(100) < 10)
                {
                    ped.Quaternion = Quaternion.RotationAxis(ped.UpVector, (float)(random.NextDouble() - 0.5)) * ped.Quaternion;
                }

                var tar = (core.PlayerPed.Position - ped.Position) * 1.5f;

                SetAnimRate(ped, 5.0f);
                Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, ped, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0,
                    0, 0);
                Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, ped, true);
                var hp = core.PlayerPed.ForwardVector;
                Function.Call(Hash.APPLY_FORCE_TO_ENTITY, ped, hp.X * 1, hp.Y * 1, hp.Z * 1, 0, 0, 0, 1, false,
                    true, true, true, true);
                Function.Call(Hash.TASK_PLAY_ANIM, ped, "move_f@generic", "idle_turn_l_-180", 9.0, -9.0, -1, 0, 0,
                    0, 0, 0);
                ped.ApplyForce(new Vector3(tar.X, tar.Y, tar.Z) * 1.0f);
                StartWater(ped);

                yield return null;
            }
        }

        private void StartWater(Ped ped)
        {
            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, 5.0f);

           Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "exp_water",
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, 1.0f,
                0, 0, 0);

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_extinguisher_water",
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, 20.0f,
                0, 0, 0);
        }


        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("HidroPomp.wav"));
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