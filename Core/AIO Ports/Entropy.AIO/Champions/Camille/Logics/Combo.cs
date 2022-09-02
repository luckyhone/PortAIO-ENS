using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille.Logics
{
    #region

    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    static class Combo
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        internal static void ExecuteQ()
        {
            if (!Q.IsReady())
            {
                return;
            }

            if (!ComboMenu.QBool.Enabled || ComboMenu.QMode.Index == 1 && Definitions.QState != QState.Charged)
            {
                return;
            }

            if (Definitions.QState == QState.Charged && ComboMenu.Q2Mode.Index == 1)
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

            if (!ComboMenu.WBool.Enabled)
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

            W.Cast(target.Position);
        }

        internal static void ExecuteE()
        {
            if (!E.IsReady())
            {
                return;
            }

            if (Definitions.CastingR || Definitions.CastingW)
            {
                return;
            }

            if (!ComboMenu.EBool.Enabled)
            {
                return;
            }

            if (!Definitions.IsOnWall)
            {
                var target = TargetSelector.GetTarget(E.Range + 800f,DamageType.Physical);
                if (target == null)
                {
                    return;
                }

                if ((target.Position.IsUnderEnemyTurret()) &&
                    ComboMenu.EUnderTurretBool.Enabled)
                {
                    return;
                }

                var walls    = LocalPlayer.Position.RotateAround(E.Range, 60).GetWalls();
                var bestWall = walls.GetBestWallToTarget(target, E.Range);
                //var wallsToE = walls.GetWallsWithTargets(E.Range);
                //if (!wallsToE.Any())
                //{
                //	return;
                //}

                //var bestWall = wallsToE.OrderBy(x => x.Distance(LocalPlayer.Instance)).FirstOrDefault(x => !x.IsZero);
                if (bestWall.IsZero)
                {
                    return;
                }

                E.Cast(bestWall);
            }
            else
            {
                var target = TargetSelector.GetTarget(1000f,DamageType.Physical);
                if (target == null)
                {
                    return;
                }

                if ((target.Position.IsUnderEnemyTurret()) &&
                    ComboMenu.EUnderTurretBool.Enabled)
                {
                    return;
                }

                if (E.Cast(target) != CastStates.SuccessfullyCasted)
                {
                    Definitions.LastETarget = target;
                }
            }
        }
    }
}