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

    static class StructureClear
    {
        public static void ExecuteQ()
        {
            if (!Q.IsReady())
            {
                return;
            }

            if (!JungleClearMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (ObjectManager.Player.ManaPercent < StructureClearMenu.QSliderBool.Value && Definitions.QState == QState.None)
            {
                return;
            }

            if (Definitions.QState == QState.Charging || Definitions.QState == QState.None)
            {
                return;
            }

            var turret = GameObjects.EnemyTurrets.FirstOrDefault(x => x.IsValidTarget(ObjectManager.Player.GetCurrentAutoAttackRange()));
            if (turret == null)
            {
                return;
            }

            Q.Cast();
        }
    }
}