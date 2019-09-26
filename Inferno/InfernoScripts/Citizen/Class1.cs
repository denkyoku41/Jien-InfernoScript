using GTA.Math;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UniRx;

namespace Inferno
{
    internal class CitizenRagdoll : InfernoScript
    {
        protected override void Setup()
        {
            OnKeyDownAsObservable
                .Where(x => x.KeyCode == Keys.F10)
                .Subscribe(_ =>
                {
                    DrawText("Citizen vincible", 3.0f);
                    StartCoroutine(VincibleCoroutine());
                });
        }

        private IEnumerable<object> VincibleCoroutine()
        {
            var peds = CachedPeds.Where(
                x => x.IsSafeExist()
                     && x.IsInRangeOf(PlayerPed.Position, 100)).ToArray();

            var vecs = CachedVehicles.Where(
               x => x.IsSafeExist() && x.IsInRangeOf(PlayerPed.Position, 100)).ToArray();

            foreach (var ped in peds)
            {
               // if (!ped.IsSafeExist()) continue;
                ped.IsInvincible = false;
                //ped.SetToRagdoll(100);
                //ped.ApplyForce(new Vector3(0, 0, 2));
                yield return null;
            }

            foreach (var vec in vecs)
            {
                // if (!ped.IsSafeExist()) continue;
                vec.IsCollisionProof = false;
                //ped.SetToRagdoll(100);
                //ped.ApplyForce(new Vector3(0, 0, 2));
                yield return null;
            }
        }
    }
}
