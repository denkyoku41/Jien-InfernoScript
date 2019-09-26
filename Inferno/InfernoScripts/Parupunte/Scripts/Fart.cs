using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("おなら")]
    //[ParupunteDebug(true)]
    class Fart : ParupunteScript
    {
        private bool AffectAllPed = false;
        private Random random;

        private List<Ped> targetPeds = new List<Ped>();

        public Fart(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            var r = new Random();

            //たまに全員に対して発動させる
            AffectAllPed = r.Next(0, 1) <= 2;
        }

        public override void OnStart()
        {
            var ptfxName = "core";
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            if (AffectAllPed)
            {
                targetPeds = core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(core.PlayerPed.Position, 50) && x.IsAlive).ToList();
            }
            targetPeds.Add(core.PlayerPed);

            foreach (var ped in targetPeds)
            {
                //コルーチン起動
                StartCoroutine(FartCoroutine(ped));
            }
        }

        IEnumerable<object> FartCoroutine(Ped ped)
        {
            core.DrawParupunteText("3", 1.0f);

            yield return WaitForSeconds(1);
            core.DrawParupunteText("2", 1.0f);
            yield return WaitForSeconds(1);
            core.DrawParupunteText("1", 1.0f);
            yield return WaitForSeconds(1);

            core.DrawParupunteText("発射！", 3.0f);
            GasExplosion(ped);
            CreateEffect(ped, "ent_sht_steam");

            if (ped.IsInVehicle())
            {
                ped.CurrentVehicle.Speed = 300;
            }

            ped.SetToRagdoll(10);

           
            random = new Random();
            switch (random.Next(0, 2) % 4) {
                case 0:
                    ped.ApplyForce(Vector3.WorldUp * 10.0f);
                    break;
                case 1:
                    ped.ApplyForce(Vector3.WorldUp * 100.0f);
                    core.PlayerPed.IsInvincible = true;
                    while (core.PlayerPed.IsInVehicle() ? core.PlayerPed.CurrentVehicle.IsInAir : core.PlayerPed.IsInAir)
                    {
                        yield return null;
                    }
                    break;
                case 2:
                    ped.ApplyForce(Vector3.WorldUp * 2.0f);
                    break;

            }

            ParupunteEnd();
        }

        private void GasExplosion(Ped ped)
        {
            //var playerPos = core.PlayerPed.Position;
            var pedPos = ped.Position;
            //var targets = core.CachedPeds.Cast<Entity>().Concat(core.CachedVehicles)
              //  .Where(x => x.IsSafeExist() && x.IsInRangeOf(playerPos, 200));


            Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
            {
                pedPos.X,
                pedPos.Y,
                pedPos.Z,
                -1,
                0.5f,
                true,
                false,
                0.5f
            });

            //foreach (var e in targets)
            //{
              //  var dir = (e.Position - playerPos).Normalized;
                //e.ApplyForce(dir * 1000.0f, Vector3.Zero, ForceType.MaxForceRot);
            //}
        }

        private void CreateEffect(Ped ped, string effect)
        {
            if (!ped.IsSafeExist()) return;
            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 3.0f;
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, effect,
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale, 0, 0, 0);
        }
    }
}
