using System;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Irelia.Logics
{
    #region

    using Misc;
    using SharpDX;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    static class OnInterruptable
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void useE(AIBaseClient targetE)
        {
            try
            {
                if (!E.IsReady())
                {
                    return;
                }

                if (targetE != null)
                {
                    if (targetE.DistanceToPlayer() <= E.Range &&
                        Environment.TickCount - E.LastCastAttemptTime > 1000 &&
                        E.Name == "IreliaE" &&
                        Combo.lastCast < Game.Time)
                    {
                        var pathStartPos = targetE.Path[0];
                        var pathEndPos = targetE.Path[targetE.Path.Length - 1];
                        var pathNorm = (pathEndPos - pathStartPos).Normalized();
                        var tempPred = targetE.PredictedPosition(1.2f);

                        if (targetE.Path.Length <= 1)
                        {
                            var cast1 = LocalPlayer.Position +
                                        (targetE.Position - LocalPlayer.Position).Normalized() * 900;
                            E.Cast(cast1);
                            Combo.e1Pos = cast1;
                            Combo.lastCast = Game.Time + 1;
                            return;
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
                                    E.Cast(cast2);
                                    Combo.e1Pos = cast2;
                                    return;
                                }
                            }
                        }
                    }

                    if (E.Name == "IreliaE2")
                    {
                        if (Combo.e1Pos != null && Combo.e1Pos != Vector3.Zero)
                        {
                            var short1 = false;
                            var short2 = false;

                            E.Delay = 0.25f + targetE.DistanceToPlayer() / 2000f;
                            E.UpdateSourcePosition(Combo.e1Pos, LocalPlayer.Position);

                            var predictions = E.GetPrediction(targetE);
                            var predPos1 = predictions.CastPosition;

                            if (predPos1.DistanceToPlayer() <= E.Range)
                            {
                                var tempCastPos = Vector3.Zero;
                                var start = Combo.e1Pos.ToVector2();
                                var end = predPos1.ToVector2();

                                var projection = LocalPlayer.Position.ToVector2().ProjectOn(start, end);
                                if (projection.SegmentPoint != Vector2.Zero)
                                {
                                    var closest = projection.SegmentPoint.ToVector3();

                                    if (closest.DistanceToPlayer() > E.Range ||
                                        predPos1.Distance(Combo.e1Pos) > closest.Distance(Combo.e1Pos) ||
                                        closest.Distance(Combo.e1Pos) < targetE.MoveSpeed * 0.625 * 1.5)
                                    {
                                        short1 = true;

                                        var pathNormE = (predPos1 - Combo.e1Pos).Normalized();
                                        var extendPos = Combo.e1Pos +
                                                        pathNormE *
                                                        (predPos1.Distance(Combo.e1Pos) +
                                                         targetE.MoveSpeed * 0.625f * 1.5f);

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

                                        if (closest.DistanceToPlayer() > E.Range ||
                                            predPos1.Distance(Combo.e1Pos) > closest.Distance(Combo.e1Pos) ||
                                            closest.Distance(Combo.e1Pos) < targetE.MoveSpeed * 0.625 * 1.5)
                                        {
                                            short2 = true;
                                            var pathNormE = (predPos1 - Combo.e1Pos).Normalized();
                                            var extendPos =
                                                Combo.e1Pos +
                                                pathNormE *
                                                (predPos1.Distance(Combo.e1Pos) + targetE.MoveSpeed * 0.625f * 1.5f);
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

                                        if (short1 == short2 &&
                                            Combo.castPos != Vector3.Zero &&
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
            }catch(Exception e)
            {
                // ignored
            }
        }

        public static void ExecuteE(AIBaseClient sender, Interrupter.InterruptSpellArgs args)
        {
            var heroSender = sender as AIHeroClient;
            if (heroSender == null || !heroSender.IsEnemy)
            {
                return;
            }

            if (!InterrupterMenu.EBool.Enabled)
            {
                return;
            }

            if (!sender.IsValidTarget(E.Range))
            {
                return;
            }

            useE(heroSender);
        }
    }
}