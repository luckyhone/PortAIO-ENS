using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.LeBlanc.Misc;
using Entropy.AIO.Utility;

namespace Entropy.AIO.LeBlanc.Logic
{
    using static Components;
    using static ChampionBase;
    static class Harass
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        internal static void ExecuteQ()
        {
            var qHarassMenu = HarassMenu.QSliderBool;
            if (!qHarassMenu.Enabled ||
                LocalPlayer.ManaPercent <= ManaManager.GetNeededMana(Q, qHarassMenu))
            {
                return;
            }

            var target = Definitions.GetMarkedTarget;
            if (target != null && target.IsValid && target.DistanceToPlayer() <= Q.Range)
            {
                Q.Cast(target);
                return;
            }

            target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            Q.Cast(target);
        }

        internal static void ExecuteW()
        {
            var wHarassMenu = HarassMenu.WSliderBool;
            if (!wHarassMenu.Enabled ||
                LocalPlayer.ManaPercent <= ManaManager.GetNeededMana(W, wHarassMenu))
            {
                return;
            }

            var target = Definitions.GetMarkedTarget;
            if (target != null && target.IsValid && target.DistanceToPlayer() <= W.Range)
            {
                W.Cast(target);
                return;
            }

            target = TargetSelector.GetTarget(W.Range,DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            W.Cast(target);
        }

        internal static void ExecuteE()
        {
            var eHarassMenu = HarassMenu.ESliderBool;
            if (!eHarassMenu.Enabled ||
                LocalPlayer.ManaPercent <= ManaManager.GetNeededMana(E, eHarassMenu))
            {
                return;
            }

            var target = Definitions.GetMarkedTarget;
            if (target != null && target.IsValid && target.DistanceToPlayer() <= E.Range)
            {
                E.Cast(target);
                return;
            }

            target = TargetSelector.GetTarget(E.Range,DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            E.Cast(target);
        }
    }
}