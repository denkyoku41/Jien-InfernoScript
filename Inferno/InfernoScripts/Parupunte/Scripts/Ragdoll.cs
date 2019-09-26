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
   // [ParupunteConfigAttribute("ズコー", "　おわり　")]
   // [ParupunteIsono("ずこー")]
   // [ParupunteDebug(true)]
    internal class Ragdoll : ParupunteScript
    {
        private Random random = new Random();
        private HashSet<int> ninjas = new HashSet<int>();
        private List<Ped> pedList = new List<Ped>();
        private SoundPlayer soundPlayerStart;

        public Ragdoll(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {

        }

        public override void OnStart()
        {
            if (core.PlayerPed.IsInVehicle())
            {
                ParupunteEnd();
            }

            ReduceCounter = new ReduceCounter(7000);
            AddProgressBar(ReduceCounter);

            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(core.PlayerPed.Position, 100) && x.IsAlive))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                ped.Task.ClearAllImmediately();
                StartCoroutine(HidroCoroutine(ped));
            }

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {

                ParupunteEnd();
            });


        }

        protected override void OnUpdate()
        {           
                foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive
                                               && x.IsInRangeOf(core.PlayerPed.Position, 30)
                                               &&
                                               !ninjas.Contains(x.Handle)))
                {
                    pedList.Add(ped);
                    ninjas.Add(ped.Handle);
                    ped.Task.ClearAllImmediately();
                    StartCoroutine(HidroCoroutine(ped));
                }

            
        }

        protected override void OnFinished()
        {

        }


        private IEnumerable<object> HidroCoroutine(Ped ped)
        {
            while (!ReduceCounter.IsCompleted)
            {
                ped.SetToRagdoll();
                core.PlayerPed.SetToRagdoll();

                var tar = (ped.Position + new Vector3(0,0,5)).Normalized();

                ped.ApplyForce(tar * 0.05f);
                core.PlayerPed.ApplyForce((core.PlayerPed.Position + new Vector3(0, 0, 5)).Normalized() * 0.05f);

                yield return null;
            }
        }



    }
}
