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

    static class LaneClear
    {
        public static void ExecuteQ()
        {
            if (!Q.IsReady())
            {
                return;
            }

            if (!LaneClearMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (ObjectManager.Player.ManaPercent < LaneClearMenu.QSliderBool.Value && Definitions.QState == QState.None)
            {
                return;
            }

            var minion = GameObjects.EnemyMinions.FirstOrDefault(
                x => x.IsValidTarget(Q.Range) && x.Health <= Q.GetDamage(x));
            if (minion == null)
            {
                return;
            }

            Q.Cast();
        }
    }
}