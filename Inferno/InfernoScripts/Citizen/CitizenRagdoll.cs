﻿using GTA.Math;
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
                .Where(x => x.KeyCode == Keys.F8)
                .Subscribe(_ =>
                {
                    DrawText("Ragdoll", 3.0f);
                    StartCoroutine(RagdollCoroutine());
                });
        }

        private IEnumerable<object> RagdollCoroutine()
        {
            var peds = CachedPeds.Where(
                x => x.IsSafeExist()
                     && x.IsRequiredForMission()
                     && x.CanRagdoll
                     && x.IsInRangeOf(PlayerPed.Position, 15)).ToArray();

            foreach (var ped in peds)
            {
                if (!ped.IsSafeExist()) continue;
                ped.SetToRagdoll(100);
                ped.ApplyForce(new Vector3(0, 0, 2));
                yield return null;
            }
        }
    }
}
