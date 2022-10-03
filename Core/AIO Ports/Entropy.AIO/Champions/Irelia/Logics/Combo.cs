using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Irelia.Logics
{
    #region

    using System.Collections.Generic;
    using System.Linq;
    using Misc;
    using SharpDX;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    class Combo
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static Dictionary<uint, AIBaseClient> posibleTargets = new Dictionary<uint, AIBaseClient>();
        public static Vector3                        e1Pos          = Vector3.Zero;
        public static Vector3                        castPos        = Vector3.Zero;

        public static Vector3 multiPos  = Vector3.Zero;
        public static Vector3 multiPos2 = Vector3.Zero;

        public static float lastCast;
        public static float qDelay;

        private static float triesMulti;

        public static void ExecuteCombo()
        {
            ExecuteQ();
            ExecuteFirstE();
            ExecuteR();
            ExecuteSecondE();
            ExecuteQGap();
            ExecuteW();
        }

        public static void ExecuteQ()
        {
            if (qDelay < Game.Time && ComboMenu.QBool.Enabled && Q.IsReady())
            {
                if (ComboMenu.priorityMarked.Enabled)
                {
                    var enemy = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(Q.Range) && x.HasBuff("ireliamark"));
                    if (enemy != null)
                    {
                        Q.CastOnUnit(enemy);
                        qDelay = Game.Time + 0.5f;
                    }
                }

                var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
                if (target != null)
                {
                    if (!ComboMenu.markedKey.Active)
                    {
                        if (lastCast < Game.Time)
                        {
                            Q.CastOnUnit(target);
                            qDelay = Game.Time + 0.5f;
                        }
                    }

                    if (ComboMenu.markedKey.Active)
                    {
                        if (Q.CanExecute(target))
                        {
                            Q.CastOnUnit(target);
                            qDelay = Game.Time + 0.5f;
                        }
                    }

                    if (ComboMenu.markedKey.Active && target.HasBuff("ireliamark"))
                    {
                        Q.CastOnUnit(target);
                        qDelay = Game.Time + 0.5f;
                    }
                }

                if (ComboMenu.jumpAround.Enabled && ComboMenu.stackMana.Value <= LocalPlayer.ManaPercent && e1Pos.IsZero)
                {
                    if (ComboMenu.stackPassive.Enabled &&
                        (!LocalPlayer.HasBuff("ireliapassivestacks") ||
                         LocalPlayer.GetBuffCount("ireliapassivestacks") < 4))
                    {
                        if (target != null)
                        {
                            var bestJump = MinionManager.BestJump(target);
                            if (bestJump != null)
                            {
                                Q.CastOnUnit(bestJump);
                            }
                        }
                    }


                    if (!ComboMenu.stackPassive.Enabled)
                    {
                        if (target != null)
                        {
                            var bestJump = MinionManager.BestJump(target);
                            if (bestJump != null)
                            {
                                Q.CastOnUnit(bestJump);
                            }
                        }
                    }
                }
            }
        }

        public static void ExecuteQGap()
        {
            if (ComboMenu.QGap.Enabled && ComboMenu.QBool.Enabled && Q.IsReady())
            {
                var targetGap = TargetSelector.GetTarget(Q.Range * 2,DamageType.Physical);
                if (targetGap != null)
                {
                    var bestMinion = MinionManager.BestGap(targetGap);
                    if (bestMinion != null && LocalPlayer.Mana >= 50)
                    {
                        if (bestMinion.Distance(targetGap) < targetGap.DistanceToPlayer() && targetGap.DistanceToPlayer() >= 250)
                        {
                            Q.CastOnUnit(bestMinion);
                        }
                    }
                }
            }
        }

        public static void ExecuteFirstE()
        {
            try
            {
                if (ComboMenu.EBool.Enabled && E.IsReady())
                {
                    var targetE = TargetSelector.GetTarget(E.Range - 150, DamageType.Physical);
                    if (targetE != null)
                    {
                        MultiE(targetE);
                        if (targetE.DistanceToPlayer() <= E.Range &&
                            Environment.TickCount - E.LastCastAttemptTime > 1000 &&
                            E.Name == "IreliaE" &&
                            lastCast < Game.Time &&
                            triesMulti < Game.Time)
                        {
                            var pathStartPos = targetE.Path[0];
                            var pathEndPos = targetE.Path[targetE.Path.Length - 1];
                            var pathNorm = (pathEndPos - pathStartPos).Normalized();
                            var tempPred = targetE.PredictedPosition(1.2f);

                            if (targetE.Path.Length <= 1)
                            {
                                var cast1 = LocalPlayer.Position +
                                            (targetE.Position - LocalPlayer.Position).Normalized() * 900;
                                if (cast1.DistanceToPlayer() <= 1000)
                                {
                                    E.Cast(cast1);
                                    e1Pos = cast1;
                                    lastCast = Game.Time + 1;
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
                                            e1Pos = cast2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception )
            {
                // ignored
            }
        }

        public static void ExecuteSecondE()
        {
            if (ComboMenu.EBool.Enabled && E.IsReady())
            {
                var targetE = TargetSelector.GetTarget(E.Range,DamageType.Physical);
                if (targetE != null)
                {
                    if (!targetE.HasBuff("ireliamark") && E.Name == "IreliaE2" && triesMulti < Game.Time)
                    {
                        if (e1Pos != Vector3.Zero)
                        {
                            var short1 = false;
                            var short2 = false;

                            E.Delay = 0.25f + targetE.DistanceToPlayer() / 2000f;
                            E.UpdateSourcePosition(e1Pos, LocalPlayer.Position);

                            var predictions = E.GetPrediction(targetE);
                            var predPos1    = predictions.CastPosition;

                            if (predPos1.DistanceToPlayer() <= E.Range)
                            {
                                var tempCastPos = Vector3.Zero;
                                var start       = e1Pos.ToVector2();
                                var end         = predPos1.ToVector2();

                                var projection = LocalPlayer.Position.ToVector2().ProjectOn(start, end);
                                if (projection.SegmentPoint != Vector2.Zero)
                                {
                                    var closest = projection.SegmentPoint.ToVector3();

                                    if (closest.DistanceToPlayer() > E.Range                 ||
                                        predPos1.Distance(e1Pos)   > closest.Distance(e1Pos) ||
                                        closest.Distance(e1Pos)    < targetE.MoveSpeed * 0.625 * 1.5)
                                    {
                                        short1 = true;

                                        var pathNormE = (predPos1 - e1Pos).Normalized();
                                        var extendPos =
                                            e1Pos +
                                            pathNormE *
                                            (predPos1.Distance(e1Pos) + targetE.MoveSpeed * 0.625f * 1.5f);

                                        if (extendPos.DistanceToPlayer() < E.Range)
                                        {
                                            tempCastPos = extendPos;
                                        }
                                        else
                                        {
                                            tempCastPos =
                                                Definitions.RayDistance(e1Pos, pathNormE, LocalPlayer.Position, E.Range);
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
                                        castPos = Vector3.Zero;

                                        var closest = projection.SegmentPoint.ToVector3();

                                        if (closest.DistanceToPlayer() > E.Range                 ||
                                            predPos1.Distance(e1Pos)   > closest.Distance(e1Pos) ||
                                            closest.Distance(e1Pos)    < targetE.MoveSpeed * 0.625 * 1.5)
                                        {
                                            short2 = true;
                                            var pathNormE = (predPos1 - e1Pos).Normalized();
                                            var extendPos =
                                                e1Pos +
                                                pathNormE *
                                                (predPos1.Distance(e1Pos) + targetE.MoveSpeed * 0.625f * 1.5f);
                                            if (extendPos.DistanceToPlayer() < E.Range)
                                            {
                                                castPos = extendPos;
                                            }
                                            else
                                            {
                                                castPos = Definitions.RayDistance(
                                                    e1Pos,
                                                    pathNormE,
                                                    LocalPlayer.Position,
                                                    E.Range);
                                            }
                                        }
                                        else
                                        {
                                            castPos = closest;
                                        }

                                        if (short1 == short2 && castPos != Vector3.Zero && castPos.DistanceToPlayer() <= E.Range)
                                        {
                                            E.Cast(castPos);
                                            e1Pos = Vector3.Zero;
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

        public static AIBaseClient bestTarget(AIBaseClient target)
        {
            AIBaseClient bestTarget     = null;
            var          lastEnemyCount = 0;
            var          list           = new List<Vector3> {LocalPlayer.Position, Vector3.Zero, target.Position};
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.NetworkId != target.NetworkId && x.IsValidTarget(E.Range)))
            {
                var pred = E.GetPrediction(enemy);
                list[1] = pred.CastPosition;
                if (pred.CollisionObjects != null)
                {
                    var count = pred.CollisionObjects.Count(x => x is AIHeroClient hero && hero.IsEnemy);

                    if (lastEnemyCount < count)
                    {
                        lastEnemyCount = count;
                        bestTarget = enemy;
                    }
                }
            }

            return bestTarget;
        }


        public static void MultiE(AIBaseClient target)
        {
            if (!E.IsReady() || target == null)
            {
                return;
            }

            var prediction = E.GetPrediction(target);
            if (E.Name == "IreliaE" && target.DistanceToPlayer() <= E.Range)
            {
                var best = bestTarget(target);
                if (best != null)
                {
                    var prediction2 = E.GetPrediction(best);
                    var EPOS = prediction.CastPosition +
                               (prediction2.CastPosition - prediction.CastPosition).Normalized() *
                               (prediction.CastPosition.Distance(prediction2.CastPosition) + 275);
                    if (!EPOS.IsZero)
                    {
                        E.Cast(EPOS);
                        triesMulti = Game.Time + 1f;
                        multiPos   = EPOS;
                    }
                }
            }

            if (multiPos != Vector3.Zero && E.Name == "IreliaE2")
            {
                var EPOS = multiPos +
                           (prediction.CastPosition - multiPos).Normalized() * (multiPos.Distance(prediction.CastPosition) + 275);
                if (!EPOS.IsZero)
                {
                    E.Cast(EPOS);
                    multiPos2 = EPOS;
                    multiPos  = Vector3.Zero;
                }
            }
        }

        public static void ExecuteW()
        {
            if (!ComboMenu.WBool.Enabled)
            {
                return;
            }

            if (!e1Pos.IsZero)
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

        public static void ExecuteR()
        {
            // Disabled
            if (ComboMenu.RMode.Index == 2)
            {
                return;
            }

            var target = TargetSelector.GetTarget(ComboMenu.RRange.Value,DamageType.Physical);
            if (target == null)
            {
                return;
            }
            

            if (target.Position.CountEnemyHeroesInRange(400) >= ComboMenu.forceR.Value)
            {
                R.Cast(target);
            }

            if (ComboMenu.wasteR.Value >= target.HealthPercent)
            {
                return;
            }

            // At X Health
            if (ComboMenu.RMode.Index == 0)
            {
                if (target.HealthPercent <= ComboMenu.hpR.Value)
                {
                    R.Cast(target);
                }
            }

            // If Killable
            if (ComboMenu.RMode.Index == 1)
            {
                if (target.Health <=
                    Damage.EDamage(target) + Damage.QDamage(target) + Damage.RDamage(target) * 2 + Damage.WDamage(target))
                {
                    R.Cast(target);
                }
            }
        }
    }
}