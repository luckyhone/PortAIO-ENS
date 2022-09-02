using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;
using StormAIO.utilities;
 using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Chogath
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu champMenu;
        private static AIHeroClient Player => ObjectManager.Player;
        private static bool _postAttack;
        private static MenuKeyBind Rhelper => champMenu["R"].GetValue<MenuKeyBind>("RI");
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            champMenu = new Menu("Chogath", "Chogath", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QM", "Mana for using Q in harass", 40),
                new MenuBool("AQ", "Auto Q on Dasher"),
                new MenuBool("AQC", "Auto Q on CC'ED target")
            };
            champMenu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
                new MenuSlider("WM", "Mana for using W in harass", 60),
                new MenuBool("AW", "Auto W on Interrupter")
            };
            champMenu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EH", "Use E in Harass"),
                new MenuBool("ET", "Use E for structure clear"),
                new MenuBool("EAA", "Only Use E PostAttack for AutoAttackReset"),
                new MenuSlider("EM", "Mana for using E in harass", 30)
            };
            champMenu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("RC", "Use R in Combo Only When target is Eatable"),
                new MenuKeyBind("RJ", "Simi R key Only UseAble for jgls", Keys.A, KeyBindType.Press),
                new MenuKeyBind("RI", "Simi R key", Keys.T, KeyBindType.Press)
            };
            champMenu.Add(rMenu);
            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Q"),
                new MenuSlider("QLC", "Only Use Q if it can hit", 3, 1, 5),
                new MenuBool("W", "W"),
                new MenuBool("E", "E"),
                new MenuSlider("Mana", "Mana for Lane Clear", 40)
            };
            champMenu.Add(laneClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W"),
            };
            champMenu.Add(killSteal);
            
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawW", "Draw W"),
                new MenuBool("DrawR", "Draw R"),
            };
            champMenu.Add(drawMenu);


            MainMenu.Main_Menu.Add(champMenu);
        }

        #endregion menu

        #region Gamestart

        public Chogath()
        {
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 175);
            Q.SetSkillshot(0.5f, 200f, float.MaxValue, false, SpellType.Circle);
            W.SetSkillshot(0.5f, 60f, int.MaxValue, false, SpellType.Cone);
            R.SetTargetted(0.25f,float.MaxValue);
            CreateMenu();
            Orbwalker.OnAfterAttack += Orbwalker_OnAction;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnDoCast += AIBaseClientOnProcessSpellCast;
            AntiGapcloser.OnGapcloser += (sender, args) =>
            {
                if (sender.IsEnemy  && champMenu["Q"].GetValue<MenuBool>("AQ").Enabled && Q.IsInRange(sender))
                    Q.Cast(args.EndPosition);
            };
            Interrupter.OnInterrupterSpell += (sender, args) =>
            {
                if (sender.IsEnemy && champMenu["W"].GetValue<MenuBool>("AW").Enabled && W.IsInRange(sender)) W.Cast(sender);
            };
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            // ReSharper disable once ObjectCreationAsStatement
            new DrawText("Simi R Key", Rhelper.Key.ToString(),Rhelper,Color.GreenYellow,Color.Red,123,132);
            AIBaseClient.OnBuffAdd += AIBaseClientOnOnBuffGain;
        }

        private void AIBaseClientOnOnBuffGain(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            if (!champMenu["Q"].GetValue<MenuBool>("AQC").Enabled) return;
            if (!sender.IsEnemy) return;
            if (!Q.IsReady()) return;
            if (!Q.IsInRange(sender)) return;
            if (!sender.HaveImmovableBuff()) return;
            Q.Cast(sender.Position);
        }

        #endregion

        private void AIBaseClientOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "VorpalSpikes") Orbwalker.ResetAutoAttackTimer();
            }
            
            if (args.Target == null) return;
            if (sender.IsEnemy && args.Target.IsMe && args.Slot == SpellSlot.R &&
                args.SData.TargetingType == SpellDataTargetType.Target)
            {
                W.Cast(sender);
            }
            
        }

        private void Orbwalker_OnAction(object sender, AfterAttackEventArgs args)
        {

                var orb = Orbwalker.GetTarget();
                if (orb != null) _postAttack = true;
                if (args.Target is AITurretClient && champMenu["E"].GetValue<MenuBool>("ET").Enabled )
                    if (E.IsReady()) E.Cast();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (champMenu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.Plum);
            if (champMenu["Drawing"].GetValue<MenuBool>("DrawW").Enabled && W.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, W.Range, Color.DarkCyan);
            if (champMenu["Drawing"].GetValue<MenuBool>("DrawR").Enabled && R.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, R.Range + Player.BoundingRadius, Color.Firebrick);
            
        }

        #region gameupdate

        private void Game_OnUpdate(EventArgs args)
        {
            if (Helper.Checker()) return;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    if(MainMenu.SpellFarm.Active) LaneClear();
                    break;
            }

            if (champMenu["R"].GetValue<MenuKeyBind>("RJ").Active) Rdamage();
            if (champMenu["R"].GetValue<MenuKeyBind>("RI").Active) SimiRKey();
            KillSteal();
        }

        #endregion

        #region Orbwalker mod

        private void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            var Rmana = !R.IsReady() ? 0 : 100;
            if (target == null) return;
            if (R.IsReady() && target.DistanceToPlayer() < R.Range + Player.BoundingRadius && R.GetDamage(target) >
                target.TrueHealth() &&
                !target.HasBuffOfType(BuffType.SpellShield) &&
                champMenu["R"].GetValue<MenuBool>("RC").Enabled)
            {
                R.Cast(target);
            }
            else
            {
                if (champMenu["Q"].GetValue<MenuBool>("QC").Enabled && Player.Mana - 60 > Rmana) CastQ(target);
                if (champMenu["W"].GetValue<MenuBool>("WC").Enabled &&
                    Player.Mana - 70 + 10 * (W.Level - 1) > Rmana)
                    CastW(target);
                if (champMenu["E"].GetValue<MenuBool>("EC").Enabled && Player.Mana - 30 > Rmana) CastE(target);
            }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null) return;
            if (champMenu["Q"].GetValue<MenuBool>("QH").Enabled &&
                champMenu["Q"].GetValue<MenuSlider>("QM").Value < Player.ManaPercent) CastQ(target);
            if (champMenu["W"].GetValue<MenuBool>("WH").Enabled && champMenu["W"].GetValue<MenuSlider>("WM").Value < Player.ManaPercent)
                CastW(target);
            if (champMenu["E"].GetValue<MenuBool>("EH").Enabled &&
                champMenu["E"].GetValue<MenuSlider>("EM").Value < Player.ManaPercent) CastE(target);
        }

        private void LaneClear()
        {
            var minons = GameObjects.GetMinions(Player.Position, Q.Range).Where(x => x.IsValid && !x.IsDead).ToList();
            bool useQ = champMenu["LaneClear"].GetValue<MenuBool>("Q").Enabled;
            bool useW = champMenu["LaneClear"].GetValue<MenuBool>("W").Enabled;
            bool useE = champMenu["LaneClear"].GetValue<MenuBool>("E").Enabled;
            var mana = champMenu["LaneClear"].GetValue<MenuSlider>("Mana").Value;
            if (minons.Any())
                if (mana > Player.ManaPercent)
                    return;
            foreach (var minon in minons.OrderBy(x => x.DistanceToPlayer()))
            {
                var laneE = GameObjects.GetMinions(Player.Position, Q.Range + Q.Width);
                var efarmpos = Q.GetCircularFarmLocation(laneE);
                if (useQ && Q.IsReady() &&
                    efarmpos.MinionsHit >= champMenu["LaneClear"].GetValue<MenuSlider>("QLC").Value &&
                    Q.IsInRange(minon)) Q.Cast(efarmpos.Position);

                if (useW && W.IsReady() && W.IsInRange(minon))
                {
                    var a = W.GetCircularFarmLocation(minons);
                    if (a.MinionsHit >= 2)
                        W.Cast(a.Position);
                }

                if (useE && E.IsReady() && minon.DistanceToPlayer() < Player.GetRealAutoAttackRange()) E.Cast();
                break;
            }

            var jgls = GameObjects.GetJungles(Q.Range).Where(x => x.IsValid && !x.IsDead).ToList();
            if (jgls.Any())
                foreach (var jgl in jgls.OrderBy(x => x.DistanceToPlayer()))
                {
                    if (useQ && Q.IsReady() && Q.IsInRange(jgl)) Q.Cast(jgl);

                    if (useW && W.IsReady() && W.IsInRange(jgl)) W.Cast(jgl);

                    if (useE && E.IsReady() && jgl.DistanceToPlayer() < Player.GetRealAutoAttackRange()) E.Cast();
                    break;
                }
        }

        #endregion


        private static void CastQ(AIBaseClient target)
        {
            if (!Q.IsReady() || target.HasBuffOfType(BuffType.SpellShield))
                return;
            var input = new PredictionInput()
            {
                Unit = target,
                Radius = (Q.Width / 2),
                Speed = Q.Speed,
                Range = 950f,
                Delay = 0.727f,
                Aoe = true,
                AddHitBox = false,
                From = Player.PreviousPosition,
                RangeCheckFrom = Player.PreviousPosition,
                Type = SpellType.Circle
            };
        }
        
        private static void CastW(AIHeroClient target)
        {
            if (!W.IsReady() || !W.IsInRange(target)) return;
            W.Cast(target);
        }

        private static void CastE(AIHeroClient target)
        {
            if (!E.IsReady()) return;
            if (target.DistanceToPlayer() < Player.GetRealAutoAttackRange() && _postAttack)
                E.Cast();
            _postAttack = false;
        }


        private void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null) return;
            if (target.Health < Q.GetDamage(target) && !target.IsInvulnerable && champMenu["KS"].GetValue<MenuBool>("Q").Enabled)
                CastQ(target);
            if (target.Health < W.GetDamage(target) && !target.IsInvulnerable && champMenu["KS"].GetValue<MenuBool>("W").Enabled)
                CastW(target);{}
        }


        private static void Rdamage()
        {
            var targets = GameObjects.GetJungles(Player.Position, R.Range + Player.BoundingRadius);
            if (targets == null) return;
            foreach (var target in targets.Where(x => x.IsValid && x.MaxHealth > 3800))
                if (1000 + Player.TotalMagicalDamage * 0.5 + Player.BonusHealth * 0.10 > target.Health &&
                    R.IsReady() &&
                    target.DistanceToPlayer() < R.Range + Player.BoundingRadius)
                    R.Cast(target);
        }

        private static void SimiRKey()
        {
            var targets = TargetSelector.GetTarget(R.Range + Player.BoundingRadius,DamageType.Physical);
            if (targets == null) return;
            R.Cast(targets);
        }
        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (W.IsReady()) Damage += W.GetDamage(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (R.IsReady()) Damage += R.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
       
    }
}