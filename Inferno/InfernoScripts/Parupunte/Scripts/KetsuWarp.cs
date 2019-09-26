﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using System.IO;
using UniRx;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("行き先あくしろよ")]
    [ParupunteIsono("けつるーら")]
    //[ParupunteDebug(true)]
    class KetsuWarp : ParupunteScript
    {
        public KetsuWarp(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            StartCoroutine(IdleCoroutine());
        }

        private IEnumerable<object> IdleCoroutine()
        {
            foreach (var w in WaitForSeconds(10))
            {
                var blip = GTA.World.GetActiveBlips()
                    .FirstOrDefault(x => x.Exists() && (int)x.Color == 84 && x.Type == 4);

                if (blip != null)
                {
                    StartCoroutine(MoveToCoroutine());
                    yield break;
                }
                yield return null;
            }
            core.DrawParupunteText("時間切れ", 3);
            ParupunteEnd();
        }

        private IEnumerable<object> MoveToCoroutine()
        {
            core.DrawParupunteText("逝ってらっしゃい！", 3);

            var target = core.PlayerPed.IsInVehicle() ? (Entity)core.PlayerPed.CurrentVehicle : (Entity)core.PlayerPed;
            target.IsInvincible = true;

            target.ApplyForce(Vector3.WorldUp * 500.0f);
            GTA.World.AddExplosion(target.Position, GTA.ExplosionType.Grenade, 0.5f, 0.5f, true, false);

            if (target is Ped)
            {
                var p = (Ped)target;
                
                p.Task.Skydive();
            }


            yield return WaitForSeconds(0.2f);


            while (target.IsSafeExist())
            {
                var targetBlip = GTA.World.GetActiveBlips().FirstOrDefault(x => x.Exists() && (int)x.Color == 84 && x.Type == 4);
                var targetposition = targetBlip;
                if (targetBlip == null || !targetBlip.Exists())
                {
                    if (target.IsSafeExist())
                    {
                        target.IsInvincible = false;
                        if (target is Ped)
                        {
                            var p = (Ped)target;
                            p.ParachuteTo(p.Position);

                        }
                    }
                   ParupunteEnd();
                    core.DrawParupunteText("おわり", 3);
                    yield break;
                }

                if (target is Ped)
                {
                    if (!((Ped)target).IsInParachuteFreeFall)
                    {
                        target.IsInvincible = false;
                        ParupunteEnd();
                        core.DrawParupunteText("おわり", 3);
                        yield break;
                    }

                }

                var goal = targetposition.Position;
                var current = target.Position;
                var dir = (goal - current).Normalized;

                var toVector = (goal - current);
                var horizontalLength = new Vector3(toVector.X, toVector.Y, toVector.Z).Length();

                //Wキーで上昇
                this.OnUpdateAsObservable
                 .Where(_ => core.IsGamePadPressed(GameKey.Space))
                 .Subscribe(_ =>
                 {
                     target.ApplyForce(Vector3.WorldUp * 5.0f);
                 });

                if (horizontalLength < 5)
                {
                    if (target.IsSafeExist())
                    {
                       // target.IsInvincible = false;
                        if (target is Ped)
                        {
                            var p = (Ped)target;
                            p.ParachuteTo(p.Position);
                        }
                        else
                        {
                            core.PlayerPed.CurrentVehicle.Repair();
                        }
                    }
                    core.DrawParupunteText("おまけに完璧な着地だ！", 3);
                    ParupunteEnd();
                    yield break;
                }


                if (horizontalLength < 100)
                {
                    target.ApplyForce(dir * 60.0f);
                }
                else
                {
                    if (core.PlayerPed.IsInVehicle())
                    {
                        target.ApplyForce((dir + Vector3.WorldUp * -0.1f) * 250.0f);
                    }
                    else
                    {
                        target.ApplyForce((dir + Vector3.WorldUp * 0.5f) * 250.0f);
                    }
                   
                    GTA.World.AddExplosion(target.Position, GTA.ExplosionType.Grenade, 0.5f, 0.5f, true, false);
                }
                yield return WaitForSeconds(0.2f);
            }

            WaitForSeconds(3.0f);

            target.IsInvincible = false;
            
            ParupunteEnd();

        }
    }
}
