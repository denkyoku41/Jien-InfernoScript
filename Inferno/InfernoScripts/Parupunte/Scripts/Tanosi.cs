﻿using System;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts;
using Inferno.InfernoScripts.Parupunte;
using UniRx;

namespace Inferno
{
    [ParupunteConfigAttribute("たのしー！", "おわり")]
    [ParupunteIsono("たのしー")]
    //[ParupunteDebug(true)]
    internal class Tanosi : ParupunteScript
    {
        private float addSpeed = 0.0f;
        public Tanosi(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        protected override void OnFinished()
        {
            core.PlayerPed.CanRagdoll = true;
            core.PlayerPed.CanSufferCriticalHits = true;
            core.PlayerPed.CanPlayGestures = true;
            core.PlayerPed.CanBeDraggedOutOfVehicle = true;
            core.PlayerPed.BlockPermanentEvents = true;

            SetAnimRate(core.PlayerPed, 0);
            Function.Call(Hash.TASK_FORCE_MOTION_STATE, core.PlayerPed, 0xFFF7E7A4, 0);
        }

        public override void OnStart()
        {
            var ptfxName = "core";

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfxName);

            var animDict = "creatures@boar@move";
            var animPlay = "gallop";
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);

            core.PlayerPed.CanRagdoll = false;
            core.PlayerPed.CanSufferCriticalHits = false;
            core.PlayerPed.CanPlayGestures = false;
            core.PlayerPed.CanBeDraggedOutOfVehicle = false;
            core.PlayerPed.BlockPermanentEvents = false;
            //メイン処理
            this.OnUpdateAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .Subscribe(_ =>
                {
                    SetAnimRate(core.PlayerPed, 6.0f + addSpeed);
                    Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, core.PlayerPed, 1000000000.0, 1, 1000, 1, 0, 0, 0, 0, 0,
                        0, 0);
                    Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, core.PlayerPed, false);
                    var hp = core.PlayerPed.ForwardVector;
                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY, core.PlayerPed, hp.X * addSpeed, hp.Y * addSpeed, hp.Z * addSpeed, 0, 0, 0, 1, false,
                       true, true, true, true);
                    Function.Call(Hash.TASK_PLAY_ANIM, core.PlayerPed, animDict, animPlay, 2.5, -2.5, -1, 9, 0,
                        0, 0, 0);

                    //徐々に加速
                    addSpeed *= 0.01f;
                    addSpeed = Math.Min(1, addSpeed);

                    // core.PlayerPed.ApplyForce(new Vector3(hp.X * 20.0f, hp.Y * 20.0f, hp.Z * -10.0f));

                });

            //定期的にエフェクト再生
            this.OnUpdateAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    StartFire();
                });

            //左右移動
            this.OnUpdateAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .Select(_ => core.GetStickValue().X)
                .Subscribe(input =>
                {
                    var player = core.PlayerPed;
                    player.Quaternion = Quaternion.RotationAxis(player.UpVector, (-input / 127.0f) * 0.2f) * player.Quaternion;
                });

            //下降
            //  this.OnUpdateAsObservable
            // .Where(_ => core.IsGamePadPressed(GameKey.Space))
            //  .Select(_ => core.GetStickValue().X)
            // .Subscribe(input =>
            //{
            //  var player = core.PlayerPed;
            // player.ApplyForce(Vector3.WorldUp * 3.0f);
            //});

            //上昇
            //    this.OnUpdateAsObservable
            //    .Where(_ => core.IsGamePadPressed(GameKey.VehicleHorn))
            //  .Select(_ => core.GetStickValue().X)
            //  .Subscribe(input =>
            //  {
            //    var player = core.PlayerPed;
            //    player.ApplyForce(Vector3.WorldDown * 6.0f);
            //});

            //ボタンを離したら中断
            this.OnUpdateAsObservable
                .Select(_ => core.IsGamePadPressed(GameKey.Sprint))
                .DistinctUntilChanged()
                .Where(x => !x)
                .Subscribe(_ =>
                {
                    addSpeed = 1.0f;
                    SetAnimRate(core.PlayerPed, 1);
                    Function.Call(Hash.TASK_FORCE_MOTION_STATE, core.PlayerPed, 0xFFF7E7A4, 0);
                });

            //死んだら爆発
            this.OnUpdateAsObservable
                .Select(_ => core.PlayerPed.IsDead)
                .DistinctUntilChanged()
                .Where(x => x).Subscribe(_ =>
                {
                    GTA.World.AddExplosion(core.PlayerPed.Position, GTA.ExplosionType.Rocket, 1.0f, 1.0f);
                });

            ReduceCounter = new ReduceCounter(25000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped, (double)rate, 0.0, 0.0);
        }

        private int StartFire()
        {
            var player = core.PlayerPed;
            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var scale = 1.5f;

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_electrical_box",
                   player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_L_Toe0, scale, 0, 0, 0);

            //  Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            //return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_dst_elec_fire",
            //      player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_R_Toe0, scale, 0, 0, 0);
        }

    }
}
