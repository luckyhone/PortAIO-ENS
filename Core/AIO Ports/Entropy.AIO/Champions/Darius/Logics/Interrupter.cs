using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius.Misc.Logics
{
    using static Components;
    using static Bases.ChampionBase;

    static class OnInterruptable
    {
        public static void ExecuteE(AIBaseClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!sender.IsValidTarget(E.Range))
            {
                return;
            }

            if (InterrupterMenu.EBool.Enabled)
            {
                E.Cast(sender);
            }
        }
    }
}