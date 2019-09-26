using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("テンペスト")]
    [ParupunteIsono("てんぺすと")]
    //[ParupunteDebug(true)]
    internal class Tempest : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();

        public Tempest(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        protected override void OnFinished()
        {
            core.PlayerPed.IsInvincible = false;
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            core.PlayerPed.IsInvincible = true;
        }

        protected override void OnUpdate()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist()
            && x.IsInRangeOf(playerPos, 100)
            && !entityList.Contains(x)
            && !x.IsCutsceneOnlyPed()))
            {

                entityList.Add(ped);
                StartCoroutine(TemepenstCoroutine(ped));

            }
            foreach (var veh in core.CachedVehicles.Where(x => x.IsSafeExist()
            && x.IsInRangeOf(playerPos, 100)
            && !entityList.Contains(x)
            ))
            {
                if (!entityList.Contains(veh))
                {
                    entityList.Add(veh);
                    StartCoroutine(TemepenstCoroutine(veh));
                }
            }
        }

        private IEnumerable<object> TemepenstCoroutine(Entity entity)
        {
            while (!ReduceCounter.IsCompleted)
            {
                if (!entity.IsSafeExist()) yield break;
                if (!entity.IsInRangeOf(core.PlayerPed.Position, 30)) yield return null;
                if (entity is Ped)
                {
                    var p = entity as Ped;
                    p.IsInvincible = true;
                    p.SetToRagdoll();
                }

                var targetPos = (core.PlayerPed.ForwardVector).Normalized();

                    var playerPos = core.PlayerPed.Position + new Vector3(0, 0, 5) + targetPos * 40;
                    var gotoPlayerVector = (playerPos - entity.Position).Normalized();
                    var lenght = gotoPlayerVector.Length();

                    var angle = 50;
                    var rotatedVector = Quaternion.RotationAxis(Vector3.WorldUp, angle)
                        .ApplyVector(gotoPlayerVector);

                    entity.ApplyForce(gotoPlayerVector * 90);

                    var mainPower = entity is Ped ? 60 : 50;
                    var upPower = entity is Ped ? 20 : 10;
                    entity.ApplyForce(rotatedVector * mainPower + Vector3.WorldUp * upPower);

                //プレイヤに向かうベクトル


                yield return null;
            }

            if (entity is Ped)
            {
                var p = entity as Ped;
                p.IsInvincible = false;
            }

        }
    }
}
