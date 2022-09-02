using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Aatrox
{

    class Methods
    {
        public Methods()
        {
            Initialize();
        }

        private static void Initialize()
        {
            GameEvent.OnGameTick                     += Aatrox.OnTick;
            Orbwalker.OnAfterAttack          += Aatrox.OnPostAttack;
            AntiGapcloser.OnGapcloser        += Aatrox.OnNewGapcloser;
            AIBaseClient.OnDoCast += Aatrox.OnProcessSpellCast;
        }
    }
}