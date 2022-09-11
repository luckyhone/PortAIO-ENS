using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius
{
    class Methods
    {

        public Methods()
        {
            Initialize();
        }

        private static void Initialize()
        {
            GameEvent.OnGameTick                      += Darius.OnTick;
            Orbwalker.OnAfterAttack           += Darius.OnPostAttack;
            Orbwalker.OnBeforeAttack            += Darius.PreAttack;
            AntiGapcloser.OnGapcloser         += Darius.OnNewGapcloser;
            Interrupter.OnInterrupterSpell += Darius.OnInterruptableSpell;
            AIBaseClient.OnDoCast  += Darius.OnProcessSpellCast;
        }
    }
}