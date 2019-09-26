using GTA;
using GTA.Math;
using GTA.Native;
using System.Media;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Inferno.InfernoScripts.Event.ChasoMode;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("野郎ぶっころしてやらー", "　何が始まるんです？　")]
    [ParupunteIsono("こまんどー")]
    //[ParupunteDebug(true)]
    internal class Commando : ParupunteScript
    {
        private Random random = new Random();
        private HashSet<int> ninjas = new HashSet<int>();
        private List<Ped> pedList = new List<Ped>();
        private SoundPlayer soundPlayerStart;

        public Commando(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
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


            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(core.PlayerPed.Position, 30) && x.IsAlive))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                ped.CanPlayGestures = false;
                ped.CanSwitchWeapons = false;
                ped.CanRagdoll = false;
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
                    ped.CanPlayGestures = true;
                    ped.CanSwitchWeapons = true;
                    ped.CanRagdoll = true;
                }
                ParupunteEnd();
            });

            soundPlayerStart?.Play();
        }

        protected override void OnUpdate()
        {
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive
                                                           && x.IsInRangeOf(core.PlayerPed.Position, 50)
                                                           &&
                                                           !ninjas.Contains(x.Handle)))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                ped.CanPlayGestures = false;
                ped.CanSwitchWeapons = false;
                ped.CanRagdoll = false;
                StartCoroutine(PlayCoroutine(ped));
            }

            foreach (var tarped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive
                                   && x.IsInRangeOf(core.PlayerPed.Position, 50) &&
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
            var animDict = "melee@knife@streamed_core";
            var animPlay = "counter_attack_r";
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);

            Inferno.InfernoCore.Publish(new ChangeWeaponEvent(Weapon.KNIFE));

            while (!ReduceCounter.IsCompleted)
            {
                if (!ped.IsSafeExist()) yield break;

                if (random.Next(100) < 100)
                {
                    ped.Quaternion = Quaternion.RotationAxis(ped.Position, (float)(random.NextDouble() - 0.5)) * ped.Quaternion;
                }

                var tar = core.PlayerPed.Position - ped.Position;

                SetAnimRate(ped, 5.0f);

                Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, ped, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0,
                    0, 0);
                Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, ped, true);
                var hp = core.PlayerPed.Position;
                Function.Call(Hash.APPLY_FORCE_TO_ENTITY, ped, hp.X * 1, hp.Y * 1, hp.Z * 1, hp.X, hp.Y, hp.Z, 1, false,
                    true, true, true, true);
                Function.Call(Hash.TASK_PLAY_ANIM, ped, animDict, animPlay, 8.0, -8.0, -1, 0, 0
                , 0, 0, 0);
                ped.ApplyForce(new Vector3(tar.X * 2.0f, tar.Y * 2.0f, tar.Z * 2.0f));

                yield return null;
            }
        }


        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("Commando.wav"));
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
