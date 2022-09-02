using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using StormAIO.utilities;

namespace StormAIO.Champions
{
    internal class Akali
    {
        #region Basics

        private static Spell Q, W, E, E2, E3, R, R2;
        private static Menu ChampMenu, _items;
        private static AIHeroClient Player => ObjectManager.Player;
        private static bool HasfirstE => Player.Spellbook.GetSpell(SpellSlot.E).Name == "AkaliE";
        private static bool HasSecondE => Player.Spellbook.GetSpell(SpellSlot.E).Name == "AkaliEb";
        private static bool HasfirstR => Player.Spellbook.GetSpell(SpellSlot.R).Name == "AkaliR";
        private static bool HasRecastR => Player.Spellbook.GetSpell(SpellSlot.R).Name == "AkaliRb";

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            ChampMenu = new Menu("Akali", "Akali");
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass")
            };
            ChampMenu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuSlider("WU", "Only Use W in Combo When Energy % < ", 50, 0, 200)
            };
            ChampMenu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("E1", "Use first E in Combo"),
                new MenuBool("E2", "Use second E in Combo")
            };
            ChampMenu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R3", "Use R  in combo "),
                new MenuBool("R", "Use first R  in combo "),
                new MenuBool("R2", "Use second R in combo  ")
            };
            ChampMenu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Use Q"),
                new MenuSlider("QE", "Only Use when Energy >", 90, 0, 200)
            };
            ChampMenu.Add(laneClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("E1", "Use first E"),
                new MenuBool("E2", "Use Second E"),
                new MenuBool("R1", "Use first R"),
                new MenuBool("R2", "Use second R"),
                new MenuBool("G", "Use Gunblade")
            };
            ChampMenu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc");
            _items = new Menu("Items", "Items")
            {
                new MenuSliderButton("UG", "Use GunBlade in Combo || when target health is below %", 60, 10)
            };
            miscMenu.Add(_items);
            ChampMenu.Add(miscMenu);
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("DrawR2", "Draw Second R", false)
            };
            ChampMenu.Add(drawMenu);


            MainMenu.Main_Menu.Add(ChampMenu);
        }

        #endregion menu

        #region Spells

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, 500);
            Q.SetSkillshot(0.25f, 70f, 1200f, false, SpellType.Cone);
            W = new Spell(SpellSlot.W, 250) {Delay = 0.3f};
            W.SetSkillshot(0.3f, 350f, 1200f, false, SpellType.Circle);
            E = new Spell(SpellSlot.E, 825);
            E.SetSkillshot(0.25f, 70f, 1200, true, SpellType.Line);
            E3 = new Spell(SpellSlot.E, 825);
            E3.SetSkillshot(0.4f, 70f, 1200, false, SpellType.Line);
            E2 = new Spell(SpellSlot.E, 25000) {Delay = 0.125f};
            R = new Spell(SpellSlot.R, 675);
            R.SetTargetted(0.3f, 1000f);
            R2 = new Spell(SpellSlot.R, 750);
            R2.SetSkillshot(0.125f, 60f, float.MaxValue, false, SpellType.Line);
        }

        #endregion

        #region Gamestart

        public Akali()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Magical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
        }

        #endregion


        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender is AITurretClient && args.Target.IsMe && W.IsReady() && !E.IsReady()) W.Cast(Player.Position);
            if (sender is AITurretClient && args.Target.IsMe && E.IsReady() &&
                Player.CountEnemyHeroesInRange(2000) == 0 && HasfirstE)
            {
                var turrents = GameObjects.EnemyTurrets.FirstOrDefault(x => x.IsValid && x.DistanceToPlayer() < 1200);
                E3.Cast(turrents);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ChampMenu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.DarkCyan);
            if (ChampMenu["Drawing"].GetValue<MenuBool>("DrawE").Enabled && E.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, E.Range, Color.Violet);
            if (ChampMenu["Drawing"].GetValue<MenuBool>("DrawR").Enabled && R.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, R.Range, Color.Firebrick);
            if (ChampMenu["Drawing"].GetValue<MenuBool>("DrawR2").Enabled && R.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, R2.Range, Color.Firebrick);
        }

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
                    if(MainMenu.SpellFarm.Active)  LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
            }


            KillSteal();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            var R3 = ChampMenu["R"].GetValue<MenuBool>("R3");
            if (ChampMenu["E"].GetValue<MenuBool>("E2").Enabled) CastE2();
            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (target == null) return;
            if (ChampMenu["R"].GetValue<MenuBool>("R").Enabled && R3.Enabled) CastR();
            if (ChampMenu["R"].GetValue<MenuBool>("R2").Enabled && R3.Enabled) CastR2();
            if (ChampMenu["Q"].GetValue<MenuBool>("QC").Enabled) CastQ();
            if (ChampMenu["W"].GetValue<MenuBool>("WC").Enabled &&
                Player.Mana < ChampMenu["W"].GetValue<MenuSlider>("WU").Value) CastW(target);
            if (ChampMenu["E"].GetValue<MenuBool>("E1").Enabled) CastE();
            if (!_items.GetValue<MenuSliderButton>("UG").Enabled) return;
            if (_items.GetValue<MenuSliderButton>("UG").ActiveValue < target.HealthPercent) return;
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            if (target == null) return;
            if (ChampMenu["Q"].GetValue<MenuBool>("QH").Enabled) CastQ();
        }

        private static void LaneClear()
        {
            if (!ChampMenu["LaneClear"].GetValue<MenuBool>("Q").Enabled ||
                Player.Mana <= ChampMenu["LaneClear"].GetValue<MenuSlider>("QE").Value) return;
            var minons = GameObjects.GetMinions(Player.Position, Q.Range);
            if (minons == null) return;
            var Lane = Q.GetCircularFarmLocation(minons);
            if (Lane.Position.IsValid() && Lane.MinionsHit >= 1)
            {
                Q.Cast(Lane.Position);
            }
                
        }

        #endregion


        #region Spell Functions

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            if (target == null) return;
            if (!Q.IsReady() || target.DistanceToPlayer() > Q.Range || Player.IsDashing())
                return;
            Q.Cast(target);
        }


        private static void CastW(AIHeroClient target)
        {
            if (!W.IsReady() || !W.IsInRange(target))
                return;
            W.Cast(Player.Position);
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(E.Range,DamageType.Magical);
            if (target == null) return;
            if (!E.IsReady() || !HasfirstE || Player.IsDashing()) return;
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

        private static void CastE2()
        {
            var target = TargetSelector.GetTarget(E2.Range,DamageType.Magical);
            if (target == null) return;
            if (!E.IsReady() || !HasSecondE || !target.HasBuff("AkaliEMis")) return;
            E2.Cast(target);
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(R.Range,DamageType.Magical);
            if (target == null) return;
            if (!R.IsReady() || !HasfirstR)
                return;
            if (R.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield)) R.Cast(target);
        }

        private static void CastR2()
        {
            var target = TargetSelector.GetTarget(R2.Range,DamageType.Magical);
            if (target == null) return;
            if (!R.IsReady() || !HasRecastR || !R2.IsInRange(target))
                return;
            if (!target.IsMoving || target.IsWindingUp)
            {
                var a = R2.GetPrediction(target);
                if (a.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) &&
                    !target.HasBuffOfType(BuffType.Invulnerability))
                    R2.Cast(a.UnitPosition);
                return;
            }

            var Rpre = R2.GetPrediction(target);
            if (Rpre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) &&
                !target.HasBuffOfType(BuffType.Invulnerability))
                R2.Cast(Rpre.UnitPosition);
        }

        private static void KillSteal()
        {
            var e2target = TargetSelector.GetTarget(E2.Range,DamageType.Magical);
            if (e2target != null)
                if (e2target.TrueHealth() < Edmg(e2target) + passivedmg(e2target) + Q.GetDamage(e2target) &&
                    ChampMenu["KS"].GetValue<MenuBool>("E2").Enabled)
                    CastE2();

            var target = TargetSelector.GetTarget(E.Range,DamageType.Magical);
            if (target == null) return;
            if (target.TrueHealth() < Q.GetDamage(target) && ChampMenu["KS"].GetValue<MenuBool>("Q").Enabled) CastQ();
            if (target.TrueHealth() < Edmg(target) && !target.HasBuffOfType(BuffType.SpellShield) &&
                ChampMenu["KS"].GetValue<MenuBool>("E1").Enabled) CastE();

            if (target.TrueHealth() < bothRdmg(target) && ChampMenu["KS"].GetValue<MenuBool>("R1").Enabled) CastR();
            if (target.TrueHealth() < bothRdmg(target, 1) && ChampMenu["KS"].GetValue<MenuBool>("R2").Enabled) CastR2();
            if (target.TrueHealth() < bothRdmg(target, 1) && ChampMenu["KS"].GetValue<MenuBool>("R2").Enabled) CastR2();
            var gunbladedmg =
                Player.CalculateMagicDamage(target, 170 + 4.588 * Player.Level + Player.TotalMagicalDamage * 0.3);
        }

        #endregion

        #region Damages

        // sdk damage is outdated 

        private static float bothRdmg(AIHeroClient target, int index = 0)
        {
            if (R.Level == 0) return 0;
            switch (index)
            {
                case 0:
                    var R1damage = 125f + 100f * (R.Level - 1);
                    var adbounus = Player.TotalAttackDamage - Player.BaseAttackDamage;
                    R1damage += adbounus * 0.5f;
                    return (float) Player.CalculateDamage(target, DamageType.Physical, R1damage);

                case 1:
                    var R2damage = 75f + 70f * (R.Level - 1);
                    var missingHealthPercent = (1 - target.Health / target.MaxHealth) * 100;
                    var totalIncreasement = 1 + 2.86f * missingHealthPercent / 100;
                    var RDmg = (R2damage + 0.3 * Player.TotalMagicalDamage) * totalIncreasement;


                    R2damage = (float) RDmg;
                    return (float) Player.CalculateDamage(target, DamageType.Physical, R2damage);
                case 2:
                    var R11damage = 125f + 100f * (R.Level - 1);
                    var ad1bounus = Player.TotalAttackDamage - Player.BaseAttackDamage;
                    R11damage += ad1bounus * 0.5f;
                    var R22damage = 75f + 70f * (R.Level - 1);
                    var m1issingHealthPercent = (1 - target.Health / target.MaxHealth) * 100;
                    var t1otalIncreasement = 1 + 2.86f * m1issingHealthPercent / 100;
                    var R2Dmg = (R22damage + 0.3 * Player.TotalMagicalDamage) * t1otalIncreasement;

                    return (float) ((float) Player.CalculateDamage(target, DamageType.Physical, R11damage) +
                                    Player.CalculateDamage(target, DamageType.Magical, R2Dmg));
            }

            return 0;
        }

        private static float Edmg(AIHeroClient target)
        {
            if (E.Level >= 1)
            {
                float damage = 50 + 35 * (E.Level - 1);
                damage += Player.TotalAttackDamage * 0.35f + Player.TotalMagicalDamage * 0.5f;
                return (float) Player.CalculateDamage(target, DamageType.Magical, damage);
            }

            return 0;
        }

        private static float passivedmg(AIHeroClient target)
        {
            var damage = 0;
            var bounsAd = (Player.TotalAttackDamage - Player.BaseAttackDamage) * 0.60f;
            if (Player.Level <= 7) damage = 39 + 3 * Player.Level;
            if (Player.Level >= 8) damage = 39 + 9 * Player.Level;
            if (Player.Level >= 13) damage = 39 + 15 * Player.Level;

            var extradamage = bounsAd + Player.TotalMagicalDamage * 0.5f;
            return (float) Player.CalculateDamage(target, DamageType.Magical, damage + extradamage);
        }
        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null) return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target);
            if (Player.HasBuff("akalipweapon")) Damage += passivedmg(target);
            if (Q.IsReady()) Damage                           += Q.GetDamage(target);
            if (E.IsReady()) Damage                           += Edmg(target);
            if (HasfirstR)   Damage                           += bothRdmg(target,1);
            if (HasRecastR)  Damage                           += bothRdmg(target,2);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100)
                Damage += (float) Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float) Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        #endregion
    }
}