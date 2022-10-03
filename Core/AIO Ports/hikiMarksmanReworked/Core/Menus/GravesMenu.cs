using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace hikiMarksmanRework.Core.Menus
{
    class GravesMenu
    {
        public static Menu Config;
        public static void MenuInit()
        {
            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("graves.q.combo", "Use Q").SetValue(true)).SetTooltip("Uses Q in Combo").TooltipColor =SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("graves.w.combo", "Use W").SetValue(true)).SetTooltip("Uses W in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("graves.e.combo", "Use E").SetValue(true)).SetTooltip("Uses E in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("graves.r.combo", "Use R").SetValue(true)).SetTooltip("Uses R in Combo (Only If Enemy Killable)").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(comboMenu);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("graves.q.harass", "Use Q").SetValue(true)).SetTooltip("Uses Q in Harass").TooltipColor = SharpDX.Color.GreenYellow;
                harassMenu.Add(new MenuBool("graves.w.harass", "Use W").SetValue(true)).SetTooltip("Uses W in Harass").TooltipColor = SharpDX.Color.GreenYellow;
                harassMenu.Add(new MenuSlider("graves.harass.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor= SharpDX.Color.GreenYellow; 
                var qToggleMenu = new Menu("Q Toggle", "Q Toggle");
                {
                    foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValid))
                    {
                        qToggleMenu.Add(new MenuBool("graves.q.toggle." + enemy.CharacterName, "(Q) " + enemy.CharacterName).SetValue(true));
                    }
                    
                    harassMenu.Add(qToggleMenu);
                }

                Config.Add(harassMenu);
            }

            var clearMenu = new Menu("Clear Settings", "Clear Settings");
            {
                clearMenu.Add(new MenuBool("graves.q.clear", "Use Q").SetValue(true)).SetTooltip("Uses Q in Clear").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("graves.q.minion.hit.count", "(Q) Min. Minion Hit",3, 1, 5)).SetTooltip("Minimum minion count for Q").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("graves.clear.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(clearMenu);
            }

            var jungleMenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                jungleMenu.Add(new MenuBool("graves.q.jungle", "Use Q").SetValue(true)).SetTooltip("Uses Q in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuBool("graves.w.jungle", "Use W").SetValue(true)).SetTooltip("Uses W in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuSlider("graves.jungle.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(jungleMenu);
            }
            var miscMenu = new Menu("Miscellaneous", "Miscellaneous");
            {
                var gapcloseSet = new Menu("Anti-Gapclose Settings", "Anti-Gapclose Settings");
                {
                    gapcloseSet.Add(new MenuList("graves.e.gapclosex", "Anti-Gapclose",new[] { "On", "Off" }, 0));
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
                    skillDraw.Add(new MenuBool("graves.q.draw", "Q Range"));
                    skillDraw.Add(new MenuBool("graves.w.draw", "W Range"));
                    skillDraw.Add(new MenuBool("graves.e.draw", "E Range"));
                    skillDraw.Add(new MenuBool("graves.r.draw", "R Range"));
                    drawMenu.Add(skillDraw);
                }
                var catcherMenu = new Menu("Catch Draws", "Catch Draws");
                {
                    catcherMenu.Add(new MenuBool("graves.catch.line", "Draw Catch Line").SetValue(true));
                    catcherMenu.Add(new MenuBool("graves.catch.circle", "Draw Catch Circle").SetValue(true));
                    catcherMenu.Add(new MenuBool("graves.catch.text", "Draw Catch Text").SetValue(true));
                    catcherMenu.Add(new MenuBool("graves.disable.catch", "Disable Catch Drawings").SetValue(true));
                    drawMenu.Add(catcherMenu);
                }
                Config.Add(drawMenu);
            }
            Config.Attach();
        }
    }
}
