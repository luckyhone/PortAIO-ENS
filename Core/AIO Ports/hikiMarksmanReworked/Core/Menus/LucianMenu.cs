using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace hikiMarksmanRework.Core.Menus
{
    class LucianMenu
    {
        public static Menu Config;
        public static void MenuInit()
        {
            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("lucian.q.combo", "Use Q").SetValue(true)).SetTooltip("Uses Q in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("lucian.e.combo", "Use E").SetValue(true)).SetTooltip("Uses E in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("lucian.w.combo", "Use W").SetValue(true)).SetTooltip("Uses W in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("lucian.r.combo", "Use R").SetValue(true)).SetTooltip("Uses R in Combo (Only Casting If Enemy Killable)").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(comboMenu);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("lucian.q.harass", "Use Q").SetValue(true)).SetTooltip("Uses Q in Harass").TooltipColor = SharpDX.Color.GreenYellow;
                harassMenu.Add(new MenuList("lucian.q.type", "Harass Type",new[] { "Extended", "Normal" }));
                harassMenu.Add(new MenuBool("lucian.w.harass", "Use W").SetValue(true)).SetTooltip("Uses W in Harass").TooltipColor = SharpDX.Color.GreenYellow;
                harassMenu.Add(new MenuSlider("lucian.harass.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                var qToggleMenu = new Menu("Q Whitelist (Extended)", "Q Whitelist (Extended)");
                {
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValid))
                    {
                        qToggleMenu.Add(new MenuBool("lucian.white" + enemy.CharacterName, "(Q) " + enemy.CharacterName).SetValue(true));
                    }
                    harassMenu.Add(qToggleMenu);
                }

                Config.Add(harassMenu);
            }

            var clearMenu = new Menu("Clear Settings", "Clear Settings");
            {
                clearMenu.Add(new MenuBool("lucian.q.clear", "Use Q").SetValue(true)).SetTooltip("Uses Q in Clear").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuBool("lucian.w.clear", "Use W").SetValue(true)).SetTooltip("Uses W in Clear").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("lucian.q.minion.hit.count", "(Q) Min. Minion Hit",3, 1, 5)).SetTooltip("Minimum minion count for Q").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("lucian.w.minion.hit.count", "(W) Min. Minion Hit",3, 1, 5)).SetTooltip("Minimum minion count for W").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("lucian.clear.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(clearMenu);
            }

            var jungleMenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                jungleMenu.Add(new MenuBool("lucian.q.jungle", "Use Q").SetValue(true)).SetTooltip("Uses Q in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuBool("lucian.w.jungle", "Use W").SetValue(true)).SetTooltip("Uses W in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuBool("lucian.e.jungle", "Use E").SetValue(true)).SetTooltip("Uses E in Jungle (Using Mouse Position)").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuSlider("lucian.jungle.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(jungleMenu);
            }

            var killStealMenu = new Menu("KillSteal Settings", "KillSteal Settings");
            {
                killStealMenu.Add(new MenuBool("lucian.q.ks", "Use Q").SetValue(true)).SetTooltip("Uses Q if Enemy Killable").TooltipColor = SharpDX.Color.GreenYellow;
                killStealMenu.Add(new MenuBool("lucian.w.ks", "Use W").SetValue(true)).SetTooltip("Uses W if Enemy Killable").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(killStealMenu);
            }

            var miscMenu = new Menu("Miscellaneous", "Miscellaneous");
            {
                var gapcloseSet = new Menu("Anti-Gapclose Settings", "Anti-Gapclose Settings");
                {
                    gapcloseSet.Add(new MenuList("lucian.e.gapclosex", "(E) Anti-Gapclose",new[] { "On", "Off" }, 1));
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
                    skillDraw.Add(new MenuBool("lucian.q.draw", "Q Range"));
                    skillDraw.Add(new MenuBool("lucian.q2.draw", "Q (Extended) Range"));
                    skillDraw.Add(new MenuBool("lucian.w.draw", "W Range"));
                    skillDraw.Add(new MenuBool("lucian.e.draw", "E Range"));
                    skillDraw.Add(new MenuBool("lucian.r.draw", "R Range"));
                    drawMenu.Add(skillDraw);
                }
                Config.Add(drawMenu);
            }
            Config.Add(new MenuKeyBind("lucian.semi.manual.ult", "Semi-Manual (R)!",Keys.A, KeyBindType.Press));

            Config.Attach();
        }
    }
}