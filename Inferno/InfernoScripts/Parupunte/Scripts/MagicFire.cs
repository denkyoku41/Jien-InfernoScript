using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    /// <summary>
    /// 尻から炎
    /// </summary>
    [ParupunteConfigAttribute("ただし魔法は尻から出る", "　お　し　り　")]
    [ParupunteIsono("おしり")]
    //[ParupunteDebug(true)]
    internal class MagicFire : ParupunteScript
    {
        private bool AffectAllPed = false;
        private List<Ped> targetPeds = new List<Ped>();

        public MagicFire(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        private uint coroutineId = 0;

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15000);
            AddProgressBar(ReduceCounter);


            targetPeds = core.CachedPeds.Where(x => x.IsSafeExist() &&  x.IsAlive).ToList();

            targetPeds.Add(core.PlayerPed);
            var vehicles = core.CachedVehicles.Where
                 (x => x.IsSafeExist() && x.IsInRangeOf(core.PlayerPed.Position, 300));

            foreach (var vehicle in vehicles)
            {
                vehicle.IsVisible = false;
            }

            if (core.PlayerPed.IsInVehicle())
            {
                core.PlayerPed.CurrentVehicle.IsVisible = false;
                core.PlayerPed.IsVisible = true;
                core.PlayerPed.CurrentVehicle.IsFireProof = true;
            }

            foreach (var ped in targetPeds)
            {
                //コルーチン起動
                ped.IsVisible = true;
                coroutineId = StartCoroutine(MagicFireCoroutine(ped));
            }

        }

        protected override void OnFinished()
        {
            StopCoroutine(coroutineId);
            //終了時に炎耐性解除
            core.PlayerPed.IsFireProof = false;

            if (core.PlayerPed.IsInVehicle())
            {
                core.PlayerPed.CurrentVehicle.IsVisible = true;
            }

        }

        private IEnumerable<object> MagicFireCoroutine(Ped ped)
        {
            var ptfxName = "core";

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfxName);


            while (!ReduceCounter.IsCompleted)
            {
                core.PlayerPed.IsFireProof = true;
                StartFire(ped);
                yield return WaitForSeconds(1);
            }

            //まだ炎が残っているのでロスタイム
            yield return WaitForSeconds(0.5f);

            ParupunteEnd();
        }

        private int StartFire(Ped ped)
        {
            //var player = core.PlayerPed;
            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 2.0f;
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");

            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_flame",
                    ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale, 0, 0, 0);
        }
    }
}
