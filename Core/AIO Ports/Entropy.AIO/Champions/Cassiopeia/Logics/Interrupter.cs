using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using static Components.InterrupterMenu;
    using static Bases.ChampionBase;
    using static Components;

    static class OnInterruptable
    {
        public static void ExecuteR(AIBaseClient sender, Interrupter.InterruptSpellArgs args)
        {

            if (!sender.IsValidTarget(R.Range))
            {
                return;
            }

            if (RBool.Enabled)
            {
                R.Cast(sender);
            }
        }
    }
}