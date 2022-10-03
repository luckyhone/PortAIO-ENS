using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Utilitys;

 namespace hikiMarksmanRework.Core.Menus
{
    class DravenMenu
    {
        public static Menu Config;
        public static Orbwalker Orbwalker;
        public static void MenuInit()
        {
            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("draven.q.combo", "Use Q").SetValue(true)).SetTooltip("Uses Q in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuSlider("draven.q.combo.axe.count", "Min. Axe Count",2, 1, 2)).SetTooltip("Min axe count for combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("draven.w.combo", "Use W").SetValue(true)).SetTooltip("Uses W in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("draven.e.combo", "Use E").SetValue(true)).SetTooltip("Uses E in Combo").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuBool("draven.r.combo", "Use R").SetValue(true)).SetTooltip("Uses R in Combo (Only Casting If Enemy Killable)").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuSlider("draven.min.ult.distance", "(R) Min. Distance",200, 200, 1000)).SetTooltip("(R) Min. Distance for Ult").TooltipColor = SharpDX.Color.GreenYellow;
                comboMenu.Add(new MenuSlider("draven.max.ult.distance", "(R) Max. Distance",2000, 1000, 3000)).SetTooltip("(R) Max. Distance for Ult").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(comboMenu);
            }

            var clearMenu = new Menu("Clear Settings", "Clear Settings");
            {
                clearMenu.Add(new MenuBool("draven.q.clear", "Use Q").SetValue(true)).SetTooltip("Uses Q in Clear").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("draven.q.lane.clear.axe.count", "Min. Axe Count",1, 1, 2)).SetTooltip("Min. Axe Count").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("draven.q.minion.count", "Min. Minion Count",4, 1, 10)).SetTooltip("Minimum Minion Count For Laneclear").TooltipColor = SharpDX.Color.GreenYellow;
                clearMenu.Add(new MenuSlider("draven.clear.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(clearMenu);
            }

            var jungleMenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                jungleMenu.Add(new MenuBool("draven.q.jungle", "Use Q").SetValue(true)).SetTooltip("Uses Q in Jungle").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuSlider("draven.q.jungle.clear.axe.count", "Min. Axe Count",1, 1, 2)).SetTooltip("Min. Axe Count").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuBool("draven.e.jungle", "Use E").SetValue(true)).SetTooltip("Uses E in Jungle (Using Mouse Position)").TooltipColor = SharpDX.Color.GreenYellow;
                jungleMenu.Add(new MenuSlider("draven.jungle.mana", "Min. Mana",50, 1, 99)).SetTooltip("Manage your Mana!").TooltipColor = SharpDX.Color.GreenYellow;
                Config.Add(jungleMenu);
            }

            var miscMenu = new Menu("Miscellaneous", "Miscellaneous");
            {
                var gapcloseSet = new Menu("Anti-Gapclose Settings", "Anti-Gapclose Settings");
                {
                    gapcloseSet.Add(new MenuBool("draven.e.antigapcloser", "(E) Anti-Gapclose").SetValue(true));
                    miscMenu.Add(gapcloseSet);
                }
                var interrupterSet = new Menu("Interrupter Settings", "Interrupter Settings");
                {
                    interrupterSet.Add(new MenuBool("draven.e.interrupter", "(E) Interrupter").SetValue(true));
                    interrupterSet.Add(
                        new MenuList("min.interrupter.danger.level", "Interrupter Danger Level",new[] { "HIGH", "MEDIUM", "LOW" }));
                    miscMenu.Add(interrupterSet);
                }
                miscMenu.Add(new MenuList("catchRadiusMode", "Catch Radius Mode", new[] { "Circle", "Sector" }, 1));

                Config.Add(miscMenu);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                var skillDraw = new Menu("Skill Draws", "Skill Draws");
                {
                    skillDraw.Add(new MenuBool("DE", "Draw E Range"));
                    skillDraw.Add(new MenuBool("DR", "Draw R Range"));
                    drawMenu.Add(skillDraw);
                }
                var axeDraw = new Menu("Axe Draws", "Axe Draws");
                {
                    axeDraw.Add(new MenuBool("DCR", "Draw Catch Radius"));
                    axeDraw.Add(new MenuBool("DAR", "Draw Axe Spots"));
                    drawMenu.Add(axeDraw);
                }
                Config.Add(drawMenu);

            }

            Config.Attach();
        }
    }
}
