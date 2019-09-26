using GTA;
using GTA.Math;
using System.IO;
using System.Collections.Generic;
using System.Media;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("H A N A Z A W A")]
    [ParupunteIsono("はなざわ")]
    //[ParupunteDebug(true)]
    class Hanazawa : ParupunteScript
    {
        private SoundPlayer soundPlayerStart;

        public Hanazawa(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
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

            var targetPositionInAri = core.PlayerPed.Position + new Vector3(0,0,1);
            var targetPositionInAri2 = core.PlayerPed.Position + new Vector3(0, 0, 10);
            var targetPositionInAri3 = core.PlayerPed.Position + new Vector3(0, 0, 5);
            var targetPositionInAri4 = core.PlayerPed.Position + new Vector3(0, 0, 1);

            var vehicleForcePower = 7;
            var pedForcePower = 21;
            var citizenvehiclePower = 8;

            player.IsCollisionProof = true;

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = true;
            }


            foreach (var s in WaitForSeconds(15))
            {
                player.IsInvincible = true;
                player.SetToRagdoll(10000);

                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.IsInvincible = true;
                        p.SetToRagdoll(10000);
                    }
                    var direction = (player.Position - entity.Position).Normalized();
                    var power = entity is Ped ? pedForcePower : citizenvehiclePower;
                    entity.ApplyForce(direction * power * 3.0f, Vector3.RandomXYZ());
                }

                if (player.IsInVehicle() && player.CurrentVehicle.IsSafeExist())
                {
                    var direction = (targetPositionInAri2 - player.Position).Normalized();
                    var power = vehicleForcePower;
                    player.CurrentVehicle.ApplyForce(direction * power * 2.0f, Vector3.RandomXYZ());
                }
                else
                {
                    var direction = (targetPositionInAri - player.Position).Normalized();
                    var power = 35;
                    player.ApplyForce(direction * power * 1.5f, Vector3.RandomXYZ());

                    this.OnUpdateAsObservable
                    .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                    .Subscribe(_ =>
                    {
                      targetPositionInAri = targetPositionInAri3;
                    });

                    this.OnUpdateAsObservable
                        .Where(_ => core.IsGamePadPressed(GameKey.VehicleHorn))
                        .Subscribe(_ =>
                        {
                            targetPositionInAri = targetPositionInAri4;
                        });
                }

                yield return null;
            }


            foreach (var s in WaitForSeconds(0.5f))
            {
                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.IsInvincible = false;
                        p.MaxHealth = 1000;
                        p.Health = p.MaxHealth;
                    }

                    var direction1 = -(player.Position - entity.Position).Normalized();
                    var power1 = entity is Ped ? pedForcePower : citizenvehiclePower;
                    entity.ApplyForce(direction1 * power1 * 3.0f, Vector3.RandomXYZ());
                }

                yield return null;
            }
            //着地するまで

            player.IsCollisionProof = false;
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