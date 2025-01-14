﻿using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace HERMES_Kalista.MyLogic.Others
{
    public static class ZezzysPunisher
    {
        private static Dictionary<float, float> _incomingDamage = new Dictionary<float, float>();
        private static Dictionary<float, float> _instantDamage = new Dictionary<float, float>();

        public static float IncomingDamage
        {
            get { return _incomingDamage.Sum(e => e.Value) + _instantDamage.Sum(e => e.Value); }
        }

        //credits to hellsing, and jquery
        public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (Program.E.IsReady())
                {
                    if ((!(sender is AIHeroClient)) && args.Target != null &&
                        args.Target.NetworkId == ObjectManager.Player.NetworkId)
                    {
                        _incomingDamage.Add(
                            ObjectManager.Player.Position.Distance(sender.Position)/args.SData.MissileSpeed +
                            Game.Time, (float) sender.GetAutoAttackDamage(ObjectManager.Player));
                    }
                    else if (sender is AIHeroClient)
                    {
                        var attacker = (AIHeroClient) sender;
                        var slot = attacker.GetSpellSlot(args.SData.Name);

                        if (slot != SpellSlot.Unknown)
                        {
                            if (slot == attacker.GetSpellSlot("SummonerDot") && args.Target != null &&
                                args.Target.NetworkId == ObjectManager.Player.NetworkId)
                            {
                                _instantDamage.Add(Game.Time + 2,
                                    (float)
                                        attacker.GetSummonerSpellDamage(ObjectManager.Player,
                                            SummonerSpell.Ignite));
                            }
                            else if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R) &&
                                     ((args.Target != null && args.Target.NetworkId == ObjectManager.Player.NetworkId) ||
                                      args.To.Distance(ObjectManager.Player.Position) <
                                      Math.Pow(args.SData.LineWidth, 2)))
                            {
                                _instantDamage.Add(Game.Time + 2,
                                    (float) attacker.GetSpellDamage(ObjectManager.Player, slot));
                            }
                        }
                    }
                }
            }

            if (sender.IsMe)
            {
                if (args.SData.Name == "KalistaExpungeWrapper")
                {
                    DelayAction.Add(250, Orbwalker.ResetAutoAttackTimer);
                }
            }
        }

        public static void OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsRecalling() || ObjectManager.Player.InFountain() || !Program.E.IsReady())
                return;

            if ((ObjectManager.Player.HealthPercent < 10 || IncomingDamage >= ObjectManager.Player.Health) && ObjectManager.Player.CountEnemyHeroesInRange(Program.E.Range) > 0)
            {
                Program.E.Cast();
            }
        }
    }
}