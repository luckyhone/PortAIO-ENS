using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Utilitys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace hikiMarksmanRework.Core.Menus
{
    class SivirMenu
    {
        public static Menu Config;
        public static void MenuInit()
        {
            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("sivir.q.combo", "Use Q").SetValue(true)).SetTooltip("Uses Q in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("sivir.w.combo", "Use W").SetValue(true)).SetTooltip("Uses W in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(comboMenu);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("sivir.q.harass", "Use Q").SetValue(true)).SetTooltip("Uses Q in Harass").TooltipColor = SharpDX.Color.GreenYellow;
                harassMenu.Add(new MenuBool("sivir.w.harass", "Use W").SetValue(true)).SetTooltip("Uses W in Harass").TooltipColor = SharpDX.Color.GreenYellow;
                harassMenu.Add(new MenuSlider("sivir.harass.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                var qToggleMenu = new Menu("Q Toggle", "Q Toggle");
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValid))
                    {
                        qToggleMenu.Add(new MenuBool("sivir.q.toggle." + enemy.CharacterName, "(Q) " + enemy.CharacterName).SetValue(true));
                    }

                    harassMenu.Add(qToggleMenu);
                }

                Config.Add(harassMenu);
            }

            var clearMenu = new Menu("Clear Settings", "Clear Settings");
            {
                clearMenu.Add(new MenuBool("sivir.q.clear", "Use Q").SetValue(true)).SetTooltip("Uses Q in Clear").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("sivir.q.minion.hit.count", "(Q) Min. Minion Hit",3, 1, 5)).SetTooltip("Minimum minion count for Q").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("sivir.clear.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(clearMenu);
            }

            var jungleMenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                jungleMenu.Add(new MenuBool("sivir.q.jungle", "Use Q").SetValue(true)).SetTooltip("Uses Q in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuBool("sivir.w.jungle", "Use W").SetValue(true)).SetTooltip("Uses W in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuSlider("sivir.jungle.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(jungleMenu);
            }
            var miscMenu = new Menu("Miscellaneous", "Miscellaneous");
            {
                var gapcloseSet = new Menu("(E) Spell Block", "(E) Spell Block");
                {
                    gapcloseSet.Add(new MenuList("sivir.e.spell.block", "(E) Spell Block ?",new[] { "On", "Off" }));
                    gapcloseSet.Add(new MenuSeparator("masterracec0mb0X", "                     Blockable Spells")).SetFontColor(SharpDX.Color.LightBlue);

                    foreach (var spellblock in EvadeDb.SpellData.SpellDatabase.Spells.Where(x => ObjectManager.Get<AIHeroClient>().Any(y => y.CharacterName == x.charName && y.IsEnemy)))
                    {
                        gapcloseSet.Add(new MenuBool("block." + spellblock.spellName, "(E) Block: " + spellblock.charName + " - Spell: " + spellblock.spellKey).SetValue(true));
                    }
                    miscMenu.Add(gapcloseSet);
                }

                Config.Add(miscMenu);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                var skillDraw = new Menu("Skill Draws", "Skill Draws");
                {
                    skillDraw.Add(new MenuBool("sivir.q.draw", "Q Range"));
                    skillDraw.Add(new MenuBool("sivir.w.draw", "W Range"));
                    skillDraw.Add(new MenuBool("sivir.e.draw", "E Range"));
                    skillDraw.Add(new MenuBool("sivir.r.draw", "R Range"));
                    drawMenu.Add(skillDraw);
                }
                Config.Add(drawMenu);
            }
            Config.Attach();
        }
    }
}