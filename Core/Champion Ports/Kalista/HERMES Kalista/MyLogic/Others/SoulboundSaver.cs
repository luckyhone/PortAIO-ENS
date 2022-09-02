using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace HERMES_Kalista.MyLogic.Others
{
    public static class SoulboundSaver
    {
        private static AIHeroClient _connectedAlly;
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
                if (_connectedAlly != null && Program.R.IsReady())
                {
                    //if ((!(sender is AIHeroClient) || args.SData.IsAutoAttack()) && args.Target != null &&
                    if ((!(sender is AIHeroClient)) && args.Target != null &&
                        args.Target.NetworkId == _connectedAlly.NetworkId)
                    {
                        _incomingDamage.Add(
                            _connectedAlly.Position.Distance(sender.Position)/args.SData.MissileSpeed +
                            Game.Time, (float) sender.GetAutoAttackDamage(_connectedAlly));
                    }
                    else if (sender is AIHeroClient)
                    {
                        var attacker = (AIHeroClient) sender;
                        var slot = attacker.GetSpellSlot(args.SData.Name);

                        if (slot != SpellSlot.Unknown)
                        {
                            if (slot == attacker.GetSpellSlot("SummonerDot") && args.Target != null &&
                                args.Target.NetworkId == _connectedAlly.NetworkId)
                            {
                                _instantDamage.Add(Game.Time + 2,
                                    (float) attacker.GetSummonerSpellDamage(_connectedAlly, SummonerSpell.Ignite));
                            }
                            else if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R) &&
                                     ((args.Target != null && args.Target.NetworkId == _connectedAlly.NetworkId) ||
                                      args.To.Distance(_connectedAlly.Position) <
                                      Math.Pow(args.SData.LineWidth, 2)))
                            {
                                _instantDamage.Add(Game.Time + 2, (float) attacker.GetSpellDamage(_connectedAlly, slot));
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
            if (!Program.ComboMenu.GetValue<MenuBool>("RComboSupport").Enabled || ObjectManager.Player.IsRecalling() || ObjectManager.Player.InFountain())
                return;

            if (_connectedAlly == null)
            {
                _connectedAlly = GameObjects.AllyHeroes.FirstOrDefault(a => a.HasBuff("kalistacoopstrikeally"));
                return;
            }
            else
            {
                if (IncomingDamage > _connectedAlly.Health && _connectedAlly.CountEnemyHeroesInRange(500) > 0)
                {
                    Program.R.Cast();
                }
                else
                {
                    if ((_connectedAlly.CharacterName == "Blitzcrank" || _connectedAlly.CharacterName == "Skarner" ||
                         _connectedAlly.CharacterName == "TahmKench"))
                    {
                        foreach (
                            var unit in
                                ObjectManager.Get<AIHeroClient>()
                                    .Where(
                                        h => h.IsEnemy && h.IsHPBarRendered && _connectedAlly.Distance(h.Position) > 800)
                            )
                        {
                            // Get buffs
                            for (int i = 0; i < unit.Buffs.Count(); i++)
                            {
                                // Check if the Soulbound is in a good range
                                var enemy = GameObjects.EnemyHeroes.Where(x => _connectedAlly.Distance(unit.Position) > 800);
                                // Check if the Soulbound is a Blitzcrank
                                // Check if the enemy is hooked
                                // Check if target was far enough for ult
                                if ((unit.Buffs[i].Name.ToLower() == "rocketgrab2" ||
                                     unit.Buffs[i].Name == "skarnerimpale".ToLower() ||
                                     unit.Buffs[i].Name.ToLower() == "tahmkenchwdevoured") &&
                                    unit.Buffs[i].IsActive && enemy.Count() > 0)
                                {
                                    Program.R.Cast();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}