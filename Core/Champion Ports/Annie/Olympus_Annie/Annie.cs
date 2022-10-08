using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;
using SharpDX;

namespace Olympus_Annie
{
    internal class Program
    {
        public static void Loads()
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }
        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Annie")
                return;

            Annie.OnLoad();
        }
    }
    internal class MenuSettings
    {
        public class Combo
        {
            public static readonly MenuSeparator menuSeparator      = new MenuSeparator("1", "Combo Settings");
            public static readonly MenuBool useQ                    = new MenuBool("useQ", "Use Q");
            public static readonly MenuBool useW                    = new MenuBool("useW", "Use W");
            public static readonly MenuBool useE                    = new MenuBool("useE", "Use E to Get Stun");
            public static readonly MenuBool useR                    = new MenuBool("useR", "Use R");
            public static readonly MenuBool useIgnite               = new MenuBool("useIgnite", "Use Ignite");
            public static readonly MenuSeparator menuSeparatorUlt   = new MenuSeparator("2", "Ult Settings");
            public static readonly MenuBool ultIfStun               = new MenuBool("ultIfStun", "Only When Stun Is Ready", false);
            public static readonly MenuBool ult1v1                  = new MenuBool("ult1v1", "Use When Only One Enemie's Around");
            public static readonly MenuSlider minEnemiesToR         = new MenuSlider("minEnemiesToR", "Min Enemies to R", 2, 1, 5);
            public static readonly MenuSeparator menuSeparatorAA    = new MenuSeparator("3", "AA Settings");
            public static readonly MenuBool aaMaxRange              = new MenuBool("aaMaxRange", "Disable AutoAttack Max Range");
        }
        public class Harass
        {
            public static readonly MenuSeparator menuSeparator              = new MenuSeparator("1", "Harass Settings");
            public static readonly MenuBool useQ                            = new MenuBool("useQ", "Use Q");
            public static readonly MenuBool useW                            = new MenuBool("useW", "Use W");
            public static readonly MenuBool lastHit                         = new MenuBool("lastHit", "Last Hit");
            public static readonly MenuSeparator menuSeparatorManaManager   = new MenuSeparator("2", "Mana Manager");
            public static readonly MenuSlider minMana                       = new MenuSlider("minMana", "Min. Mana Percent", 50, 1, 99);
        }
        public class LaneClear
        {
            public static readonly MenuSeparator menuSeparator              = new MenuSeparator("1", "LaneClear Settings");
            public static readonly MenuBool useQ                            = new MenuBool("useQ", "Use Q");
            public static readonly MenuBool useW                            = new MenuBool("useW", "Use W");
            public static readonly MenuSeparator menuSeparatorQ             = new MenuSeparator("2", "Q Settings");
            public static readonly MenuBool useQonlyLastHit                 = new MenuBool("useQonlyLastHit", "Use Q Only to LastHit");
            public static readonly MenuList qMode                           = new MenuList("qMode", "Mode Q", new[] { "Always", "AlwaysIfNoEnemiesAround", "AlwaysIfNoStun" });
            public static readonly MenuSeparator menuSeparatorW             = new MenuSeparator("3", "W Settings");
            public static readonly MenuSlider wMinMinions                   = new MenuSlider("wMinMinions", "Min W Hitcount", 3, 1, 7);
            public static readonly MenuSeparator menuSeparatorManaManager   = new MenuSeparator("4", "Mana Manager");
            public static readonly MenuSlider minMana                       = new MenuSlider("minMana", "Min. Mana Percent", 60, 1, 99);
        }
        public class JungleClear
        {
            public static readonly MenuSeparator menuSeparator              = new MenuSeparator("1", "JungleClear Settings");
            public static readonly MenuBool useQ                            = new MenuBool("useQ", "Use Q");
            public static readonly MenuBool useW                            = new MenuBool("useW", "Use W");
            public static readonly MenuBool useE                            = new MenuBool("useE", "Use E");
            public static readonly MenuSeparator menuSeparatorE             = new MenuSeparator("2", "E Settings");
            public static readonly MenuSlider minHpForE                     = new MenuSlider("minHpForE", "Below HP Percent", 60, 1, 99);
            public static readonly MenuSeparator menuSeparatorManaManager   = new MenuSeparator("3", "Mana Manager");
            public static readonly MenuSlider minMana                       = new MenuSlider("minMana", "Min. Mana Percent", 30, 1, 99);
            public static readonly MenuSlider minManaForE                   = new MenuSlider("minManaForE", "Min. Mana Percent For E", 60, 1, 99);
        }
        public class LastHit
        {
            public static readonly MenuSeparator menuSeparator  = new MenuSeparator("1", "LastHit Settings");
            public static readonly MenuBool useQ                = new MenuBool("useQ", "Use Q");
            public static readonly MenuList qMode               = new MenuList("qMode", "Mode Q", new[] { "Always", "AlwaysIfNoEnemiesAround", "AlwaysIfNoStun" });
        }
        public class Misc
        {
            public static readonly MenuSeparator menuSeparator              = new MenuSeparator("1", "Misc Settings");
            public static readonly MenuBool autoE                           = new MenuBool("autoE", "Auto E");
            public static readonly MenuBool interrupter                     = new MenuBool("interrupter", "Interrupter");
            public static readonly MenuBool gapcloser                       = new MenuBool("gapcloser", "Gapcloser");
            public static readonly MenuSeparator menuSeparatorAS            = new MenuSeparator("2", "AutoStrack Settings");
            public static readonly MenuBool autoStackEnable                 = new MenuBool("autoStackEnable", "Enable");
            public static readonly MenuBool autoStackOnlyOnSpawn            = new MenuBool("autoStackOnSpawn", "Only On Spawn");
            public static readonly MenuBool autoStackW                      = new MenuBool("autoStackW", "Use W");
            public static readonly MenuBool autoStackE                      = new MenuBool("autoStackE", "Use E");
            public static readonly MenuSeparator menuSeparatorManaManager   = new MenuSeparator("3", "Mana Manager");
            public static readonly MenuSlider minManaAS                     = new MenuSlider("minManaAS", "Min. Mana Percent", 80, 1, 99);
            public static readonly MenuSeparator menuSeparatorKS            = new MenuSeparator("4", "KillSteal Settings");
            public static readonly MenuBool killstealEnable                 = new MenuBool("killstealEnable", "Enable");
            public static readonly MenuBool killstealQ                      = new MenuBool("killstealQ", "Use Q");
            public static readonly MenuBool killstealW                      = new MenuBool("killstealW", "Use W");
            public static readonly MenuBool killstealR                      = new MenuBool("killstealR", "Use R", false);
            public static readonly MenuBool killstealIgnite                 = new MenuBool("killstealIgnite", "Use Ignite");
            public static readonly MenuSeparator menuSeparatorFR            = new MenuSeparator("5", "Flash + R Settings");
            public static readonly MenuBool flashRCanKill                   = new MenuBool("flashRCanKill", "If Can Kill", false);
            public static readonly MenuBool flashRIfStun                    = new MenuBool("flashRIfStun", "If Can Stun");
        }
        public class Drawing
        {
            public static readonly MenuSeparator menuSeparator          = new MenuSeparator("1", "Drawings");
            public static readonly MenuBool disableDrawing              = new MenuBool("disableDrawing", "Disable", false);
            public static readonly MenuBool saveStun                    = new MenuBool("saveStun", "Save Stun Text");
            public static readonly MenuBool drawDMG                     = new MenuBool("drawDMG", "Draw Damage on HpBar");
            public static readonly MenuSeparator menuSeparatorRange     = new MenuSeparator("2", "Spell Drawings");
            public static readonly MenuBool drawQ                       = new MenuBool("drawQ", "Draw Q Range");
            public static readonly MenuBool drawW                       = new MenuBool("drawW", "Draw W Range");
            public static readonly MenuBool drawR                       = new MenuBool("drawR", "Draw R Range");
            public static readonly MenuBool drawFlashR                  = new MenuBool("drawFlashR", "Draw Flash + R Range");
        }
        public class Credits
        {
            public static readonly MenuSeparator luniEloFactory     = new MenuSeparator("luniEloFactory", "LuNi (EloFactory Annie)");
            public static readonly MenuSeparator nightMoon032       = new MenuSeparator("nightMoon032", "NightMoon032 (Flower's Annie)");
            public static readonly MenuSeparator jotaBritoDev       = new MenuSeparator("jotaBritoDev", "JotaBritoDev (Korean Annie)");
            public static readonly MenuSeparator sayuto             = new MenuSeparator("sayuto", "Sayuto (DaoHungAIO)");
        }
        public class Keys
        {
            public static readonly MenuSeparator menuSeparatorKeys  = new MenuSeparator("5", "Keys Settings");
            public static readonly MenuKeyBind harassToggle         = new MenuKeyBind("harassToggle", "Harass Key", EnsoulSharp.SDK.MenuUI.Keys.H, KeyBindType.Toggle);
            public static readonly MenuKeyBind saveStunToggle       = new MenuKeyBind("saveStunToggle", "Save Stun Key", EnsoulSharp.SDK.MenuUI.Keys.N, KeyBindType.Toggle);            public static readonly MenuKeyBind flashR               = new MenuKeyBind("flashR", "Flash + R Key", EnsoulSharp.SDK.MenuUI.Keys.T, KeyBindType.Press);
        }
    }
    internal class Annie
    {
        private static SpellSlot summonerFlash, summonerIgnite;
        private static Spell Q, W, E, R, FR;
        private static AIHeroClient objPlayer = ObjectManager.Player;
        private static Menu myMenu;

        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q, 625f);
            Q.SetTargetted(0.25f, 1400f);

            W = new Spell(SpellSlot.W, 500f);
            W.SetSkillshot(0.50f, 200f, float.MaxValue, false, SpellType.Cone);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R, 600f);
            R.SetSkillshot(0.20f, 100f, float.MaxValue, false, SpellType.Circle);
            R.MinHitChance = HitChance.High;

            FR = new Spell(SpellSlot.R, 1200f);
            R.SetSkillshot(0.20f, 100f, float.MaxValue, false, SpellType.Circle);

            summonerFlash  = objPlayer.GetSpellSlot("SummonerFlash");
            summonerIgnite = objPlayer.GetSpellSlot("SummonerDot");

            myMenu = new Menu(objPlayer.CharacterName, "Olympus.Annie", true);

            #region Menu Init

            var comboMenu = new Menu("comboMenu", "Combo")
            {
                MenuSettings.Combo.menuSeparator,
                MenuSettings.Combo.useQ,
                MenuSettings.Combo.useW,
                MenuSettings.Combo.useE,
                MenuSettings.Combo.useR,
                MenuSettings.Combo.useIgnite,
                MenuSettings.Combo.menuSeparatorUlt,
                MenuSettings.Combo.ultIfStun,
                MenuSettings.Combo.ult1v1,
                MenuSettings.Combo.minEnemiesToR,
                MenuSettings.Combo.menuSeparatorAA,
                MenuSettings.Combo.aaMaxRange
            };
            myMenu.Add(comboMenu);

            var harassMenu = new Menu("harassMenu", "Harass")
            {
                MenuSettings.Harass.menuSeparator,
                MenuSettings.Harass.useQ,
                MenuSettings.Harass.useW,
                MenuSettings.Harass.lastHit,
                MenuSettings.Harass.menuSeparatorManaManager,
                MenuSettings.Harass.minMana
            };
            myMenu.Add(harassMenu);

            var laneClearMenu = new Menu("laneClearMenu", "Lane Clear")
            {
                MenuSettings.LaneClear.menuSeparator,
                MenuSettings.LaneClear.useQ,
                MenuSettings.LaneClear.useW,
                MenuSettings.LaneClear.menuSeparatorQ,
                MenuSettings.LaneClear.useQonlyLastHit,
                MenuSettings.LaneClear.qMode,
                MenuSettings.LaneClear.menuSeparatorW,
                MenuSettings.LaneClear.wMinMinions,
                MenuSettings.LaneClear.menuSeparatorManaManager,
                MenuSettings.LaneClear.minMana
            };
            myMenu.Add(laneClearMenu);

            var jungleClearMenu = new Menu("jungleClearMenu", "Jungle Clear")
            {
                MenuSettings.JungleClear.menuSeparator,
                MenuSettings.JungleClear.useQ,
                MenuSettings.JungleClear.useW,
                MenuSettings.JungleClear.useE,
                MenuSettings.JungleClear.menuSeparatorE,
                MenuSettings.JungleClear.minHpForE,
                MenuSettings.JungleClear.menuSeparatorManaManager,
                MenuSettings.JungleClear.minMana,
                MenuSettings.JungleClear.minManaForE
            };
            myMenu.Add(jungleClearMenu);

            var lastHitMenu = new Menu("lastHitMenu", "Last Hit")
            {
                MenuSettings.LastHit.menuSeparator,
                MenuSettings.LastHit.useQ,
                MenuSettings.LastHit.qMode
            };
            myMenu.Add(lastHitMenu);

            var miscMenu = new Menu("miscMenu", "Misc")
            {
                MenuSettings.Misc.menuSeparator,
                MenuSettings.Misc.autoE,
                MenuSettings.Misc.interrupter,
                MenuSettings.Misc.gapcloser,

                new Menu("autoStackMenu", "Auto Stack")
                {
                    MenuSettings.Misc.menuSeparatorAS,
                    MenuSettings.Misc.autoStackEnable,
                    MenuSettings.Misc.autoStackOnlyOnSpawn,
                    MenuSettings.Misc.autoStackW,
                    MenuSettings.Misc.autoStackE,
                    MenuSettings.Misc.menuSeparatorManaManager,
                    MenuSettings.Misc.minManaAS
                },

                new Menu("killStealMenu", "KillSteal")
                {
                    MenuSettings.Misc.menuSeparatorKS,
                    MenuSettings.Misc.killstealEnable,
                    MenuSettings.Misc.killstealQ,
                    MenuSettings.Misc.killstealW,
                    MenuSettings.Misc.killstealR,
                    MenuSettings.Misc.killstealIgnite
                },

                new Menu("flashRMenu", "Flash + R")
                {
                    MenuSettings.Misc.menuSeparatorFR,
                    MenuSettings.Misc.flashRCanKill,
                    MenuSettings.Misc.flashRIfStun,
                },
            };
            myMenu.Add(miscMenu);

            var drawingMenu = new Menu("drawingMenu", "Drawing")
            {
                MenuSettings.Drawing.menuSeparator,
                MenuSettings.Drawing.disableDrawing,
                MenuSettings.Drawing.saveStun,
                MenuSettings.Drawing.drawDMG,
                MenuSettings.Drawing.menuSeparatorRange,
                MenuSettings.Drawing.drawQ,
                MenuSettings.Drawing.drawW,
                MenuSettings.Drawing.drawR,
                MenuSettings.Drawing.drawFlashR
            };
            myMenu.Add(drawingMenu);

            var creditsMenu = new Menu("creditsMenu", "Credits")
            {
                MenuSettings.Credits.luniEloFactory,
                MenuSettings.Credits.nightMoon032,
                MenuSettings.Credits.jotaBritoDev,
                MenuSettings.Credits.sayuto
            };
            myMenu.Add(creditsMenu);

            myMenu.Add(MenuSettings.Keys.menuSeparatorKeys);
            myMenu.Add(MenuSettings.Keys.harassToggle).AddPermashow();
            myMenu.Add(MenuSettings.Keys.saveStunToggle).AddPermashow();
            myMenu.Add(MenuSettings.Keys.flashR).AddPermashow();

            myMenu.Attach();

            #endregion

            GameEvent.OnGameTick                     += OnUpdate;
            Drawing.OnDraw                  += OnDraw;
            Drawing.OnEndScene              += OnEndScene;
            Orbwalker.OnBeforeAttack              += OnBeforeAttack;
            Orbwalker.OnNonKillableMinion              += OnNonKillable;
            AntiGapcloser.OnGapcloser           += OnGapcloser;
            Interrupter.OnInterrupterSpell  += OnInterrupterSpell;
        }

        private static void OnNonKillable(object sender, NonKillableMinionEventArgs args)
        {
            switch (args.Target.Type)
            {
                case GameObjectType.AIMinionClient:
                    if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit || Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                    {
                        var target = (AIMinionClient)args.Target;

                        if (target.IsValidTarget() && target.IsMinion())
                        {
                            if (target.Health > Q.GetDamage(target))
                                return;
                            if (Q.Instance.CooldownExpires - Game.Time > 0.3f)
                                return;

                        }
                    }
                    break;
                case GameObjectType.AITurretClient:
                    break;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (objPlayer.IsDead || objPlayer.IsRecalling())
                return;
            if (MenuGUI.IsChatOpen || MenuGUI.IsShopOpen)
                return;

            if (MenuSettings.Misc.killstealEnable.Enabled)
                KillSteal();
            if (MenuSettings.Misc.autoE.Enabled)
                AutoE();
            if (MenuSettings.Misc.autoStackEnable.Enabled)
                AutoStack();
            if (MenuSettings.Keys.flashR.Active)
                FlashR();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }
        }

        #region Orbwalker Modes

        private static void Combo()
        {
            if (MenuSettings.Combo.useE.Enabled && E.IsReady() && getBuffCounts() == 3 && !objPlayer.HasBuff("anniepassiveprimed"))
            {
                E.Cast();
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (MenuSettings.Combo.useR.Enabled && R.IsReady() && !objPlayer.HasBuff("AnnieRController") && target.IsValidTarget(R.Range))
            {
                if (MenuSettings.Combo.ultIfStun.Enabled && !objPlayer.HasBuff("anniepassiveprimed"))
                    return;

                if (MenuSettings.Combo.minEnemiesToR.Value == 1)
                {
                    if (target.Health < getDamage(target, true, true, false, false))
                        return;

                    R.Cast(target.Position);
                }
                else
                {
                    if (MenuSettings.Combo.ult1v1.Enabled && objPlayer.CountEnemyHeroesInRange(1500) == 1)
                    {
                        if (target.Health < getDamage(target, true, true, false, false))
                            return;

                        R.Cast(target.Position);
                    }
                    else
                    {
                        var getTargetPrediction = R.GetPrediction(target, true);

                        if (getTargetPrediction.Hitchance >= HitChance.High && getTargetPrediction.AoeTargetsHit.Count() >= MenuSettings.Combo.minEnemiesToR.Value)
                        {
                            R.Cast(getTargetPrediction.CastPosition);
                        }
                    }
                }
            }
            if (MenuSettings.Combo.useQ.Enabled && Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            if (MenuSettings.Combo.useW.Enabled && W.IsReady() && target.IsValidTarget(W.Range))
            {
                W.Cast(target.Position);
            }
            if (MenuSettings.Combo.useIgnite.Enabled && summonerIgnite.IsReady() && target.IsValidTarget(600))
            {
                if (target.Health > getDamage(target, false, false, false, true))
                    return;

                objPlayer.Spellbook.CastSpell(summonerIgnite, target);
            }
        }
        private static void Harass()
        {
            if (!MenuSettings.Keys.harassToggle.Active)
                return;

            if (MenuSettings.Keys.saveStunToggle.Active && objPlayer.HasBuff("anniepassiveprimed"))
                return;
            if (objPlayer.ManaPercent < MenuSettings.LaneClear.minMana.Value)
                return;

            if (getBuffCounts() == 3 && E.IsReady() && !objPlayer.HasBuff("anniepassiveprimed"))
                E.Cast();

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
                return;

            if (MenuSettings.Harass.useQ.Enabled && Q.IsReady())
            {
                if (!target.IsValidTarget(Q.Range))
                    return;

                Q.Cast(target);
            }
            if (MenuSettings.Harass.useW.Enabled && W.IsReady())
            {
                if (!target.IsValidTarget(W.Range))
                    return;

                W.Cast(target.Position);
            }
            /* TODO: tibbers harass, but ensoulsharp doesnt support movepet yet 
             * 
            if (objPlayer..HasBuff("AnnieRController"))
            {

            }
             
             */

            if (MenuSettings.Harass.lastHit.Enabled)
                LastHit();
        }
        private static void LaneClear()
        {
            if (MenuSettings.Keys.saveStunToggle.Active && objPlayer.HasBuff("anniepassiveprimed"))
                return;

            var allMinions = GameObjects.EnemyMinions;

            if (MenuSettings.LaneClear.useQ.Enabled && Q.IsReady())
            {
                var minTarget = allMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion()).OrderBy(m => m.Health / m.MaxHealth * 100).ToList();

                if (!minTarget.Any())
                    return;

                foreach (var min in minTarget)
                {
                    if (min.Health < objPlayer.GetAutoAttackDamage(min))
                        return;

                    if (MenuSettings.LaneClear.useQonlyLastHit.Enabled && Q.GetDamage(min) >= min.Health)
                    {
                        switch (MenuSettings.LaneClear.qMode.SelectedValue)
                        {
                            case "Always":
                                Q.Cast(min);
                                break;
                            case "AlwaysIfNoEnemiesAround":
                                if (objPlayer.CountEnemyHeroesInRange(1000) == 0)
                                    Q.Cast(min);
                                break;
                            case "AlwaysIfNoStun":
                                if (!objPlayer.HasBuff("anniepassiveprimed"))
                                    Q.Cast(min);
                                break;
                        }
                    }
                    else if (!MenuSettings.LaneClear.useQonlyLastHit.Enabled)
                    {
                        if (objPlayer.ManaPercent < MenuSettings.LaneClear.minMana.Value)
                            return;

                        switch (MenuSettings.LaneClear.qMode.SelectedValue)
                        {
                            case "Always":
                                Q.Cast(min);
                                break;
                            case "AlwaysIfNoEnemiesAround":
                                if (objPlayer.CountEnemyHeroesInRange(1000) == 0)
                                    Q.Cast(min);
                                break;
                            case "AlwaysIfNoStun":
                                if (!objPlayer.HasBuff("anniepassiveprimed"))
                                    Q.Cast(min);
                                break;
                        }
                    }
                }
            }
            if (MenuSettings.LaneClear.useW.Enabled && W.IsReady())
            {
                if (objPlayer.ManaPercent < MenuSettings.LaneClear.minMana.Value)
                    return;

                var minTarget = allMinions.Where(x => x.IsValidTarget(W.Range) && x.IsMinion()).ToList();

                if (!minTarget.Any())
                    return;
                if (objPlayer.IsUnderAllyTurret(100))
                    return;

                var circularFarmLocation = W.GetCircularFarmLocation(minTarget);

                if (circularFarmLocation.MinionsHit >= MenuSettings.LaneClear.wMinMinions.Value)
                    W.Cast(circularFarmLocation.Position);
            }
        }
        private static void JungleClear()
        {
            if (MenuSettings.Keys.saveStunToggle.Active && objPlayer.HasBuff("anniepassiveprimed"))
                return;

            var allMobs = GameObjects.Jungle.Where(x => x.IsValidTarget()).OrderByDescending(x => x.MaxHealth);

            if (allMobs.Count() == 0)
                return;

            foreach (var min in allMobs)
            {
                if (MenuSettings.JungleClear.useQ.Enabled && Q.IsReady())
                {
                    if (!min.IsValidTarget(Q.Range))
                        return;
                    if (min.Health < objPlayer.GetAutoAttackDamage(min))
                        return;

                    if (min.Health < Q.GetDamage(min))
                    {
                        Q.Cast(min, true);
                    }
                    else if (objPlayer.ManaPercent > MenuSettings.JungleClear.minMana.Value)
                    {
                        Q.Cast(min, true);
                    }
                }
                if (MenuSettings.JungleClear.useW.Enabled && W.IsReady())
                {
                    if (!min.IsValidTarget(W.Range))
                        return;

                    DelayAction.Add(50, () =>
                        W.Cast(min.Position));
                }
                if (MenuSettings.JungleClear.useE.Enabled && E.IsReady())
                {
                    if (objPlayer.ManaPercent < MenuSettings.JungleClear.minManaForE.Value)
                        return;
                    if (objPlayer.HealthPercent > MenuSettings.JungleClear.minHpForE.Value)
                        return;

                    DelayAction.Add(50, () =>
                        E.Cast());
                }
            }
        }
        private static void LastHit()
        {
            var allMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion()).OrderBy(m => m.Health / m.MaxHealth * 100).ToList();

            if (!allMinions.Any())
                return;
            if (!Q.IsReady())
                return;

            foreach (var min in allMinions)
            {
                if (min.Health < objPlayer.GetAutoAttackDamage(min))
                    return;
                if (min.Health > Q.GetDamage(min))
                    return;

                switch (MenuSettings.LaneClear.qMode.SelectedValue)
                {
                    case "Always":
                        Q.Cast(min);
                        break;
                    case "AlwaysIfNoEnemiesAround":
                        if (objPlayer.CountEnemyHeroesInRange(1000) == 0)
                            Q.Cast(min);
                        break;
                    case "AlwaysIfNoStun":
                        if (!objPlayer.HasBuff("anniepassiveprimed"))
                            Q.Cast(min);
                        break;
                }
            }
        }

        #endregion

        #region Events

        private static void OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (MenuSettings.Combo.aaMaxRange.Enabled && objPlayer.Distance(args.Target) >= 550)
            {
                args.Process = false;
            }
        }
        private static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (MenuSettings.Misc.gapcloser.Enabled && sender.IsEnemy && !objPlayer.IsDead)
            {
                if ( E.IsReady() && getBuffCounts() == 3)
                {
                    E.Cast();
                }

                if (!objPlayer.HasBuff("anniepassiveprimed"))
                    return;

                if (Q.IsReady() && sender.IsValidTarget(Q.Range))
                    Q.Cast(sender);
                else if (W.IsReady() && sender.IsValidTarget(W.Range))
                    W.Cast(sender);
                    
            }
        }
        private static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (MenuSettings.Misc.interrupter.Enabled && args.DangerLevel >= Interrupter.DangerLevel.High)
            {
                if (!sender.IsValidTarget(Q.Range))
                    return;

                if (E.IsReady() && getBuffCounts() == 3)
                {
                    E.Cast();
                }

                if (objPlayer.HasBuff("anniepassiveprimed"))
                {
                    if (W.IsReady() && sender.IsValidTarget(W.Range))
                    {
                        W.Cast(sender.Position);
                        return;
                    }
                    else if (Q.IsReady() && sender.IsValidTarget(Q.Range))
                    {
                        Q.Cast(sender);
                        return;
                    }
                    else if (R.IsReady() && sender.IsValidTarget(R.Range))
                    {
                        R.Cast(sender.Position);
                        return;
                    }
                }
            }
        }

        #endregion

        #region Drawing

        private static void OnDraw(EventArgs args)
        {
            if (MenuSettings.Drawing.disableDrawing.Enabled)
                return;

            var playerPos = Drawing.WorldToScreen(objPlayer.Position);

            if (MenuSettings.Drawing.saveStun.Enabled)
            {
                Drawing.DrawText(playerPos.X - 50, playerPos.Y + 15, System.Drawing.Color.White, "Save Stun:");
                Drawing.DrawText(playerPos.X + 13, playerPos.Y + 15, MenuSettings.Keys.saveStunToggle.Active ? System.Drawing.Color.LimeGreen : System.Drawing.Color.Red, MenuSettings.Keys.saveStunToggle.Active ? "Enabled" : "Disabled");
            }
            if (MenuSettings.Drawing.drawQ.Enabled)
            {
                CircleRender.Draw(objPlayer.Position, Q.Range, Color.AliceBlue);
            }
            if (MenuSettings.Drawing.drawW.Enabled)
            {
                CircleRender.Draw(objPlayer.Position, W.Range, Color.CadetBlue);
            }
            if (MenuSettings.Drawing.drawR.Enabled)
            {
                CircleRender.Draw(objPlayer.Position, R.Range, Color.DarkSlateBlue);
            }
            if (MenuSettings.Drawing.drawFlashR.Enabled)
            {
                if (!R.IsReady() || ObjectManager.Player.Spellbook.CanUseSpell(summonerFlash) != 0)
                    return;
                if (ObjectManager.Player.HasBuff("AnnieRController"))
                    return;

                CircleRender.Draw(objPlayer.Position, R.Range + 600, Color.DarkBlue);
            }
        }
        private static void OnEndScene(EventArgs args)
        {
            if (MenuSettings.Drawing.disableDrawing.Enabled)
                return;
            if (!MenuSettings.Drawing.drawDMG.Enabled)
                return;

            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(2000) && !x.IsDead && x.IsHPBarRendered))
            {
                Vector2 pos = Drawing.WorldToScreen(target.Position);

                if (!pos.IsOnScreen())
                    return;

                var damage = getDamage(target, true, true, true, true);

                var hpBar = target.HPBarPosition;

                if (damage > target.Health)
                {
                    Drawing.DrawText(hpBar.X + 69, hpBar.Y - 45, System.Drawing.Color.White, "KILLABLE");
                }

                var damagePercentage = ((target.Health - damage) > 0 ? (target.Health - damage) : 0) / target.MaxHealth;
                var currentHealthPercentage = target.Health / target.MaxHealth;

                var startPoint = new Vector2(hpBar.X - 45 + damagePercentage * 104, hpBar.Y - 18);
                var endPoint = new Vector2(hpBar.X - 45 + currentHealthPercentage * 104, hpBar.Y - 18);

                Drawing.DrawLine(startPoint, endPoint, 12, System.Drawing.Color.Yellow);
            }
        }

        #endregion

        #region Misc

        private static void KillSteal()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(target => target.IsValidTarget(Q.Range)))
            {
                if (target == null)
                    return;
                if (target.HasBuff("SionPassiveZombie"))
                    return;
                if (target.HasBuffOfType(BuffType.Invulnerability) && target.HasBuffOfType(BuffType.SpellImmunity))
                    return;

                if (MenuSettings.Misc.killstealQ.Enabled && Q.IsReady() && target.Health + target.MagicalShield < Q.GetDamage(target) && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target, true);
                    return;
                }
                if (MenuSettings.Misc.killstealW.Enabled && W.IsReady() && target.Health + target.MagicalShield < W.GetDamage(target) && target.IsValidTarget(W.Range))
                {
                    W.Cast(target, true);
                    return;
                }
                if (MenuSettings.Misc.killstealR.Enabled && R.IsReady() && target.Health + target.MagicalShield < R.GetDamage(target) && target.IsValidTarget(R.Range))
                {
                    R.Cast(target, true);
                    return;
                }
                if (MenuSettings.Misc.killstealIgnite.Enabled && summonerIgnite.IsReady() && target.Health < objPlayer.GetSummonerSpellDamage(target, SummonerSpell.Ignite) && target.IsValidTarget(600))
                {
                    objPlayer.Spellbook.CastSpell(summonerIgnite, target);
                    return;
                }
            }
        }
        private static void AutoE()
        {
            if (!MenuSettings.Misc.autoE.Enabled)
                return;

            if (E.IsReady() && objPlayer.HasBuff("anniepassiveprimed") && objPlayer.HealthPercent <= 10)
            {
                if (objPlayer.Mana < Q.Mana + W.Mana + R.Mana)
                    return;

                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear || Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
                    return;

                if (objPlayer.CountEnemyHeroesInRange(2000) == 0)
                    return;

                DelayAction.Add(100, () => 
                    E.Cast());
            }
        }
        private static void AutoStack()
        {
            if (objPlayer.HasBuff("anniepassiveprimed"))
                return;
            if (MenuSettings.Keys.flashR.Active)
                return;

            if (objPlayer.InFountain())
            {
                if (W.IsReady() && MenuSettings.Misc.autoStackW.Enabled)
                    W.Cast(objPlayer.Position);
                if (E.IsReady() && MenuSettings.Misc.autoStackE.Enabled)
                    E.Cast();
            }
            else if (!MenuSettings.Misc.autoStackOnlyOnSpawn.Enabled && MenuSettings.Misc.minManaAS.Value < objPlayer.ManaPercent && !objPlayer.InFountain())
            {
                var enemyMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(1500));
                var jungleMinions = GameObjects.Jungle.Where(x => x.IsValidTarget(1500));

                if (objPlayer.CountEnemyHeroesInRange(1500) != 0 || enemyMinions.Count() > 0 || jungleMinions.Count() > 0)
                    return;

                if (E.IsReady() && MenuSettings.Misc.autoStackE.Enabled)
                {
                    E.Cast();
                }
                else if (W.IsReady() && MenuSettings.Misc.autoStackW.Enabled)
                {
                    W.Cast(objPlayer.Position);
                }
            }
        }
        private static void FlashR()
        {
            objPlayer.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (R.IsReady() && summonerFlash.IsReady() && !objPlayer.HasBuff("AnnieRController"))
            {
                if (E.IsReady() && getBuffCounts() == 3)
                {
                    E.Cast();
                }

                if (MenuSettings.Misc.flashRIfStun.Enabled && !objPlayer.HasBuff("anniepassiveprimed"))
                    return;

                var target = TargetSelector.GetTarget(FR.Range,DamageType.Magical);
                var getTargetPrediction = FR.GetPrediction(target, true);

                if (getTargetPrediction.Hitchance >= HitChance.High)
                {
                    if (MenuSettings.Misc.flashRCanKill.Enabled && target.Health > getDamage(target, true, true, true, true))
                        return;

                    if (objPlayer.CountEnemyHeroesInRange(1500) == 1)
                    {
                        objPlayer.Spellbook.CastSpell(summonerFlash, target.Position);

                        DelayAction.Add(100, () => 
                            R.Cast(getTargetPrediction.CastPosition));
                    }
                    else
                    {
                        if (getTargetPrediction.AoeTargetsHit.Count() >= 2)
                        {
                            objPlayer.Spellbook.CastSpell(summonerFlash, target.Position);

                            DelayAction.Add(100, () =>
                                R.Cast(getTargetPrediction.CastPosition));
                        }
                    }
                }
            }
        }

        #region Extensions

        private static float getDamage(AIBaseClient target, bool q = false, bool w = false, bool r = false, bool ignite = false)
        {
            float damage = 0;

            if (target == null || target.IsDead)
                return 0;

            if (target.HasBuffOfType(BuffType.Invulnerability))
                return 0;
            if (target.HasBuff("KingredRNoDeathBuff") || target.HasBuff("FioraW") || target.HasBuff("UndyingRage"))
                return 0;

            if (q && Q.IsReady())
                damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.Q);
            if (w && W.IsReady())
                damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.W);
            if (r && R.IsReady() && !objPlayer.HasBuff("AnnieRController"))
                damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.R);

            if (ignite && summonerIgnite.IsReady())
                damage += (float)objPlayer.GetSummonerSpellDamage(target, SummonerSpell.Ignite);

            if (objPlayer.GetBuffCount("itemmagicshankcharge") == 100) // oktw
                damage += (float)objPlayer.CalculateMagicDamage(target, 100 + 0.1 * objPlayer.TotalMagicalDamage);

            if (objPlayer.HasBuff("SummonerExhaust"))
                damage = damage * 0.6f;

            if (target.HasBuff("ManaBarrier") && target.HasBuff("BlitzcrankManaBarrierCO"))
                damage += target.Mana / 2f;
            if (target.HasBuff("GarenW"))
                damage = damage * 0.7f;

            return damage;
        }
        private static int getBuffCounts()
        {
            int count = 0;

            if (objPlayer.HasBuff("anniepassivestack"))
                count = objPlayer.GetBuffCount("anniepassivestack");
            else if (!objPlayer.HasBuff("anniepassivestack") || objPlayer.HasBuff("anniepassiveprimed"))
                count = 0;

            return count;
        }

        #endregion

        #endregion
    }
}