using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Utilitys;

namespace hikiMarksmanRework.Core.Menus
{
    class VayneMenu
    {
        public static Menu Config;
        public static void MenuInit()
        {
            var comboMenu = new Menu(":: General Settings (1337)", ":: General Settings (1337)");
            {
                comboMenu.Add(new MenuBool("vayne.q.combo", "Use (Q)").SetValue(true)); //+
                comboMenu.Add(new MenuBool("vayne.e.combo", "Use (E)").SetValue(true)); //+
                comboMenu.Add(new MenuBool("vayne.r.combo", "Use (R)").SetValue(true));
                comboMenu.Add(new MenuList("vayne.q.type", "Tumble Method",new[] { "Safe", "Cursor Position" })); //+
                comboMenu.Add(new MenuList("vayne.e.type", "Condemn Method",new[] { "PRADASMART", "VHR:BASIC","SHINE" ,"MARKSMAN", "SHARPSHOOTER", "360" })); //+
                comboMenu.Add(new MenuSlider("vayne.e.push.distance", "(E) Push Distance",390, 300, 475)); //+
                comboMenu.Add(new MenuBool("vayne.condemn.jungle.mobs", "Condemn Jungle Mobs").SetValue(true)).SetTooltip("Red & Blue & Crimson Raptor & Gromp & Krug & Rift Scuttler"); // +
                comboMenu.Add(new MenuBool("vayne.tumble.jungle.mobs", "Tumble Jungle Mobs").SetValue(true)).SetTooltip("Red & Blue & Crimson Raptor & Gromp & Krug & Rift Scuttler"); // +
                comboMenu.Add(new MenuBool("vayne.q.after.aa", "(Q) -> After -> (AA) ?").SetValue(true)); // +
                comboMenu.Add(new MenuSlider("vayne.auto.r.enemy.count", "Auto (R) Enemy Count",5, 1, 5));
                comboMenu.Add(new MenuSlider("vayne.auto.r.search.range", "Auto (R) Search Range",700, 1, 2000));
                comboMenu.Add(new MenuSlider("vayne.auto.r.minimum.health", "Auto (R) Minimum Health",69, 1, 99));
                comboMenu.Add(new MenuBool("vayne.auto.q.if.enemy.has.2.stack", "Auto (Q) If Enemy Has 2 Silver Bolt").SetValue(false)).SetTooltip("Casting (Q) to Safe Position"); //+
                Config.Add(comboMenu);
            }

            var miscMenu = new Menu(":: Miscellaneous", ":: Miscellaneous");
            {
                var gapcloseSet = new Menu("Anti-Gapclose Settings", "Anti-Gapclose Settings");
                {
                    gapcloseSet.Add(new MenuList("vayne.e.gapclosex", "(E) Anti-Gapclose",new[] { "On", "Off" }, 1));
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
            
            var drawMenu = new Menu(":: Draw Settings", ":: Draw Settings");
            {
                var skillDraw = new Menu("Skill Draws", "Skill Draws");
                {
                    skillDraw.Add(new MenuBool("vayne.q.draw", "Q Range"));
                    skillDraw.Add(new MenuBool("vayne.e.draw", "E Range"));
                    drawMenu.Add(skillDraw);
                }
                Config.Add(drawMenu);
            }
            Config.Add(new MenuList("harass.type", "Harass Method",new[] { "AA -> AA -> (Q) Harass", "AA -> AA -> (E) Harass" }));
            Config.Add(new MenuSlider("vayne.harass.mana", "Min. Mana Percent",450, 300, 475)).SetTooltip("Manage your mana for harass");
            Config.Attach();
        }
    }
}
