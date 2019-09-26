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
    [ParupunteConfigAttribute("ケツファンネル", "おわり")]
    [ParupunteIsono("けつふぁん")]
   // [ParupunteDebug(true)]
    class KetsuFunnel : ParupunteScript
    {
        public KetsuFunnel(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            core.PlayerPed.IsInvincible = true;
            AddProgressBar(ReduceCounter);

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                WaitForSeconds(3.0f);
                core.PlayerPed.IsInvincible = false;
                ParupunteEnd();
            });

            StartCoroutine(ElectricalCoroutine());
        }



        IEnumerable<object> ElectricalCoroutine()
        {
            var pos = core.PlayerPed.Position;
            while (IsActive)
            {
                pos = core.PlayerPed.Position;
                var bones = new[] { Bone.IK_Head, Bone.IK_L_Foot, Bone.IK_L_Hand, Bone.IK_R_Foot, Bone.IK_R_Hand };
                foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(pos, 30)))
                {
                    var vec = (ped.Position - pos).Normalized;
                    var random1 = (float)Random.NextDouble() / 2.0f;

                    if (ped.IsInVehicle())
                    {

                        NativeFunctions.ShootSingleBulletBetweenCoords(
                            pos + new Vector3(0, 0, random1) + vec,
                            ped.GetBoneCoord(Bone.IK_Head), 1, WeaponHash.RPG,null, 200.0f);
                    }
                    else
                    {
                        //適当な体の部位に向かって撃つ
                        var target = bones[Random.Next(0, bones.Length)];
                        NativeFunctions.ShootSingleBulletBetweenCoords(
                               pos + new Vector3(0, 0, random1) + vec,
                               ped.GetBoneCoord(target), 1, WeaponHash.RPG, null, 200.0f);
                    }

                    if (core.PlayerPed.IsDead)
                    {
                        ParupunteEnd();
                    }
                    
                }
                yield return WaitForSeconds(0.7f);
            }
        }
    }
}
