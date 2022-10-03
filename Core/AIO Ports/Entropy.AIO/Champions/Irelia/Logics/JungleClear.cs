using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Irelia.Logics
{
    #region

    using System.Linq;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    static class JungleClear
    {
        public static float delayChecks;

        public static void ExecuteQ()
        {
            if (!JungleClearMenu.QBool.Enabled)
            {
                return;
            }

            if (!Q.IsReady())
            {
                return;
            }

            if (delayChecks > Game.Time)
            {
                return;
            }

            if (JungleClearMenu.QMarked.Enabled)
            {
                var minion = GameObjects.Jungle.
                                         Where(x => x.IsValidTarget(Q.Range) && (Q.CanExecute(x) || x.HasBuff("ireliamark"))).
                                         OrderBy(z => z.MaxHealth).
                                         FirstOrDefault();
                if (minion != null)
                {
                    Q.CastOnUnit(minion);
                }
            }
            else
            {
                var minion = GameObjects.Jungle.
                                         Where(x => x.IsValidTarget(Q.Range) && (Q.CanExecute(x) || x.HasBuff("ireliamark"))).
                                         OrderBy(z => z.MaxHealth).
                                         FirstOrDefault();
                if (minion != null)
                {
                    Q.CastOnUnit(minion);
                }

                var minionAll = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range)).
                                            OrderBy(z => z.MaxHealth).
                                            FirstOrDefault();
                if (minionAll != null)
                {
                    Q.CastOnUnit(minionAll);
                }
            }
        }

        public static void ExecuteE()
        {
            if (!JungleClearMenu.EBool.Enabled)
            {
                return;
            }

            if (!E.IsReady())
            {
                return;
            }

            var minionAll = GameObjects.Jungle.Where(x => x.IsValidTarget(E.Range)).OrderBy(z => z.MaxHealth).FirstOrDefault();
            if (minionAll == null)
            {
                return;
            }

            OnInterruptable.useE(minionAll);
            delayChecks = Game.Time + 0.5f;
        }
    }
}