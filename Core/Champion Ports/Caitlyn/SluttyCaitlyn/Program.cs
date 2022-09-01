using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using PortAIO;
using SharpDX;
using SPrediction;

namespace Slutty_Caitlyn
{
    internal class Program
    {
        public const string ChampName = "Caitlyn";
        public const string Menuname = "Slutty Caitlyn";
        public static Menu Config,drawings;
        public static Spell Q, W, E, R;
        public float QMana, EMana;

        private static readonly AIHeroClient Player = ObjectManager.Player;

        public static void Loads()
        {
            Game_OnGameLoad();
        }

        static void Game_OnGameLoad()
        {
            if (Player.CharacterName != ChampName)
                return;

            Q = new Spell(SpellSlot.Q, 1280f);
            W = new Spell(SpellSlot.W, 800f);
            E = new Spell(SpellSlot.E, 980f);
            R = new Spell(SpellSlot.R, 3000f);


            Q.SetSkillshot(0.65f, 90f, 2200f, false, SpellType.Line);
            W.SetSkillshot(1.5f, 1f, 1750f, false, SpellType.Circle);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SpellType.Line);
            R.SetSkillshot(0.7f, 200f, 1500f, false, SpellType.Circle);
            
            Config = new Menu(Menuname, Menuname, true);
            
            drawings = Config.Add(new Menu("Drawings","Drawings"));
            drawings.Add(new MenuBool("qDraw", "Q Drawing").SetValue(true));
            drawings.Add(new MenuBool("eDraw", "W Drawing").SetValue(true));
            drawings.Add(new MenuBool("wDraw", "E Drawing").SetValue(true));
            drawings.Add(new MenuBool("rDraw", "R Drawing").SetValue(true));
            var drawDamageMenu = new MenuBool("RushDrawEDamage", "W Damage").SetValue(true);
            var drawFill = new MenuBool("RushDrawWDamageFill", "W Damage Fill"); // SeaGreen
            drawings.Add(drawDamageMenu);
            drawings.Add(drawFill);
            
            var combo = Config.Add(new Menu("Combo","Combo"));
            combo.Add(new MenuBool("UseQ", "Use Q").SetValue(true));
            combo.Add(new MenuBool("UseQr", "Reduce Use Q Usage").SetValue(true));
            combo.Add(new MenuKeyBind("UseRM", "Semi Manual R",Keys.R, KeyBindType.Press));
            

            var harass =Config.Add(new Menu("Harras","Harras"));
            harass.Add(new MenuBool("UseQH", "Use Q").SetValue(true));
            harass.Add(new MenuBool("UseQrh", "Reduce Use Q Usage").SetValue(true));
            harass.Add(new MenuSlider("useH", "Harras if %Mana >",50));

            var laneclear = Config.Add(new Menu("LaneClear","LaneClear"));
            laneclear.Add(new MenuBool("useQ2L", "Use Q to lane clear").SetValue(true));
            laneclear.Add(new MenuSlider("useQSlider", "Min minions for Q",3, 1, 20));
            laneclear.Add(new MenuSlider("useMSlider", "Lane Clear if %Mana >",50));


            var killsteal = Config.Add(new Menu("KillSteal","KillSteal"));
            killsteal.Add(new MenuBool("KS", "Kill Steal")).SetValue(true);
            killsteal.Add(new MenuBool("useQ2KS", "Use Q ks").SetValue(true));
            killsteal.Add(new MenuBool("useR2KS", "Use R").SetValue(true));

            var misc =Config.Add(new Menu("Misc","Misc"));
            misc.Add(new MenuBool("UseW", "Auto W").SetValue(true));
            misc.Add(new MenuBool("UseQa", "Auto Q On stunned").SetValue(true));


