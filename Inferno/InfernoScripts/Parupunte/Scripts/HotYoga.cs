using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using Inferno.Utilities;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("ほっとよが")]
    [ParupunteConfigAttribute("HOT YOGA")]
    //[ParupunteDebug(true)]
    internal class Hotyoga : ParupunteScript
    {
        private Model pedModel;
        private Model pedModel2;

        public Hotyoga(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            pedModel = new Model(PedHash.Fabien);
            pedModel2 = new Model(PedHash.AmandaTownley);

        }

        public override void OnStart()
        {
            StartCoroutine(HotyogaCoroutine());
        }

        private IEnumerable<object> HotyogaCoroutine()
        {

            var player = core.PlayerPed;

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = true;
            }

            foreach (var s in WaitForSeconds(1))
            {
                var ped = GTA.World.CreatePed(pedModel, player.Position.AroundRandom2D(8));
                if (ped.IsSafeExist())
                {
                    ped.MarkAsNoLongerNeeded();
                    Ignition(ped);

                }
                yield return s;

                var ped2 = GTA.World.CreatePed(pedModel2, player.Position.AroundRandom2D(8));
                if (ped2.IsSafeExist())
                {
                    ped2.MarkAsNoLongerNeeded();
                    Ignition2(ped2);
                }
                yield return s;

            }
            WaitForSeconds(3.0f);

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = false;
            }

            ParupunteEnd();
        }

        private void Ignition(Ped ped)
        {
            if (!ped.IsSafeExist()) return;
            var pos = ped.Position;
            GTA.World.AddOwnedExplosion(ped, pos, GTA.ExplosionType.Molotov1, 3.0f, 0.0f);
        }
        private void Ignition2(Ped ped2)
        {
            if (!ped2.IsSafeExist()) return;
            var pos = ped2.Position;
            GTA.World.AddOwnedExplosion(ped2, pos, GTA.ExplosionType.Molotov1, 3.0f, 0.0f);
        }
    }
}
