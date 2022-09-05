using System;
using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace OlympusAIO.Helpers
{
    static class MenuManager
    {
        public static Menu HumanizerMenu = new Menu("HumanizerMenu", "Humanizer");

        public static Menu ComboMenu = new Menu("ComboMenu", "Combo");

        public static Menu HarassMenu = new Menu("HarassMenu", "Harass");

        public static Menu LaneClearMenu = new Menu("LaneClearMenu", "Lane Clear");

        public static Menu JungleClearMenu = new Menu("JungleClearMenu", "Jungle Clear");

        public static Menu LastHitMenu = new Menu("LastHitMenu", "Last Hit");

        public static Menu MiscMenu = new Menu("MiscMenu", "Misc");

        public static Menu DrawingsMenu = new Menu("DrawingsMenu", "Drawings");

        public static Menu CreditsMenu = new Menu("CreditsMenu", "Credits");

        public static Menu ComboWhiteList = new Menu("WhiteList", "White List");

        public static Menu ComboRWhiteList = new Menu("RWhiteList", "R White List");

        public static Menu JungleWhiteList = new Menu("WhiteList", "White List");

        public static Menu MiscKillSteal = new Menu("KillStealMenu", "Kill Steal");

        public static Menu MiscGapcloserMenu = new Menu("GapcloserMenu", "Gapcloser");

        public static Menu MiscInterrupterMenu = new Menu("InterrupterMenu", "Interrupter");

        public static Menu DamageIndicatorMenu = new Menu("DamageIndicatorMenu", "Damage Indicator");

        public static Menu SpellRangesMenu = new Menu("SpellRangesMenu", "Spell Ranges");

        public static Menu DrawingsMiscMenu = new Menu("DrawingsMiscMenu", "Misc");

        public static MenuBool AddBool(Menu menu, string name, string displayName, bool defaultValue)
        {
            return menu.Add(new MenuBool(name, displayName, defaultValue));
        }
        public static MenuSliderButton AddSliderBool(Menu menu, string name, string displayName, int index, int min, int max, bool defaultValue)
        {
            return menu.Add(new MenuSliderButton(name, displayName, index, min, max, defaultValue));
        }
        public static MenuSlider AddSlider(Menu menu, string name, string displayName, int value, int min, int max)
        {
            return menu.Add(new MenuSlider(name, displayName, value, min, max));
        }
        public static MenuKeyBind AddKeyBind(Menu menu, string name, string displayName, Keys key, KeyBindType type)
        {
            return menu.Add(new MenuKeyBind(name, displayName, key, type));
        }
        public static MenuSeparator AddSeparator(Menu menu, string name, string displayName)
        {
            return menu.Add(new MenuSeparator(name, displayName));
        }
        public static MenuList AddList(Menu menu, string name, string displayName, string[] value, int index)
        {
            return menu.Add(new MenuList(name, displayName, value, index));
        }

        public static class Execute
        {
            public static void General()
            {
                AddSeparator(OlympusAIO.MainMenu, "GeneralSettings", "General Settings");
                AddBool(OlympusAIO.MainMenu, "DisableAntiDC", "Disable Anti-DC in Core", true);
                SpellManager.SpellList.ForEach(x => AddSlider(HumanizerMenu, x, x, UtilityManager.GetWaitTimeForHumanizerByChampion(x), 20, 250));
                AddSlider(HumanizerMenu, "IssueOrder", "Move", UtilityManager.GetMoveWaitTimeForHumanizer(), 20, 250);
                OlympusAIO.MainMenu.Add(HumanizerMenu);
                AddBool(OlympusAIO.MainMenu, "SupportMode", "Support Mode", false);
                AddSliderBool(OlympusAIO.MainMenu, "DisableAAInCombo", "Disable AA in Combo | If level >= ", 2, 2, 18, false);

                if (!OlympusAIO.SupportedChampions.All(x => !string.Equals(x, OlympusAIO.objPlayer.CharacterName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    AddBool(OlympusAIO.MainMenu, "DisableManaManagerIfBlueBuff", "Ignore Mana Manager If Blue Buff", true);
                    AddKeyBind(OlympusAIO.MainMenu, "SpellFarm", "Enabled Spell Farm", Keys.J, KeyBindType.Toggle).SetValue(true).Permashow(true, "Enabled Spell Farm [MouseWheel] &");
                    AddKeyBind(OlympusAIO.MainMenu, "SpellHarass", "Enabled Spell Harass", Keys.H, KeyBindType.Toggle).SetValue(true).Permashow();

                    AddSeparator(OlympusAIO.MainMenu, "ChampionSettings", "Champion Settings");
                    OlympusAIO.MainMenu.Add(ComboMenu);
                    OlympusAIO.MainMenu.Add(HarassMenu);
                    OlympusAIO.MainMenu.Add(LaneClearMenu);
                    OlympusAIO.MainMenu.Add(JungleClearMenu);
                    OlympusAIO.MainMenu.Add(LastHitMenu);
                    OlympusAIO.MainMenu.Add(MiscMenu);
                    OlympusAIO.MainMenu.Add(DrawingsMenu);
                    OlympusAIO.MainMenu.Add(CreditsMenu);
                }
            }
            public static void AurelionSol()
            {
                AddBool(ComboMenu, "Q", "Use Q", true);
                AddBool(ComboMenu, "W", "Use W", true);
                AddBool(ComboMenu, "R", "Use R", true);

                AddSeparator(ComboMenu, "QSettings", "Q Settings");
                AddSlider(ComboMenu, "QHits", "Min Hits", 1, 1, 3);
                AddList(ComboMenu, "QHitChance", "HitChance:", UtilityManager.HitchanceNameArray, 2);

                AddSeparator(ComboMenu, "WSettings", "W Settings");
                AddSlider(ComboMenu, "WHits", "Min Hits", 1, 1, 3);

                AddSeparator(ComboMenu, "RSettings", "R Settings");
                AddKeyBind(ComboMenu, "SemiRKey", "Semi-Manual R Key:", Keys.T, KeyBindType.Press).Permashow();
                AddSlider(ComboMenu, "RHits", "R Hits", 1, 1, 3);

                AddBool(HarassMenu, "Q", "Use Q", true);
                AddBool(HarassMenu, "W", "Use W", true);

                AddSeparator(HarassMenu, "QSettings", "Q Settings");
                AddList(HarassMenu, "QHitChance", "HitChance:", UtilityManager.HitchanceNameArray, 2);

                AddSeparator(HarassMenu, "ManaManager", "Mana Manager");
                AddSlider(HarassMenu, "MinMana", "If Mana >= x", 60, 0, 100);

                AddBool(LaneClearMenu, "Q", "Use Q", true);
                AddBool(LaneClearMenu, "W", "Use W", true);

                AddSeparator(LaneClearMenu, "QSettings", "Q Settings");
                AddSlider(LaneClearMenu, "QHits", "Min Hits", 3, 1, 6);

                AddSeparator(LaneClearMenu, "WSettings", "W Settings");
                AddSlider(LaneClearMenu, "WHits", "Min Hits", 3, 1, 6);

                AddSeparator(LaneClearMenu, "ManaManager", "Mana Manager");
                AddSlider(LaneClearMenu, "MinMana", "If Mana >= x", 50, 0, 100);

                AddBool(JungleClearMenu, "Q", "Use Q", true);
                AddBool(JungleClearMenu, "W", "Use W", true);

                AddSeparator(JungleClearMenu, "ManaManager", "Mana Manager");
                AddSlider(JungleClearMenu, "MinMana", "If Mana >= x", 15, 0, 100);

                AddBool(MiscKillSteal, "Disable", "Disable", false);

                AddSeparator(MiscKillSteal, "Settings", "Settings");
                AddBool(MiscKillSteal, "Q", "Use Q", true);
                AddBool(MiscKillSteal, "R", "Use R", true);
                MiscMenu.Add(MiscKillSteal);

                AddBool(MiscGapcloserMenu, "Q", "Use Q", true);
                MiscMenu.Add(MiscGapcloserMenu);

                AddBool(MiscInterrupterMenu, "Q", "Use Q", true);
                MiscMenu.Add(MiscInterrupterMenu);

                AddBool(DrawingsMenu, "Disable", "Disable", false);
                AddBool(DrawingsMenu, "SpellFarm", "Spell Farm", true);
                AddBool(DamageIndicatorMenu, "Enable", "Enable", true);
                DrawingsMenu.Add(DamageIndicatorMenu);
                AddBool(SpellRangesMenu, "QRange", "Q Range", true);
                AddBool(SpellRangesMenu, "WRange", "W Range", true);
                AddBool(SpellRangesMenu, "ERange", "E Range", false);
                AddBool(SpellRangesMenu, "RRange", "R Range", true);
                DrawingsMenu.Add(SpellRangesMenu);

                AddSeparator(CreditsMenu, "Exory", "Exory");
                AddSeparator(CreditsMenu, "Sayuto", "Sayuto");
                AddSeparator(CreditsMenu, "N1ghtMoon", "N1ghtMoon");
                AddSeparator(CreditsMenu, "Frondal", "Frondal");
                AddSeparator(CreditsMenu, "Zypppy", "Zypppy");
            }
            public static void Evelynn()
            {
                AddBool(ComboMenu, "Q", "Use Q", true);
                AddBool(ComboMenu, "W", "Use W", true);
                AddBool(ComboMenu, "E", "Use E", true);
                AddBool(ComboMenu, "R", "Use R", true);

                AddSeparator(ComboMenu, "QSettings", "Q Settings");
                AddBool(ComboMenu, "QOnlyIfFullyAllured", "Only If Enemy Is Fully Allured", true);

                AddSeparator(ComboMenu, "WSettings", "W Settings");
                AddList(ComboMenu, "WSelectBy", "Select By:", new[] { "Most AD", "Most AP", "Lowest Health", "Most Priority" }, 2);
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        AddBool(ComboWhiteList, target.CharacterName.ToLower(), target.CharacterName, true);
                    }
                    ComboMenu.Add(ComboWhiteList);
                }

                AddSeparator(ComboMenu, "ESettings", "E Settings");
                AddBool(ComboMenu, "EOnlyIfFullyAllured", "Only If Enemy Is Fully Allured", true);

                AddSeparator(ComboMenu, "RSettings", "R Settings");
                AddKeyBind(ComboMenu, "SemiRKey", "Semi-Manual R Key:", Keys.T, KeyBindType.Press).Permashow();
                AddSliderBool(ComboMenu, "RAoE", "If can hit >= x", 2, 1, 5, true);
                AddSlider(ComboMenu, "RSafetyRange", "Safety Range", 450, 200, 800);
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        AddBool(ComboRWhiteList, target.CharacterName.ToLower(), target.CharacterName, true);
                    }
                    ComboMenu.Add(ComboRWhiteList);
                }

                AddBool(HarassMenu, "Q", "Use Q", true);
                AddBool(HarassMenu, "E", "Use E", true);

                AddSeparator(HarassMenu, "ManaManager", "Mana Manager");
                AddSlider(HarassMenu, "MinMana", "If Mana >= x", 60, 0, 100);

                AddSeparator(HarassMenu, "WhiteList", "White List");
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        AddBool(HarassMenu, target.CharacterName.ToLower(), target.CharacterName, true);
                    }
                }

                AddBool(LaneClearMenu, "Q", "Use Q", true);
                AddBool(LaneClearMenu, "E", "Use E", true);

                AddSeparator(LaneClearMenu, "QSettings", "Q Settings");
                AddSlider(LaneClearMenu, "QHits", "If Hitable minions >= x", 3, 1, 5);

                AddSeparator(LaneClearMenu, "ManaManager", "Mana Manager");
                AddSlider(LaneClearMenu, "MinMana", "If Mana >= x", 50, 0, 100);

                JungleWhiteList = new Menu("JungleWhiteList", "Allure: WhiteList");
                {
                    foreach (var target in UtilityManager.JungleList)
                    {
                        AddBool(JungleWhiteList, target, target, true);
                    }
                    JungleClearMenu.Add(JungleWhiteList);
                }

                AddBool(JungleClearMenu, "Q", "Use Q", true);
                AddBool(JungleClearMenu, "W", "Use W", true);
                AddBool(JungleClearMenu, "E", "Use E", true);

                AddSeparator(JungleClearMenu, "ManaManager", "Mana Manager");
                AddSlider(JungleClearMenu, "MinMana", "If Mana >= x", 10, 0, 100);

                AddBool(MiscMenu, "AASemiAllured", "Don't AA Semi-Allured Targets", true);

                AddBool(MiscKillSteal, "Disable", "Disable", false);

                AddSeparator(MiscKillSteal, "Settings", "Settings");
                AddBool(MiscKillSteal, "E", "Use E", true);
                AddBool(MiscKillSteal, "R", "Use R", true);
                MiscMenu.Add(MiscKillSteal);

                AddBool(MiscGapcloserMenu, "R", "Use R", true);
                MiscMenu.Add(MiscGapcloserMenu);

                MiscMenu.Add(MiscInterrupterMenu);

                AddBool(DrawingsMenu, "Disable", "Disable", false);
                AddBool(DrawingsMenu, "SpellFarm", "Spell Farm", true);
                AddBool(DamageIndicatorMenu, "Enable", "Enable", true);
                DrawingsMenu.Add(DamageIndicatorMenu);
                AddBool(SpellRangesMenu, "QRange", "Q Range", true);
                AddBool(SpellRangesMenu, "WRange", "W Range", true);
                AddBool(SpellRangesMenu, "ERange", "E Range", false);
                AddBool(SpellRangesMenu, "RRange", "R Range", false);
                DrawingsMenu.Add(SpellRangesMenu);
                AddBool(DrawingsMiscMenu, "RPos", "R Position After Cast", true);
                DrawingsMenu.Add(DrawingsMiscMenu);

                AddSeparator(CreditsMenu, "Exory", "Exory");
                AddSeparator(CreditsMenu, "Sayuto", "Sayuto");
                AddSeparator(CreditsMenu, "N1ghtMoon", "N1ghtMoon");
            }
            public static void Heimerdinger()
            {
                AddBool(ComboMenu, "Q", "Use Q", true);
                AddBool(ComboMenu, "W", "Use W", true);
                AddBool(ComboMenu, "E", "Use E", true);
                AddBool(ComboMenu, "R", "Use R", true);

                AddSeparator(ComboMenu, "QSettings", "Q Settings");
                AddSliderBool(ComboMenu, "QUpgradeHits", "Upgrade Q If Enemies >=", 3, 2, 5, true);

                AddSeparator(ComboMenu, "RSettings", "R Settings");
                AddList(ComboMenu, "RMode", "Mode:", new[] { "E-R-W", "W-R-E" }, 0);

                AddBool(HarassMenu, "Q", "Use Q", true);
                AddBool(HarassMenu, "W", "Use W", true);
                AddBool(HarassMenu, "E", "Use E", true);

                AddSeparator(HarassMenu, "ManaManager", "Mana Manager");
                AddSlider(HarassMenu, "MinMana", "If Mana >= x", 60, 0, 100);

                AddSeparator(HarassMenu, "WhiteList", "White List");
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        AddBool(HarassMenu, target.CharacterName.ToLower(), target.CharacterName, true);
                    }
                }

                AddBool(LaneClearMenu, "Q", "Use Q", true);
                AddBool(LaneClearMenu, "W", "Use W", true);
                AddBool(LaneClearMenu, "E", "Use E", true);

                AddSeparator(LaneClearMenu, "ESettings", "E Settings");
                AddSlider(LaneClearMenu, "EMinHits", "Min Hits >=", 3, 1, 5);

                AddSeparator(LaneClearMenu, "ManaManager", "Mana Manager");
                AddSlider(LaneClearMenu, "MinMana", "If Mana >= x", 50, 0, 100);

                AddBool(JungleClearMenu, "Q", "Use Q", true);
                AddBool(JungleClearMenu, "W", "Use W", true);
                AddBool(JungleClearMenu, "E", "Use E", true);

                AddSeparator(JungleClearMenu, "ManaManager", "Mana Manager");
                AddSlider(JungleClearMenu, "MinMana", "If Mana >= x", 10, 0, 100);

                AddBool(MiscKillSteal, "Disable", "Disable", false);

                AddSeparator(MiscKillSteal, "Settings", "Settings");
                AddBool(MiscKillSteal, "W", "Use W", true);
                AddBool(MiscKillSteal, "E", "Use E", true);
                MiscMenu.Add(MiscKillSteal);

                AddBool(MiscGapcloserMenu, "E", "Use E", true);
                MiscMenu.Add(MiscGapcloserMenu);

                AddBool(MiscInterrupterMenu, "E", "Use E", true);
                MiscMenu.Add(MiscInterrupterMenu);

                AddBool(DrawingsMenu, "Disable", "Disable", false);
                AddBool(DrawingsMenu, "SpellFarm", "Spell Farm", true);
                AddBool(DamageIndicatorMenu, "Enable", "Enable", true);
                DrawingsMenu.Add(DamageIndicatorMenu);
                AddBool(SpellRangesMenu, "QRange", "Q Range", false);
                AddBool(SpellRangesMenu, "WRange", "W Range", true);
                AddBool(SpellRangesMenu, "ERange", "E Range", true);
                AddBool(SpellRangesMenu, "RRange", "R Range", false);
                DrawingsMenu.Add(SpellRangesMenu);

                DrawingsMenu.Add(DrawingsMiscMenu);

                AddSeparator(CreditsMenu, "Exory", "Exory");
                AddSeparator(CreditsMenu, "Sayuto", "Sayuto");
                AddSeparator(CreditsMenu, "N1ghtMoon", "N1ghtMoon");
                AddSeparator(CreditsMenu, "berbb", "berbb");
                AddSeparator(CreditsMenu, "Zypppy", "Zypppy");
                AddSeparator(CreditsMenu, "Seph", "Seph");
            }
            public static void Lissandra()
            {
                AddBool(ComboMenu, "Q", "Use Q", true);
                AddBool(ComboMenu, "W", "Use W", true);
                AddBool(ComboMenu, "E", "Use E", true);
                AddBool(ComboMenu, "R", "Use R", true);

                AddSeparator(ComboMenu, "QSettings", "Q Settings");
                AddList(ComboMenu, "QHitchance", "Hitchance >=", UtilityManager.HitchanceNameArray, 2);

                AddSeparator(ComboMenu, "ESettings", "E Settings");
                AddSlider(ComboMenu, "EAoE", "AoE >=", 2, 1, 5);

                AddSeparator(ComboMenu, "RSettings", "R Settings");
                AddSlider(ComboMenu, "RHP", "Use R if HP is under %", 15, 0, 100);
                AddSlider(ComboMenu, "RDefense", "Self cast R If Enemy >=", 3, 1, 5);
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        AddBool(ComboRWhiteList, target.CharacterName.ToLower(), target.CharacterName, true);
                    }
                    ComboMenu.Add(ComboRWhiteList);
                }

                AddBool(HarassMenu, "Q", "Use Q", true);

                AddSeparator(HarassMenu, "ManaManager", "Mana Manager");
                AddSlider(HarassMenu, "MinMana", "If Mana >= x", 60, 0, 100);

                AddSeparator(HarassMenu, "WhiteList", "White List");
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        AddBool(HarassMenu, target.CharacterName.ToLower(), target.CharacterName, true);
                    }
                }

                AddBool(LaneClearMenu, "Q", "Use Q", true);
                AddBool(LaneClearMenu, "W", "Use W", true);

                AddSeparator(LaneClearMenu, "WSettings", "W Settings");
                AddSlider(LaneClearMenu, "WMinHits", "Min Hits", 3, 1, 6);

                AddSeparator(LaneClearMenu, "ManaManager", "Mana Manager");
                AddSlider(LaneClearMenu, "MinMana", "Min. Mana", 50, 0, 100);

                AddBool(JungleClearMenu, "Q", "Use Q", true);
                AddBool(JungleClearMenu, "W", "Use W", true);
                AddBool(JungleClearMenu, "E", "Use E", true);
                AddBool(JungleClearMenu, "E2", "Use E2", false);

                AddSeparator(JungleClearMenu, "ManaManager", "Mana Manager");
                AddSlider(JungleClearMenu, "MinMana", "Min. Mana", 10, 0, 100);

                AddBool(LastHitMenu, "Q", "Use Q", true);

                AddSeparator(LastHitMenu, "ManaManager", "Mana Manager");
                AddSlider(LastHitMenu, "MinMana", "Min. Mana", 50, 0, 100);

                AddKeyBind(MiscMenu, "Flee", "Flee Key:", Keys.Z, KeyBindType.Press).Permashow();

                AddBool(MiscKillSteal, "Disable", "Disable", false);

                AddSeparator(MiscKillSteal, "Settings", "Settings");
                AddBool(MiscKillSteal, "Q", "Use Q", true);
                AddBool(MiscKillSteal, "W", "Use W", true);
                MiscMenu.Add(MiscKillSteal);

                AddBool(MiscGapcloserMenu, "W", "Use W", true);
                AddBool(MiscGapcloserMenu, "E", "Use E", true);
                AddBool(MiscGapcloserMenu, "R", "Use R", true);
                MiscMenu.Add(MiscGapcloserMenu);

                AddBool(MiscInterrupterMenu, "W", "Use W", true);
                AddBool(MiscInterrupterMenu, "R", "Use R", true);
                MiscMenu.Add(MiscInterrupterMenu);

                AddBool(DrawingsMenu, "Disable", "Disable", false);
                AddBool(DrawingsMenu, "SpellFarm", "Spell Farm", true);
                AddBool(DamageIndicatorMenu, "Enable", "Enable", true);
                DrawingsMenu.Add(DamageIndicatorMenu);
                AddBool(SpellRangesMenu, "QRange", "Q Range", true);
                AddBool(SpellRangesMenu, "WRange", "W Range", true);
                AddBool(SpellRangesMenu, "ERange", "E Range", true);
                AddBool(SpellRangesMenu, "RRange", "R Range", false);
                DrawingsMenu.Add(SpellRangesMenu);

                DrawingsMenu.Add(DrawingsMiscMenu);

                AddSeparator(CreditsMenu, "Exory", "Exory");
                AddSeparator(CreditsMenu, "Sayuto", "Sayuto");
                AddSeparator(CreditsMenu, "N1ghtMoon", "N1ghtMoon");
                AddSeparator(CreditsMenu, "berbb", "berbb");
                AddSeparator(CreditsMenu, "Zypppy", "Zypppy");
                AddSeparator(CreditsMenu, "Seph", "Seph");
            }
            public static void Poppy()
            {
                AddBool(ComboMenu, "Q", "Use Q", true);
                AddBool(ComboMenu, "W", "Use W", true);
                AddBool(ComboMenu, "E", "Use E", true);
                AddBool(ComboMenu, "R", "Use R", true);

                AddSeparator(ComboMenu, "WSettings", "W Settings");
                AddSlider(ComboMenu, "WHealth", "If have lower than x% HP", 40, 10, 100);

                AddSeparator(ComboMenu, "ESettings", "E Settings");
                AddBool(ComboMenu, "EWall", "Only To near walls", true);
                AddBool(ComboMenu, "ETower", "Block E when enemy is under turret", true);
                AddBool(ComboMenu, "EFlash", "Use Flash + E to wall", true);
                AddKeyBind(ComboMenu, "EFlashForced", "Force Flash+E Key", Keys.T, KeyBindType.Press).Permashow();

                AddSeparator(ComboMenu, "RSettings", "R Settings");
                AddBool(ComboMenu, "RDefense", "Use R when Player is in danger", true);

                AddBool(HarassMenu, "Q", "Use Q", true);

                AddSeparator(HarassMenu, "ManaManager", "Mana Manager");
                AddSlider(HarassMenu, "MinMana", "If Mana >= x", 60, 0, 100);

                AddSeparator(HarassMenu, "WhiteList", "White List");
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        AddBool(HarassMenu, target.CharacterName.ToLower(), target.CharacterName, true);
                    }
                }

                AddBool(LaneClearMenu, "Q", "Use Q", true);
                AddBool(LaneClearMenu, "E", "Use E", true);

                AddSeparator(LaneClearMenu, "QSettings", "Q Settings");
                AddSlider(LaneClearMenu, "QMinHits", "Min Hits", 3, 1, 6);

                AddSeparator(LaneClearMenu, "ManaManager", "Mana Manager");
                AddSlider(LaneClearMenu, "MinMana", "Min. Mana", 50, 0, 100);

                AddBool(JungleClearMenu, "Q", "Use Q", true);
                AddBool(JungleClearMenu, "E", "Use E", true);

                AddSeparator(JungleClearMenu, "ManaManager", "Mana Manager");
                AddSlider(JungleClearMenu, "MinMana", "Min. Mana", 10, 0, 100);

                AddBool(MiscKillSteal, "Disable", "Disable", false);

                AddSeparator(MiscKillSteal, "Settings", "Settings");
                AddBool(MiscKillSteal, "Q", "Use Q", true);
                AddBool(MiscKillSteal, "E", "Use E", true);
                MiscMenu.Add(MiscKillSteal);

                AddBool(MiscGapcloserMenu, "W", "Use W", true);
                MiscMenu.Add(MiscGapcloserMenu);

                AddBool(MiscInterrupterMenu, "R", "Use R", true);
                MiscMenu.Add(MiscInterrupterMenu);

                AddBool(DrawingsMenu, "Disable", "Disable", false);
                AddBool(DrawingsMenu, "SpellFarm", "Spell Farm", true);
                AddBool(DamageIndicatorMenu, "Enable", "Enable", true);
                DrawingsMenu.Add(DamageIndicatorMenu);
                AddBool(SpellRangesMenu, "QRange", "Q Range", true);
                AddBool(SpellRangesMenu, "WRange", "W Range", true);
                AddBool(SpellRangesMenu, "ERange", "E Range", true);
                AddBool(SpellRangesMenu, "RRange", "R Range", false);
                DrawingsMenu.Add(SpellRangesMenu);

                DrawingsMenu.Add(DrawingsMiscMenu);

                AddSeparator(CreditsMenu, "Exory", "Exory");
                AddSeparator(CreditsMenu, "Sayuto", "Sayuto");
                AddSeparator(CreditsMenu, "N1ghtMoon", "N1ghtMoon");
                AddSeparator(CreditsMenu, "berbb", "berbb");
                AddSeparator(CreditsMenu, "Soresu", "Soresu");
            }
            public static void Teemo()
            {
                AddBool(ComboMenu, "Q", "Use Q", true);
                AddBool(ComboMenu, "W", "Use W", true);
                AddBool(ComboMenu, "R", "Use R", true);

                AddSeparator(ComboMenu, "QSettings", "Q Settings");
                AddBool(ComboMenu, "QPrioritizeADC", "Prioritize ADC", false);
                AddList(ComboMenu, "QMode", "Mode", new[] { "Normal", "Only AA Range" }, 1);

                AddSeparator(ComboMenu, "RSettings", "R Settings");
                AddSlider(ComboMenu, "RSave", "Save X Shrooms", 2, 1, 3);
                AddBool(ComboMenu, "Stealth", "Stop combo while stealth in brush", false);

                AddBool(HarassMenu, "Q", "Use Q", true);

                AddSeparator(HarassMenu, "ManaManager", "Mana Manager");
                AddSlider(HarassMenu, "MinMana", "If Mana >= x", 60, 0, 100);

                AddSeparator(HarassMenu, "WhiteList", "White List");
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        AddBool(HarassMenu, target.CharacterName.ToLower(), target.CharacterName, true);
                    }
                }

                AddBool(LaneClearMenu, "Q", "Use Q", false);
                AddBool(LaneClearMenu, "R", "Use R", true);

                AddSeparator(LaneClearMenu, "RSettings", "R Settings");
                AddSlider(LaneClearMenu, "RMinHits", "If minions >=", 3, 1, 6);
                AddSlider(LaneClearMenu, "RSave", "Save X Shrooms", 1, 0, 5);

                AddSeparator(LaneClearMenu, "ManaManager", "Mana Manager");
                AddSlider(LaneClearMenu, "MinMana", "Min. Mana", 50, 0, 100);

                AddBool(JungleClearMenu, "Q", "Use Q", true);
                AddBool(JungleClearMenu, "W", "Use W", true);
                AddBool(JungleClearMenu, "R", "Use R", true);

                AddSeparator(JungleClearMenu, "WSettings", "W Settings");
                AddSlider(JungleClearMenu, "WMana", "If ManaPercent >=", 60, 0, 100);

                AddSeparator(JungleClearMenu, "ManaManager", "Mana Manager");
                AddSlider(JungleClearMenu, "MinMana", "Min. Mana", 10, 0, 100);

                AddBool(LastHitMenu, "Q", "Use Q", true);

                AddSeparator(LastHitMenu, "ManaManager", "Mana Manager");
                AddSlider(LastHitMenu, "MinMana", "If Mana >= x", 40, 0, 100);

                AddKeyBind(MiscMenu, "Flee", "Flee Key:", Keys.Z, KeyBindType.Press).Permashow();

                AddBool(MiscMenu, "AutoW", "Auto W", false);
                AddBool(MiscMenu, "AutoR", "Auto Place Shrooms", true);

                AddSeparator(MiscMenu, "AutoRSettings", "Auto R Settings");
                AddBool(MiscMenu, "AutoRZhonya", "Use on Teleport, Zhonya etc.", true);
                AddSlider(MiscMenu, "AutoRSave", "Save X Shrooms", 1, 0, 5);
                AddSlider(MiscMenu, "AutoRWait", "Wait X ms after cast", 5000, 1000, 8000);

                AddBool(MiscKillSteal, "Disable", "Disable", false);

                AddSeparator(MiscKillSteal, "Settings", "Settings");
                AddBool(MiscKillSteal, "Q", "Use Q", true);
                MiscMenu.Add(MiscKillSteal);

                AddBool(MiscGapcloserMenu, "Q", "Use Q", true);
                AddBool(MiscGapcloserMenu, "W", "Use W", true);
                AddBool(MiscGapcloserMenu, "R", "Use R", true);
                MiscMenu.Add(MiscGapcloserMenu);

                AddBool(MiscInterrupterMenu, "Q", "Use Q", true);
                MiscMenu.Add(MiscInterrupterMenu);

                AddBool(DrawingsMenu, "Disable", "Disable", false);
                AddBool(DrawingsMenu, "SpellFarm", "Spell Farm", true);
                AddBool(DamageIndicatorMenu, "Enable", "Enable", true);
                DrawingsMenu.Add(DamageIndicatorMenu);
                AddBool(SpellRangesMenu, "QRange", "Q Range", true);
                AddBool(SpellRangesMenu, "WRange", "W Range", false);
                AddBool(SpellRangesMenu, "ERange", "E Range", false);
                AddBool(SpellRangesMenu, "RRange", "R Range", false);
                DrawingsMenu.Add(SpellRangesMenu);
                AddBool(DrawingsMiscMenu, "ShroomPos", "Shrooms Circles", true);
                DrawingsMenu.Add(DrawingsMiscMenu);

                AddSeparator(CreditsMenu, "Exory", "Exory");
                AddSeparator(CreditsMenu, "Sayuto", "Sayuto");
                AddSeparator(CreditsMenu, "N1ghtMoon", "N1ghtMoon");
                AddSeparator(CreditsMenu, "berbb", "berbb");
                AddSeparator(CreditsMenu, "Soresu", "Soresu");
            }
        }
    }
}
