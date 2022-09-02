using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Damages.SummonerSpells;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using SPrediction;

namespace EloFactory_Ahri
{
    internal class Program
    {
        public const string ChampionName = "Ahri";
        

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite = new Spell(SpellSlot.Unknown, 600);

        public static float QMANA;
        public static float WMANA;
        public static float EMANA;
        public static float RMANA;
        
        public static Items.Item HealthPotion = new Items.Item(2003, 0);

        public static Menu Config, ks,skinchanger;

        private static AIHeroClient Player;

        public static int[] abilitySequence;
        public static int qOff = 0, wOff = 0, eOff = 0, rOff = 0;

        public static void Game_OnGameLoad()
        {
            Player = ObjectManager.Player;

            if (Player.CharacterName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 965f);
            W = new Spell(SpellSlot.W, 650f);
            E = new Spell(SpellSlot.E, 965f);
            R = new Spell(SpellSlot.R, 450f);

            Q.SetSkillshot(0.2f, 100f, 1000f, false, SpellType.Line);
            W.SetSkillshot(0.7f, 650f, float.MaxValue, false, SpellType.Line);
            E.SetSkillshot(0.2f, 70f, 1000f, true, SpellType.Line);

            var ignite = Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name == "summonerdot");
            if (ignite != null)
                Ignite.Slot = ignite.Slot;

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            
            abilitySequence = new int[] { 1, 3, 1, 2, 1, 4, 1, 2, 1, 3, 4, 2, 2, 2, 3, 4, 3, 3 };

            Config = new Menu(ChampionName + " By LuNi", ChampionName + " By LuNi", true);
            
            var combo = Config.Add(new Menu("Combo", "Combo"));
            ks = combo.Add(new Menu("KS Mode", "KS Mode"));
            ks.Add(new MenuBool("Ahri.UseIgniteKS", "KS With Ignite").SetValue(true));
            ks.Add(new MenuBool("Ahri.UseQKS", "KS With Q").SetValue(true));
            ks.Add(new MenuBool("Ahri.UseWKS", "KS With W").SetValue(true));
            ks.Add(new MenuBool("Ahri.UseEKS", "KS With E").SetValue(true));
            combo.Add(new MenuBool("Ahri.UseQCombo", "Use Q In Combo").SetValue(true));
            combo.Add(new MenuBool("Ahri.UseWCombo", "Use W In Combo").SetValue(true));
            combo.Add(new MenuBool("Ahri.UseECombo", "Use E In Combo").SetValue(true));
            combo.Add(new MenuBool("Ahri.UseRCombo", "Use R + E If Killable In Combo").SetValue(true));

            var harss = Config.Add(new Menu("Harass", "Harass"));
            harss.Add(new MenuBool("Ahri.UseQHarass", "Use Q In Harass").SetValue(true));
            harss.Add(new MenuSlider("Ahri.QMiniManaHarass", "Minimum Mana To Use Q In Harass",0, 0, 100));
            harss.Add(new MenuBool("Ahri.UseQOnlyEHarass", "Use Q Only When E Hit In Harass ").SetValue(false));
            harss.Add(new MenuBool("Ahri.UseWHarass", "Use W In Harass").SetValue(true));
            harss.Add(new MenuSlider("Ahri.WMiniManaHarass", "Minimum Mana To Use W In Harass",50, 0, 100));
            harss.Add(new MenuBool("Ahri.UseWOnlyEHarass", "Use W Only When E Hit In Harass ").SetValue(true));
            harss.Add(new MenuBool("Ahri.UseEHarass", "Use E In Harass").SetValue(true));
            harss.Add(new MenuSlider("Ahri.EMiniManaHarass", "Minimum Mana To Use E In Harass",0, 0, 100));
            harss.Add(new MenuKeyBind("Ahri.HarassActive", "Harass!", Keys.C, KeyBindType.Press));
            harss.Add(new MenuKeyBind("Ahri.HarassActiveT", "Harass (toggle)!", Keys.Y, KeyBindType.Toggle));

