using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille.Logics
{
    #region

    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    static class JungleClear
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

            if (ObjectManager.Player.ManaPercent < JungleClearMenu.QSliderBool.Value && Definitions.QState == QState.None)
            {
                return;
            }

            if (Definitions.QState == QState.Charging)
            {
                return;
            }

            var minion = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(Q.Range));
            if (minion == null)
            {
                return;
            }

            Q.Cast();
        }
    }
}