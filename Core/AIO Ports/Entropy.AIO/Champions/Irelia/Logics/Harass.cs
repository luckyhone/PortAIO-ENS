using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Irelia.Logics
{
    #region

    using System.Collections.Generic;
    using Misc;
    using SharpDX;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    class Harass
    {
        public static Dictionary<uint, AIBaseClient> posibleTargets = new Dictionary<uint, AIBaseClient>();
        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static void ExecuteHarass()
        {
            ExecuteQ();
            ExecuteE();
            ExecuteQGap();
            ExecuteW();
        }

        public static void ExecuteQGap()
        {
            if (HarassMenu.QGap.Enabled && HarassMenu.QBool.Enabled && Q.IsReady())
            {
                var targetGap = TargetSelector.GetTarget(Q.Range * 2,DamageType.Physical);
                if (targetGap != null)
                {
                    var bestMinion = MinionManager.BestGap(targetGap);
                    if (bestMinion != null && LocalPlayer.ManaPercent >= 50)
                    {
                        if (bestMinion.Distance(targetGap) < targetGap.DistanceToPlayer() && targetGap.DistanceToPlayer() >= 250)
                        {
                            Q.CastOnUnit(bestMinion);
                        }
                    }
                }
            }
        }

        public static void ExecuteQ()
        {
            if (Combo.qDelay < Game.Time && HarassMenu.QBool.Enabled && Q.IsReady())
            {
                var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
                if (target != null)
                {
                    if (!ComboMenu.markedKey.Active)
                    {
                        if (Combo.lastCast < Game.Time)
                        {
                            Q.CastOnUnit(target);
                            Combo.qDelay = Game.Time + 0.5f;
                        }
                    }

                    if (ComboMenu.markedKey.Active)
                    {
                        if (Q.CanExecute(target))
                        {
                            Q.CastOnUnit(target);
                            Combo.qDelay = Game.Time + 0.5f;
                        }
                    }

                    if (ComboMenu.markedKey.Active && target.HasBuff("ireliamark"))
                    {
                        Q.CastOnUnit(target);
                        Combo.qDelay = Game.Time + 0.5f;
                    }
                }
            }
        }

        public static void ExecuteE()
        {
            if (HarassMenu.EBool.Enabled && E.IsReady())
            {
                var targetE = TargetSelector.GetTarget(E.Range - 150,DamageType.Physical);
                if (targetE != null)
                {
                    if (targetE.DistanceToPlayer()         <= E.Range   &&
                        Environment.TickCount - E.LastCastAttemptTime > 1000       &&
                        E.Name                   == "IreliaE" &&
                        Combo.lastCast                     < Game.Time)
                    {
                        var pathStartPos = targetE.Path[0];
                        var pathEndPos   = targetE.Path[targetE.Path.Length - 1];
                        var pathNorm     = (pathEndPos - pathStartPos).Normalized();
                        var tempPred     = targetE.PredictedPosition(1.2f);

                        if (targetE.Path.Length <= 1)
                        {
                            var cast1 = LocalPlayer.Position +
                                        (targetE.Position - LocalPlayer.Position).Normalized() * 900;
                            if (cast1.DistanceToPlayer() <= 1000)
                            {
                                E.Cast(cast1);
                                Combo.e1Pos    = cast1;
                                Combo.lastCast = Game.Time + 1;
                                return;
                            }
                        }

                        if (targetE.Path.Length > 1)
                        {
                            if (tempPred != Vector3.Zero)
                            {
                                var dist1 = tempPred.DistanceToPlayer();
                                if (dist1 <= 900)
                                {
                                    var dist2 = targetE.DistanceToPlayer();
                                    if (dist1 < dist2)
                                    {
                                        pathNorm *= -1;
                                    }

                                    var cast2 = Definitions.RayDistance(targetE.Position,
                                                                        pathNorm,
                                                                        LocalPlayer.Position,
                                                                        900);
                                    if (cast2.DistanceToPlayer() <= 1000)
                                    {
                                        E.Cast(cast2);
                                        Combo.e1Pos = cast2;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (HarassMenu.EBool.Enabled && E.IsReady())
            {
                var targetE = TargetSelector.GetTarget(E.Range,DamageType.Physical);
                if (targetE != null)
                {
                    if (!targetE.HasBuff("ireliamark") && E.Name == "IreliaE2")
                    {
                        if (Combo.e1Pos != Vector3.Zero)
                        {
                            var short1 = false;
                            var short2 = false;

                            E.Delay = 0.25f + targetE.DistanceToPlayer() / 2000f;
                            E.UpdateSourcePosition(Combo.e1Pos, LocalPlayer.Position);

                            var predictions = E.GetPrediction(targetE);
                            var predPos1    = predictions.CastPosition;

                            if (predPos1.DistanceToPlayer() <= E.Range)
                            {
                                var tempCastPos = Vector3.Zero;
                                var start       = Combo.e1Pos.ToVector2();
                                var end         = predPos1.ToVector2();

                                var projection = LocalPlayer.Position.ToVector2().ProjectOn(start, end);
                                if (projection.SegmentPoint != Vector2.Zero)
                                {
                                    var closest = projection.SegmentPoint.ToVector3();

                                    if (closest.DistanceToPlayer()     > E.Range                       ||
                                        predPos1.Distance(Combo.e1Pos) > closest.Distance(Combo.e1Pos) ||
                                        closest.Distance(Combo.e1Pos)  < targetE.MoveSpeed * 0.625 * 1.5)
                                    {
                                        short1 = true;

                                        var pathNormE = (predPos1 - Combo.e1Pos).Normalized();
                                        var extendPos =
                                            Combo.e1Pos +
                                            pathNormE *
                                            (predPos1.Distance(Combo.e1Pos) + targetE.MoveSpeed * 0.625f * 1.5f);

                                        if (extendPos.DistanceToPlayer() < E.Range)
                                        {
                                            tempCastPos = extendPos;
                                        }
                                        else
                                        {
                                            tempCastPos =
                                                Definitions.RayDistance(Combo.e1Pos,
                                                                        pathNormE,
                                                                        LocalPlayer.Position,
                                                                        E.Range);
                                        }
                                    }
                                    else
                                    {
                                        tempCastPos = closest;
                                    }
                                }

                                if (tempCastPos != Vector3.Zero)
                                {
                                    E.Delay = 0.25f + tempCastPos.DistanceToPlayer() / 2000f;
                                    E.Speed = float.MaxValue;

                                    if (projection.SegmentPoint != Vector2.Zero)
                                    {
                                        Combo.castPos = Vector3.Zero;

                                        var closest = projection.SegmentPoint.ToVector3();

                                        if (closest.DistanceToPlayer()     > E.Range                       ||
                                            predPos1.Distance(Combo.e1Pos) > closest.Distance(Combo.e1Pos) ||
                                            closest.Distance(Combo.e1Pos)  < targetE.MoveSpeed * 0.625 * 1.5)
                                        {
                                            short2 = true;
                                            var pathNormE = (predPos1 - Combo.e1Pos).Normalized();
                                            var extendPos = Combo.e1Pos +
                                                            pathNormE *
                                                            (predPos1.Distance(Combo.e1Pos) +
                                                             targetE.MoveSpeed * 0.625f * 1.5f);
                                            if (extendPos.DistanceToPlayer() < E.Range)
                                            {
                                                Combo.castPos = extendPos;
                                            }
                                            else
                                            {
                                                Combo.castPos =
                                                    Definitions.RayDistance(Combo.e1Pos,
                                                                            pathNormE,
                                                                            LocalPlayer.Position,
                                                                            E.Range);
                                            }
                                        }
                                        else
                                        {
                                            Combo.castPos = closest;
                                        }

                                        if (short1                           == short2       &&
                                            Combo.castPos                    != Vector3.Zero &&
                                            Combo.castPos.DistanceToPlayer() <= E.Range)
                                        {
                                            E.Cast(Combo.castPos);
                                            Combo.e1Pos = Vector3.Zero;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void AutoW()
        {
            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);
            if (target != null)
            {
                var prediction = W.GetPrediction(target);
                if (LocalPlayer.HasBuff("ireliawdefense"))
                {
                    if (Game.Time - Definitions.wHeld >= 0 && Game.Time - Definitions.wHeld <= 0.3)
                    {
                        if (Game.Time - Definitions.chargeW > 1.4f)
                        {
                            if (prediction.Hitchance != HitChance.OutOfRange)
                            {
                                W.ShootChargedSpell(prediction.CastPosition);
                            }
                        }
                    }

                    if (Game.Time - Definitions.wHeld >= 1)
                    {
                        if (Game.Time - Definitions.chargeW > ComboMenu.wRelease.Value / 1000f)
                        {
                            if (prediction.Hitchance != HitChance.OutOfRange)
                            {
                                W.ShootChargedSpell(prediction.CastPosition);
                            }
                        }
                    }

                    if (target.DistanceToPlayer() >= W.Range - 100)
                    {
                        if (prediction.Hitchance != HitChance.OutOfRange)
                        {
                            W.ShootChargedSpell(prediction.CastPosition);
                        }
                    }
                }
            }
        }

        public static void ExecuteW()
        {
            if (!HarassMenu.WBool.Enabled)
            {
                return;
            }

            if (!Combo.e1Pos.IsZero)
            {
                return;
            }

            var target = TargetSelector.GetTarget(W.Range - 150,DamageType.Physical);
            if (target != null && !Q.IsReady())
            {
                if (!LocalPlayer.HasBuff("ireliawdefense"))
                {
                    W.StartCharging(target.Position);
                }
            }
        }
    }
}