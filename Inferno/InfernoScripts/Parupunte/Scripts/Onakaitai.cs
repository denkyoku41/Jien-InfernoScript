﻿using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("おなかいたい")]
    //[ParupunteDebug(true)]
    internal class Onakaitai : ParupunteScript
    {
        private readonly string petroEffect = "ent_sht_petrol";

        private bool AffectAllPed = false;

        private List<Ped> targetPeds = new List<Ped>();

        public Onakaitai(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetNames()
        {
            Name = AffectAllPed ? "みんな糞まみれや" : "おなかいたい";
            EndMessage = () => "ついでに着火";
        }

        public override void OnSetUp()
        {
         //   var r = new Random();

            //たまに全員に対して発動させる
           // AffectAllPed = r.Next(0,1)  <= 2;
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

            if (core.PlayerPed.IsInVehicle())
            {
                core.PlayerPed.CurrentVehicle.IsVisible = false;
                core.PlayerPed.IsVisible = true;

            }

            targetPeds = core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive).ToList();

            targetPeds.Add(core.PlayerPed);

            //コルーチン起動
            foreach (var ped in targetPeds)
            {
                ped.IsVisible = true;
                StartCoroutine(OilCoroutine(ped));
            }

            //終わったら着火する
            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {

                if (core.PlayerPed.IsInVehicle())
                {
                    core.PlayerPed.CurrentVehicle.IsVisible = true;

                }
                else
                {
                    foreach (var ped in targetPeds.Where(x => x.IsSafeExist() && x.IsAlive))
                    {
                        Ignition(ped);
                    }
                }
                ParupunteEnd();
            });
        }

        private IEnumerable<object> OilCoroutine(Ped ped)
        {
            while (!ReduceCounter.IsCompleted)
            {
                CreateEffect(ped, petroEffect);

                yield return WaitForSeconds(1);
            }
        }

        private void CreateEffect(Ped ped, string effect)
        {
            if (!ped.IsSafeExist()) return;
            var offset = new Vector3(0.1f, 0.1f, 0.0f);
            var rotation = new Vector3(-80.0f, -10.0f, 0.0f);
            var scale = 3.0f;

            var offset1 = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation1 = new Vector3(80.0f, 10.0f, 0.0f);
            var scale1 = 3.0f;

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, effect,
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Head, scale, 0, 0, 0);

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, effect,
               ped, offset1.X, offset1.Y, offset1.Z, rotation1.X, rotation1.Y, rotation1.Z, (int)Bone.SKEL_Pelvis, scale1, 0, 0, 0);

        }

        /// <summary>
        /// 点火
        /// </summary>
        private void Ignition(Ped ped)
        {
            if (!ped.IsSafeExist()) return;
            var pos = ped.Position;
            GTA.World.AddOwnedExplosion(ped, pos, GTA.ExplosionType.Bullet, 0.0f, 0.0f);
        }
    }
}
