﻿using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("びっぐふっと")]
    //[ParupunteDebug(true)]
    internal class Bigfoot : ParupunteScript
    {
        private Model pedModel;
        private string name;
        private Random random;

        public Bigfoot(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetNames()
        {
            Name = name;
        }

        public override void OnSetUp()
        {
            random = new Random();

            switch (random.Next(0, 100) % 1)
            {
                case 0:
                    pedModel = new Model(PedHash.Orleans);
                    name = "ビッグ・フット";
                    break;

            }
        }

        public override void OnStart()
        {
            StartCoroutine(SpawnCharacter());
        }

        private IEnumerable<object> SpawnCharacter()
        {
            var player = core.PlayerPed;
            foreach (var s in WaitForSeconds(2))
            {
                var ped = GTA.World.CreatePed(pedModel, player.Position.AroundRandom2D(15));
                if (ped.IsSafeExist())
                {
                    ped.MarkAsNoLongerNeeded();
                    GiveWeaponTpPed(ped);
                }
                yield return s;
            }
            ParupunteEnd();
        }

        /// <summary>
        /// 市民に武器をもたせる
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>装備した武器</returns>
        private void GiveWeaponTpPed(Ped ped)
        {
            if (!ped.IsSafeExist()) return;

            //車に乗っているなら車用の武器を渡す
            var weapon = Enum.GetValues(typeof(WeaponHash))
                .Cast<WeaponHash>()
                .OrderBy(c => random.Next())
                .FirstOrDefault();

            var weaponhash = (int)weapon;

            ped.SetDropWeaponWhenDead(false); //武器を落とさない
            ped.GiveWeapon(weaponhash, 1000); //指定武器所持
            ped.EquipWeapon(weaponhash); //武器装備
        }
    }
}