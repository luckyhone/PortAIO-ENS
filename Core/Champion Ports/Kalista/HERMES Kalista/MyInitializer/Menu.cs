using System;
using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HERMES_Kalista.MyUtils;

namespace HERMES_Kalista.MyInitializer
{
    public static partial class HERMESLoader
    {
        public static void LoadMenu()
        {
            ConstructMenu();
            //InitOrbwalker();
            FinishMenuInit();
        }

        public static void ConstructMenu()
        {
            try
            {
                Program.MainMenu = new Menu("pradamenu", "HERMES Kalista", true);
                Program.ComboMenu = new Menu("combomenu", "General Settings");
                Program.LaneClearMenu = new Menu("laneclearmenu", "Laneclear Settings");
                Program.EscapeMenu = new Menu("escapemenu", "Escape Settings");

                Program.DrawingsMenu = new Menu("drawingsmenu", "Drawing Settings");
                Program.DrawingsMenu.Add(new MenuBool("streamingmode", "Disable All Drawings", false));
                //Program.DrawingsMenu.Add(new MenuBool("drawenemywaypoints", "Draw Enemy Waypoints"));
                Program.DrawingsMenu.Add(new MenuBool("EDraw", "Draw E range"));
                Program.SkinhackMenu = new Menu("skinhackmenu", "Skin Hack");
                //Program.Orbwalker

                Program.ComboMenu.Add(new MenuBool("KiteOrbwalker", "USE KITING ORBWALKER LOGIC?", false));
                Program.ComboMenu.Add(new MenuBool("AAResetQ", "Use Q to reset AA? (UNTESTED)", false));
                Program.ComboMenu.Add(new MenuBool("QCombo", "USE Q", true));
                Program.ComboMenu.Add(new MenuSlider("QMinMana", "Min mana% for Q", 10, 0, 100));
                Program.ComboMenu.Add(new MenuBool("EComboMinionReset", "USE SMART E RESET", true));
                Program.ComboMenu.Add(new MenuSlider("EComboMinionResetStacksNew", "Min enemy stacks for SMART RESET",
                    3, 1,
                    33));
                Program.ComboMenu.Add(new MenuBool("UseE2Tilt", "Use E to TILT enemies?"));
                Program.ComboMenu.Add(new MenuSlider("EComboMinStacks", "Min stacks for E poke", 5, 1, 30));
                Program.ComboMenu.Add(new MenuSlider("DamageReductionE", "Reduce E dmg by", 0, 0, 300));
                Program.ComboMenu.Add(new MenuSlider("DamageAdditionerE", "Increase E dmg by", 0, 0, 300));
                Program.ComboMenu.Add(new MenuBool("RComboSelf", "USE R TO SELF-PEEL", false));
                Program.ComboMenu.Add(new MenuBool("RComboSupport", "USE R TO SAVE SUPP"));
                Program.ComboMenu.Add(new MenuBool("MinionOrbwalking", "RBWALK ON MINIONS?", false));

                Program.LaneClearMenu.Add(new MenuBool("LaneclearE", "Use E"));
                Program.LaneClearMenu.Add(new MenuSlider("LaneclearEMinMana", "Min Mana% for E Laneclear", 50, 1, 100));
                Program.LaneClearMenu.Add(new MenuSlider("LaneclearEMinions", "Min minions killable by E", 2, 1, 6));

                var antigcmenu = Program.EscapeMenu.Add(new Menu("antigapcloser", "Anti-Gapcloser"));
                foreach (var hero in GameObjects.EnemyHeroes)
                {
                    var championName = hero.CharacterName;
                    antigcmenu.Add(
                        new MenuBool(championName, "antigc " + championName).SetValue(
                            Lists.CancerChamps.Any(entry => championName == entry)));
                }

                Program.SkinhackMenu.Add(
                    new MenuList("skin", "Skin: ", new[]
                    {
                        "Classic", "BloodMoon", "Championship"
                    }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void FinishMenuInit()
        {
            Program.MainMenu.Add(Program.ComboMenu);
            Program.MainMenu.Add(Program.LaneClearMenu);
            Program.MainMenu.Add(Program.EscapeMenu);
            Program.MainMenu.Add(Program.SkinhackMenu);
            Program.MainMenu.Add(Program.DrawingsMenu);
            //Program.MainMenu.Add(Program.OrbwalkerMenu);
            Program.MainMenu.Add(new MenuSeparator("Author", "Author: Hermes"));
            Program.MainMenu.Attach();
        }
    }

}