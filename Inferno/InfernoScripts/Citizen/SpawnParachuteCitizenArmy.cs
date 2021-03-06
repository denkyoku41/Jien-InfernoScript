﻿using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using System;
using System.Collections.Generic;
using Inferno.Utilities;
using UniRx;

namespace Inferno
{

    /// <summary>
    /// 市民を生成してパラシュート降下させる
    /// </summary>
    internal class SpawnParachuteCitizenArmy : InfernoScript
    {
        class SpawnParachuteCitizenArmyConfig : InfernoConfig
        {
            /// <summary>
            /// 生成間隔
            /// </summary>
            public int SpawnDurationSeconds { get; set; } = 5;

            public override bool Validate()
            {
                return SpawnDurationSeconds > 0;
            }
        }

        protected override string ConfigFileName { get; } = "SpawnParachuteCitizenArmy.conf";
        private SpawnParachuteCitizenArmyConfig config;
        private int SpawnDurationSeconds => config?.SpawnDurationSeconds ?? 5;

        protected override void Setup()
        {
            config = LoadConfig<SpawnParachuteCitizenArmyConfig>();
            CreateInputKeywordAsObservable("carmy")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("SpawnParachuteCitizenArmy:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            CreateTickAsObservable(TimeSpan.FromSeconds(SpawnDurationSeconds))
                .Where(_ => IsActive)
                .Subscribe(_ => CreateParachutePed());
        }

        private void CreateParachutePed()
        {
            if (!PlayerPed.IsSafeExist()) return;
            var playerPosition = PlayerPed.Position;

            var velocity = PlayerPed.Velocity;
            //プレイヤが移動中ならその進行先に生成する
            var ped =
                NativeFunctions.CreateRandomPed(playerPosition + 3 * velocity + new Vector3(0, 0, 50).AroundRandom2D(50));

            if (!ped.IsSafeExist()) return;

            ped.MarkAsNoLongerNeeded();
            ped.Task.ClearAllImmediately();
            ped.TaskSetBlockingOfNonTemporaryEvents(true);
            ped.SetPedKeepTask(true);
            ped.AlwaysKeepTask = true;
            //プレイヤ周囲15mを目標に降下
            var targetPosition = playerPosition.AroundRandom2D(15);
            ped.ParachuteTo(targetPosition);

            //着地までカオス化させない
            StartCoroutine(PedOnGroundedCheck(ped));
        }

        /// <summary>
        /// 市民が着地するまで監視する
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private IEnumerable<Object> PedOnGroundedCheck(Ped ped)
        {
            //市民無敵化
            ped.IsInvincible = true;
            ped.SetNotChaosPed(true);
            for (var i = 0; i < 10; i++)
            {
                yield return WaitForSeconds(1);

                //市民が消えていたり死んでたら監視終了
                if (!ped.IsSafeExist()) yield break;
                if (ped.IsDead) yield break;

                //着地していたら監視終了
                if (!ped.IsInAir)
                {
                    break;
                }
            }

            if (ped.IsSafeExist())
            {
                ped.SetNotChaosPed(false);
                ped.IsInvincible = false;
                ped.MarkAsNoLongerNeeded();
            }
        }
    }
}
