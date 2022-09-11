using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Core.Plugins;
using HikiCarry.Core.Predictions;
using HikiCarry.Core.Utilities;
using LeagueSharpCommon;
using PortAIO;
using SPrediction;
using Color = SharpDX.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;

namespace HikiCarry.Champions
{
    internal class Sivir
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        public Sivir()
        {

            Q = new Spell(SpellSlot.Q, 1250f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SpellType.Line);

            SpellDatabase.InitalizeSpellDatabase();

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));
                comboMenu.Add(new MenuBool("w.combo", "Use (W)", true).SetValue(true));
                Initializer.Config.Add(comboMenu);
            }
            
            var shieldmenu = new Menu("Shield Settings", "Shield Settings");
            {
                shieldmenu.Add(new MenuBool("sivir.shield", "Use Shield!", true).SetValue(true));
                shieldmenu.Add(new MenuSeparator("info.sivir.1", "                       Evadeable Spells")).SetFontColor(SharpDX.Color.Yellow);

                foreach (var spell in HeroManager.Enemies.SelectMany(enemy => SpellDatabase.EvadeableSpells.Where(p => p.ChampionName == enemy.CharacterName && p.IsSkillshot)))
                {
                    shieldmenu.Add(new MenuBool($"e.protect.{spell.SpellName}",
                        $"{spell.ChampionName} ({spell.Slot})").SetValue(true));
                }

                foreach (var spell in HeroManager.Enemies.SelectMany(enemy => SpellDatabase.TargetedSpells.Where(p => p.ChampionName == enemy.CharacterName && p.IsTargeted)))
                {
                    shieldmenu.Add(new MenuBool($"e.protect.targetted.{spell.SpellName}",
                            $"{spell.ChampionName} ({spell.Slot})").SetValue(true));

                }
                Initializer.Config.Add(shieldmenu);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(true));
                harassMenu.Add(new MenuSlider("harass.mana", "Harass Mana Percent",30, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var clearmenu = new Menu("LaneClear Settings","LaneClear Settings");
            {
                clearmenu.Add(new MenuBool("q.laneclear", "Use (Q)", true).SetValue(true));
                clearmenu.Add(new MenuSlider("q.minion.count", "(Q) Min. Minion Count",3, 1, 5));
                clearmenu.Add(new MenuBool("w.laneclear", "Use (W)", true).SetValue(true));
                clearmenu.Add(new MenuSlider("w.minion.count", "(W) Min. Minion Count",3, 1, 5));
                clearmenu.Add(new MenuSlider("clear.mana", "Clear Mana Percent",30, 1, 99));
                Initializer.Config.Add(clearmenu);
            }

            var junglemenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                junglemenu.Add(new MenuBool("q.jungle", "Use (Q)", true).SetValue(true));
                junglemenu.Add(new MenuBool("w.jungle", "Use (Q)", true).SetValue(true));
                junglemenu.Add(new MenuSlider("jungle.mana", "Jungle Mana Percent",30, 1, 99));
                Initializer.Config.Add(junglemenu);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                var drawDamageMenu = new MenuBool("RushDrawEDamage", "Combo Damage").SetValue(true);
                var drawFill = new MenuColor("RushDrawEDamageFill", "Combo Damage Fill",Color.Gold);

                var damageDraws = drawMenu.Add(new Menu("Damage Draws", "Damage Draws"));
                damageDraws.Add(drawDamageMenu);
                damageDraws.Add(drawFill);
                
                Initializer.Config.Add(drawMenu);
            }
            Game.OnUpdate += SivirOnUpdate;
            AIBaseClient.OnDoCast += SivirOnProcess;
            AIBaseClient.OnProcessSpellCast += SivirOnSpellCast;

        }

        private float TotalDamage(AIHeroClient hero)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Q.GetDamage(hero);
            }
            if (W.IsReady())
            {
                damage += W.GetDamage(hero);
            }
           
            return (float)damage;
        }

        private void SivirOnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe && W.IsReady() && !Utilities.Enabled("w.combo"))
            {
                return;
            }

            if (ObjectManager.Player.Spellbook.IsAutoAttack && args.Target is AIHeroClient && Orbwalker.ActiveMode == OrbwalkerMode.Combo
                && sender.Type == GameObjectType.AIHeroClient)
            {
                W.Cast();
            }
        }

        private void SivirOnProcess(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (!Initializer.Config["Shield Settings"]["sivir.shield"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if (sender.IsEnemy && sender is AIHeroClient &&
                    Initializer.Config["Shield Settings"]["e.protect." + args.SData.Name] != null &&
                    Initializer.Config["Shield Settings"]["e.protect." + args.SData.Name].GetValue<MenuBool>().Enabled
                    && args.End.Distance(ObjectManager.Player.Position) < 200)
                {
                    E.Cast();
                }

                if (sender.IsEnemy && sender is AIHeroClient && args.Target.IsMe && args.Target != null &&
                    Initializer.Config["Shield Settings"]["e.protect.targetted." + args.SData.Name] != null &&
                    Initializer.Config["Shield Settings"]["e.protect.targetted." + args.SData.Name].GetValue<MenuBool>()
                        .Enabled
                    && args.Target.IsMe)
                {
                    E.Cast();
                }
            }
            catch (Exception)
            {
                //
            }
        }

        private void SivirOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    OnCombo();
                    break;
                case OrbwalkerMode.Harass:
                    OnMixed();
                    break;
                case OrbwalkerMode.LaneClear:
                    OnClear();
                    OnJungle();
                    break;
            }
        }

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && Utilities.Enabled("q.combo") && target.IsValidTarget(Q.Range))
                {
                    Q.Do(target, Utilities.HikiChance("hitchance"));
                }
            }

        }

        private static void OnMixed()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("harass.mana"))
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && Utilities.Enabled("q.harass") && target.IsValidTarget(Q.Range))
                {
                    Q.Do(target, Utilities.HikiChance("hitchance"));
                }
                
            }
        }

        private static void OnClear()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("clear.mana"))
            {
                return;
            }

            if (Q.IsReady() && Utilities.Enabled("q.laneclear"))
            {
                var minionlist = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);
                var whitlist = W.GetLineFarmLocation(minionlist);
                if (whitlist.MinionsHit >= Utilities.Slider("q.minion.count"))
                {
                    Q.Cast(whitlist.Position);
                }
            }

            if (W.IsReady() && Utilities.Enabled("w.laneclear"))
            {
                var minionlist = MinionManager.GetMinions(ObjectManager.Player.AttackRange);
                if (minionlist.Count() >= Utilities.Slider("w.minion.count") && minionlist != null)
                {
                    W.Cast();
                }
            }
        }

        private static void OnJungle()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("jungle.mana"))
            {
                return;
            }

            if (Q.IsReady() && Utilities.Enabled("q.jungle"))
            {
                var target = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionManager.MinionTypes.All,MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (target != null)
                {
                    Q.Do(target,HitChance.High);
                }
            }

            if (W.IsReady() && Utilities.Enabled("w.jungle"))
            {
                var target = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.IsValidTarget(ObjectManager.Player.AttackRange));

                if (target != null)
                {
                    W.Cast();
                }
            }
        }
    }
}
