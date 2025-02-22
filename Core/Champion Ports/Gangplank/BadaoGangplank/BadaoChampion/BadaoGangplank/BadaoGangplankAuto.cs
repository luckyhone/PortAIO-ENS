﻿using System;
using System.Linq;
using BadaoGP;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace BadaoKingdom.BadaoChampion.BadaoGangplank
{
    public static class BadaoGangplankAuto
    {
        public static AIHeroClient Player { get { return ObjectManager.Player; } }
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Environment.TickCount - BadaoGangplankCombo.LastCondition >= 100 + Game.Ping)
            {
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
                                    BadaoGangplankCombo.LastCondition = Environment.TickCount;
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
                                if (ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, barrel.Bottle))
                                {
                                    BadaoGangplankCombo.LastCondition = Environment.TickCount;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (BadaoMainVariables.W.IsReady() &&
                BadaoGangplankVariables.AutoWLowHealth.GetValue<MenuBool>().Enabled &&
                BadaoGangplankVariables.AutoWLowHealthValue.GetValue<MenuSlider>().Value >= Player.Health*100/Player.MaxHealth)
            {
                BadaoMainVariables.W.Cast();
            }
            if (BadaoMainVariables.W.IsReady()
                && BadaoGangplankVariables.AutoWCC.GetValue<MenuBool>().Enabled)
            {
                foreach (var bufftype in new BuffType[] {BuffType.Stun, BuffType.Snare, BuffType.Suppression,
                BuffType.Silence,BuffType.Taunt,BuffType.Charm,BuffType.Blind,BuffType.Fear,BuffType.Polymorph})
                {
                    if (Player.HasBuffOfType(bufftype))
                        BadaoMainVariables.W.Cast();
                }
            }
            if (BadaoMainVariables.Q.IsReady())
            {
                foreach (var hero in GameObjects.EnemyHeroes.Where(x => x.BadaoIsValidTarget(BadaoMainVariables.Q.Range) 
                && BadaoMainVariables.Q.GetDamage(x) >= x.Health))
                {
                    BadaoMainVariables.Q.Cast(hero);
                }
            }
        }
    }
}