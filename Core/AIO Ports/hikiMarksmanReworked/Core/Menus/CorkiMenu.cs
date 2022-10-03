using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;

namespace hikiMarksmanRework.Core.Menus
{
    class CorkiMenu
    {
        public static Menu Config;
        public static void MenuInit()
        {
            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("corki.q.combo", "Use Q").SetValue(true)).SetTooltip("Uses Q in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("corki.e.combo", "Use E").SetValue(true)).SetTooltip("Uses E in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("corki.r.combo", "Use R").SetValue(true)).SetTooltip("Uses R in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(comboMenu);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("corki.q.harass", "Use Q").SetValue(true)).SetTooltip("Uses Q in Harass").TooltipColor = SharpDX.Color.GreenYellow;
                harassMenu.Add(new MenuBool("corki.r.harass", "Use W").SetValue(true)).SetTooltip("Uses R in Harass").TooltipColor = SharpDX.Color.GreenYellow;
                harassMenu.Add(new MenuSlider("ezreal.harass.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                var qToggleMenu = new Menu("R Toggle", "R Toggle");
                {
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValid))
                    {
                        qToggleMenu.Add(new MenuBool("corki.r.toggle." + enemy.CharacterName, "(R) " + enemy.CharacterName).SetValue(true));
                    }
                    harassMenu.Add(qToggleMenu);
                }

                Config.Add(harassMenu);
            }

            var clearMenu = new Menu("Clear Settings", "Clear Settings");
            {
                clearMenu.Add(new MenuBool("corki.q.clear", "Use Q").SetValue(true)).SetTooltip("Uses Q in Clear").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("corki.q.minion.hit.count", "(Q) Min. Minion Hit",3, 1, 5)).SetTooltip("Minimum minion count for Q").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("corki.clear.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(clearMenu);
            }

            var jungleMenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                jungleMenu.Add(new MenuBool("corki.q.jungle", "Use Q").SetValue(true)).SetTooltip("Uses Q in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuBool("corki.e.jungle", "Use E").SetValue(true)).SetTooltip("Uses E in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuBool("corki.r.jungle", "Use R").SetValue(true)).SetTooltip("Uses R in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuSlider("corki.jungle.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(jungleMenu);
            }

            var killStealMenu = new Menu("KillSteal Settings", "KillSteal Settings");
            {
                killStealMenu.Add(new MenuBool("ezreal.q.ks", "Use Q").SetValue(true)).SetTooltip("Uses Q if Enemy Killable").TooltipColor = SharpDX.Color.GreenYellow;
                killStealMenu.Add(new MenuBool("ezreal.r.ks", "Use R").SetValue(true)).SetTooltip("Uses R if Enemy Killable").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(killStealMenu);
            }

            var miscMenu = new Menu("Miscellaneous", "Miscellaneous");
            {
                var gapcloseSet = new Menu("Anti-Gapclose Settings", "Anti-Gapclose Settings");
                {
                    gapcloseSet.Add(new MenuList("corki.w.gapclosex", "Anti-Gapclose",new[] { "On", "Off" }, 1));
                    gapcloseSet.Add(new MenuSeparator("masterracec0mb0X", "             Custom Anti-Gapcloser")).SetFontColor(SharpDX.Color.LightBlue);
                    foreach (var gapclose in AntiGapcloseSpell.GapcloseableSpells.Where(x => ObjectManager.Get<AIHeroClient>().Any(y => y.CharacterName == x.ChampionName && y.IsEnemy)))
                    {
                        gapcloseSet.Add(new MenuBool("gapclose." + gapclose.ChampionName, "Anti-Gapclose: " + gapclose.ChampionName + " - Spell: " + gapclose.Slot).SetValue(true));
                        gapcloseSet.Add(new MenuSlider("gapclose.slider." + gapclose.ChampionName, "" + gapclose.ChampionName + " Priorty",gapclose.DangerLevel, 1, 5));
                    }
                    miscMenu.Add(gapcloseSet);
                }

                Config.Add(miscMenu);
            }

            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                var skillDraw = new Menu("Skill Draws", "Skill Draws");
                {
                    skillDraw.Add(new MenuBool("corki.q.draw", "Q Range"));
                    skillDraw.Add(new MenuBool("corki.w.draw", "W Range"));
                    skillDraw.Add(new MenuBool("corki.e.draw", "E Range"));
                    skillDraw.Add(new MenuBool("corki.r.draw", "R Range"));
                    drawMenu.Add(skillDraw);
                }
                var catcherMenu = new Menu("Catch Draws", "Catch Draws");
                {
                    catcherMenu.Add(new MenuBool("corki.catch.line", "Draw Catch Line").SetValue(true));
                    catcherMenu.Add(new MenuBool("corki.catch.circle", "Draw Catch Circle").SetValue(true));
                    catcherMenu.Add(new MenuBool("corki.catch.text", "Draw Catch Text").SetValue(true));
                    catcherMenu.Add(new MenuBool("corki.disable.catch", "Disable Catch Drawings").SetValue(false));
                    drawMenu.Add(catcherMenu);
                }
                Config.Add(drawMenu);
            }
            Config.Attach();
        }
        public static void OrbwalkerInit()
        {
        }
    }
}
