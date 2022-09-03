using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Cassiopeia.Misc;
using PortAIO;
using SharpDX;
using Damage = Entropy.AIO.Cassiopeia.Misc.Damage;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using static Bases.ChampionBase;
    using static Components.ComboMenu;

    static class Combo
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteCombo()
        {
            if (LocalPlayer.HasItem(ItemId.Rylais_Crystal_Scepter) && RylaisE.Enabled)
            {
                if (!WStart.Enabled)
                {
                    ExecuteE();
                    ExecuteQ();
                    ExecuteW();
                }
                else
                {
                    ExecuteE();
                    ExecuteW();
                    ExecuteQ();
                }
            }

            if (!WStart.Enabled)
            {
                ExecuteQ();
                ExecuteE();
                ExecuteW();
            }
            else
            {
                ExecuteW();
                ExecuteQ();
                ExecuteE();
            }
        }

        public static void ExecuteQ()
        {
            if (!QBool.Enabled || !Q.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            if (target == null)
            {
                return;
            }

            Q.Cast(target);
        }

        public static void ExecuteR()
        {
            if (!R.IsReady())
            {
                return;
            }

            if (LocalPlayer.CountEnemyHeroesInRange(rRange.Value) != 1)
            {
                return;
            }

            if (RMode.Index == 0)
            {
                var target = TargetSelector.GetTarget(R.Range,DamageType.Magical);
                if (target == null                                          ||
                    FaceR.Enabled && !target.IsFacing(LocalPlayer) ||
                    target.HealthPercent > hpR.Value                          ||
                    target.HealthPercent <= wasteR.Value)
                {
                    return;
                }

                R.Cast(target);
            }

            if (RMode.Index == 1)
            {
                var target = TargetSelector.GetTarget(R.Range,DamageType.Magical);
                if (target == null                                          ||
                    FaceR.Enabled && !target.IsFacing(LocalPlayer) ||
                    target.Health          > Damage.R(target)                   ||
                    target.HealthPercent <= wasteR.Value)
                {
                    return;
                }

                R.Cast(target);
            }
        }

        private static List<Vector2> Grouped(Vector3[] pos)
        {
            var posReal      = pos.Select(item => item.To2D()).ToArray();
            var items        = new List<Vector2>();
            var player2D     = LocalPlayer.Position.To2D();
            var biggestItems = items;
            for (var i = 0; i < posReal.Length; i++)
            {
                items = new List<Vector2>();
                var current  = posReal[i];
                var currentP = current - player2D;
                items.Add(current);

                var angle = 0f;
                for (var j = 0; j < posReal.Length; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var cAngle = currentP.AngleBetweenEx(posReal[j] - player2D);
                    if (cAngle > 0 && cAngle < 72)
                    {
                        items.Add(posReal[j]);
                        if (cAngle > angle)
                        {
                            angle = cAngle;
                        }
                    }
                }

                if (items.Count > biggestItems.Count)
                {
                    biggestItems = items;
                }
            }

            return biggestItems;
        }


        public static Tuple<Vector3, int> GetBestPosition(IEnumerable<AIBaseClient> targets)
        {
            var preds = Grouped(targets.Where(enemy => enemy.IsValidTarget(R.Range + enemy.MoveSpeed * R.Delay)).
                                        Select(item => Definitions.GetMovementPrediction(item, R.Delay)).
                                        Where(pred =>
                                        {
                                            var dst = pred.Distance(LocalPlayer.Position);
                                            return dst < R.Range * R.Range;
                                        }).
                                        ToArray());

            if (preds.Count > 0)
            {
                var final = new Vector3();
                for (var i = 0; i < preds.Count; i++)
                {
                    final.X += preds[i].X;
                    final.Z += preds[i].Y;
                }

                final.X /= preds.Count;
                final.Z /= preds.Count;
                return new Tuple<Vector3, int>(final, preds.Count);
            }

            return null;
        }

        public static void TeamfightR()
        {
            if (!R.IsReady())
            {
                return;
            }

            if (facingTeam.Enabled)
            {
                var bestPosFacing =
                    GetBestPosition(GameObjects.EnemyHeroes.Where(x => x.IsFacing(LocalPlayer)));

                if (bestPosFacing?.Item2 >= teamFight.Value)
                {
                    R.Cast(bestPosFacing.Item1);
                }
            }
            else
            {
                var bestPos = GetBestPosition(GameObjects.EnemyHeroes);
                if (bestPos?.Item2 >= teamFight.Value)
                {
                    R.Cast(bestPos.Item1);
                }
            }
        }


        public static void ExecuteW()
        {
            if (!WBool.Enabled || !W.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(W.Range,DamageType.Magical);
            if (target == null || target.DistanceToPlayer() <= 500)
            {
                return;
            }

            W.Cast(target);
        }

        public static void ExecuteE()
        {
            if (!EBool.Enabled || !E.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(E.Range,DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (Definitions.CachedPoisoned.ContainsKey((uint)target.NetworkId))
            {
                E.CastOnUnit(target);
            }

            if (EPoisonBool.Enabled)
            {
                return;
            }

            E.CastOnUnit(target);
        }
    }
}