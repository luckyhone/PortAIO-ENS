using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using StormAIO.utilities;

namespace StormAIO.Champions
{
    internal class Yorick
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;
        private static bool HasfirstQ => Player.Spellbook.GetSpell(SpellSlot.Q).Name == "YorickQ";
        private static bool HasSecondQ => Player.Spellbook.GetSpell(SpellSlot.Q).Name == "YorickQ2";
        
        private static float sheenTimer;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            ChampMenu = new Menu("Yorick", "Yorick", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QM", "Mana for using Q in harass", 40)
            };
            ChampMenu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
                new MenuSlider("WHM", "Only Use W in Harass When mana % > ", 40)
            };
            ChampMenu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EH", "Use E in Harass"),
                new MenuSlider("EM", "Mana for using E in harass", 30)
            };
            ChampMenu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Use R"),
                new MenuSlider("RC", "Use R When there are ", 1, 1, 5)
            };
            ChampMenu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuSliderButton("Q", "Q || Only Use when Mana % >", 40),
            };
            ChampMenu.Add(laneClearMenu);
            var LastHit = new Menu("LastHit", "LastHit")
            {
                new MenuBool("Q", "Q ")
            };
            ChampMenu.Add(LastHit);
            var StructureMenu = new Menu("StructureClear","Structure Clear")
            {
                new MenuBool("QT", "Use Q on Turrents")
            };
            ChampMenu.Add(StructureMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("E", "Use E")
            };
            ChampMenu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseW", "Auto W dasher"),
                new MenuBool("UseW2", "Auto W Interrupter")
            };
            ChampMenu.Add(miscMenu);
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q", false),
                new MenuBool("DrawW", "Draw W", false),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
            };
            ChampMenu.Add(drawMenu);


            MainMenu.Main_Menu.Add(ChampMenu);
        }

        #endregion menu

        #region Spells

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 600);
            W.SetSkillshot(1f, 225, float.MaxValue, false, SpellType.Circle);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 600);
            R.SetTargetted(0.50f, float.MaxValue);
            E.SetSkillshot(0.33f, 100f, 500, false, SpellType.Line);
        }

        #endregion
        #region Gamestart

        public Yorick()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnAfterAttack += OrbwalkerOnOnAction;
            AIBaseClient.OnDoCast += AIBaseClientOnOnProcessSpellCast;
            Dash.OnDash += (sender, args) =>
            {
                if (sender.IsEnemy && W.IsReady() && ChampMenu["Misc"].GetValue<MenuBool>("UseW").Enabled &&
                    W.IsInRange(sender))
                {
                    var wpre = W.GetPrediction(sender);
                    if (wpre.Hitchance == HitChance.Dash) W.Cast(args.EndPos);
                }
            };
            Interrupter.OnInterrupterSpell += (sender, args) =>
            {
                if (sender.IsEnemy && W.IsReady() && ChampMenu["Misc"].GetValue<MenuBool>("UseW2").Enabled &&
                    W.IsInRange(sender))
                {
                    var wpre = W.GetPrediction(sender);
                    if (wpre.Hitchance >= HitChance.High) W.Cast(wpre.UnitPosition);
                }
            };
            AIBaseClient.OnBuffRemove += delegate(AIBaseClient sender, AIBaseClientBuffRemoveEventArgs args)
            {
                if (sender.IsMe)
                    if (args.Buff.Name == "sheen" || args.Buff.Name == "TrinityForce")
                        sheenTimer = Variables.GameTimeTickCount + 1.7f;
            };
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
        }

        #endregion

        #region Args

        private void OrbwalkerOnOnAction(object sender, AfterAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                var target = TargetSelector.GetTarget(Player.GetRealAutoAttackRange() + 50,DamageType.Physical);
                var RManaCost = !R.IsReady() ? 0 : 100;
                var QManaCost = Q.Level < 1 ? 0 : 25;
                if (ChampMenu["Q"].GetValue<MenuBool>("QC").Enabled && Player.Mana - QManaCost > RManaCost) CastQ(target);
            }
        }

        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "YorickQ") Orbwalker.ResetAutoAttackTimer();

            if (sender.IsMe && args.Target is AITurretClient && HasfirstQ &&
                ChampMenu["StructureClear"].GetValue<MenuBool>("QT").Enabled && Orbwalker.ActiveMode == OrbwalkerMode.LaneClear) Q.Cast();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ChampMenu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Player.GetRealAutoAttackRange() + 50, Color.DarkCyan);
            if (ChampMenu["Drawing"].GetValue<MenuBool>("DrawW").Enabled && W.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, W.Range, Color.DarkCyan);
            if (ChampMenu["Drawing"].GetValue<MenuBool>("DrawE").Enabled && E.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, E.Range, Color.Violet);
            if (ChampMenu["Drawing"].GetValue<MenuBool>("DrawR").Enabled && R.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, R.Range, Color.Firebrick);
        }
        

        #endregion
        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
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
                   if (MainMenu.SpellFarm.Active) LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }


            KillSteal();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1500,DamageType.Physical);
            var RManaCost = !R.IsReady() ? 0 : 100;
            var QManaCost = Q.Level < 1 ? 0 : 25;
            var WManaCost = W.Level < 1 ? 0 : 70;
            var EManaCost = E.Level < 1 ? 0 : 50 + 5 * E.Level;
            if (target == null) return;
            if (ChampMenu["R"].GetValue<MenuBool>("R").Enabled) CastR(target);
            if (ChampMenu["Q"].GetValue<MenuBool>("QC").Enabled && Player.Mana - QManaCost > RManaCost) CastQ2(target);
            if (ChampMenu["W"].GetValue<MenuBool>("WC").Enabled && Player.Mana - WManaCost > RManaCost) CastW(target);
            if (ChampMenu["E"].GetValue<MenuBool>("EC").Enabled && Player.Mana - EManaCost > RManaCost) CastE(target);
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);
            if (target == null) return;
            if (ChampMenu["Q"].GetValue<MenuBool>("QH").Enabled &&
                ChampMenu["Q"].GetValue<MenuSlider>("QM").Value < Player.ManaPercent)
            {
                CastQ(target);
                CastQ2(target);
            }

            if (ChampMenu["W"].GetValue<MenuBool>("WH").Enabled &&
                ChampMenu["W"].GetValue<MenuSlider>("WHM").Value < Player.ManaPercent) CastW(target);
            if (ChampMenu["E"].GetValue<MenuBool>("EH").Enabled &&
                ChampMenu["E"].GetValue<MenuSlider>("EM").Value < Player.ManaPercent) CastE(target);
        }

        private static void LaneClear()
        {
            if (!ChampMenu["LaneClear"].GetValue<MenuSliderButton>("Q").Enabled ||
                Player.ManaPercent < ChampMenu["LaneClear"].GetValue<MenuSliderButton>("Q").ActiveValue) return;
            var minons = GameObjects.GetMinions(Player.Position, Player.GetRealAutoAttackRange() + 50)
                .Where(x => x.IsValid && !x.IsDead).ToList();
            if (minons.Any())
                foreach (var minon in minons.OrderBy(x => x.DistanceToPlayer()))
                    if (Q.IsReady() &&
                        Q.IsInRange(minon) && Q.GetDamage(minon) >= HealthPrediction.GetPrediction(minon,0,20) && HasfirstQ)
                    {
                        Q.Cast();
                        Orbwalker.Attack(minon);
                    }
        }

        private static void LastHit()
        {
            if (!ChampMenu["LastHit"].GetValue<MenuBool>("Q").Enabled) return;
            var minons = GameObjects.GetMinions(Player.Position, Player.GetRealAutoAttackRange() + 50)
                .Where(x => x.IsValid && !x.IsDead).ToList();
            if (minons.Any())

                foreach (var minon in minons.OrderBy(x => x.DistanceToPlayer()))
                    if (Q.IsReady() &&
                        Q.IsInRange(minon) && Q.GetDamage(minon) >= HealthPrediction.GetPrediction(minon,0,20) && HasfirstQ)
                    {
                        Orbwalker.Attack(minon);
                        Q.Cast();
                    }
        }

        #endregion


        #region Spell Functions

        private static void CastQ(AIHeroClient target)
        {
            if (!Q.IsReady() || target.DistanceToPlayer() > Player.GetRealAutoAttackRange() + 50)
                return;
            if (HasfirstQ) Q.Cast();
        }

        private static void CastQ2(AIHeroClient target)
        {
            if (Q.IsReady() || !HasSecondQ || target.DistanceToPlayer() > 700)
                return;
            Q.Cast();
        }

        private static void CastW(AIHeroClient target)
        {
            if (!W.IsReady() || !W.IsInRange(target))
                return;

            if (!target.IsMoving || target.IsWindingUp)
            {
                var a = W.GetPrediction(target);
                if (a.Hitchance >= HitChance.High) W.Cast(a.UnitPosition);
                return;
            }

            var wpre = W.GetPrediction(target);
            if (wpre.Hitchance >= HitChance.High) W.Cast(wpre.CastPosition);
        }

        private static void CastE(AIHeroClient target)
        {
            if (!E.IsReady()) return;
            if (!target.IsMoving || target.IsWindingUp)
            {
                var a = E.GetPrediction(target);
                if (a.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield))
                    E.Cast(a.UnitPosition);
                return;
            }

            var Epre = E.GetPrediction(target);
            if (Epre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) && E.IsInRange(target))
                E.Cast(Epre.CastPosition);
        }

        private static void CastR(AIHeroClient target)
        {
            if (!R.IsReady() || Player.CountEnemyHeroesInRange(1000) < ChampMenu["R"].GetValue<MenuSlider>("RC").Value)
                return;
            if (R.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield)) R.Cast(target);
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (target == null) return;
            if (target.TrueHealth() < E.GetDamage(target)  &&
                ChampMenu["KS"].GetValue<MenuBool>("E").Enabled) CastE(target);
        }

        #endregion

        #region Extra functions
        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage                  += (float) Player.GetAutoAttackDamage(target) + Sheen(target);
            if (Q.IsReady()) Damage += Q.GetDamage(target) + Sheen(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (R.IsReady()) Damage += R.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        private static float Sheen(AIBaseClient target)
        {
            float damage = 0;

            if (Player.HasItem(ItemId.Sheen) && sheenTimer < Variables.GameTimeTickCount)
            {
                var item = new Items.Item(ItemId.Sheen, 600);
                if (item.IsReady && !Player.HasBuff("sheen"))
                    damage = (float) Player.CalculateDamage(target,
                        DamageType.Physical,
                        Player.BaseAttackDamage);
            }

            if (Player.HasItem(ItemId.Trinity_Force) && sheenTimer < Variables.GameTimeTickCount)
            {
                var item = new Items.Item(ItemId.Trinity_Force, 600);
                if (item.IsReady && !Player.HasBuff("TrinityForce"))
                    damage = (float) (Player.CalculateDamage(target,
                                          DamageType.Physical,
                                          Player.BaseAttackDamage) *
                                      2f);
            }

            return damage;
        }
        #endregion
    }
}