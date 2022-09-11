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
using SPrediction;
using Color = SharpDX.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;

namespace HikiCarry.Champions
{
    internal class Tristana
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        public Tristana()
        {

            Q = new Spell(SpellSlot.Q, 585);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 630);
            R = new Spell(SpellSlot.R, 630);

            W.SetSkillshot(0.25f, 150, 1200, false, SpellType.Circle);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));
                comboMenu.Add(new MenuBool("q.combo.debuff", "Use (Q) If Enemy Debuffed", true).SetValue(true));
                comboMenu.Add(new MenuBool("e.combo", "Use (E)", true).SetValue(true));
                comboMenu.Add(new MenuBool("r.combo", "Use (R)", true).SetValue(true));
                Initializer.Config.Add(comboMenu);
            }

            var esettings = new Menu("(E) Settings", "(E) Settings");
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    esettings.Add(new MenuBool("e." +enemy.CharacterName, "(E): "+ enemy.CharacterName, true).SetValue(true));
                }
                Initializer.Config.Add(esettings);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(false));
                harassMenu.Add(new MenuBool("e.harass", "Use (E)", true).SetValue(true));
                harassMenu.Add(new MenuSlider("harass.mana", "Harass Mana Percent", 30, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var clearmenu = new Menu("LaneClear Settings","LaneClear Settings");
            {
                clearmenu.Add(new MenuBool("q.laneclear", "Use (Q)", true).SetValue(true));
                clearmenu.Add(new MenuSlider("q.minion.count", "(Q) Min. Minion Count", 3, 1, 5));

                clearmenu.Add(new MenuBool("e.laneclear", "Use (E)", true).SetValue(true));
                clearmenu.Add(new MenuSlider("e.minion.count", "(E) Min. Minion Count", 3, 1, 5));
                clearmenu.Add(new MenuSlider("clear.mana", "Clear Mana Percent", 30, 1, 99));
                Initializer.Config.Add(clearmenu);
            }

            var junglemenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                junglemenu.Add(new MenuBool("q.jungle", "Use (Q)", true).SetValue(true));
                junglemenu.Add(new MenuBool("e.jungle", "Use (E)", true).SetValue(true));
                junglemenu.Add(new MenuSlider("jungle.mana", "Jungle Mana Percent", 30, 1, 99));
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
            Game.OnUpdate += TristanaOnUpdate;

        }

        private float TotalDamage(AIHeroClient hero)
        {
            var damage = 0d;
 
            if (W.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            if (E.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            if (R.IsReady())
            {
                damage += R.GetDamage(hero);
            }
            return (float)damage;
        }

        private void TristanaOnUpdate(EventArgs args)
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
            var target = TargetSelector.GetTarget(800, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && Utilities.Enabled("q.combo") && target.IsValidTarget(Q.Range))
                {
                    if (Utilities.Enabled("q.combo.debuff") && target.HasBuff("tristanaecharge"))
                    {
                        Q.Cast();
                    }

                    if (!Utilities.Enabled("q.combo.debuff"))
                    {
                        Q.Cast();
                    }

                }

                if (E.IsReady() && Utilities.Enabled("e.combo") && target.IsValidTarget(E.Range))
                {
                    if (Utilities.Enabled("e."+target.CharacterName))
                    {
                        E.CastOnUnit(target);
                    }
                }

                if (R.IsReady() && Utilities.Enabled("r.combo"))
                {
                    if (target.IsValidTarget(R.Range) && R.GetDamage(target) > target.Health)
                    {
                        R.CastOnUnit(target);
                    }
                }
            }

        }

        private static void OnMixed()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("harass.mana"))
            {
                return;
            }

            var target = TargetSelector.GetTarget(800, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && Utilities.Enabled("q.harass") && target.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }

                if (E.IsReady() && Utilities.Enabled("e.harass") && Utilities.Enabled("e." + target.CharacterName)
                    && target.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(target);
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
                if (minionlist != null && minionlist.Count() >= Utilities.Slider("q.minion.count"))
                {
                    Q.Cast();
                }
            }

            if (E.IsReady() && Utilities.Enabled("e.laneclear"))
            {
                var minionlist = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range);
                if (minionlist != null && minionlist.Count() >= Utilities.Slider("e.minion.count"))
                {
                    E.CastOnUnit(minionlist[0]);
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
                var target = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (target != null)
                {
                    Q.Cast();
                }
            }

            if (E.IsReady() && Utilities.Enabled("e.jungle"))
            {
                var target = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.IsValidTarget(E.Range));

                if (target != null)
                {
                    E.CastOnUnit(target);
                }
            }

        }
    }
}
