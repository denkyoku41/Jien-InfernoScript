using GTA;
using GTA.Math;
using System.IO;
using System.Collections.Generic;
using System.Media;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("磯野ー！空飛ぼうぜ！2")]
    [ParupunteIsono("イソノ")]
    //[ParupunteDebug(true)]
    class Isono2 : ParupunteScript
    {
        private SoundPlayer soundPlayerStart;

        public Isono2(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
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



            var vehicleForcePower = 7;
            var pedForcePower = 21;
            var citizenvehiclePower = 20;

            player.IsCollisionProof = true;

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = true;
            }


            foreach (var s in WaitForSeconds(3))
            {

                    foreach (var entity in entities.Where(x => x.IsSafeExist()))
                    {
                        if (entity is Ped)
                        {
                            var p = entity as Ped;
                            p.IsInvincible = true;
                            p.SetToRagdoll(4000);
                        }
                        var targetPositionInAri = core.PlayerPed.Position +
                           ((core.PlayerPed.ForwardVector).Normalized) * 30 + new Vector3(0, 0, 200);
                        var direction = (targetPositionInAri - entity.Position).Normalized();
                        var power = entity is Ped ? pedForcePower : citizenvehiclePower;
                        entity.ApplyForce(direction * power * 0.6f);
                    }
                 


                yield return null;
            }


            foreach (var s in WaitForSeconds(6))
            {
                player.IsInvincible = true;

                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.IsInvincible = false;
                        p.MaxHealth = 5000;
                        p.Health = p.MaxHealth;
                        p.SetToRagdoll(6000);
                    }
                    var targetPosition = (core.PlayerPed.ForwardVector).Normalized();

                    var playerPos = core.PlayerPed.Position + new Vector3(0, 0, 0) + targetPosition * 10;
                    var playerPos2 = core.PlayerPed.Position - new Vector3(0,0,1);

                    var direction = entity is Ped? (playerPos2 - entity.Position).Normalized() : (playerPos - entity.Position).Normalized();
                    var power = entity is Ped ? pedForcePower : citizenvehiclePower;
                    entity.ApplyForce(direction * power * 1.0f);
                }

                yield return null;
            }


            player.IsCollisionProof = false;

            foreach (var s in WaitForSeconds(0.5f))
            {

                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.IsInvincible = false;
                        p.MaxHealth = 5000;
                        p.Health = p.MaxHealth;
                        p.SetToRagdoll(6000);
                    }
                    var targetPosition = (core.PlayerPed.ForwardVector).Normalized();

                    var playerPos = core.PlayerPed.Position + new Vector3(0, 0, 0) + targetPosition * 30;
                    var playerPos2 = core.PlayerPed.Position - new Vector3(0, 0, 50);

                    var direction = entity is Ped ? (playerPos2 - entity.Position).Normalized() : (playerPos - entity.Position).Normalized();
                    var power = entity is Ped ? 0 : citizenvehiclePower;
                    entity.ApplyForce(-direction * power * 1.0f);
                }

                yield return null;
            }

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = false;
                player.CurrentVehicle.Repair();
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
