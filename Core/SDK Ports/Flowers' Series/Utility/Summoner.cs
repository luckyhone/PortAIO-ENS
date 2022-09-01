using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Damages.SummonerSpells;
using EnsoulSharp.SDK.MenuUI;
using Flowers_Series.Common;
using SebbyLib;

namespace Flowers_Series.Utility
{
    internal class Summoner
    {
        private static AIHeroClient Me => Program.Me;
        private static Menu Menu => Tools.Menu;

        private static SpellSlot Ignite = SpellSlot.Unknown;
        private static SpellSlot Exhaust = SpellSlot.Unknown;
        private static SpellSlot Heal = SpellSlot.Unknown;

        private static float IgniteRange = 600f, ExhaustRange = 650f;

        public static void Inject()
        {
            Ignite = Me.GetSpellSlot("SummonerIgnite");
            Exhaust = Me.GetSpellSlot("SummonerExhaust");
            Heal = Me.GetSpellSlot("SummonerHeal");


            var SummonerMenu = Menu.Add(new Menu("Summoner", "Summoner"));
            {
                if (Ignite != SpellSlot.Unknown)
                {
                    var IgniteMenu = SummonerMenu.Add(new Menu("Ignite", "Ignite"));
                    {
                        IgniteMenu.Add(new MenuBool("Enable", "Enabled", Tools.EnableActivator));
                        IgniteMenu.Add(new MenuBool("Combo", "Use In Combo", true));
                        IgniteMenu.Add(new MenuBool("KillSteal", "Use In KillSteal", true));
                    }

                    Manager.WriteConsole("Ignite Menu Load");
                }

                if (Heal != SpellSlot.Unknown)
                {
                    var HealMenu = SummonerMenu.Add(new Menu("Heal", "Heal"));
                    {
                        HealMenu.Add(new MenuBool("Enable", "Enabled", Tools.EnableActivator));
                        HealMenu.Add(new MenuBool("AllyHeal", "AllyHeal", true));
                        HealMenu.Add(new MenuSeparator("Credit", "Credit: Sebby"));
                    }

                    Manager.WriteConsole("Heal Menu Load");
                }

                if (Exhaust != SpellSlot.Unknown)
                {
                    var ExhaustMenu = SummonerMenu.Add(new Menu("Exhaust", "Exhaust"));
                    {
                        ExhaustMenu.Add(new MenuBool("Enable", "Enabled", true));

                        if (GameObjects.EnemyHeroes.Any())
                        {
                            GameObjects.EnemyHeroes.ForEach(i => ExhaustMenu.Add(new MenuBool(i.CharacterName.ToLower(), "Use On" + i.CharacterName, true)));
                        }

                        ExhaustMenu.Add(new MenuSeparator("Credit", "Credit: Kurisu"));
                    }

                    Manager.WriteConsole("Heal Menu Load");
                }
            }

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Ignite != SpellSlot.Unknown && Ignite.IsReady() && Menu["Summoner"]["Ignite"].GetValue<MenuBool>("Enable").Enabled)
            {
                IgniteLogic();
            }

            if (Heal != SpellSlot.Unknown && Heal.IsReady() && Menu["Summoner"]["Heal"].GetValue<MenuBool>("Enable").Enabled)
            {
                HealLogic();
            }

            if (Exhaust != SpellSlot.Unknown && Exhaust.IsReady() && Menu["Summoner"]["Exhaust"].GetValue<MenuBool>("Enable").Enabled)
            {
                ExhaustLogic();
            }
        }

        private static void ExhaustLogic()
        {
            var hid = GameObjects.EnemyHeroes.OrderByDescending(h => h.TotalAttackDamage).FirstOrDefault(h => h.IsValidTarget(ExhaustRange + 250));

            foreach (var hero in GameObjects.AllyHeroes)
            {
                var enemies = TargetSelector.GetTargets(ExhaustRange, DamageType.Physical);

                if (enemies.Count() == 0 || hid == null)
                {
                    continue;
                }

                foreach (var target in enemies)
                {
                    if (!Menu["Summoner"]["Exhaust"].GetValue<MenuBool>(target.CharacterName.ToLower()).Enabled)
                    {
                        continue;
                    }

                    if (target.DistanceToPlayer() > 1250)
                    {
                        continue;
                    }

                    if (target.DistanceToPlayer() <= ExhaustRange)
                    {
                        if (hero.Health / hero.MaxHealth * 100 <= 60)
                        {
                            if (hero.IsFacing(target))
                            {
                                if (target.NetworkId == hid.NetworkId)
                                {
                                    Me.Spellbook.CastSpell(Exhaust, target);
                                }
                            }
                        }

                        if (target.Health / target.MaxHealth * 100 <= 30)
                        {
                            if (!target.IsFacing(hero))
                            {
                                Me.Spellbook.CastSpell(Exhaust, target);
                            }
                        }
                    }
                }
            }
        }

        private static void HealLogic()
        {
            foreach (var ally in GameObjects.AllyHeroes.Where(ally => ally.IsValid && !ally.IsDead && ally.HealthPercent < 50 && Me.Distance(ally.Position) < 700))
            {
                double dmg = OktwCommon.GetIncomingDamage(ally, 1);

                var enemys = ally.CountEnemyHeroesInRange(700);

                if (dmg == 0 && enemys == 0)
                {
                    continue;
                }

                if (!Menu["Summoner"]["Heal"].GetValue<MenuBool>("AllyHeal").Enabled && !ally.IsMe)
                {
                    return;
                }

                if (ally.Health - dmg < enemys * ally.Level * 15)
                {
                    Me.Spellbook.CastSpell(Heal, ally);
                }
                else if (ally.Health - dmg < ally.Level * 10)
                {
                    Me.Spellbook.CastSpell(Heal, ally);
                }
            }
        }

        private static void IgniteLogic()
        {
            var target = Manager.GetTarget(IgniteRange, DamageType.True);

            if (Manager.CheckTarget(target))
            {
                if (Manager.InCombo && Menu["Summoner"]["Ignite"].GetValue<MenuBool>("Combo").Enabled && target.IsValidTarget(IgniteRange) && target.HealthPercent < 20)
                {
                    Me.Spellbook.CastSpell(Ignite, target);
                    return;
                }

                if (GetIgniteDamage(target) > target.Health && target.IsValidTarget(IgniteRange))
                {
                    Me.Spellbook.CastSpell(Ignite, target);
                    return;
                }
            }
        }

        private static double GetIgniteDamage(AIHeroClient target)
        {
            return 50 + 20 * Me.Level - (target.HPRegenRate / 5 * 3);
        }
    }
}