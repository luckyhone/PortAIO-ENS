using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Core.Plugins;
using HikiCarry.Core.Predictions;
using HikiCarry.Core.Utilities;
using SPrediction;
using Color = SharpDX.Color;

namespace HikiCarry.Champions
{
    internal class Quinn
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        internal static string QuinnMarkBuffName => "quinnw";

        public Quinn()
        {

            Q = new Spell(SpellSlot.Q, 1000);
            E = new Spell(SpellSlot.E, 700);
            W = new Spell(SpellSlot.W, 2100);
            R = new Spell(SpellSlot.R, 550);

            E.SetTargetted(0.25f, 2000f);
            Q.SetSkillshot(0.25f, 90f, 1550, true, SpellType.Line);
            

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));
                comboMenu.Add(new MenuBool("e.combo", "Use (E)", true).SetValue(true));
                Initializer.Config.Add(comboMenu);
            }

            var esettings = new Menu("(E) Settings", "(E) Settings");
            {
                foreach (var enemy in GameObjects.EnemyHeroes)
                {
                    esettings.Add(new MenuBool("e." +enemy.CharacterName, "(E): "+ enemy.CharacterName, true).SetValue(true));
                }
                Initializer.Config.Add(esettings);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(true));
                harassMenu.Add(new MenuBool("e.harass", "Use (E)", true).SetValue(false));
                harassMenu.Add(new MenuSlider("harass.mana", "Harass Mana Percent", 30, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var clearmenu = new Menu("LaneClear Settings","LaneClear Settings");
            {
                clearmenu.Add(new MenuBool("q.laneclear", "Use (Q)", true).SetValue(true));
                clearmenu.Add(new MenuSlider("q.minion.count", "(Q) Min. Minion Count", 2, 1, 5));
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
            Initializer.Config.Add(new MenuBool("force.orbwalker", "Force Orbwalker to MARKED Enemy", true).SetValue(true));

            Game.OnUpdate += QuinnOnUpdate;
            Orbwalker.OnBeforeAttack += QuinnBeforeAttack;

        }

        private float TotalDamage(AIHeroClient hero)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Q.GetDamage(hero);
            }

            if (E.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            return (float)damage;
        }

        private void QuinnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (args.Target != null && args.Target is AIHeroClient && Utilities.Enabled("force.orbwalker")
                && ((AIHeroClient)args.Target).HasBuff(QuinnMarkBuffName))
            {
                var target = TargetSelector.GetTarget(ObjectManager.Player.AttackRange,
                        DamageType.Physical);
                if (target != null && Utilities.HighChamps.Contains(target.CharacterName))
                {
                    Orbwalker.ForceTarget = (target);
                }
            }
        }

        private void QuinnOnUpdate(EventArgs args)
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
                    Q.Do(target,Utilities.HikiChance("hitchance"));

                }

                if (E.IsReady() && Utilities.Enabled("e.combo") && target.IsValidTarget(E.Range))
                {
                    if (Utilities.Enabled("e."+target.CharacterName))
                    {
                        E.CastOnUnit(target);
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
                    Q.Do(target,Utilities.HikiChance("hitchance"));
                }

                if (E.IsReady() && Utilities.Enabled("e.harass") && target.IsValidTarget(E.Range))
                {
                    if (Utilities.Enabled("e." + target.CharacterName))
                    {
                        E.CastOnUnit(target);
                    }
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
                    Q.Do(target,Utilities.HikiChance("hitchance"));
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
