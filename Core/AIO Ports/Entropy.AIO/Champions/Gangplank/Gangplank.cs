using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;
using Entropy.AIO.Gangplank.Logics;
using Entropy.AIO.Gangplank.Misc;
using Flowers_Viktor;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;
using static Entropy.AIO.Gangplank.Components;
using Geometry = LeagueSharpCommon.Geometry.Geometry;

namespace Entropy.AIO.Gangplank
{

    sealed class Gangplank : ChampionBase
    {
        public Gangplank()
        {
            new Spells();
            new Menus();
            new Methods();
            new Drawings(Q, W, E, R);
        }

        public static void OnProcessBasicAttack(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (args.Target == null || !args.Target.IsValid)
            {
                return;
            }

            if (args.Target.Name == "Barrel")
            {
                var barrel = (Barrel) args.Target;
                if (barrel != null)
                {
                    if (barrel.Health > 1)
                    {
                        if (sender.IsMelee)
                        {
                            barrel.Decay((int) (Definitions.Player.AttackCastDelay * 1000 + Game.Ping));
                        }
                        else
                        {
                            barrel.Decay(
                                (int) ((int) (Geometry.Distance(args.To, args.Start) / args.SData.MissileSpeed) +
                                       Game.Ping));
                        }
                    }
                }
            }
        }

