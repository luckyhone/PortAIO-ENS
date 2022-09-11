using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Core.Plugins;
using HikiCarry.Core.Predictions;
using SharpDX;
using SPrediction;
using Utilities = HikiCarry.Core.Utilities.Utilities;
namespace HikiCarry.Champions
{
    internal class Caitlyn
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        public Caitlyn()
        {
            

            Q = new Spell(SpellSlot.Q, 1240);
            W = new Spell(SpellSlot.W, 820);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 2000);

            Q.SetSkillshot(0.25f, 60f, 2000f, false, SpellType.Line);
            W.SetSkillshot(1.5f, 20f, float.MaxValue, false, SpellType.Circle);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SpellType.Line);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));
                //comboMenu.Add(new MenuBool("w.combo", "Use (W)", true).SetValue(true));
                comboMenu.Add(new MenuBool("e.combo", "Use (E)", true).SetValue(true));
                comboMenu.Add(new MenuBool("r.combo", "Use (R)", true).SetValue(true));
                Initializer.Config.Add(comboMenu);
            }
            
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(true));
                harassMenu.Add(new MenuBool("e.harass", "Use (E)", true).SetValue(true));
                harassMenu.Add(new MenuSlider("harass.mana", "Harass Mana Percent", 30, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var clearmenu = new Menu("LaneClear Settings","LaneClear Settings");
            {
                clearmenu.Add(new MenuBool("q.laneclear", "Use (Q)", true).SetValue(true));
                clearmenu.Add(new MenuSlider("q.minion.count", "(Q) Min. Minion Count", 3, 1, 5));
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

            var miscMenu = new Menu("Misc Settings", "Misc Settings");
            {
                var dashinterrupter = new Menu("Dash Interrupter", "Dash Interrupter");
                {
                    dashinterrupter.Add(new MenuBool("dash.block", "Use (W) for Block Dash!", true).SetValue(true));
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

            Game.OnUpdate += CaitlynOnUpdate;
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
                && sender.IsValidTarget(W.Range) && W.IsReady())
            {
                var starttick = Environment.TickCount;
                var speed = args.Speed;
                var startpos = sender.ServerPosition.ToVector2();
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
                        W.Cast(endpos);
                    }
                }
            }
        }

        private void OnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (sender.IsEnemy && sender is AIHeroClient)
            {
                var arrivetime = sender.Position.Distance(args.EndPos.ToVector3())/args.Speed;
                var spelltime = ObjectManager.Player.Position.Distance(args.EndPos.ToVector3())/W.Speed + W.Range;

                if (arrivetime > spelltime)
                {
                    W.Cast(args.EndPos);
                }
            }
        }

        private void CaitlynOnUpdate(EventArgs args)
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
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && Utilities.Enabled("q.combo") && target.IsValidTarget(Q.Range))
                {
                    Q.Do(target,Utilities.HikiChance("hitchance"));
                }

                if (E.IsReady() && Utilities.Enabled("e.combo") && target.IsValidTarget(E.Range))
                {
                    E.Do(target, Utilities.HikiChance("hitchance"));
                }

                if (R.IsReady() && Utilities.Enabled("r.combo") && target.IsValidTarget(R.Range) && R.GetDamage(target) > target.Health)
                {
                    R.CastOnUnit(target);
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
                    Q.Do(target, HitChance.High);
                }

            }

            if (E.IsReady() && Utilities.Enabled("e.jungle"))
            {
                var target = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionManager.MinionTypes.All,MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.IsValidTarget(W.Range));

                if (target != null)
                {
                    E.Do(target,HitChance.High);
                }

            }
        }
    }
}