            var lasthit = Config.Add(new Menu("LastHit", "LastHit"));
            lasthit.Add(new MenuBool("Ahri.UseQLastHit", "Use Q In LastHit").SetValue(false));
            lasthit.Add(new MenuSlider("Ahri.QMiniManaLastHit", "Minimum Mana To Use Q In LastHit",80, 0, 100));
            lasthit.Add(new MenuBool("Ahri.SafeQLastHit", "Never Use Q In LastHit When Enemy Close To Your Position").SetValue(false));

            var laneclear = Config.Add(new Menu("LaneClear", "LaneClear"));
            laneclear.Add(new MenuBool("Ahri.UseQLaneClear", "Use Q in LaneClear").SetValue(true));
            laneclear.Add(new MenuSlider("Ahri.QMiniManaLaneClear", "Minimum Mana To Use Q In LaneClear",30, 0, 100));
            laneclear.Add(new MenuSlider("Ahri.QLaneClearCount", "Minimum Minion To Use Q In LaneClear",3, 1, 6));
            laneclear.Add(new MenuBool("Ahri.UseWLaneClear", "Use W in LaneClear").SetValue(true));
            laneclear.Add(new MenuSlider("Ahri.WMiniManaLaneClear", "Minimum Mana To Use W In LaneClear",70, 0, 100));
            laneclear.Add(new MenuSlider("Ahri.WLaneClearCount", "Minimum Minion To Use W In LaneClear",4, 1, 6));

            var jungleclear = Config.Add(new Menu("JungleClear", "JungleClear"));
            jungleclear.Add(new MenuBool("Ahri.UseQJungleClear", "Use Q In JungleClear").SetValue(true));
            jungleclear.Add(new MenuSlider("Ahri.QMiniManaJungleClear", "Minimum Mana To Use Q In JungleClear",0, 0, 100));
            jungleclear.Add(new MenuBool("Ahri.UseWJungleClear", "Use W In JungleClear").SetValue(true));
            jungleclear.Add(new MenuSlider("Ahri.WMiniManaJungleClear", "Minimum Mana To Use W In JungleClear",0, 0, 100));
            jungleclear.Add(new MenuBool("Ahri.UseEJungleClear", "Use E In JungleClear").SetValue(true));
            jungleclear.Add(new MenuSlider("Ahri.EMiniManaJungleClear", "Minimum Mana To Use E In JungleClear",0, 0, 100));
            jungleclear.Add(new MenuBool("Ahri.SafeJungleClear", "Dont Use Spell In Jungle Clear If Enemy in Dangerous Range").SetValue(true));

            var misc = Config.Add(new Menu("Misc", "Misc"));
            skinchanger = misc.Add(new Menu("Skin Changer", "Skin Changer"));
            skinchanger.Add(new MenuBool("Ahri.SkinChanger", "Use Skin Changer").SetValue(false));
            skinchanger.Add(new MenuList("Ahri.SkinChangerName", "Skin choice",new[] { "Classic", "Dynasty", "Midnight", "Foxfire", "Popstar", "Dauntless" }));
            misc.Add(new MenuBool("Ahri.EInterrupt", "Interrupt Spells With E").SetValue(true));
            misc.Add(new MenuBool("Ahri.AutoEEGC", "Auto E On Gapclosers").SetValue(true));
            misc.Add(new MenuBool("Ahri.AutoWEGC", "Auto W On Gapclosers").SetValue(true));
            misc.Add(new MenuBool("Ahri.AutoEWhenEnemyCast", "Always Auto Use E On Enemy Attack").SetValue(false));
            misc.Add(new MenuBool("Ahri.AutoQWhenEnemyCast", "Always Auto Use Q On Enemy Attack").SetValue(false));
            misc.Add(new MenuBool("Ahri.AutoQWhenE", "Only Auto Use Q When E Hit").SetValue(false));
            misc.Add(new MenuBool("Ahri.AutoPotion", "Use Auto Potion").SetValue(true));
            misc.Add(new MenuBool("Ahri.AutoLevelSpell", "Auto Level Spell").SetValue(true));

