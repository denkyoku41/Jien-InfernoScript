using GTA.Math;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UniRx;

namespace Inferno
{
    internal class CitizenInvincible : InfernoScript
    {
        protected override void Setup()
        {
            OnKeyDownAsObservable
                .Where(x => x.KeyCode == Keys.F11)
                .Subscribe(_ =>
                {
                    DrawText("Citizen Invincible", 3.0f);
                    StartCoroutine(RagdollCoroutine());
                });
        }

        private IEnumerable<object> RagdollCoroutine()
        {
            var peds = CachedPeds.Where(
                x => x.IsSafeExist()
                     && x.IsInRangeOf(PlayerPed.Position, 100)).ToArray();

            var vecs = CachedVehicles.Where(
                x => x.IsSafeExist() && x.IsInRangeOf(PlayerPed.Position, 100)).ToArray();

            foreach (var ped in peds)
            {
               // if (!ped.IsSafeExist()) continue;
                ped.IsInvincible = true;
                //ped.SetToRagdoll(100);
                //ped.ApplyForce(new Vector3(0, 0, 2));
                yield return null;
            }

            foreach (var vec in vecs)
            {
                // if (!ped.IsSafeExist()) continue;
                vec.IsCollisionProof = true;
                //ped.SetToRagdoll(100);
                //ped.ApplyForce(new Vector3(0, 0, 2));
                yield return null;
            }
        }
    }
}
