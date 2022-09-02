using EnsoulSharp.SDK;

namespace Entropy.AIO.Neeko.Logics
{
    using static Components;
    using static Bases.ChampionBase;

    static class Combo
    {
        public static void ExecuteQ()
        {
            if (!ComboMenu.QBool.Enabled)
            {
                return;
            }

            var bestTarget = TargetSelector.GetTarget(Q.Range,DamageType.Magical);

            if (bestTarget == null || ComboMenu.QOnlyIfEnemyCCed.Enabled && !bestTarget.IsStunned)
            {
                return;
            }

            Q.Cast(bestTarget);
        }

        public static void ExecuteE()
        {
            if (!ComboMenu.EBool.Enabled)
            {
                return;
            }

            var bestTarget = TargetSelector.GetTarget(E.Range,DamageType.Magical);

            if (bestTarget == null)
            {
                return;
            }

            E.Cast(bestTarget);
        }
    }
}