            /*Config.Add(new Menu("Auto Potions", "autoP"));
            Config.Add("autoP").AddItem(new MenuItem("autoPO", "Auto Health Potion").SetValue(true));
            Config.Add("autoP").AddItem(new MenuItem("HP", "Health Potions")).SetValue(true);
            Config.Add("autoP").AddItem(new MenuItem("HPSlider", "Minimum %Health for Potion")).SetValue(new Slider(50));
            Config.Add("autoP").AddItem(new MenuItem("MANA", "Auto Mana Potion").SetValue(true));
            Config.Add("autoP").AddItem(new MenuItem("MANASlider", "Minimum %Mana for Potion")).SetValue(new Slider(50));
            */

            Config.Add(new MenuKeyBind("dashte", "Dash EQ to on target",Keys.C, KeyBindType.Press));

            Config.Add(new MenuKeyBind("fleekey", "Use Flee Mode",Keys.A, KeyBindType.Press));
            
            Config.Attach();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Config.GetValue<MenuKeyBind>("fleekey").Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (E.IsReady())
                {
                    DelayAction.Add(300, () => E.Cast(Game.CursorPos.Extend(Player.Position, 5000)));
                }
            }

            /*
            if (Config.Item("dasht").GetValue<KeyBind>().Active)
            {
                EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (E.IsReady())
                {
                    E.Cast(Game.CursorPos.Extend(Player.Position, 5000));
                }
            }
             */

