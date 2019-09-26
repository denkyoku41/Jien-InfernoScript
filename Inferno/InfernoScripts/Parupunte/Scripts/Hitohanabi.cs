using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using Inferno.Utilities;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("ひとはなび", "きたねぇ花火だ")]
    [ParupunteIsono("ひとはなび")]
    //[ParupunteDebug(true)]
    internal class Hitohanabi : ParupunteScript
    {
        public Hitohanabi(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(5000);
            AddProgressBar(ReduceCounter);
            //コルーチン起動
            StartCoroutine(HitohanabiCoroutine());
        }

        private IEnumerable<object> HitohanabiCoroutine()
        {
            //プレイや周辺の15m上空を設定
            var pedList = new HashSet<Ped>();
            //タイマが終わるまでカウントし続ける
            while (!ReduceCounter.IsCompleted)
            {
                foreach (
                    var targetPed in
                        core.CachedPeds.Where(
                            x => x.IsSafeExist()
                            && x.IsAlive
                            && x.IsHuman
                            && !x.IsCutsceneOnlyPed()
                            && x.IsInRangeOf(core.PlayerPed.Position, 100))
                    )
                {

                    //まだの人をリストにくわえる
                    if (pedList.Count < 30 && !pedList.Contains(targetPed))
                    {
                        if (PedGroup.Exists(core.PlayerPed.CurrentPedGroup) && core.PlayerPed.CurrentPedGroup.Contains(targetPed)) { continue; }

                        var relationShip = targetPed.RelationshipGroup;
                        if (relationShip == core.GetGTAObjectHashKey("PLAYER")) { continue; }//ミッション上での仲間は除外する(誤判定が起きる場合があるので暫定)

                        pedList.Add(targetPed);
                        if (targetPed.IsInVehicle()) targetPed.Task.ClearAllImmediately();
                        targetPed.CanRagdoll = true;
                        targetPed.SetToRagdoll();
                    }
                }

                foreach (var targetPed in pedList.Where(x => x.IsSafeExist()))
                {
                    //すいこむ
                    var targetPos = (core.PlayerPed.ForwardVector).Normalized();
                    var targetPosition = core.PlayerPed.Position + new Vector3(0, 0, 10) + targetPos * 40;
                    var direction = targetPosition - targetPed.Position;
                    targetPed.FreezePosition = false;
                    targetPed.SetToRagdoll();
                    var lenght = direction.Length();
                    if (lenght > 5)
                    {
                        direction.Normalize();
                        targetPed.ApplyForce(direction * lenght.Clamp(0, 5) * 30);
                    }
                }
                yield return null;
            }


            //バクハツシサン
            foreach (var targetPed in pedList.Where(x => x.IsSafeExist()))
            {
                var targetPos = (core.PlayerPed.ForwardVector).Normalized();
                var targetPosition = core.PlayerPed.Position + new Vector3(0, 0, 10) + targetPos * 40;
                GTA.World.AddExplosion(targetPosition, GTA.ExplosionType.Plane, 2.0f, 0.0f);
                targetPed.ApplyForce(InfernoUtilities.CreateRandomVector() * 10);

            }

        //終了
        ParupunteEnd();
        }

    }
}
