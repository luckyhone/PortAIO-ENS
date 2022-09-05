using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;
using SharpDX;

namespace Olympus_Veigar
{
    internal class Program
    {
        public static void Loads()
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }
        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Veigar")
                return;

            Veigar.OnLoad();
        }
    }
    internal class MenuSettings
    {
        public class Combo
        {
            public static MenuSeparator comboSeparator          = new MenuSeparator("comboSeparator", "Combo Settings");
            public static MenuBool useQ                         = new MenuBool("useQ", "Use Q");
            public static MenuBool useW                         = new MenuBool("useW", "Use W");
            public static MenuBool useE                         = new MenuBool("useE", "Use E");
            public static MenuBool useR                         = new MenuBool("useR", "Use R");
            public static MenuBool useIgnite                    = new MenuBool("useIgnite", "Use Ignite");
            public static MenuSeparator wSettingsSeparator      = new MenuSeparator("wSettingsSeparator", "W Settings");
            public static MenuList wMode                        = new MenuList("wMode", "Select Mode", new[] { "Common Prediction", "Only On Stunned Enemies" }, 0);
            public static MenuSeparator eSettingsSeparator      = new MenuSeparator("eSettingsSeparator", "E Settings");
            public static MenuBool eImmobile                    = new MenuBool("eImmobile", "Don't Use E on Immobile Enemies");
            public static MenuList eMode                        = new MenuList("eMode", "Select Mode:", new[] { "Target On The Center", "Target On The Edge" }, 1);
        }
        public class Harass
        {
            public static MenuSeparator harassSeparator         = new MenuSeparator("harassSeparator", "Harass Settings");
            public static MenuBool useQ                         = new MenuBool("useQ", "Use Q");
            public static MenuBool useW                         = new MenuBool("useW", "Use W");
            public static MenuSeparator wSettingsSeparator      = new MenuSeparator("wSettingsSeparator", "W Settings");
            public static MenuList wMode                        = new MenuList("wMode", "Select Mode", new[] { "Common Prediction", "Only On Stunned Enemies" }, 0);
            public static MenuSeparator manaSeperator           = new MenuSeparator("manaSeperator", "Mana Manager");
            public static MenuSlider minMana                    = new MenuSlider("minMana", "Min Mana Percent", 50, 0, 100);
        }
        public class LaneClear
        {
            public static MenuSeparator laneClearSeperator      = new MenuSeparator("laneClearSeperator", "Lane Clear Settings");
            public static MenuBool useQ                         = new MenuBool("useQ", "Use Q");
            public static MenuBool useW                         = new MenuBool("useW", "Use W");
            public static MenuSeparator qSettingsSeperator      = new MenuSeparator("qSettingsSeperator", "Q Settings");
            public static MenuBool autostackClear               = new MenuBool("autostackClear", "Auto Stack");
            public static MenuList qStackMode                   = new MenuList("qStackMode", "Select Mode:", new[] { "LastHit 1 Minion", "LastHit 2 Minions" }, 1);
            public static MenuSeparator wSettingsSeperator      = new MenuSeparator("wSettingsSeperator", "W Settings");
            public static MenuSlider minHitsW                   = new MenuSlider("minHitsW", "Min Minions Hit", 3, 1, 6);
            public static MenuSeparator manaSeperator           = new MenuSeparator("manaSeperator", "Mana Manager");
            public static MenuSlider minManaQ                   = new MenuSlider("minManaQ", "Min Mana Percent For Q", 40, 0, 100);
            public static MenuSlider minManaW                   = new MenuSlider("minManaW", "Min Mana Percent For W", 60, 0, 100);
        }
        public class JungleClear
        {
            public static MenuSeparator jungleClearSeparator    = new MenuSeparator("jungleClearSeparator", "Jungle Clear Settings");
            public static MenuBool useQ                         = new MenuBool("useQ", "Use Q");
            public static MenuBool useW                         = new MenuBool("useW", "Use W");
            public static MenuBool useE                         = new MenuBool("useE", "Use E");
            public static MenuSeparator qSettingsSeparator      = new MenuSeparator("qSettingsSeparator", "Q Settings");
            public static MenuList qMode                        = new MenuList("qMode", "Select Mode:", new[] { "All Monsters", "Big Monsters" }, 0);
            public static MenuSeparator wSettingsSeparator      = new MenuSeparator("wSettingsSeparator", "W Settings");
            public static MenuList wMode                        = new MenuList("wMode", "Select Mode:", new[] { "All Monsters", "Big Monsters" }, 0);
            public static MenuSeparator eSettingsSeparator      = new MenuSeparator("eSettingsSeparator", "E Settings");
            public static MenuList eMode                        = new MenuList("eMode", "Select Mode:", new[] { "All Monsters", "Big Monsters" }, 0);
            public static MenuSeparator manaSeperator           = new MenuSeparator("manaSeperator", "Mana Manager");
            public static MenuSlider minManaQ                   = new MenuSlider("minManaQ", "Min Mana Percent For Q", 30, 0, 100);
            public static MenuSlider minManaW                   = new MenuSlider("minManaW", "Min Mana Percent For W", 30, 0, 100);
            public static MenuSlider minManaE                   = new MenuSlider("minManaE", "Min Mana Percent For E", 60, 0, 100);
        }
        public class LastHit
        {
            public static MenuSeparator lastHitSeparator        = new MenuSeparator("lastHitSeparator", "Last Hit Settings");
            public static MenuBool useQ                         = new MenuBool("useQ", "Use Q");
            public static MenuSeparator qSettingsSeperator      = new MenuSeparator("qSettingsSeperator", "Q Settings");
            public static MenuBool autostackClear               = new MenuBool("autostackClear", "Auto Stack");
            public static MenuList qStackMode                   = new MenuList("qStackMode", "Select Mode:", new[] { "LastHit 1 Minion", "LastHit 2 Minions" }, 0);

        }
        public class Misc
        {
            public static MenuSeparator miscSeparator           = new MenuSeparator("miscSeparator", "Misc Settings");
            public static MenuBool interrupter                  = new MenuBool("interrupter", "Interrupter");
            public static MenuBool gapcloser                    = new MenuBool("gapcloser", "Gapcloser");
            public static MenuSeparator killStealSeparator      = new MenuSeparator("killStealSeparator", "KillSteal Settings");
            public static MenuBool killstealEnable              = new MenuBool("killstealEnable", "Enable");
            public static MenuBool killstealQ                   = new MenuBool("killstealQ", "Use Q");
            public static MenuBool killstealW                   = new MenuBool("killstealW", "Use W");
            public static MenuBool killstealR                   = new MenuBool("killstealR", "Use R");
            public static MenuBool killstealIgnite              = new MenuBool("killstealIgnite", "Use Ignite");
        }
        public class Drawing
        {
            public static MenuSeparator drawingSeparator        = new MenuSeparator("drawingSeparator", "Drawings");
            public static MenuBool disableDrawings              = new MenuBool("disableDrawings", "Disable", false);
            public static MenuBool drawAutoStack                = new MenuBool("drawAutoStack", "Auto Stack Text");
            public static MenuBool drawDmg                      = new MenuBool("drawDmg", "Damage Indicator");
            public static MenuSeparator rangesSeperator         = new MenuSeparator("rangesSeperator", "Spell Ranges");
            public static MenuBool drawQ                        = new MenuBool("drawQ", "Q Range");
            public static MenuBool drawW                        = new MenuBool("drawW", "W Range");
            public static MenuBool drawE                        = new MenuBool("drawE", "E Range");
            public static MenuBool drawR                        = new MenuBool("drawR", "R Range");
        }
        public class Credits
        {
            public static MenuSeparator toyota7                 = new MenuSeparator("toyota7", "Toyota7 (T7 Veigar)");
            public static MenuSeparator xQxCPMxQx               = new MenuSeparator("xQxCPMxQx", "xQxCPMxQx (Placebo Veigar)");
            public static MenuSeparator sayuto                  = new MenuSeparator("sayuto", "Sayuto (DasHungAIO)");

        }
        public class Keys
        {
            public static MenuSeparator keysSeperator           = new MenuSeparator("keysSeperator", "Keys Settings");
            public static MenuKeyBind harassToggle              = new MenuKeyBind("harassToggle", "Harass Key", EnsoulSharp.SDK.MenuUI.Keys.H, KeyBindType.Toggle);
            public static MenuKeyBind farmToggle                = new MenuKeyBind("farmToggle", "Spell Farm Key", EnsoulSharp.SDK.MenuUI.Keys.J, KeyBindType.Toggle);        }
    }
    internal class Veigar
    {
        private static SpellSlot summonerIgnite;
        private static Spell Q, W, E, R;
        private static AIHeroClient objPlayer = ObjectManager.Player;
        private static Menu myMenu;

        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q, 950f);
            Q.SetSkillshot(0.25f, 70f, 2000f, true, SpellType.Line);

            W = new Spell(SpellSlot.W, 900f);
            W.SetSkillshot(1.25f, 225f, 0, false, SpellType.Circle);

            E = new Spell(SpellSlot.E, 700f);
            E.SetSkillshot(0.5f, 380f, 0, false, SpellType.Circle);

            R = new Spell(SpellSlot.R, 650f);
            R.SetTargetted(0.25f, float.MaxValue);

            #region Menu Init

            myMenu = new Menu(objPlayer.CharacterName, "Olympus.Veigar", true);

            var comboMenu = new Menu("comboMenu", "Combo")
            {
                MenuSettings.Combo.comboSeparator,
                MenuSettings.Combo.useQ,
                MenuSettings.Combo.useW,
                MenuSettings.Combo.useE,
                MenuSettings.Combo.useR,
                MenuSettings.Combo.useIgnite,
                MenuSettings.Combo.wSettingsSeparator,
                MenuSettings.Combo.wMode,
                MenuSettings.Combo.eSettingsSeparator,
                MenuSettings.Combo.eImmobile,
                MenuSettings.Combo.eMode,
            };
            myMenu.Add(comboMenu);

            var harassMenu = new Menu("harassMenu", "Harass")
            {
                MenuSettings.Harass.harassSeparator,
                MenuSettings.Harass.useQ,
                MenuSettings.Harass.useW,
                MenuSettings.Harass.wSettingsSeparator,
                MenuSettings.Harass.wMode,
                MenuSettings.Harass.manaSeperator,
                MenuSettings.Harass.minMana,
            };
            myMenu.Add(harassMenu);

            var laneClearMenu = new Menu("laneClearMenu", "Lane Clear")
            {
                MenuSettings.LaneClear.laneClearSeperator,
                MenuSettings.LaneClear.useQ,
                MenuSettings.LaneClear.useW,
                MenuSettings.LaneClear.qSettingsSeperator,
                MenuSettings.LaneClear.autostackClear,
                MenuSettings.LaneClear.qStackMode,
                MenuSettings.LaneClear.wSettingsSeperator,
                MenuSettings.LaneClear.minHitsW,
                MenuSettings.LaneClear.manaSeperator,
                MenuSettings.LaneClear.minManaQ,
                MenuSettings.LaneClear.minManaW,
            };
            myMenu.Add(laneClearMenu);

            var jungleClearMenu = new Menu("jungleClearMenu", "Jungle Clear")
            {
                MenuSettings.JungleClear.jungleClearSeparator,
                MenuSettings.JungleClear.useQ,
                MenuSettings.JungleClear.useW,
                MenuSettings.JungleClear.useE,
                MenuSettings.JungleClear.qSettingsSeparator,
                MenuSettings.JungleClear.qMode,
                MenuSettings.JungleClear.wSettingsSeparator,
                MenuSettings.JungleClear.wMode,
                MenuSettings.JungleClear.eSettingsSeparator,
                MenuSettings.JungleClear.eMode,
                MenuSettings.JungleClear.manaSeperator,
                MenuSettings.JungleClear.minManaQ,
                MenuSettings.JungleClear.minManaW,
                MenuSettings.JungleClear.minManaE,
            };
            myMenu.Add(jungleClearMenu);

            var lastHitMenu = new Menu("lastHitMenu", "Last Hit")
            {
                MenuSettings.LastHit.lastHitSeparator,
                MenuSettings.LastHit.useQ,
                MenuSettings.LastHit.qSettingsSeperator,
                MenuSettings.LastHit.autostackClear,
                MenuSettings.LastHit.qStackMode,
            };
            myMenu.Add(lastHitMenu);

            var miscMenu = new Menu("miscMenu", "Misc")
            {
                MenuSettings.Misc.miscSeparator,
                MenuSettings.Misc.interrupter,
                MenuSettings.Misc.gapcloser,

                new Menu("killStealMenu", "KillSteal")
                {
                    MenuSettings.Misc.killStealSeparator,
                    MenuSettings.Misc.killstealEnable,
                    MenuSettings.Misc.killstealQ,
                    MenuSettings.Misc.killstealW,
                    MenuSettings.Misc.killstealR,
                    MenuSettings.Misc.killstealIgnite,
                },
            };
            myMenu.Add(miscMenu);

            var drawingMenu = new Menu("drawingMenu", "Drawings")
            {
                MenuSettings.Drawing.drawingSeparator,
                MenuSettings.Drawing.disableDrawings,
                MenuSettings.Drawing.drawDmg,
                MenuSettings.Drawing.rangesSeperator,
                MenuSettings.Drawing.drawQ,
                MenuSettings.Drawing.drawW,
                MenuSettings.Drawing.drawE,
                MenuSettings.Drawing.drawR,
            };
            myMenu.Add(drawingMenu);

            var creditsMenu = new Menu("creditsMenu", "Credits")
            {
                MenuSettings.Credits.toyota7,
                MenuSettings.Credits.xQxCPMxQx,
                MenuSettings.Credits.sayuto,
            };
            myMenu.Add(creditsMenu);

            myMenu.Add(MenuSettings.Keys.keysSeperator);
            myMenu.Add(MenuSettings.Keys.harassToggle.SetValue(true)).Permashow();
            myMenu.Add(MenuSettings.Keys.farmToggle.SetValue(true)).Permashow();

            myMenu.Attach();

            #endregion

            GameEvent.OnGameTick                     += OnUpdate;
            Drawing.OnDraw                  += OnDraw;
            Drawing.OnEndScene              += OnEndScene;
            Orbwalker.OnBeforeAttack              += OnAction;
            Interrupter.OnInterrupterSpell  += OnInterrupterSpell;
            AntiGapcloser.OnGapcloser           += OnGapcloser;
        }
        private static void OnUpdate(EventArgs args)
        {
            if (objPlayer.IsDead || objPlayer.IsRecalling())
                return;
            if (MenuGUI.IsChatOpen || MenuGUI.IsShopOpen)
                return;

            if (MenuSettings.Misc.killstealEnable.Enabled)
                KillSteal();

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
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);

            if (target == null || target.IsDead || target.IsAlly)
                return;

            if (MenuSettings.Combo.useE.Enabled && E.IsReady())
            {
                if (!target.IsValidTarget(E.Range))
                    return;

                if (MenuSettings.Combo.eImmobile.Enabled && target.HasBuffOfType(BuffType.Stun))
                    return;

                var getPrediction = E.GetPrediction(target);

                switch (MenuSettings.Combo.eMode.SelectedValue)
                {
                    case "Target On The Center":
                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            E.Cast(getPrediction.CastPosition);
                        }
                        break;
                    case "Target On The Edge":
                        if (getPrediction.CastPosition.DistanceToPlayer() < E.Range - 5)
                        {
                            E.Cast(getPrediction.CastPosition.Extend(objPlayer.Position, 300));
                        }
                        break;
                }
            }
            if (MenuSettings.Combo.useQ.Enabled && Q.IsReady())
            {
                if (!target.IsValidTarget(Q.Range - 50))
                    return;

                var getPrediction = Q.GetPrediction(target);

                if (getPrediction.CollisionObjects.Count() < 2 && getPrediction.Hitchance >= HitChance.High)
                    Q.Cast(getPrediction.CastPosition);
            }
            if (MenuSettings.Combo.useW.Enabled && W.IsReady())
            {
                if (!target.IsValidTarget(W.Range - 50))
                    return;
                if (E.IsReady() && objPlayer.ManaPercent > Q.Mana + W.Mana)
                    return;

                var getPrediction = W.GetPrediction(target);

                switch (MenuSettings.Combo.wMode.SelectedValue)
                {
                    case "Common Prediction":
                        if (getPrediction.Hitchance >= HitChance.High || target.HasBuffOfType(BuffType.Slow))
                        {
                            W.Cast(getPrediction.CastPosition);
                        }
                        break;
                    case "Only On Stunned Enemies":
                        if (getPrediction.Hitchance == HitChance.Immobile)
                        {
                            W.Cast(getPrediction.CastPosition);
                        }
                        break;
                }
            }
            if (MenuSettings.Combo.useR.Enabled && R.IsReady())
            {
                if (!target.IsValidTarget(R.Range - 15))
                    return;
                if (Q.IsReady() && getDamage(target, true, false, false, false) > target.Health)
                    return;

                if (target.Health < getDamage(target, false, false, true, true))
                    R.CastOnUnit(target);
            }
            if (MenuSettings.Combo.useIgnite.Enabled && summonerIgnite.IsReady())
            {
                if (!target.IsValidTarget(600))
                    return;

                if (target.Health < getDamage(target, false, false, false, true))
                    objPlayer.Spellbook.CastSpell(summonerIgnite, target);
            }
        }
        private static void Harass()
        {
            if (!MenuSettings.Keys.harassToggle.Active)
                return;
            if (objPlayer.ManaPercent < MenuSettings.Harass.minMana.Value)
                return;

            var target = TargetSelector.GetTarget(Q.Range + 150,DamageType.Magical);

            if (target == null || target.IsDead)
                return;

            if (MenuSettings.Harass.useQ.Enabled && Q.IsReady())
            {
                if (!target.IsValidTarget(Q.Range - 50))
                    return;

                var getPrediction = Q.GetPrediction(target);

                if (getPrediction.CollisionObjects.Count() < 2 && getPrediction.Hitchance >= HitChance.High)
                {
                    Q.Cast(getPrediction.CastPosition);
                }
            }
            if (MenuSettings.Harass.useW.Enabled && W.IsReady())
            {
                if (!target.IsValidTarget(W.Range))
                    return;

                var getPrediction = W.GetPrediction(target);

                switch (MenuSettings.Harass.wMode.SelectedValue)
                {
                    case "Common Prediction":
                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            W.Cast(getPrediction.CastPosition);
                        }
                        break;
                    case "Only On Stunned Enemies":
                        if (getPrediction.Hitchance == HitChance.Immobile)
                        {
                            W.Cast(getPrediction.CastPosition);
                        }
                        break;
                }
            }
        }
        private static void LaneClear()
        {
            if (!MenuSettings.Keys.farmToggle.Active)
                return;
            var allMinions = GameObjects.EnemyMinions.Where(x => x.IsMinion() && !x.IsDead).OrderBy(x => x.Distance(objPlayer.Position));

            if (allMinions.Count() == 0)
                return;
            
            if (MenuSettings.LaneClear.useQ.Enabled && Q.IsReady())
            {
                if (objPlayer.ManaPercent < MenuSettings.LaneClear.minManaQ.Value)
                    return;

                if (MenuSettings.LaneClear.autostackClear.Enabled)
                {
                    foreach (var min in allMinions.Where(x => x.IsValidTarget(Q.Range - 15) && x.Health < Q.GetDamage(x)))
                    {
                        var getPrediction = Q.GetPrediction(min, true);
                        var getCollisions = getPrediction.CollisionObjects.ToList();

                        switch (MenuSettings.LaneClear.qStackMode.SelectedValue)
                        {
                            case "LastHit 1 Minion":
                                if (getCollisions.Any() && getCollisions.Count() <= 1)
                                {
                                    Q.Cast(getPrediction.CastPosition);
                                }
                                else
                                {
                                    Q.Cast(getPrediction.CastPosition);
                                }
                                break;
                            case "LastHit 2 Minions":
                                if (getCollisions.Any() && (getCollisions.Count() == 1 && getCollisions.FirstOrDefault().Health < Q.GetDamage(getCollisions.FirstOrDefault()) - 10))
                                {
                                    Q.Cast(getPrediction.CastPosition);
                                }
                                else if (getCollisions.Count() == 2 && getCollisions[0].Health < Q.GetDamage(getCollisions[0]) - 10 && getCollisions[1].Health < Q.GetDamage(getCollisions[1]) - 10)
                                {
                                    Q.Cast(getPrediction.CastPosition);
                                }
                                break;
                        }
                    }
                }
            }
            if (MenuSettings.LaneClear.useW.Enabled && W.IsReady())
            {
                if (objPlayer.ManaPercent < MenuSettings.LaneClear.minManaW.Value)
                    return;

                var minTarget = allMinions.Where(x => x.IsValidTarget(W.Range)).OrderBy(x => MinionTypes.Ranged).ToList();

                if (!minTarget.Any())
                    return;

                var circularFarmLocation = W.GetCircularFarmLocation(minTarget, W.Width);

                if (circularFarmLocation.MinionsHit >= MenuSettings.LaneClear.minHitsW.Value)
                {
                    W.Cast(circularFarmLocation.Position);
                }
            }
        }
        private static void JungleClear()
        {
            if (!MenuSettings.Keys.farmToggle.Active)
                return;

            var allMonsters = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range) && x.IsJungle()).ToList();

            if (allMonsters.Count() == 0)
                return;

            foreach (var mob in allMonsters)
            {
                int monsterJungleType = (int)mob.GetJungleType(); // 5 - large, 9 - legendary, 3 - small,

                if (MenuSettings.JungleClear.useQ.Enabled && Q.IsReady())
                {
                    if (objPlayer.ManaPercent < MenuSettings.JungleClear.minManaQ.Value)
                        return;

                    var getPrediction = Q.GetPrediction(mob);

                    switch (MenuSettings.JungleClear.qMode.SelectedValue)
                    {
                        case "All Monsters":
                            if (monsterJungleType == 5 || monsterJungleType == 9 || monsterJungleType == 3)
                            {
                                if (getPrediction.CollisionObjects.Count() < 2)
                                    Q.Cast(getPrediction.CastPosition);
                            }
                            break;
                        case "Big Monsters":
                            if (monsterJungleType == 5 || monsterJungleType == 9)
                            {
                                if (getPrediction.CollisionObjects.Count() < 2)
                                    Q.Cast(getPrediction.CastPosition);
                            }
                            break;
                    }
                }
                if (MenuSettings.JungleClear.useW.Enabled && W.IsReady())
                {
                    if (objPlayer.ManaPercent < MenuSettings.JungleClear.minManaW.Value)
                        return;
                    if (mob.Health < Q.GetDamage(mob))
                        return;

                    var getCircularFarmLocation = W.GetCircularFarmLocation(allMonsters, W.Width);

                    switch (MenuSettings.JungleClear.wMode.SelectedValue)
                    {
                        case "All Monsters":
                            if (monsterJungleType == 5 || monsterJungleType == 9 || monsterJungleType == 3)
                            {
                                if (getCircularFarmLocation.MinionsHit >= 1)
                                    W.Cast(getCircularFarmLocation.Position);
                            }
                            break;
                        case "Big Monsters":
                            if (monsterJungleType == 5 || monsterJungleType == 9)
                            {
                                if (getCircularFarmLocation.MinionsHit >= 1)
                                    W.Cast(getCircularFarmLocation.Position);
                            }
                            break;
                    }
                }
                if (MenuSettings.JungleClear.useE.Enabled && E.IsReady())
                {
                    if (objPlayer.ManaPercent < MenuSettings.JungleClear.minManaE.Value)
                        return;
                    if (mob.Health < Q.GetDamage(mob) + W.GetDamage(mob))
                        return;
                    if (mob.HasBuffOfType(BuffType.Stun))
                        return;

                    var getPrediction = E.GetPrediction(mob, false, E.Width);

                    switch (MenuSettings.JungleClear.eMode.SelectedValue)
                    {
                        case "All Monsters":
                            if (monsterJungleType == 5 || monsterJungleType == 3)
                            {
                                if (getPrediction.CastPosition.DistanceToPlayer() < E.Range - 5)
                                {
                                    E.Cast(getPrediction.CastPosition.Extend(objPlayer.Position, 300));
                                }
                            }
                            break;
                        case "Big Monsters":
                            if (monsterJungleType == 5)
                            {
                                if (getPrediction.CastPosition.Distance(objPlayer.Position) < E.Range - 5)
                                {
                                    switch (mob.IsMoving)
                                    {
                                        case true:
                                            E.Cast(getPrediction.CastPosition.Extend(objPlayer.Position, -30));
                                            break;
                                        case false:
                                            E.Cast(getPrediction.CastPosition.Extend(objPlayer.Position, 300));
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
        private static void LastHit()
        {
            var allMinions = GameObjects.EnemyMinions.Where(x => x.IsMinion() && !x.IsDead).OrderBy(x => x.Distance(objPlayer.Position));

            if (MenuSettings.LastHit.useQ.Enabled && Q.IsReady())
            {
                if (MenuSettings.LastHit.autostackClear.Enabled)
                {
                    foreach (var min in allMinions.Where(x => x.IsValidTarget(Q.Range - 15) && x.Health < Q.GetDamage(x)))
                    {
                        var getPrediction = Q.GetPrediction(min, true);
                        var getCollisions = getPrediction.CollisionObjects.ToList();

                        switch (MenuSettings.LastHit.qStackMode.SelectedValue)
                        {
                            case "LastHit 1 Minion":
                                if (getCollisions.Any() && getCollisions.Count() <= 1)
                                {
                                    Q.Cast(getPrediction.CastPosition);
                                }
                                else
                                {
                                    Q.Cast(getPrediction.CastPosition);
                                }
                                break;
                            case "LastHit 2 Minions":
                                if (getCollisions.Any() && (getCollisions.Count() == 1 && getCollisions.FirstOrDefault().Health < Q.GetDamage(getCollisions.FirstOrDefault()) - 10))
                                {
                                    Q.Cast(getPrediction.CastPosition);
                                }
                                else if (getCollisions.Count() == 2 && getCollisions[0].Health < Q.GetDamage(getCollisions[0]) - 10 && getCollisions[1].Health < Q.GetDamage(getCollisions[1]) - 10)
                                {
                                    Q.Cast(getPrediction.CastPosition);
                                }
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Events

        private static void OnAction(object sender, BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear || Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {

                switch (args.Target.Type)
                {
                    case GameObjectType.AIMinionClient:
                        if (args.Target != null)
                        {
                            var minTarget = (AIMinionClient)args.Target;

                            if (Q.Instance.CooldownExpires < 0.3f)
                            {
                                if (MenuSettings.LaneClear.autostackClear.Enabled && minTarget.Health > Q.GetDamage(minTarget))
                                    return;

                                args.Process = false;
                            }
                        }
                        break;
                    case GameObjectType.AITurretClient:
                        break;
                }
            }
        }
        private static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs arg)
        {
            if (!MenuSettings.Misc.interrupter.Enabled)
                return;
            if (!E.IsReady())
                return;

            if (sender.IsEnemy && sender.DistanceToPlayer() < 550)
            {
                var extendPosition = sender.Position.Extend(objPlayer.Position, 300);
                E.Cast(extendPosition);
            }
        }
        private static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!MenuSettings.Misc.gapcloser.Enabled)
                return;
            if (!E.IsReady())
                return;

            if (args != null && args.EndPosition.DistanceToPlayer() < 350)
                E.Cast(objPlayer.Position);
        }

        #endregion

        #region Drawings

        private static void OnDraw(EventArgs args)
        {
            if (MenuSettings.Drawing.disableDrawings.Enabled)
                return;

            if (MenuSettings.Drawing.drawQ.Enabled && Q.IsReady())
            {
                Render.Circle.DrawCircle(objPlayer.Position, Q.Range, System.Drawing.Color.AliceBlue);
            }
            if (MenuSettings.Drawing.drawW.Enabled && W.IsReady())
            {
                Render.Circle.DrawCircle(objPlayer.Position, W.Range, System.Drawing.Color.Beige);
            }
            if (MenuSettings.Drawing.drawE.Enabled && E.IsReady())
            {
                Render.Circle.DrawCircle(objPlayer.Position, W.Range, System.Drawing.Color.DodgerBlue);
            }
            if (MenuSettings.Drawing.drawR.Enabled && R.IsReady())
            {
                Render.Circle.DrawCircle(objPlayer.Position, R.Range, System.Drawing.Color.DarkBlue);
            }
        }
        private static void OnEndScene(EventArgs args)
        {
            if (MenuSettings.Drawing.disableDrawings.Enabled)
                return;
            if (!MenuSettings.Drawing.drawDmg.Enabled)
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
            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range)))
            {
                if (target == null || target.HasBuffOfType(BuffType.Invulnerability) || target.HasBuffOfType(BuffType.SpellImmunity))
                    return;

                if (MenuSettings.Misc.killstealR.Enabled && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (target.Health < Q.GetDamage(target) && Q.IsReady())
                        return;

                    if (target.Health + target.MagicalShield < R.GetDamage(target))
                        R.Cast(target);
                }
                if (MenuSettings.Misc.killstealQ.Enabled && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    var getPrediction = Q.GetPrediction(target);

                    if (target.Health + target.MagicalShield < Q.GetDamage(target) && getPrediction.CollisionObjects.Count() < 2)
                    {
                        Q.Cast(getPrediction.CastPosition);
                    }
                }
                if (MenuSettings.Misc.killstealW.Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    var getPrediction = W.GetPrediction(target);

                    if (target.Health + target.MagicalShield < W.GetDamage(target) && getPrediction.Hitchance >= HitChance.High)
                    {
                        W.Cast(getPrediction.CastPosition);
                    }
                }
                if (MenuSettings.Misc.killstealIgnite.Enabled && summonerIgnite.IsReady() && target.IsValidTarget(600))
                {
                    if (target.Health + target.MagicalShield < objPlayer.GetSummonerSpellDamage(target, SummonerSpell.Ignite))
                    {
                        objPlayer.Spellbook.CastSpell(summonerIgnite, target);
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

            if (q && Q.IsReady())
                damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.Q);
            if (w && W.IsReady())
                damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.W);
            if (r && R.IsReady())
                damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.R);

            if (ignite && summonerIgnite.IsReady())
                damage += (float)objPlayer.GetSummonerSpellDamage(target, SummonerSpell.Ignite);

            if (objPlayer.GetBuffCount("itemmagicshankcharge") == 100) // oktw sebby
                damage += (float)objPlayer.CalculateMagicDamage(target, 100 + 0.1 * objPlayer.TotalMagicalDamage);

            if (target.HasBuff("ManaBarrier") && target.HasBuff("BlitzcrankManaBarrierCO"))
                damage += target.Mana / 2f;
            if (target.HasBuff("GarenW"))
                damage = damage * 0.7f;

            return damage;
        }

        #endregion

        #endregion
    }
}