            if (Config.GetValue<MenuKeyBind>("dashte").Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target == null) return;
                if (E.IsReady() && Q.IsReady()
                    && Player.Mana > Q.Instance.ManaCost + E.Instance.ManaCost)
                {
                    E.Cast(target.Position);
                    Q.Cast(target.Position);
                }
            }

            if (Config.GetValue<MenuKeyBind>("UseRM").Active
                && R.IsReady())
            {
                ManualR();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                Mixed();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                LaneClear();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.None)
            {
                KillSteal();
            }
            AutoQ();
            AutoW();
        }
        
        private static void AutoQ()
        {
            AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null)
                return;

            var qSpell = Config.GetValue<MenuBool>("UseQa").Enabled;

            if (qSpell)
            {
                if ((target.HasBuffOfType(BuffType.Slow)
                     || target.HasBuffOfType(BuffType.Charm)
                     || target.HasBuffOfType(BuffType.Stun)
                     || target.HasBuffOfType(BuffType.Snare)
                     || target.HasBuffOfType(BuffType.Knockup)
                     || target.HasBuffOfType(BuffType.Suppression))
                    && !target.IsZombie())
                {
                    Q.Cast(target);
                }
            }
        }
        
        // NOT MINE, THIS IS SEBBY'S
        static double UnitIsImmobileUntil(AIBaseClient unit)
        {
            var time =
                unit.Buffs.Where(buff =>buff.IsActive && Game.Time <= buff.EndTime &&
                                        (buff.Type == BuffType.Charm
                                         || buff.Type == BuffType.Knockup
                                         || buff.Type == BuffType.Suppression 
                                         || buff.Type == BuffType.Stun
                                         || buff.Type == BuffType.Snare)).Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return (time - Game.Time);
        }
        private static void AutoW()
        {
            foreach (
                var Object in
                ObjectManager.Get<AIBaseClient>().Where(enemy => enemy.Distance(Player.Position) < W.Range
                                                                && enemy.Team
                                                                != Player.Team
                                                                && (enemy.HasBuff("teleport_target"))))
            {
                W.Cast(Object.Position);
            }

            var wSpell = Config.GetValue<MenuBool>("UseW").Enabled;
            var qSpell = Config.GetValue<MenuBool>("UseQa").Enabled;
            if (wSpell)
            {
                foreach (AIHeroClient target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range)))
                {
                    if (target != null)
                    {
                        if (UnitIsImmobileUntil(target) >= W.Delay - 0.5
                            && W.IsReady()
                            && target.IsValidTarget(W.Range)) 
                            W.Cast(target);
                    }
                    if (target != null
                        && qSpell)
                    {
                        if (UnitIsImmobileUntil(target) >= Q.Delay
                            && Q.IsReady()
                            && target.IsValidTarget(Q.Range))
                            Q.Cast(target);
                    }
                }

            }
        }
        
        
        
        private static void Combo()
        {
            var qSpell = Config.GetValue<MenuBool>("UseQ").Enabled;
            var qrSpell = Config.GetValue<MenuBool>("UseQr").Enabled;
            AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var minionsQ = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);
            var qHit = Q.GetLineFarmLocation(minionsQ, 100);

            if ((target != null)
                && !target.IsZombie())
            {

                if (qSpell
                    && !qrSpell
                    && Q.IsReady()
                    && qHit.MinionsHit <= 4
                    && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (qSpell
                    && qrSpell
                    && Q.IsReady()
                    && Player.Distance(target) >= 500
                    && Player.Mana >= Q.Instance.ManaCost*2f
                    && Player.CountEnemyHeroesInRange(1000) <= 3
                    && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
            }
        }
        
        private static void LaneClear()
        {
            var q2LSpell = Config.GetValue<MenuBool>("useQ2L").Enabled;
            var qSlider = Config.GetValue<MenuSlider>("useQSlider").Value;
            var mSlider = Config.GetValue<MenuSlider>("useMSlider").Value;
            var minionCount = MinionManager.GetMinions(Player.Position, Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);
            if (Player.ManaPercent < mSlider)
                return;
            {
                var mfarm = Q.GetLineFarmLocation(minionCount);
                if (q2LSpell
                    && minionCount.Count >= qSlider
                    && mfarm.MinionsHit >= qSlider)
                {
                    Q.Cast(mfarm.Position);
                }
            }
        }
        
        private static void Mixed()
        {
            var qSpell = Config.GetValue<MenuBool>("UseQH").Enabled;
            var qrSpell = Config.GetValue<MenuBool>("UseQrh").Enabled;
            AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var minionsQ = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);
            if(target == null || minionsQ == null) return;
            var qHit = Q.GetLineFarmLocation(minionsQ, 100);
            if (qSpell
                && !qrSpell
                && Q.IsReady()
                && qHit.MinionsHit <= 4
                && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            if (qSpell
                && qrSpell
                && Q.IsReady()
                && Player.Distance(target) > 800
                && qHit.MinionsHit <= 4
                && target.HealthPercent < Player.HealthPercent
                && target.IsFacing(Player)
                && Player.CountEnemyHeroesInRange(1000) == 1
                && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
        }
        
        private static void KillSteal()
        {
            var ks = Config.GetValue<MenuBool>("KS").Enabled;
            if (!ks)
                return;
            
            AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null) return;

            var prediction = R.GetPrediction(target);
            var qcollision = R.GetCollision(Player.Position.To2D(),
                new List<Vector2> { prediction.CastPosition.To2D() });
            var playerncol = qcollision.Where(x => !(x is AIHeroClient)).Count(x => x.IsTargetable);
            var rSpell = Config.GetValue<MenuBool>("useR2KS").Enabled;
            var qSpell = Config.GetValue<MenuBool>("useQ2KS").Enabled;

            if (rSpell
                && R.IsReady()
                && target.IsValidTarget(R.Range)
                && Player.CountEnemyHeroesInRange(2000) <= 3
                && playerncol == 0
                && target.Health < R.GetDamage(target)*0.6f)
            {
                R.CastOnUnit(target);
            }

            if (qSpell
                && Q.IsReady()
                && target.Health < Q.GetDamage(target)
                && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if (Config.GetValue<MenuBool>("qDraw").Enabled && Q.Level > 0)
            {
                CircleRender.Draw(Player.Position, Q.Range, Color.Green);
            }
            if (Config.GetValue<MenuBool>("eDraw").Enabled && E.Level > 0)
            {
                CircleRender.Draw(Player.Position, E.Range, Color.Gold);
            }
            if (Config.GetValue<MenuBool>("wDraw").Enabled && W.Level > 0)
            {
                CircleRender.Draw(Player.Position, W.Range, Color.BlueViolet);
            }
        }
        
        private static void ManualR()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (target == null)
                return;

            if (target.IsValidTarget()
                && !target.IsZombie())
            {
                R.CastOnUnit(target);
            }
        }
    }
}