using GTA;
using GTA.Math;
using System.IO;
using System.Collections.Generic;
using System.Media;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("磯野ー！空飛ぼうぜ！")]
    [ParupunteIsono("いその")]
    //[ParupunteDebug(true)]
    class Isono : ParupunteScript
    {
        private SoundPlayer soundPlayerStart;

        public Isono(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            SetUpSound();
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            StartCoroutine(IsonoCoroutine());

            soundPlayerStart?.Play();
        }

        private IEnumerable<object> IsonoCoroutine()
        {
            var player = core.PlayerPed;
            var entities =
                core.CachedVehicles
                    .Concat(core.CachedPeds.Cast<Entity>())
                    .Where(x => x.IsSafeExist() && x.IsInRangeOf(player.Position, 100));


            var targetPositionInAri = core.PlayerPed.Position + new Vector3(0, 0, 500);

            var vehicleForcePower = 7;
            var pedForcePower = 21;
            var citizenvehiclePower = 10;

            player.CanRagdoll = true;
            player.SetToRagdoll(10000);
            player.IsCollisionProof = true;

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = true;
            }

            WaitForSeconds(1.0f);

            foreach (var s in WaitForSeconds(4))
            {
                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.SetToRagdoll(4000);
                    }
                    var direction = (targetPositionInAri - entity.Position).Normalized();
                    var power = entity is Ped ? pedForcePower : citizenvehiclePower;
                    entity.ApplyForce(direction * power * 0.6f, Vector3.RandomXYZ());
                }
                if (player.IsInVehicle() && player.CurrentVehicle.IsSafeExist())
                {
                    player.CurrentVehicle.ApplyForce(Vector3.WorldUp * vehicleForcePower);
                }
                else
                {

                    player.ApplyForce(Vector3.WorldUp * pedForcePower);
                }
                yield return null;
            }

            targetPositionInAri = core.PlayerPed.Position - new Vector3(0, 0, 500);

            foreach (var s in WaitForSeconds(4))
            {
                player.IsInvincible = true;
                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.SetToRagdoll(4000);
                    }
                    var direction = (targetPositionInAri - entity.Position).Normalized();
                    var power = entity is Ped ? pedForcePower : citizenvehiclePower;
                    entity.ApplyForce(direction * power * 2.0f, Vector3.RandomXYZ());
                }
                if (player.IsInVehicle() && player.CurrentVehicle.IsSafeExist())
                {
                    player.CurrentVehicle.ApplyForce(Vector3.WorldUp * pedForcePower * -2.0f);
                }
                else
                {

                    player.ApplyForce(Vector3.WorldUp * vehicleForcePower * -8.0f);
                }
                yield return null;
            }
            //着地するまで
            while (player.IsInVehicle() ? player.CurrentVehicle.IsInAir : player.IsInAir)
            {
                yield return null;
            }
            player.IsCollisionProof = false;
            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = false;
            }
            player.IsInvincible = false;
            ParupunteEnd();
        }
        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("Isono.wav"));
            if (setupWav != null)
            {
                soundPlayerStart = new SoundPlayer(setupWav);
            }
        }

        private string[] LoadWavFiles(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                return new string[0];
            }

            return Directory.GetFiles(targetPath).Where(x => Path.GetExtension(x) == ".wav").ToArray();
        }
    }
    
}
