#region

using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#endregion

namespace Evade
{
    internal static class Config
    {
        public const bool PrintSpellData = false;
        public const bool Debug = false;
        public const bool TestOnAllies = true;
        public const int SkillShotsExtraRadius = 13;
        public const int SkillShotsExtraRange = 20;
        public const int GridSize = 10;
        public const int ExtraEvadeDistance = 30;
        public const int PathFindingDistance = 60;
        public const int PathFindingDistance2 = 35;
        public const int DiagonalEvadePointsCount = 7;
        public const int DiagonalEvadePointsStep = 20;
        public const int CrossingTimeOffset = 250;
        public const int EvadingFirstTimeOffset = 250;
        public const int EvadingSecondTimeOffset = 80;
        public const int EvadingRouteChangeTimeOffset = 250;
        public const int EvadePointChangeInterval = 300;
        public static int LastEvadePointChangeT = 0;

        public static Menu Menu, evadeSpells, drawings,skillShots,misc;

        public static void CreateMenu()
        {
            Menu = new Menu("Evade", "Evade", true);

            //Create the evade spells submenus.
            evadeSpells = new Menu("evadeSpells","Evade spells");
            foreach (var spell in EvadeSpellDatabase.Spells)
            {
                var subMenu = new Menu(spell.Name, spell.Name);
                subMenu.Add(
                    new MenuSlider("DangerLevel" + spell.Name, "Danger level",spell.DangerLevel, 1, 5));

                if (spell.IsTargetted && spell.ValidTargets.Contains(SpellValidTargets.AllyWards))
                {
                    subMenu.Add(new MenuBool("WardJump" + spell.Name, "WardJump").SetValue(true));
                }

                subMenu.Add(new MenuBool("Enabled" + spell.Name, "Enabled").SetValue(true));

                evadeSpells.Add(subMenu);
            }
            Menu.Add(evadeSpells);

            //Create the skillshots submenus.
            skillShots = new Menu("Skillshots", "Skillshots");

            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {
                if (hero.Team != ObjectManager.Player.Team || Config.TestOnAllies)
                {
                    foreach (var spell in SpellDatabase.Spells)
                    {
                        if (String.Equals(spell.ChampionName, hero.CharacterName, StringComparison.InvariantCultureIgnoreCase) ||
                            spell.ChampionName == "AllChampions")
                        {
                            var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                            subMenu.Add(
                                new MenuSlider("DangerLevel" + spell.MenuItemName, "Danger level",spell.DangerValue, 1, 5));

                            subMenu.Add(
                                new MenuBool("IsDangerous" + spell.MenuItemName, "Is Dangerous").SetValue(
                                    spell.IsDangerous));

                            subMenu.Add(new MenuBool("Draw" + spell.MenuItemName, "Draw").SetValue(true));
                            subMenu.Add(new MenuBool("Enabled" + spell.MenuItemName, "Enabled").SetValue(!spell.DisabledByDefault));

                            skillShots.Add(subMenu);
                        }
                    }
                    //if (hero.ChampionName == "Viego" && !Config.TestOnAllies)
                    //{
                    //    foreach (var hero2 in ObjectManager.Get<Obj_AI_Hero>())
                    //    {
                    //        if (hero2.Team != hero.Team)
                    //        {
                    //            foreach (var spell in SpellDatabase.Spells)
                    //            {
                    //                if (spell.Slot != SpellSlot.R)
                    //                {
                    //                    var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                    //                    subMenu.AddItem(
                    //                        new MenuItem("DangerLevel" + spell.MenuItemName, "Danger level").SetValue(
                    //                            new Slider(spell.DangerValue, 5, 1)));

                    //                    subMenu.AddItem(
                    //                        new MenuItem("IsDangerous" + spell.MenuItemName, "Is Dangerous").SetValue(
                    //                            spell.IsDangerous));

                    //                    subMenu.AddItem(new MenuItem("Draw" + spell.MenuItemName, "Draw").SetValue(true));
                    //                    subMenu.AddItem(new MenuItem("Enabled" + spell.MenuItemName, "Enabled").SetValue(!spell.DisabledByDefault));

                    //                    skillShots.AddSubMenu(subMenu);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }

            Menu.Add(skillShots);

            var shielding = new Menu("Shielding","Ally shielding");

            foreach (var ally in ObjectManager.Get<AIHeroClient>())
            {
                if (ally.IsAlly && !ally.IsMe)
                {
                    shielding.Add(
                        new MenuBool("shield" + ally.CharacterName, "Shield " + ally.CharacterName).SetValue(true));
                }
            }
            Menu.Add(shielding);

            var spellBlocker = new Menu("SpellBlocker","Spell Blocker");
            spellBlocker.Add(new MenuBool("spellBlockerQ", "Q").SetValue(SpellBlocker.ShouldBlock(SpellSlot.Q)));
            spellBlocker.Add(new MenuBool("spellBlockerW", "W").SetValue(SpellBlocker.ShouldBlock(SpellSlot.W)));
            spellBlocker.Add(new MenuBool("spellBlockerE", "E").SetValue(SpellBlocker.ShouldBlock(SpellSlot.E)));
            spellBlocker.Add(new MenuBool("spellBlockerR", "R").SetValue(SpellBlocker.ShouldBlock(SpellSlot.R)));
            Menu.Add(spellBlocker);

            var collision = new Menu("Collision", "Collision");
            collision.Add(new MenuBool("MinionCollision", "Minion collision").SetValue(false));
            collision.Add(new MenuBool("HeroCollision", "Hero collision").SetValue(false));
            collision.Add(new MenuBool("YasuoCollision", "Yasuo wall collision").SetValue(true));
            collision.Add(new MenuBool("EnableCollision", "Enabled").SetValue(false));
            //TODO add mode.
            Menu.Add(collision);

            drawings = new Menu("Drawings", "Drawings");
            drawings.Add(new MenuColor("EnabledColor", "Enabled spell color",Color.White.ToSharpDxColor()));
            drawings.Add(new MenuColor("DisabledColor", "Disabled spell color",Color.Red.ToSharpDxColor()));
            drawings.Add(new MenuColor("MissileColor", "Missile color",Color.LimeGreen.ToSharpDxColor()));
            drawings.Add(new MenuSlider("Border", "Border Width",2, 1, 5));

            drawings.Add(new MenuBool("EnableDrawings", "Enabled").SetValue(true));
            drawings.Add(new MenuBool("DrawWarningMsg", "Draw Right Click warning msg").SetValue(true));
            Menu.Add(drawings);

            misc = new Menu("Misc", "Misc");
            misc.Add(new MenuList("BlockSpells", "Block spells while evading",new []{"No", "Only dangerous", "Always"}, 1));
            misc.Add(new MenuSlider("AllowAaLevel", "Allow auto-attacks danger level",4, 1, 5));
            misc.Add(new MenuBool("DisableFow", "Disable fog of war dodging").SetValue(false));
            misc.Add(new MenuBool("ShowEvadeStatus", "Show Evade Status").SetValue(false));
            if (ObjectManager.Player.CharacterName == "Olaf")
            {
                misc.Add(
                    new MenuBool("DisableEvadeForOlafR", "Automatic disable Evade when Olaf's ulti is active!")
                        .SetValue(true));
            }

            Menu.Add(misc);

            Menu.Add(
                new MenuKeyBind("Enabled", "Enabled",Keys.K, KeyBindType.Toggle, true)).AddPermashow( "Evade");

            Menu.Add(
                new MenuKeyBind("OnlyDangerous", "Dodge only dangerous",Keys.Space, KeyBindType.Press)).AddPermashow();

            Menu.Attach();
        }
    }
}