            var draw = Config.Add(new Menu("Drawings", "Drawings"));
            draw.Add(new MenuBool("QRange", "Q range")); // Indigo
            draw.Add(new MenuBool("WRange", "W range")); // Green
            draw.Add(new MenuBool("ERange", "E range")); // Green
            draw.Add(new MenuBool("RRange", "R range")); // Gold
            draw.Add(new MenuBool("DrawOrbwalkTarget", "Draw Orbwalk target").SetValue(true));

            Config.Attach();
            
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnDoCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Dash.OnDash += Unit_OnDash;
        }

        #region ToogleOrder Game_OnUpdate
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.GetValue<MenuBool>("Ahri.AutoLevelSpell").Enabled) LevelUpSpells();

            if (Player.IsDead) return;

            if (Player.GetBuffCount("Recall") == 1) return;

            ManaManager();
            PotionManager();

            KillSteal();

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                LaneClear();
                JungleClear();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {
                LastHit();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                LastHit();
            }

            if (Config.GetValue<MenuKeyBind>("Ahri.HarassActive").Active ||
                Config.GetValue<MenuKeyBind>("Ahri.HarassActiveT").Active)
            {
                Harass();
            }
        }
        #endregion
        
        #region LastHit
        public static void LastHit()
        {

            var useQ = Config.GetValue<MenuBool>("Ahri.UseQLastHit").Enabled;

            var QMinMana = Config.GetValue<MenuSlider>("Ahri.QMiniManaLastHit").Value;

            var allMinionsQ = MinionManager.GetMinions(Q.Range);

            foreach (var minion in allMinionsQ)
            {
                if (useQ && Q.IsReady() && minion.Health > Player.GetAutoAttackDamage(minion) && minion.Health < Q.GetDamage(minion) * 0.9)
                {
                    Q.CastIfHitchanceEquals(minion, HitChance.High);
                }
            }

        }
        #endregion
        
        #region Harass

        public static void Harass()
        {

            var targetH = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if(targetH == null) return;

            var useQ = Config.GetValue<MenuBool>("Ahri.UseQHarass").Enabled;
            var useW = Config.GetValue<MenuBool>("Ahri.UseWHarass").Enabled;
            var useE = Config.GetValue<MenuBool>("Ahri.UseEHarass").Enabled;

            var QMinMana = Config.GetValue<MenuSlider>("Ahri.QMiniManaHarass").Value;
            var WMinMana = Config.GetValue<MenuSlider>("Ahri.WMiniManaHarass").Value;
            var EMinMana = Config.GetValue<MenuSlider>("Ahri.EMiniManaHarass").Value;


            if (useE && E.IsReady() && Player.Distance(targetH) < E.Range && Player.ManaPercent >= EMinMana)
            {
                E.CastIfHitchanceEquals(targetH, HitChance.VeryHigh);
            }

            if (useQ && (!useE || !Config.GetValue<MenuBool>("Ahri.UseQOnlyEHarass").Enabled ||
                         (Config.GetValue<MenuBool>("Ahri.UseQOnlyEHarass").Enabled &&
                          (targetH.HasBuffOfType(BuffType.Stun) || targetH.HasBuffOfType(BuffType.Snare) ||
                           targetH.HasBuffOfType(BuffType.Charm) || targetH.HasBuffOfType(BuffType.Fear) ||
                           targetH.HasBuffOfType(BuffType.Taunt)))) && Q.IsReady() &&
                Player.Distance(targetH) < Q.Range && Player.ManaPercent >= QMinMana)
            {
                Q.CastIfHitchanceEquals(targetH, HitChance.VeryHigh);
            }

            if (useW && (!useE || !Config.GetValue<MenuBool>("Ahri.UseWOnlyEHarass").Enabled ||
                         (Config.GetValue<MenuBool>("Ahri.UseWOnlyEHarass").Enabled &&
                          (targetH.HasBuffOfType(BuffType.Stun) || targetH.HasBuffOfType(BuffType.Snare) ||
                           targetH.HasBuffOfType(BuffType.Charm) || targetH.HasBuffOfType(BuffType.Fear) ||
                           targetH.HasBuffOfType(BuffType.Taunt)))) && W.IsReady() &&
                Player.Distance(targetH) < W.Range && Player.ManaPercent >= WMinMana)
            {
                W.Cast();
            }
        }

        #endregion
        
        #region KillSteal

        public static void KillSteal()
        {

            var useIgniteKS = ks.GetValue<MenuBool>("Ahri.UseIgniteKS").Enabled;
            var useQKS = ks.GetValue<MenuBool>("Ahri.UseQKS").Enabled;
            var useWKS = ks.GetValue<MenuBool>("Ahri.UseWKS").Enabled;
            var useEKS = ks.GetValue<MenuBool>("Ahri.UseEKS").Enabled;


            foreach (var target in ObjectManager.Get<AIHeroClient>()
                         .Where(target => !target.IsMe && target.Team != ObjectManager.Player.Team))
            {

                if (useWKS && W.IsReady() && Player.Mana >= WMANA && target.Health < W.GetDamage(target) &&
                    Player.Distance(target) < W.Range - 50 && Player.CountEnemyHeroesInRange(W.Range) == 1 &&
                    !target.IsDead && target.IsValidTarget())
                {
                    W.Cast();
                    return;
                }

                if (useQKS && Q.IsReady() && Player.Mana >= QMANA && target.Health < Q.GetDamage(target) &&
                    Player.Distance(target) < Q.Range - 50 && !target.IsDead && target.IsValidTarget())
                {
                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    return;
                }

                if (useEKS && E.IsReady() && Player.Mana >= EMANA && target.Health < E.GetDamage(target) &&
                    Player.Distance(target) < E.Range - 50 && !target.IsDead && target.IsValidTarget())
                {
                    E.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    return;
                }

                if (useIgniteKS && Ignite.Slot != SpellSlot.Unknown &&
                    Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite) > target.Health &&
                    target.IsValidTarget(Ignite.Range))
                {
                    Ignite.Cast(target, true);
                }

                if (useWKS && useQKS && W.IsReady() && Q.IsReady() && Player.Mana >= WMANA + QMANA &&
                    target.Health < W.GetDamage(target) + Q.GetDamage(target) &&
                    Player.Distance(target) < W.Range - 50 && Player.CountEnemyHeroesInRange(W.Range) == 1 &&
                    !target.IsDead && target.IsValidTarget())
                {
                    W.Cast();
                    return;
                }

                if (useWKS && useEKS && W.IsReady() && E.IsReady() && Player.Mana >= WMANA + EMANA &&
                    target.Health < W.GetDamage(target) + E.GetDamage(target) &&
                    Player.Distance(target) < E.Range - 50 && Player.CountEnemyHeroesInRange(W.Range) == 1 &&
                    !target.IsDead && target.IsValidTarget())
                {
                    E.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    return;
                }

                if (useQKS && useEKS && Q.IsReady() && E.IsReady() && Player.Mana >= QMANA + EMANA &&
                    target.Health < E.GetDamage(target) + Q.GetDamage(target) &&
                    Player.Distance(target) < E.Range - 50 && !target.IsDead && target.IsValidTarget())
                {
                    E.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    return;
                }

                if (useIgniteKS && useWKS && Ignite.Slot != SpellSlot.Unknown && W.IsReady() && Player.Mana >= WMANA &&
                    target.Health < W.GetDamage(target) + Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite) &&
                    Player.Distance(target) < W.Range - 50 && Player.CountEnemyHeroesInRange(W.Range) == 1 &&
                    !target.IsDead && target.IsValidTarget())
                {
                    W.Cast();
                    return;
                }

                if (useIgniteKS && useQKS && Ignite.Slot != SpellSlot.Unknown && Q.IsReady() && Player.Mana >= QMANA &&
                    target.Health < Q.GetDamage(target) + Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite) &&
                    Player.Distance(target) < 600 && !target.IsDead && target.IsValidTarget())
                {
                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    return;
                }

                if (useIgniteKS && useEKS && Ignite.Slot != SpellSlot.Unknown && E.IsReady() && Player.Mana >= EMANA &&
                    target.Health < E.GetDamage(target) + Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite) &&
                    Player.Distance(target) < E.Range - 50 && !target.IsDead && target.IsValidTarget())
                {
                    E.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    return;
                }

            }
        }

        #endregion
        
        #region LaneClear

        public static void LaneClear()
        {

            var useQ = Config.GetValue<MenuBool>("Ahri.UseQLaneClear").Enabled;
            var useW = Config.GetValue<MenuBool>("Ahri.UseWLaneClear").Enabled;

            var QMinMana = Config.GetValue<MenuSlider>("Ahri.QMiniManaLaneClear").Value;
            var WMinMana = Config.GetValue<MenuSlider>("Ahri.WMiniManaLaneClear").Value;

            var allMinionsQ = MinionManager.GetMinions(Q.Range);

            if (useQ && Q.IsReady() && Player.Mana >= QMANA && Player.ManaPercent >= QMinMana)
            {

                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, Q.Width);
                if (Qfarm.MinionsHit >= Config.GetValue<MenuSlider>("Ahri.QLaneClearCount").Value)
                    Q.Cast(Qfarm.Position);
            }

            if (useW && W.IsReady() && Player.Mana >= WMANA + QMANA && Player.ManaPercent >= WMinMana)
            {
                if (allMinionsQ.Count(x => x.IsValidTarget(W.Range)) >=
                    Config.GetValue<MenuSlider>("Ahri.WLaneClearCount").Value)
                {
                    W.Cast();
                }
            }
        }

        #endregion
        
        #region JungleClear
        public static void JungleClear()
        {

            var useQ = Config.GetValue<MenuBool>("Ahri.UseQJungleClear").Enabled;
            var useW = Config.GetValue<MenuBool>("Ahri.UseWJungleClear").Enabled;
            var useE = Config.GetValue<MenuBool>("Ahri.UseWJungleClear").Enabled;

            var QMinMana = Config.GetValue<MenuSlider>("Ahri.QMiniManaJungleClear").Value;
            var WMinMana = Config.GetValue<MenuSlider>("Ahri.WMiniManaJungleClear").Value;
            var EMinMana = Config.GetValue<MenuSlider>("Ahri.EMiniManaJungleClear").Value;

            var allMinionsQ = MinionManager.GetMinions(Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral);
            var MinionN = MinionManager.GetMinions(Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (!MinionN.IsValidTarget() || MinionN == null)
            {
                LaneClear();
                return;
            }

            if (Config.GetValue<MenuBool>("Ahri.SafeJungleClear").Enabled && Player.CountEnemyHeroesInRange(1500) > 0) return;

            if (useE && E.IsReady() && Player.Distance(MinionN) < E.Range && Player.Mana >= EMANA + QMANA && Player.ManaPercent >= EMinMana)
            {
                E.CastIfHitchanceEquals(MinionN, HitChance.VeryHigh);
            }

            if (useW && W.IsReady() && Player.Distance(MinionN) < Player.AttackRange && Player.Mana >= WMANA + QMANA && Player.ManaPercent >= WMinMana)
            {
                W.Cast();
            }

            if (useQ && Q.IsReady() && Player.Mana >= QMANA && Player.ManaPercent >= QMinMana)
            {

                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, Q.Width);
                if (Qfarm.MinionsHit >= 1)
                    Q.Cast(Qfarm.Position);
            }



        }
        #endregion
        
        #region PotionManager
        public static void PotionManager()
        {
            if (Player.Level == 1 && Player.CountEnemyHeroesInRange(1000) == 1 && Player.Health >= Player.MaxHealth * 0.35) return;
            if (Player.Level == 1 && Player.CountEnemyHeroesInRange(1000) == 2 && Player.Health >= Player.MaxHealth * 0.50) return;

            if (Config.GetValue<MenuBool>("Ahri.AutoPotion").Enabled && !Player.InFountain() && !Player.IsRecalling() && !Player.IsDead)
            {

                #region HealthPotion
                if (HealthPotion.IsReady && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("ItemCrystalFlask"))
                {

                    if (Player.MaxHealth > Player.Health + 150 && Player.CountEnemyHeroesInRange(1000) > 0 &&
                        Player.Health < Player.MaxHealth * 0.75)
                    {
                        HealthPotion.Cast();
                    }

                    else if (Player.MaxHealth > Player.Health + 150 && Player.CountEnemyHeroesInRange(1000) == 0 &&
                        Player.Health < Player.MaxHealth * 0.6)
                    {
                        HealthPotion.Cast();
                    }

                }
                #endregion
                
            }
        }
        #endregion

        private static void Obj_AI_Base_OnProcessSpellCast(AIBaseClient unit, AIBaseClientProcessSpellCastEventArgs args)
        {
            double ShouldUseOn = ShouldUse(args.SData.Name);
            if (unit.Team != ObjectManager.Player.Team && ShouldUseOn >= 0f && unit.IsValidTarget(Q.Range))
            {

                if (Config.GetValue<MenuBool>("Ahri.EInterrupt").Enabled && E.IsReady() && Player.Mana >= EMANA && Player.Distance(unit) < E.Range - 25)
                {
                    E.CastIfHitchanceEquals(unit, HitChance.VeryHigh);
                }

            }

            if (Config.GetValue<MenuBool>("Ahri.AutoEWhenEnemyCast").Enabled && (((AIHeroClient) unit ).IsValid && !((AITurretClient) unit).IsValid) && unit.IsEnemy && args.Target.IsMe && E.IsReady() && Player.Distance(unit) < E.Range - 25)
            {
                E.CastIfHitchanceEquals(unit, HitChance.VeryHigh);
            }

            if ((Config.GetValue<MenuBool>("Ahri.AutoQWhenEnemyCast").Enabled && ((AIHeroClient) unit).IsValid &&
                 !((AITurretClient) unit).IsValid && unit.IsEnemy && args.Target.IsMe ||
                 Config.GetValue<MenuBool>("Ahri.AutoQWhenE").Enabled && unit.HasBuff("AhriSeduce")) && Q.IsReady() &&
                Player.Distance(unit) < Q.Range - 25)
            {
                Q.CastIfHitchanceEquals(unit, HitChance.VeryHigh);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (Config.GetValue<MenuBool>("Ahri.AutoEEGC").Enabled && E.IsReady() &&
                (Player.Mana >= EMANA + RMANA || Player.Mana < RMANA * 0.8) && Player.Distance(sender) < E.Range - 20)
            {
                E.CastIfHitchanceEquals(sender, HitChance.VeryHigh);
            }

            if (Config.GetValue<MenuBool>("Ahri.AutoWEGC").Enabled && W.IsReady() &&
                (Player.Mana >= WMANA + RMANA || Player.Mana < RMANA * 0.8) && Player.Distance(sender) < W.Range - 20)
            {
                W.Cast();
            }
        }

        #region Interupt Spell List
        public static double ShouldUse(string SpellName)
        {
            if (SpellName == "KatarinaR")
                return 0;
            if (SpellName == "AlZaharNetherGrasp")
                return 0;
            if (SpellName == "GalioIdolOfDurand")
                return 0;
            if (SpellName == "LuxMaliceCannon")
                return 0;
            if (SpellName == "MissFortuneBulletTime")
                return 0;
            if (SpellName == "CaitlynPiltoverPeacemaker")
                return 0;
            if (SpellName == "EzrealTrueshotBarrage")
                return 0;
            if (SpellName == "InfiniteDuress")
                return 0;
            if (SpellName == "VelkozR")
                return 0;
            if (SpellName == "XerathLocusOfPower2")
                return 0;
            if (SpellName == "Drain")
                return 0;
            if (SpellName == "Crowstorm")
                return 0;
            if (SpellName == "ReapTheWhirlwind")
                return 0;
            if (SpellName == "FallenOne")
                return 0;
            if (SpellName == "JudicatorIntervention")
                return 0;
            if (SpellName == "KennenShurikenStorm")
                return 0;
            if (SpellName == "LucianR")
                return 0;
            if (SpellName == "SoulShackles")
                return 0;
            if (SpellName == "NamiQ")
                return 0;
            if (SpellName == "AbsoluteZero")
                return 0;
            if (SpellName == "Pantheon_GrandSkyfall_Jump")
                return 0;
            if (SpellName == "RivenMartyr")
                return 0;
            if (SpellName == "RivenTriCleave_03")
                return 0;
            if (SpellName == "RunePrison")
                return 0;
            if (SpellName == "SkarnerImpale")
                return 0;
            if (SpellName == "UndyingRage")
                return 0;
            if (SpellName == "VarusQ")
                return 0;
            if (SpellName == "MonkeyKingSpinToWin")
                return 0;
            if (SpellName == "YasuoRKnockUpComboW")
                return 0;
            if (SpellName == "ZacE")
                return 0;
            if (SpellName == "ZacR")
                return 0;
            if (SpellName == "UrgotSwap2")
                return 0;
            return -1;
        }
        # endregion
        
        #region PlayerDamage
        public static float getComboDamage(AIHeroClient hero)
        {
            double damage = 0;
            if (E.IsReady())
            {
                damage += Damage.GetSpellDamage(Player, hero, SpellSlot.E);
            }
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(Player, hero, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(Player, hero, SpellSlot.W);
            }
            if (Player.Spellbook.CanUseSpell(Player.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                damage += (float)Player.GetSummonerSpellDamage(hero, SummonerSpell.Ignite);
            }
            return (float)damage;
        }
        #endregion

        #region ManaManager
        public static void ManaManager()
        {

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            RMANA = R.Instance.ManaCost;

            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }
        #endregion
        
        #region Combo

        public static void Combo()
        {
            var useQ = Config.GetValue<MenuBool>("Ahri.UseQCombo").Enabled;
            
            
            var useW = Config.GetValue<MenuBool>("Ahri.UseWCombo").Enabled;
            
            
            var useE = Config.GetValue<MenuBool>("Ahri.UseECombo").Enabled;
            
            
            var useR = Config.GetValue<MenuBool>("Ahri.UseRCombo").Enabled;
            
            var targetR = TargetSelector.GetTarget(Q.Range + R.Range, DamageType.Magical);
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target.IsValidTarget())
            {
                #region Sort E combo mode
                if(useE && E.IsReady() && Player.Mana >= EMANA && Player.Distance(target) < E.Range)
                {
                    E.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                }
                #endregion
                
                #region Sort R combo mode
                if (useR && R.IsReady() && E.IsReady() && Player.Mana > RMANA + QMANA + WMANA + EMANA && Player.HealthPercent >= targetR.HealthPercent)
                {
                    if (Player.Distance(targetR) < R.Range + E.Range - 75 && Player.Distance(targetR) > E.Range - 50 && getComboDamage(targetR) > targetR.Health)
                    {
                        R.Cast(targetR.Position);
                    }
                }
                #endregion

                #region Sort W combo mode
                if (useW && W.IsReady() && Player.Mana >= WMANA && Player.Distance(target) < W.Range)
                {
                    W.Cast();
                }
                #endregion

                #region Sort Q combo mode
                if (useQ && Q.IsReady() && Player.Mana >= QMANA && Player.Distance(target) < Q.Range)
                {
                    QLogic();
                }
                #endregion
            }
        }
        
        #endregion

        private static void Unit_OnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            var useQ = Config.GetValue<MenuBool>("Ahri.UseQCombo").Enabled;
            var useE = Config.GetValue<MenuBool>("Ahri.UseWCombo").Enabled;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            
            if(!sender.IsEnemy) return;

            if (sender.NetworkId == target.NetworkId)
            {
                if (useE && E.IsReady() && Player.Mana >= EMANA && args.EndPos.Distance(Player) < E.Range)
                {
                    var delay = (int)(args.EndTick - Game.Time - E.Delay - 0.1f);
                    if (delay > 0)
                    {
                        DelayAction.Add(delay * 1000, () => E.Cast(args.EndPos));
                    }
                    else
                    {
                        E.Cast(args.EndPos);
                    }
                }

                if (useQ && Q.IsReady() && Player.Mana >= QMANA && args.EndPos.Distance(Player) < Q.Range)
                {

                    var delay = (int)(args.EndTick - Game.Time - Q.Delay - 0.1f);
                    if (delay > 0)
                    {
                        DelayAction.Add(delay * 1000, () => Q.Cast(args.EndPos));
                    }
                    else
                    {
                        Q.Cast(args.EndPos);
                    }
                }
            }
        }

        #region DrawingRange
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.GetValue<MenuBool>("QRange").Enabled)
            {
                CircleRender.Draw(Player.Position,Q.Range,Color.Indigo);
            }
            if (Config.GetValue<MenuBool>("WRange").Enabled)
            {
                CircleRender.Draw(Player.Position,Q.Range,Color.Green);
            }
            if (Config.GetValue<MenuBool>("ERange").Enabled)
            {
                CircleRender.Draw(Player.Position,Q.Range,Color.Green);
            }
            if (Config.GetValue<MenuBool>("RRange").Enabled)
            {
                CircleRender.Draw(Player.Position,Q.Range,Color.Gold);
            }

            if (Config.GetValue<MenuBool>("DrawOrbwalkTarget").Enabled)
            {
                var orbT = Orbwalker.GetTarget();
                if (orbT.IsValidTarget())
                    CircleRender.Draw(orbT.Position, 100, Color.Pink);
            }
        }
        #endregion
        
        #region Up Spell

        private static void LevelUpSpells()
        {
            int qL = Player.Spellbook.GetSpell(SpellSlot.Q).Level + qOff;
            int wL = Player.Spellbook.GetSpell(SpellSlot.W).Level + wOff;
            int eL = Player.Spellbook.GetSpell(SpellSlot.E).Level + eOff;
            int rL = Player.Spellbook.GetSpell(SpellSlot.R).Level + rOff;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = new int[] {0, 0, 0, 0, 0};
                for (int i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[abilitySequence[i] - 1] = level[abilitySequence[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }
        
        #endregion
        
        #region QLogic
        public static void QLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (Player.CountEnemyHeroesInRange(1300) > 1)
            {
                if (target.CountAllyHeroesInRange(Q.Width) >= 1)
                {
                    Q.Cast(target, true, true);
                    return;
                }
                if (target.CountAllyHeroesInRange(Q.Width) == 0)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    return;
                }
                return;
            }

            if (Player.CountEnemyHeroesInRange(1300) == 1)//1
            {
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                return;
            }
            return;
        }
        #endregion
    }
}