using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Core.Predictions;
using LeagueSharpCommon;
using SharpDX;
using SPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Utilities = HikiCarry.Core.Utilities.Utilities;

 namespace HikiCarry.Champions
{
    internal class Jinx
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        
        public static bool IsMinigun => !ObjectManager.Player.HasBuff("JinxQ");
        public static float MinigunRange => 575f;
        public static bool IsFishBone => ObjectManager.Player.HasBuff("JinxQ");
        public static float FishBoneRange => 75f + 25f * Q.Level;
        public static int FishBoneAoeRadius => 200;
        

        public Jinx()
        {
            Q = new Spell(SpellSlot.Q, ObjectManager.Player.AttackRange);
            W = new Spell(SpellSlot.W, 1490f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 2500f);

            W.SetSkillshot(0.6f, 75f, 3300f, true, SpellType.Line);
            E.SetSkillshot(1.2f, 1f, 1750f, false, SpellType.Circle);
            R.SetSkillshot(0.7f, 140f, 1500f, false, SpellType.Line);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));
                comboMenu.Add(new MenuBool("w.combo", "Use (W)", true).SetValue(true));
                comboMenu.Add(new MenuBool("e.combo", "Use (E)", true).SetValue(true));
                comboMenu.Add(new MenuBool("r.combo", "Use (R)", true).SetValue(true));
                comboMenu.Add(new MenuSlider("ult.distance", "Ult Max Distance", 1500, 1, 2000));
                Initializer.Config.Add(comboMenu);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(true));
                harassMenu.Add(new MenuBool("w.harass", "Use (W)", true).SetValue(true));
                harassMenu.Add(new MenuSlider("harass.mana", "Harass Mana Percent", 30, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var clearmenu = new Menu("LaneClear Settings", "LaneClear Settings");
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
                    dashinterrupter.Add(new MenuBool("dash.block", "Use (E) for Block Dash!", true).SetValue(true));
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

            Game.OnUpdate += JinxOnUpdate;
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
            if (sender.IsEnemy && sender is AIHeroClient && args.IsDash && Initializer.Config["Misc Settings"]["Dash Interrupter"]["dash.block"].GetValue<MenuBool>().Enabled
                && sender.IsValidTarget(W.Range) && W.IsReady())
            {
                var starttick = Utils.TickCount;
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
        private void JinxOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    OnCombo();
                    break;
                case OrbwalkerMode.Harass:
                    OnHarass();
                    break;
                case OrbwalkerMode.LaneClear:
                    OnClear();
                    break;
                case OrbwalkerMode.LastHit:
                OnLastHit();
                break;
            }

            ImmobileTarget();
        }

        private void OnLastHit()
        {
            if (Q.IsReady())
            {
                if (IsFishBone)
                {
                    Q.Cast();
                }
            }
        }

        private void OnCombo()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && Utilities.Enabled("q.combo"))
                {
                    if (IsMinigun && target.Distance(ObjectManager.Player.Position) >= MinigunRange)
                    {
                        Q.Cast();
                    }
                    if (IsFishBone && target.Distance(ObjectManager.Player.Position) <= MinigunRange)
                    {
                        Q.Cast();
                    }
                    if (IsFishBone && target.Distance(ObjectManager.Player.Position) > FishBoneRange)
                    {
                        Q.Cast();
                    }
                }

                if (W.IsReady() && Utilities.Enabled("w.combo"))
                {
                    W.Do(target,Utilities.HikiChance("hitchance"));
                }

                if (R.IsReady() && Utilities.Enabled("r.combo") && 
                    target.IsValidTarget(Utilities.Slider("ult.distance")) && target.Health < R.GetDamage(target))
                {
                    R.Do(target,Utilities.HikiChance("hitchance"));
                }
            }
            
        }
        private void OnHarass()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("harass.mana"))
            {
                return;
            }

            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);
            if (target != null)
            {
                
                if (Q.IsReady() && Utilities.Enabled("q.harass"))
                {
                    if (IsMinigun && target.Distance(ObjectManager.Player.Position) >= MinigunRange)
                    {
                        Q.Cast();
                    }
                    if (IsFishBone && target.Distance(ObjectManager.Player.Position) <= MinigunRange)
                    {
                        Q.Cast();
                    }
                    if (IsFishBone && target.Distance(ObjectManager.Player.Position) > FishBoneRange)
                    {
                        Q.Cast();
                    }
                }

                if (W.IsReady() && target.IsValidTarget(W.Range) && Utilities.Enabled("w.harass"))
                {
                    W.Do(target,Utilities.HikiChance("hitchance"));
                }
            }
        }
        private void OnClear()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("clear.mana"))
            {
                return;
            }

            if (Q.IsReady() && Utilities.Enabled("q.laneclear"))
            {
                var minion = MinionManager.GetMinions(Q.Range);
                if (minion != null && minion.Count() >= Utilities.Slider("q.minion.count"))
                {
                    if (IsMinigun && minion[0].Distance(ObjectManager.Player.Position) >= MinigunRange)
                    {
                        Q.Cast();
                    }
                    if (IsFishBone && minion[0].Distance(ObjectManager.Player.Position) <= MinigunRange)
                    {
                        Q.Cast();
                    }
                }
            }
        }

        private static void ImmobileTarget()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null)
            {
                if (E.IsReady() && Utilities.Enabled("e.combo") && (target.HasBuffOfType(BuffType.Slow)
                                                                    || target.HasBuffOfType(BuffType.Charm) ||
                                                                    target.HasBuffOfType(BuffType.Fear) ||
                                                                    target.HasBuffOfType(BuffType.Stun) ||
                                                                    target.HasBuffOfType(BuffType.Taunt)
                                                                    || target.HasBuffOfType(BuffType.Snare)))
                {
                    E.Cast(target.Position);
                }
               
            }
        }
    }
}
