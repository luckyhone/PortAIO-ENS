using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille.Logics
{
    #region

    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    static class Harass
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        internal static void ExecuteQ()
        {
            if (!Q.IsReady())
            {
                return;
            }

            if (!HarassMenu.QSliderBool.Enabled || HarassMenu.QMode.Index == 1 && Definitions.QState != QState.Charged)
            {
                return;
            }

            if (Definitions.QState == QState.Charged && HarassMenu.Q2Mode.Index == 1)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < HarassMenu.QSliderBool.Value && Definitions.QState == QState.None)
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (Definitions.QState == QState.None || Definitions.QState == QState.Charged)
            {
                Q.Cast();
            }
        }

        internal static void ExecuteW()
        {
            if (!W.IsReady())
            {
                return;
            }

            if (!HarassMenu.WSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < HarassMenu.WSliderBool.Value)
            {
                return;
            }

            if (Definitions.CastingR || Definitions.IsDashing)
            {
                return;
            }

            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (target.Distance(LocalPlayer) <= LocalPlayer.GetCurrentAutoAttackRange() ||
                target.Distance(LocalPlayer) <= W.Range && Q.IsReady()                        ||
                Definitions.QState == QState.Casted                                                ||
                Definitions.QState == QState.Charging                                              ||
                Definitions.QState == QState.Charged)
            {
                return;
            }

            W.Cast(target);
        }
    }
}