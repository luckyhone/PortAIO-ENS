using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;
using Entropy.AIO.Gangplank.Misc;
using Flowers_Karthus.Common;
using Flowers_Viktor;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;
using static Entropy.AIO.Gangplank.Components;

namespace Entropy.AIO.Gangplank.Logics
{
    using static ChampionBase;
    static class Combo
    {
        public static void DoCombo()
        {
            /*if (Orbwalker.Is)
            {
                return;
            }*/

            var target = TargetSelector.GetTarget(E.Range + Definitions.ExplosionRadius,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (target.IsValidTarget())
            {
                var nearestBarrel = BarrelManager.GetNearestBarrel();
                if (nearestBarrel != null)
                {
                    var chainedBarrels = BarrelManager.GetChainedBarrels(nearestBarrel);
                    if (chainedBarrels.Any(x => BarrelManager.BarrelWillHit(x, target)))
                    {
                        //If any of the chained barrels will hit, cast Q on best barrel.
                        var barrelToQ = BarrelManager.GetBestBarrelToQ(chainedBarrels);
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
                    else
                    {
                        //No chained barrels will hit, so let's chain them OR try and triple barrel.
                        if (chainedBarrels.Count >= 2 && ComboMenu.Triple.Enabled)
                        {
                            //There are chained barrels, so let's see if any are in range to be triple comboed.
                            var barrelToComboFrom =
                                chainedBarrels.FirstOrDefault(x => BarrelManager.GetEnemiesInChainRadius(x).Count >= 1);
                            if (barrelToComboFrom == null)
                            {
                                return;
                            }

                            var barrelToQ = BarrelManager.GetBestBarrelToQ(chainedBarrels);
                            if (barrelToQ != null)
                            {
                                if (Q.IsReady())
                                {
                                    var timeWhenCanE = Definitions.LastECast + 500;
                                    var delay        = timeWhenCanE          - Environment.TickCount;
                                    delay = delay <= 0 ? 0 : delay;
                                    var castDelay = ComboMenu.Triple.Enabled ? delay : 0;
                                    DelayAction.Add(castDelay,() =>
                                                      {
                                                          //if(E.Ready)
                                                          Q.Cast(barrelToQ.Object);
                                                      });
                                }

                                //if (barrelToQ.Object.IsInAutoAttackRange() && Orbwalker.Implementation.CanAttack() && MenuManager.Combo["explodeQCooldown"].Enabled)
                                //{
                                //    Orbwalker.Implementation.ForceTarget(barrelToQ.Object);
                                //    Orbwalker.Implementation.Attack(barrelToQ.Object);
                                //}
                            }
                        }
                        else
                        {
                            //Try to set up the triple barrel ourselves.
                            if (ComboMenu.Triple.Enabled &&
                                chainedBarrels.Any(x => x.Object.DistanceToPlayer() <= Q.Range &&
                                                        x.TimeAt1HP - Environment.TickCount          <= 250     &&
                                                        E.Ammo                                > 1        &&
                                                        x.Object.Distance(target) <=
                                                        Definitions.ChainRadius * 2 + Definitions.ExplosionRadius &&
                                                        x.Object.Distance(target) > Definitions.ChainRadius) &&
                                target.Distance(Definitions.Player) <= E.Range + Definitions.ExplosionRadius)
                            {
                                var bestCastPos = ExtendEx.Extend(nearestBarrel.ServerPosition,
                                                                  target.Position,
                                                                  Definitions.ChainRadius - 5);
                                BarrelManager.CastE(bestCastPos);
                            }
                            else
                            {
                                var bestChainPosition = BarrelManager.GetBestChainPosition(target, nearestBarrel);
                                if (bestChainPosition != Vector3.Zero                                     &&
                                    target.IsInRange(Definitions.Player, E.Range)                         &&
                                    DistanceEx.Distance(Definitions.Player, bestChainPosition) <= E.Range &&
                                    E.IsReady()                                                               &&
                                    nearestBarrel.CanChain)
                                {
                                    //Render.Line(nearestBarrel.ServerPosition.ToScreenPosition(), bestChainPosition.ToScreenPosition(), 5, true, Color.Red);
                                    if (BarrelManager.CastE(bestChainPosition))
                                    {
                                        if (Q.IsReady() && nearestBarrel.CanQ)
                                        {
                                            Q.Cast(nearestBarrel.Object);
                                        }
                                        else if (nearestBarrel.CanAA                                          &&
                                                 nearestBarrel.Object.InAutoAttackRange(Definitions.Player) &&
                                                 Orbwalker.CanAttack())
                                        {
                                            Orbwalker.Attack(nearestBarrel.Object);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!ComboMenu.EFirst.Enabled)
                    {
                        return;
                    }

                    if (E.IsReady() && E.Ammo > 1)
                    {
                        BarrelManager.CastE(Definitions.Player.Position);
                    }
                }
            }
        }
    }
}