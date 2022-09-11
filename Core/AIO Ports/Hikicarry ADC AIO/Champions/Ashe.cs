using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using ExorAIO.Utilities;
using HikiCarry.Core.Plugins;
using HikiCarry.Core.Predictions;
using LeagueSharpCommon;
using SharpDX;
using SPrediction;
using Color = SharpDX.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Utilities = HikiCarry.Core.Utilities.Utilities;

namespace HikiCarry.Champions
{
    internal class Ashe
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        public Ashe()
        {
            
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1200);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 2500);

            W.SetSkillshot(0.5f, 100, 902, true, SpellType.Cone);
            R.SetSkillshot(0.25f, 130f, 1600f, false, SpellType.Line);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));
                comboMenu.Add(new MenuBool("w.combo", "Use (W)", true).SetValue(true));
                comboMenu.Add(new MenuBool("r.combo", "Use (R)", true).SetValue(true));
                Initializer.Config.Add(comboMenu);
            }
            
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(true));
                harassMenu.Add(new MenuBool("w.harass", "Use (W)", true).SetValue(true));
                harassMenu.Add(new MenuSlider("harass.mana", "Harass Mana Percent",30, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var clearmenu = new Menu("LaneClear Settings","LaneClear Settings");
            {
                clearmenu.Add(new MenuBool("w.laneclear", "Use (W)", true).SetValue(true));
                clearmenu.Add(new MenuSlider("w.minion.count", "(W) Min. Minion Count",3, 1, 5));
                clearmenu.Add(new MenuSlider("clear.mana", "Clear Mana Percent",30, 1, 99));
                Initializer.Config.Add(clearmenu);
            }

            var junglemenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                junglemenu.Add(new MenuBool("q.jungle", "Use (Q)", true).SetValue(true));
                junglemenu.Add(new MenuBool("w.jungle", "Use (W)", true).SetValue(true));
                junglemenu.Add(new MenuSlider("jungle.mana", "Jungle Mana Percent",30, 1, 99));
                Initializer.Config.Add(junglemenu);
            }

            var miscMenu = new Menu("Misc Settings", "Misc Settings");
            {
                var dashinterrupter = new Menu("Dash Interrupter", "Dash Interrupter");
                {
                    dashinterrupter.Add(new MenuBool("dash.block", "Use (R) for Block Dash!", true).SetValue(true));
                    miscMenu.Add(dashinterrupter);
                }
                Initializer.Config.Add(miscMenu);
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

            Game.OnUpdate += AsheOnUpdate;
            AIBaseClient.OnProcessSpellCast += AsheOnSpellCast;
            AIBaseClient.OnNewPath += ObjAiHeroOnOnNewPath;

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

        private void ObjAiHeroOnOnNewPath(AIBaseClient sender, AIBaseClientNewPathEventArgs args)
        {
            if (sender.IsEnemy && sender is AIHeroClient && args.IsDash && Utilities.Enabled("dash.block")
                && sender.IsValidTarget(1000) && R.IsReady())
            {
                var starttick = Utils.TickCount;
                var speed = args.Speed;
                var startpos = sender.ServerPosition.To2D();
                var path = args.Path;
                var forch = args.Path.OrderBy(x => starttick + (int)(1000 * (new Vector3(x.X, x.Y, x.Z).
                    Distance(startpos.ToVector3()) / speed))).FirstOrDefault();
                {
                    var endpos = new Vector3(forch.X, forch.Y, forch.Z);
                    var endtick = starttick + (int)(1000 * (endpos.Distance(startpos.ToVector3())
                        / speed));
                    var duration = endtick - starttick;

                    if (duration < starttick)
                    {
                        R.Cast(endpos);
                    }
                }
            }
        }

        private void AsheOnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Target is AIHeroClient && sender.Spellbook.IsAutoAttack)
            {
                if (Q.IsReady() && Utilities.Enabled("q.combo") && ((AIHeroClient)args.Target).IsValidTarget(ObjectManager.Player.AttackRange) &&
                    sender.HasBuff("asheqcastready") && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    Q.Cast();
                }

                if (Q.IsReady() && Utilities.Enabled("q.harass") && ((AIHeroClient)args.Target).IsValidTarget(ObjectManager.Player.AttackRange) &&
                    sender.HasBuff("asheqcastready") && Orbwalker.ActiveMode == OrbwalkerMode.Harass)
                {
                    Q.Cast();
                }
            }
        }

        private void AsheOnUpdate(EventArgs args)
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
            var target = TargetSelector.GetTarget(2000, DamageType.Physical);
            if (target != null)
            {
                if (W.IsReady() && Utilities.Enabled("w.combo") && target.IsValidTarget(W.Range))
                {
                    W.Do(target,Utilities.HikiChance("hitchance"));
                }

                if (R.IsReady() && Utilities.Enabled("r.combo"))
                {
                    if (target.IsValidTarget(R.Range) && R.GetDamage(target) > target.Health)
                    {
                        R.Do(target,Utilities.HikiChance("hitchance"));
                    }

                    if (target.IsValidTarget(R.Range / 6))
                    {
                        R.Do(target,Utilities.HikiChance("hitchance"));
                    }

                    if (target.IsValidTarget(ObjectManager.Player.AttackRange) && 
                        target.HasBuffOfType(BuffType.Slow))
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

            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target != null)
            {
                if (W.IsReady() && Utilities.Enabled("w.harass") && target.IsValidTarget(W.Range))
                {
                    W.Do(target, Utilities.HikiChance("hitchance"));
                }
            }
        }

        private static void OnClear()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("clear.mana"))
            {
                return;
            }

            if (W.IsReady() && Utilities.Enabled("w.laneclear"))
            {
                var minionlist = MinionManager.GetMinions(ObjectManager.Player.Position, W.Range);
                var whitlist = W.GetCircularFarmLocation(minionlist);
                if (whitlist.MinionsHit >= Utilities.Slider("w.minion.count"))
                {
                    W.Cast(whitlist.Position);
                }
            }
        }

        private static void OnJungle()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("jungle.mana"))
            {
                return;
            }

            if (W.IsReady() && Utilities.Enabled("w.jungle"))
            {
                var target = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(W.Range));

                if (target != null)
                {
                    W.Do(target,HitChance.High);
                }

            }
        }
    }
}