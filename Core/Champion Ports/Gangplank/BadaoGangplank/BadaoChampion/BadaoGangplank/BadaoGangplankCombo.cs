﻿using System;
using System.Linq;
using BadaoGP;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using PortAIO;

namespace BadaoKingdom.BadaoChampion.BadaoGangplank
{
    public static class BadaoGangplankCombo
    {
        public static int LastCondition;
        public static int Estack { get{ return BadaoMainVariables.E.Instance.Ammo; } }
        public static AIHeroClient  Player { get { return ObjectManager.Player; } }
        public static void BadaoActivate ()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
                return;
            if (Environment.TickCount - LastCondition >= 100 + Game.Ping)
            {
                foreach (var hero in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget()))
                {
                    var pred = Prediction.GetPrediction(hero, 0.5f).UnitPosition;
                    if (BadaoMainVariables.Q.IsReady() && BadaoMainVariables.E.IsReady())
                    {
                        foreach (var barrel in BadaoGangplankBarrels.QableBarrels(350))
                        {
                            var nbarrels = BadaoGangplankBarrels.ChainedBarrels(barrel);
                            if (nbarrels.Any(x => x.Bottle.Distance(pred) <= 990 /*+ hero.BoundingRadius*/)
                                && !nbarrels.Any(x => x.Bottle.Distance(pred) <= 330 /*+ hero.BoundingRadius*/))
                            {
                                for (int i = 990; i >= 400; i -= 20)
                                {
                                    var mbarrels = nbarrels.Where(x => x.Bottle.Distance(pred) <= i).OrderBy(x => x.Bottle.Distance(pred));
                                    foreach (var mbarrel in mbarrels)
                                    {
                                        var pos = mbarrel.Bottle.Position.Extend(pred, i -330);
                                        if (Player.Distance(pos) < BadaoMainVariables.E.Range)
                                        {
                                            Orbwalker.AttackEnabled = false;
                                            Orbwalker.MoveEnabled = false;
                                            DelayAction.Add(100 + Game.Ping, () =>
                                            {
                                                Orbwalker.AttackEnabled = true;
                                                Orbwalker.MoveEnabled = true;
                                            });
                                            BadaoMainVariables.E.Cast(pos);
                                            LastCondition = Environment.TickCount;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        foreach (var barrel in BadaoGangplankBarrels.QableBarrels())
                        {
                            // choi mot luc ba thung
                            var nbarrels = BadaoGangplankBarrels.ChainedBarrels(barrel);
                            if (barrel.Bottle.Distance(pred) <= 330 + 660 + 660 && !(barrel.Bottle.Distance(pred) <= 330 + 660) && BadaoMainVariables.E.Instance.Ammo >= 2)
                            {
                                for (int i = 330 + 660 + 660; i >= 380 + 660; i -= 20)
                                {
                                    if (barrel.Bottle.Distance(pred) <= i)
                                    {
                                        var pos1 = barrel.Bottle.Position.To2D().Extend(pred.To2D(), 660);
                                        var pos2 = barrel.Bottle.Position.To2D().Extend(pred.To2D(), i - 330);
                                        if (BadaoMainVariables.E.IsInRange(pos1) && BadaoMainVariables.E.IsInRange(pos2)
                                            && !pos1.IsWall() && !pos2.IsWall())
                                        {
                                            Orbwalker.AttackEnabled = false;
                                            Orbwalker.MoveEnabled = false;
                                            DelayAction.Add(100 + Game.Ping + 875, () =>
                                            {
                                                Orbwalker.AttackEnabled = true;
                                                Orbwalker.MoveEnabled = true;
                                            });
                                            BadaoMainVariables.E.Cast(pos1);
                                            DelayAction.Add(550, () => BadaoMainVariables.Q.Cast(barrel.Bottle));
                                            DelayAction.Add(875, () => BadaoMainVariables.E.Cast(pos2));
                                            LastCondition = Environment.TickCount + 875;
                                            return;
                                        }
                                    }
                                }
                            }
                            foreach (var nbarrel in nbarrels)
                            {
                                if (nbarrel.Bottle.Distance(pred) <= 330 + 660 + 660 && !(nbarrel.Bottle.Distance(pred) <= 330 + 660) && BadaoMainVariables.E.Instance.Ammo >= 2)
                                {
                                    for (int i = 330 + 660 + 660; i >= 380 + 660; i -= 20)
                                    {
                                        if (nbarrel.Bottle.Distance(pred) <= i)
                                        {
                                            var pos1 = nbarrel.Bottle.Position.To2D().Extend(pred.To2D(), 660);
                                            var pos2 = nbarrel.Bottle.Position.To2D().Extend(pred.To2D(), i - 330);
                                            if (BadaoMainVariables.E.IsInRange(pos1) && BadaoMainVariables.E.IsInRange(pos2)
                                                && !pos1.IsWall() && !pos2.IsWall())
                                            {
                                                Orbwalker.AttackEnabled = false;
                                                Orbwalker.MoveEnabled = false;
                                                DelayAction.Add(100 + Game.Ping + 875, () =>
                                                {
                                                    Orbwalker.AttackEnabled = true;
                                                    Orbwalker.MoveEnabled = true;
                                                });
                                                BadaoMainVariables.E.Cast(pos1);
                                                DelayAction.Add(550, () => BadaoMainVariables.Q.Cast(barrel.Bottle));
                                                DelayAction.Add(875, () => BadaoMainVariables.E.Cast(pos2));
                                                LastCondition = Environment.TickCount + 875;
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (var hero in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget()))
                {
                    var pred = Prediction.GetPrediction(hero, 0.5f).UnitPosition;
                    if (Orbwalker.CanAttack() && BadaoMainVariables.E.IsReady())
                    {
                        foreach (var barrel in BadaoGangplankBarrels.AttackableBarrels(350))
                        {
                            var nbarrels = BadaoGangplankBarrels.ChainedBarrels(barrel);
                            if (nbarrels.Any(x => x.Bottle.Distance(pred) <= 990 /*+ hero.BoundingRadius*/)
                                && !nbarrels.Any(x => x.Bottle.Distance(pred) <= 330 /*+ hero.BoundingRadius*/))
                            {
                                for (int i = 990; i >= 400; i -= 20)
                                {
                                    var mbarrels = nbarrels.Where(x => x.Bottle.Distance(pred) <= i).OrderBy(x => x.Bottle.Distance(pred));
                                    foreach (var mbarrel in mbarrels)
                                    {
                                        var pos = mbarrel.Bottle.Position.Extend(pred, i - 330);
                                        if (Player.Distance(pos) < BadaoMainVariables.E.Range)
                                        {
                                            Orbwalker.AttackEnabled = false;
                                            Orbwalker.MoveEnabled = false;
                                            DelayAction.Add(100 + Game.Ping, () =>
                                            {
                                                Orbwalker.AttackEnabled = true;
                                                Orbwalker.MoveEnabled = true;
                                            });
                                            BadaoMainVariables.E.Cast(pos);
                                            LastCondition = Environment.TickCount;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        foreach (var barrel in BadaoGangplankBarrels.AttackableBarrels())
                        {
                            // choi mot luc ba thung
                            var nbarrels = BadaoGangplankBarrels.ChainedBarrels(barrel);
                            if (barrel.Bottle.Distance(pred) <= 330 + 660 + 660 && !(barrel.Bottle.Distance(pred) <= 330 + 660) && BadaoMainVariables.E.Instance.Ammo >= 2)
                            {
                                for (int i = 330 + 660 + 660; i >= 380 + 660; i -= 20)
                                {
                                    if (barrel.Bottle.Distance(pred) <= i)
                                    {
                                        var pos1 = barrel.Bottle.Position.To2D().Extend(pred.To2D(), 660);
                                        var pos2 = barrel.Bottle.Position.To2D().Extend(pred.To2D(), i - 330);
                                        if (BadaoMainVariables.E.IsInRange(pos1) && BadaoMainVariables.E.IsInRange(pos2)
                                            && !pos1.IsWall() && !pos2.IsWall())
                                        {
                                            Orbwalker.AttackEnabled = false;
                                            Orbwalker.MoveEnabled = false;
                                            DelayAction.Add(100 + Game.Ping + 875, () =>
                                            {
                                                Orbwalker.AttackEnabled = true;
                                                Orbwalker.MoveEnabled = true;
                                            });
                                            BadaoMainVariables.E.Cast(pos1);
                                            DelayAction.Add(550, () => Player.IssueOrder(GameObjectOrder.AttackUnit, barrel.Bottle));
                                            DelayAction.Add(875, () => BadaoMainVariables.E.Cast(pos2));
                                            LastCondition = Environment.TickCount + 875;
                                            return;
                                        }
                                    }
                                }
                            }
                            foreach (var nbarrel in nbarrels)
                            {
                                if (nbarrel.Bottle.Distance(pred) <= 330 + 660 + 660 && !(nbarrel.Bottle.Distance(pred) <= 330 + 660) && BadaoMainVariables.E.Instance.Ammo >= 2)
                                {
                                    for (int i = 330 + 660 + 660; i >= 380 + 660; i -= 20)
                                    {
                                        if (nbarrel.Bottle.Distance(pred) <= i)
                                        {
                                            var pos1 = nbarrel.Bottle.Position.To2D().Extend(pred.To2D(), 660);
                                            var pos2 = nbarrel.Bottle.Position.To2D().Extend(pred.To2D(), i - 330);
                                            if (BadaoMainVariables.E.IsInRange(pos1) && BadaoMainVariables.E.IsInRange(pos2)
                                                && !pos1.IsWall() && !pos2.IsWall())
                                            {
                                                Orbwalker.AttackEnabled = false;
                                                Orbwalker.MoveEnabled = false;
                                                DelayAction.Add(100 + Game.Ping + 875, () =>
                                                {
                                                    Orbwalker.AttackEnabled = true;
                                                    Orbwalker.MoveEnabled = true;
                                                });
                                                BadaoMainVariables.E.Cast(pos1);
                                                DelayAction.Add(550, () => BadaoMainVariables.Q.Cast(barrel.Bottle));
                                                DelayAction.Add(875, () => BadaoMainVariables.E.Cast(pos2));
                                                LastCondition = Environment.TickCount + 875;
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (var hero in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget()))
                {
                    var pred = Prediction.GetPrediction(hero, 0.5f).UnitPosition;
                    if (BadaoMainVariables.Q.IsReady())
                    {
                        foreach (var barrel in BadaoGangplankBarrels.QableBarrels())
                        {
                            var nbarrels = BadaoGangplankBarrels.ChainedBarrels(barrel);
                            if (nbarrels.Any(x => x.Bottle.Distance(pred) <= 330 /*+ hero.BoundingRadius*/))
                            {
                                Orbwalker.AttackEnabled = false;
                                Orbwalker.MoveEnabled = false;
                                DelayAction.Add(100 + Game.Ping, () =>
                                {
                                    Orbwalker.AttackEnabled = true;
                                    Orbwalker.MoveEnabled = true;
                                });
                                if (BadaoMainVariables.Q.Cast(barrel.Bottle) == CastStates.SuccessfullyCasted)
                                {
                                    LastCondition = Environment.TickCount;
                                    return;
                                }
                            }
                        }
                    }
                }

                foreach (var hero in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget()))
                {
                    var pred = Prediction.GetPrediction(hero, 0.5f).UnitPosition;
                    if (Orbwalker.CanAttack())
                    {
                        foreach (var barrel in BadaoGangplankBarrels.AttackableBarrels())
                        {
                            var nbarrels = BadaoGangplankBarrels.ChainedBarrels(barrel);
                            if (nbarrels.Any(x => x.Bottle.Distance(pred) <= 330 /*+ hero.BoundingRadius*/))
                            {
                                Orbwalker.AttackEnabled = false;
                                Orbwalker.MoveEnabled = false;
                                DelayAction.Add(100 + Game.Ping, () =>
                                {
                                    Orbwalker.AttackEnabled = true;
                                    Orbwalker.MoveEnabled = true;
                                });
                                if (Player.IssueOrder(GameObjectOrder.AttackUnit, barrel.Bottle))
                                {
                                    LastCondition = Environment.TickCount;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (Estack >= 2 && BadaoMainVariables.E.IsReady() && BadaoGangplankVariables.ComboE1.GetValue<MenuBool>().Enabled)
            {
                var target = TargetSelector.GetTarget(BadaoMainVariables.E.Range, DamageType.Physical);
                if( target.BadaoIsValidTarget())
                {
                    var pred = Prediction.GetPrediction(target, 0.5f).UnitPosition;
                    if (!BadaoGangplankBarrels.Barrels.Any(x => x.Bottle.Distance(pred) <= 660 /*+ target.BoundingRadius*/))
                        BadaoMainVariables.E.Cast(pred);
                }
            }
            if (BadaoMainVariables.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(BadaoMainVariables.Q.Range, DamageType.Physical);
                if (target.BadaoIsValidTarget())
                {
                    bool useQ = true;
                    foreach (var barrel in BadaoGangplankBarrels.DelayedBarrels(1000))
                    {
                        var nbarrels = BadaoGangplankBarrels.ChainedBarrels(barrel);
                        if (BadaoMainVariables.E.IsReady()
                            && nbarrels.Any(x => x.Bottle.Distance(target.Position) <= 660 + target.BoundingRadius)
                            && !nbarrels.Any(x => x.Bottle.Distance(target.Position) <= 330 + target.BoundingRadius))
                        {
                            useQ = false;
                            break;
                        }
                        else if (nbarrels.Any(x => x.Bottle.Distance(target.Position) <= 330 + target.BoundingRadius))
                        {
                            useQ = false;
                            break;
                        }
                    }
                    if (BadaoGangplankVariables.ComboQSave.GetValue<MenuBool>().Enabled)
                    {
                        foreach (var barrel in BadaoGangplankBarrels.DelayedBarrels(10000))
                        {
                            var nbarrels = BadaoGangplankBarrels.ChainedBarrels(barrel);
                            foreach (var hero in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget()))
                            {
                                if (nbarrels.Any(x => x.Bottle.Distance(hero.Position) <= 330 + hero.BoundingRadius))
                                {
                                    useQ = false;
                                    break;
                                }
                            }
                            if (useQ == false)
                                break;
                        }
                    }
                    if (useQ)
                    {
                        BadaoMainVariables.Q.Cast(target);
                    }
                }
            }
        }
    }
}