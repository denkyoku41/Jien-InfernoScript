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
    [ParupunteConfigAttribute("花火大会（音量注意）", "おわり")]
    [ParupunteIsono("はなび２")]
    //[ParupunteDebug(true)]
    class FireWork2 : ParupunteScript
    {
        public FireWork2(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(10 * 1000);

            var dayTime = GTA.World.CurrentDayTime;
            Function.Call(Hash.SET_CLOCK_TIME, 1, dayTime.Minutes, dayTime.Seconds);

            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            StartCoroutine(ElectricalCoroutine());
        }

        IEnumerable<object> ElectricalCoroutine()
        {
            var pos = core.PlayerPed.Position;
            while (IsActive)
            {
                pos = core.PlayerPed.Position;
                var bones = new[] { Bone.IK_Head, Bone.IK_L_Foot, Bone.IK_L_Hand, Bone.IK_R_Foot, Bone.IK_R_Hand };
                foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(pos, 60)))
                {
                    var vec = (ped.Position - pos).Normalized;

                    if (ped.IsInVehicle())
                    {
                        
                        NativeFunctions.ShootSingleBulletBetweenCoords(
                            pos + new Vector3(0, 0, 50) + vec,
                            ped.GetBoneCoord(Bone.IK_Head), 1, WeaponHash.Firework, null, 380.0f);
                    }
                    else
                    {
                        //適当な体の部位に向かって撃つ
                        var target = bones[Random.Next(0, bones.Length)];
                        NativeFunctions.ShootSingleBulletBetweenCoords(
                               pos + new Vector3(0, 0, 50) + vec,
                               ped.GetBoneCoord(target), 1, WeaponHash.Firework, null, 380.0f);
                    }
                }
                yield return WaitForSeconds(1.0f);
            }
        }
    }
}
