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
    internal class Varus
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        public Varus()
        {

            Q = new Spell(SpellSlot.Q, 1625);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R, 1075);

            Q.SetCharged("VarusQ","VarusQ",250, 1600, 1.2f);

            Q.SetSkillshot(0.25f, 70f, 1500f, false, SpellType.Line);
            E.SetSkillshot(.50f, 250, 1400, false, SpellType.Circle);
            R.SetSkillshot(.25f, 120, 1950, false, SpellType.Line);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));
                comboMenu.Add(new MenuSlider("q.combo.charge", "(Q) Minimum Charge", 700, 1, 1600));
                comboMenu.Add(new MenuBool("e.combo", "Use (E)", true).SetValue(true));
                comboMenu.Add(new MenuBool("r.combo", "Use (R)", true).SetValue(true));
                Initializer.Config.Add(comboMenu);
            }
            
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(true));
                harassMenu.Add(new MenuSlider("q.harass.charge", "(Q) Minimum Charge", 700, 1, 1600));
                harassMenu.Add(new MenuBool("e.harass", "Use (E)", true).SetValue(false));
                harassMenu.Add(new MenuSlider("harass.mana", "Harass Mana Percent", 30, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var clearmenu = new Menu("LaneClear Settings","LaneClear Settings");
            {
                clearmenu.Add(new MenuBool("q.laneclear", "Use (Q)", true).SetValue(true));
                clearmenu.Add(new MenuSlider("q.clear.charge", "(Q) Minimum Charge", 700, 1, 1600));
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
            Game.OnUpdate += VarusOnUpdate;

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
            if (R.IsReady())
            {
                damage += R.GetDamage(hero);
            }
            return (float)damage;
        }

        private void VarusOnUpdate(EventArgs args)
        {
            Orbwalker.AttackEnabled =(!Q.IsCharging);

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
            var target = TargetSelector.GetTarget(2000, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && Utilities.Enabled("q.combo") && target.IsValidTarget(Q.Range))
                {
                    Q.StartCharging();
                    if (Q.IsCharging)
                    {
                        if (Q.Range >= Utilities.Slider("q.combo.charge"))
                        {
                            Q.Do(target,Utilities.HikiChance("hitchance"));
                        }
                    }
                    
                }
                if (E.IsReady() && Utilities.Enabled("e.combo") && target.IsValidTarget(E.Range))
                {
                    E.Do(target,Utilities.HikiChance("hitchance"));
                }

                if (R.IsReady() && Utilities.Enabled("r.combo"))
                {
                    if (target.IsValidTarget(R.Range - 1000) && R.GetDamage(target) > target.Health)
                    {
                        R.Do(target,Utilities.HikiChance("hitchance"));
                    }

                    if (target.IsValidTarget(ObjectManager.Player.AttackRange) && Q.IsReady() && 
                        target.Health < R.GetDamage(target) + Q.GetDamage(target))
                    {
                        R.Do(target, Utilities.HikiChance("hitchance"));
                    }

                    if (target.IsValidTarget(ObjectManager.Player.AttackRange) && Q.IsReady() &&
                        target.Health < R.GetDamage(target) / 2 && Utilities.IsImmobile(target))
                    {
                        R.Do(target, Utilities.HikiChance("hitchance"));
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

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && Utilities.Enabled("q.harass") && target.IsValidTarget(Q.Range))
                {
                    Q.StartCharging();
                    if (Q.IsCharging)
                    {
                        if (Q.Range >= Utilities.Slider("q.combo.charge"))
                        {
                            Q.Do(target, Utilities.HikiChance("hitchance"));
                        }
                    }
                }

                if (E.IsReady() && Utilities.Enabled("e.harass") && target.IsValidTarget(E.Range))
                {
                    E.Do(target, Utilities.HikiChance("hitchance"));
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
                if (minionlist.Count() >= Utilities.Slider("q.minion.count"))
                {
                    Q.StartCharging();
                    var whitlist = W.GetLineFarmLocation(minionlist);
                    if (Q.IsCharging && Q.Range >= Utilities.Slider("q.clear.charge")
                        && whitlist.MinionsHit >= Utilities.Slider("q.minion.count"))
                    {
                        Q.Cast(whitlist.Position);
                    }
                }
            }

            if (E.IsReady() && Utilities.Enabled("e.laneclear"))
            {
                var minionlist = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);
                var whitlist = W.GetCircularFarmLocation(minionlist);
                if (whitlist.MinionsHit >= Utilities.Slider("e.minion.count"))
                {
                    E.Cast(whitlist.Position);
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

            if (E.IsReady() && Utilities.Enabled("e.jungle"))
            {
                var target = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.IsValidTarget(E.Range));

                if (target != null)
                {
                    E.Do(target, HitChance.High);
                }

            }
        }
    }
}
