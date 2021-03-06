﻿using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("がるぱん")]
    //[ParupunteDebug(true)]
    class Garupan : ParupunteScript
    {
        enum PlanType
        {
            Kosokoso,
            Kottsun
        }
        private PlanType planType;
        private string _name;
        private List<Entity> resources = new List<Entity>();
 

        public Garupan(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            base.OnSetUp();
            planType = (PlanType)Random.Next(0, Enum.GetValues(typeof(PlanType)).Length);

            switch (planType)
            {
               case PlanType.Kosokoso:
                   _name = "こそこそ作戦、開始です！";
                    break;
                case PlanType.Kottsun:
                    _name = "こっつん作戦、開始です！";
                    break;
            }
        }

        public override void OnSetNames()
        {
            Name = _name;
            SubName = "ガルパンは、いいぞ";
            EndMessage = () =>
            {
                if (core.PlayerPed.IsAlive)
                {
                    return "おしまい";
                }
                else
                {
                    EndMessageDisplayTime = 4.0f;
                    return "フラッグ車走行不能！大洗女子学園の勝利！";
                }
            };
        }

        public override void OnStart()
        {
            switch (planType)
            {
                case PlanType.Kosokoso:
                    StartKosokoso();
                    break;
                case PlanType.Kottsun:
                    KottsunStart();
                    break;
            }

            //プレイヤが死んだら終了
            this.OnUpdateAsObservable
                .Where(_ => !core.PlayerPed.IsAlive)
                .FirstOrDefault()
                .Subscribe(_ => ParupunteEnd());

            this.OnFinishedAsObservable
                .Subscribe(_ =>
                {
                    core.PlayerPed.IsInvincible = false;
                    core.PlayerPed.CurrentVehicle.IsCollisionProof = false;
                    foreach (var t in resources)
                    {
                        if (t.IsSafeExist())
                        {
                            t.MarkAsNoLongerNeeded();
                            t.Health = -1;
                        }
                    }
                });
        }

        #region こそこそ作戦
        private void StartKosokoso()
        {
            ReduceCounter = new ReduceCounter(8 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

          //  core.PlayerPed.IsInvincible = true;
            core.PlayerPed.CurrentVehicle.IsCollisionProof = true;
            //戦車を遠目にたくさん配置して、遭遇したら攻撃してくる
            foreach (var i in Enumerable.Range(0, 2))
            {

                var ppos = core.PlayerPed.Position;
                var targetPos = (core.PlayerPed.ForwardVector).Normalized();

                var vp = SpawnTank(ppos + targetPos * 10);
             //   if (vp == null) continue;
                var ped = vp.Item2;
               // ped.Task.FightAgainst(core.PlayerPed);
                var tank = vp.Item1;
                resources.Add(tank);
                resources.Add(ped);
                tank.EnginePowerMultiplier = 20.0f;
                tank.EngineTorqueMultiplier = 20.0f;
                tank.MaxSpeed = 300;
            }
        }

        #endregion

        #region こっつん作戦

        void KottsunStart()
        {
            //ニトロで挟んで圧死させる
            ReduceCounter = new ReduceCounter(3 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            StartCoroutine(KottsunTankCoroutine(true));
            StartCoroutine(KottsunTankCoroutine(false));
        }

        //戦車を生成して数秒後に突進させる
        IEnumerable<object> KottsunTankCoroutine(bool isForward)
        {
            //遠目につくる
            var ppos = core.PlayerPed.Position;

            var vp = SpawnTank(ppos + new Vector3(0, isForward ? -30 : 30, 0));
            if (vp == null) ParupunteEnd();
            var ped = vp.Item2;
            var tank = vp.Item1;
            tank.EnginePowerMultiplier = 20.0f;
            tank.EngineTorqueMultiplier = 20.0f;
            tank.MaxSpeed = 300;
            resources.Add(tank);
            resources.Add(ped);
            this.OnFinishedAsObservable
                .Where(_ => tank.IsSafeExist() && tank.IsAlive)
                .Subscribe(_ => tank.PetrolTankHealth = -1);

            yield return WaitForSeconds(2);
            if (!tank.IsSafeExist()) yield break;
            //演出用
            Function.Call(Hash.ADD_EXPLOSION, tank.Position.X, tank.Position.Y, tank.Position.Z, -1, 0.0f, true, false, 0.1f);
            tank.Speed = isForward ? 100 : -100;

            yield return WaitForSeconds(2);
            if (ped.IsSafeExist())
            {
              //  ped.Task.FightAgainst(core.PlayerPed);
            }
        }
        #endregion

        private System.Tuple<Vehicle, Ped> SpawnTank(float range)
        {
            return SpawnTank(core.PlayerPed.Position.Around(range));
        }

        private System.Tuple<Vehicle, Ped> SpawnTank(Vector3 position)
        {
            var model = new Model(VehicleHash.Rhino);
            //戦車生成
            var tank = GTA.World.CreateVehicle(model, position);
            if (!tank.IsSafeExist()) return null;

            //乗員召喚
            var ped = tank.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.Tonya));
            if (!ped.IsSafeExist()) return null;
            ped.SetNotChaosPed(true);

            //自動開放
            AutoReleaseOnGameEnd(tank);
            AutoReleaseOnGameEnd(ped);

            return new System.Tuple<Vehicle, Ped>(tank, ped);
        }
    }
}