        public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == SpellSlot.Q && args.Target.Name == "Barrel")
            {
                var barrel = (Barrel) args.Target;
                Definitions.LastQCast = Environment.TickCount;
                //Console.WriteLine($"[Cast] Distance from barrel: {Player.Distance(barrel.Object)}");
                //1 part
                if (barrel.Object.Distance(Definitions.Player)    >= 585 &&
                    barrel.Object.Distance(Definitions.Player)    <= 660 &&
                    BarrelManager.GetChainedBarrels(barrel).Count == 1) //1 part combo only works at max range.
                {
                    var enemies = BarrelManager.GetEnemiesInChainRadius(barrel);
                    var bestEnemy = enemies.Where(x => x.IsValidTarget()).
                                            OrderBy(x => x.Distance(barrel.Object)).
                                            ThenBy(x => x.Health).
                                            FirstOrDefault();
                    if (bestEnemy != null)
                    {
                        var bestChainPosition = BarrelManager.GetBestChainPosition(bestEnemy, barrel);
                        if (bestChainPosition != Vector3.Zero                         &&
                            bestEnemy.IsInRange(Definitions.Player, E.Range)          &&
                            Definitions.Player.Distance(bestChainPosition) <= E.Range &&
                            E.IsReady())
                        {
                            BarrelManager.CastE(bestChainPosition);
                        }
                    }
                }
                else if (ComboMenu.Triple.Enabled)
                {
                    //Triple barrel
                    var chainedBarrels = BarrelManager.GetChainedBarrels(barrel);
                    if (chainedBarrels.Count > 1)
                    {
                        var barrelsCanChain = chainedBarrels.
                                              Where(x => BarrelManager.GetEnemiesInChainRadius(x).Count > 0 &&
                                                         x.NetworkId                                    != barrel.NetworkId).
                                              ToList();
                        if (barrelsCanChain.Count == 0)
                        {
                            barrelsCanChain = chainedBarrels.
                                              Where(x => BarrelManager.GetEnemiesInChainRadius(x, false).Count > 0 &&
                                                         x.NetworkId !=
                                                         barrel.NetworkId).
                                              ToList();
                        }

                        var bestBarrel = barrelsCanChain.
                                         OrderByDescending(x => x.Created).
                                         ThenBy(x => x.Object.Distance(Definitions.Player)).
                                         FirstOrDefault();
                        if (bestBarrel == null)
                        {
                            return;
                        }

                        var enemiesCanChainTo = BarrelManager.GetEnemiesInChainRadius(bestBarrel);
                        if (enemiesCanChainTo.Count == 0)
                        {
                            enemiesCanChainTo = BarrelManager.GetEnemiesInChainRadius(bestBarrel, false);
                        }

                        if (enemiesCanChainTo.Count > 0)
                        {
                            var bestEnemy = enemiesCanChainTo.
                                            OrderBy(x => x.Health).
                                            ThenBy(x => x.Distance(Definitions.Player)).
                                            FirstOrDefault();
                            if (bestEnemy != null)
                            {
                                var bestChainPosition = BarrelManager.GetBestChainPosition(bestEnemy, bestBarrel);
                                /*
                                 TODO : Change 'GetEnemiesInChainRadius' to just get enemies in chain radius, regardless of ExplosionRadius
                                */
                                if (bestChainPosition                     != Vector3.Zero                &&
                                    DistanceEx.Distance(bestChainPosition, bestEnemy) <= Definitions.ExplosionRadius &&
                                    DistanceEx.Distance(bestChainPosition, bestBarrel.Object) >
                                    Definitions.ExplosionRadius - ComboMenu.ExtraTripleRange.Value &&
                                    Definitions.Player.Distance(bestChainPosition) <= E.Range)
                                {
                                    DelayAction.Add(250,() =>
                                                      {
                                                          //if (E.Ready)
                                                          BarrelManager.CastE(bestChainPosition);
                                                      });
                                }
                            }
                        }
                    }
                }
            }
            else if (sender.IsMe && args.Slot == SpellSlot.E)
            {
                var casted = Environment.TickCount;
                BarrelManager.CastedBarrels.Add(args.To);
                Definitions.LastECast = casted;
            }
        }

        public static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
            {
                var created = Environment.TickCount;
                //Console.WriteLine($"Create delay: {Game.TickCount - LastECast}");
                BarrelManager.Barrels.Add(new Barrel(sender as AIBaseClient, created));
            }
        }

        public static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            AutoExplode();
            AttackNearestBarrel();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo.DoCombo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass.ExecuteQ();
                    break;
            }
        }

        private static void AutoExplode()
        {
            var closestHittingBarrel = BarrelManager.GetBarrelsThatWillHit().FirstOrDefault();
            if (closestHittingBarrel != null)
            {
                var chainedBarrels = BarrelManager.GetChainedBarrels(closestHittingBarrel);
                var barrelToQ      = BarrelManager.GetBestBarrelToQ(chainedBarrels);
                if (barrelToQ != null)
                {
                    if (Q.IsReady())
                    {
                        var timeWhenCanE = Definitions.LastECast + 500;
                        var delay        = timeWhenCanE          - Environment.TickCount;
                        delay = E.Ammo < 1 ? 0 : delay <= 0 ? 0 : delay;
                        var castDelay = ComboMenu.Triple.Enabled ? delay : 0;
                        DelayAction.Add(castDelay,() => Q.Cast(barrelToQ.Object));
                        return;
                    }

                    if (barrelToQ.Object.InAutoAttackRange(Definitions.Player) &&
                        Orbwalker.CanAttack()                                    &&
                        ComboMenu.ExplodeAA.Enabled)
                    {
                        Orbwalker.ForceTarget = (barrelToQ.Object);
                        Orbwalker.Attack(barrelToQ.Object);
                    }
                }
            }
        }

        private static void AttackNearestBarrel()
        {
            if (ComboMenu.AADecayLevel.Enabled && Definitions.Player.Level > ComboMenu.AADecayLevel.Value)
            {
                return;
            }

            var nearestBarrel = BarrelManager.GetNearestBarrel();
            if (nearestBarrel != null)
            {
                if (Definitions.Player.InAutoAttackRange(nearestBarrel.Object) &&
                    nearestBarrel.Health >= 2                                    &&
                    Orbwalker.CanAttack())
                {
                    Orbwalker.Attack(nearestBarrel.Object);
                }
            }
        }

        public static void OnCustomTick(EventArgs args)
        {
            //BarrelManager.Barrels.RemoveAll(x => x.Object.IsDead || x.Object.Name != "Barrel");
            try
            {
                foreach (var barrel in BarrelManager.Barrels)
                {
                    if (barrel.Object.IsDead || barrel.Object.Name != "Barrel")
                    {
                        BarrelManager.Barrels.Remove(barrel);
                        BarrelManager.CastedBarrels.RemoveAll(x => barrel.Object.Distance(x) <= 5);
                    }
                }
            }
            catch (Exception e)
            {
                //Ignore
            }
        }
    }
}