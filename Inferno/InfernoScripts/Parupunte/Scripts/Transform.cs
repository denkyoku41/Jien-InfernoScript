using System;
using System.Linq;
using GTA;
using GTA.Native;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    //[ParupunteConfigAttribute("変身GOGOベイビー")]
    //[ParupunteDebug(true)]
    class Transform : ParupunteScript
    {
        public Transform(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {

            ReduceCounter = new ReduceCounter(30 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());


            var initialModel = core.PlayerPed.Model;

            var hashed = Enum.GetValues(typeof(PedHash)).Cast<PedHash>().ToArray();
            var targetHash = hashed[Random.Next(hashed.Length)];
            var targetModel = new Model(targetHash);

           // var targetModel = 0x1250D7BA;

            Game.Player.ChangeModel(targetModel);
            this.OnFinishedAsObservable
                .Subscribe(_ => Game.Player.ChangeModel(initialModel));

        }


    }
}
