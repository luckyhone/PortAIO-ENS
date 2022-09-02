using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille.Logics
{
    #region

    using System.Linq;
    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    static class Weaving
    {
        internal static void ExecuteQ()
        {
            if (!Q.IsReady())
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo when !ComboMenu.QBool.Enabled:
                case OrbwalkerMode.Harass when !HarassMenu.QSliderBool.Enabled ||
                                                ObjectManager.Player.ManaPercent < HarassMenu.QSliderBool.Value:
                case OrbwalkerMode.LaneClear when !StructureClearMenu.QSliderBool.Enabled ||
                                                   ObjectManager.Player.ManaPercent < StructureClearMenu.QSliderBool.Value:
                    return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                case OrbwalkerMode.Harass:
                    var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
                    if (target == null)
                    {
                        return;
                    }

                    break;
                case OrbwalkerMode.LaneClear:
                    var turret = GameObjects.EnemyTurrets.FirstOrDefault(x => x.IsValidTarget(ObjectManager.Player.GetCurrentAutoAttackRange()));
                    if (turret == null)
                    {
                        return;
                    }

                    break;
            }

            if (Definitions.QState == QState.None || Definitions.QState == QState.Charged)
            {
                Q.Cast();
            }
        }
    }
}