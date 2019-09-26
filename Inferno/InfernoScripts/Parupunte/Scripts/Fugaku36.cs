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
    [ParupunteConfigAttribute("富岳三十六景", "　現　代　")]
    [ParupunteIsono("ほくさい")]
    //[ParupunteDebug(true)]
    internal class Fugaku36 : ParupunteScript
    {
        private Random random = new Random();
        private HashSet<int> ninjas = new HashSet<int>();
        private List<Ped> pedList = new List<Ped>();
        private SoundPlayer soundPlayerStart;
        private readonly string petroEffect = "ent_sht_petrol";

        public Fugaku36(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            SetUpSound();
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15000);
            AddProgressBar(ReduceCounter);

            var ptfxName = "core";

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
                                                           && x.IsInRangeOf(core.PlayerPed.Position, 100)
                                                           &&
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
            var animDict = "move_crouch_proto";
            var animPlay = "sprint_turn_l";
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);

            while (!ReduceCounter.IsCompleted)
            {
                if (!ped.IsSafeExist()) yield break;
                if (ped.IsDead)
                {
                    GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Rocket, 1.0f, 1.0f);
                    yield break;
                }

                if (random.Next(100) < 100)
                {
                    ped.Quaternion = Quaternion.RotationAxis(ped.Position, (float)(random.NextDouble() - 0.5)) * ped.Quaternion;
                }

                SetAnimRate(ped, 5.0f);
                Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, ped, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0,
                    0, 0);
                Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, ped, true);
                var hp = core.PlayerPed.Position;
                Function.Call(Hash.APPLY_FORCE_TO_ENTITY, ped, hp.X * 1, hp.Y * 1, hp.Z * 1, hp.X, hp.Y, hp.Z, 1, false,
                    true, true, true, true);
                Function.Call(Hash.TASK_PLAY_ANIM, ped, animDict, animPlay, 5.0, -5.0, -1, 9, 0
                , 0, 0, 0);

                StartFire(ped, petroEffect);

                yield return null;
            }
        }

        private void StartFire(Ped ped, string effect)
        {

            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 2.0f;

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_flame",
                    ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale, 0, 0, 0);

        }


        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("Hokusai.wav"));
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
