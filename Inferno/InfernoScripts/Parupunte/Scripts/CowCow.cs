using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("黒毛和牛")]
    [ParupunteIsono("もーもー")]
    //[ParupunteDebug(true)]
    internal class CowCow : ParupunteScript
    {
        public CowCow(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            StartCoroutine(SpawnCharacter());
        }

        private IEnumerable<object> SpawnCharacter()
        {
            foreach (var s in WaitForSeconds(1))
            {
                Spawn();
                Spawn();
                yield return s;
            }
            ParupunteEnd();
        }

        private void Spawn()
        {
            var player = core.PlayerPed;
            var lion = GTA.World.CreatePed(new Model(PedHash.Cow), player.Position.Around(2));
            if (lion.IsSafeExist())
            {
                lion.MarkAsNoLongerNeeded();
                lion.MaxHealth = 10000;
                lion.Health = lion.MaxHealth;
             //   lion.Task.FightAgainst(core.PlayerPed);
            }
        }
    }
}
