using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace Slutty_Thresh
{
    internal class MenuConfig
    {
        public static Menu Config, comboMenu;
        public const string Menuname = "Slutty Thresh";

        public static void CreateMenuu()
        {
            Config = new Menu(Menuname, Menuname, true);

            var drawMenu = Config.Add(new Menu("Drawings", "Drawings"));
            drawMenu.Add(new MenuBool("Draw", "Display Drawings").SetValue(true));
            drawMenu.Add(new MenuBool("qDraw", "Draw [Q]").SetValue(true));
            drawMenu.Add(new MenuBool("wDraw", "Draw [W]").SetValue(true));
            drawMenu.Add(new MenuBool("eDraw", "Draw [E]").SetValue(true));
            drawMenu.Add(new MenuBool("qfDraw", "Draw Flash-[Q] Range").SetValue(true));

            comboMenu = new Menu("combospells", "Combo Settings");
            {
                var qsettings = new Menu("settings", "Q (Death Sentence) Settings");
                {
                    qsettings.Add(new MenuBool("useQ", "Use [Q] (Death Sentence)").SetValue(true));
                    qsettings.Add(new MenuBool("smartq", "Smart [Q]").SetValue(true));
                    qsettings.Add(new MenuBool("useQ1", "Use 2nd [Q] (Death Leap)").SetValue(true));
                    qsettings.Add(
                        new MenuSlider("useQ2", "Set 2nd-[Q] Delay (Death Leap)", 1000, 0, 1500));
                    qsettings.Add(
                        new MenuSlider("qrange", "Use [Q] Only if Target Range >=", 500, 0, 1040));
                    comboMenu.Add(qsettings);
                }
                comboMenu.Add(new MenuBool("useE", "Use [E] (Flay)").SetValue(true));
                comboMenu.Add(new MenuList("combooptions", "Set [E] Mode", new[] {"Push", "Pull"}, 1));
                comboMenu.Add(new MenuBool("useR", "Use [R] (The Box)").SetValue(true));
                comboMenu.Add(
                    new MenuSlider("rslider", "Use [R] Only if X Target(s) in Range", 3, 1, 5));
            }
            Config.Add(comboMenu);

            var lantMenu = new Menu("Lantern Settings", "lantern");
            {
                foreach (var hero in
                         ObjectManager.Get<AIHeroClient>()
                             .Where(x => x.IsAlly
                                         && !x.IsMe))
                {
                    {
                        lantMenu.Add(new MenuList("healop" + hero.CharacterName, hero.CharacterName,
                            new[] {"Lantern", "No Lantern"}));

                        lantMenu.Add(
                            new MenuSlider("hpsettings" + hero.CharacterName, "Lantern When % HP <", 20));
                    }

                }

                lantMenu.Add(new MenuSlider("manalant", "Set % Mana for Lantern", 50));
                lantMenu.Add(new MenuBool("autolantern", "Auto-Lantern Ally if [Q] hits").SetValue(false));
            }

            var laneMenu = new Menu("laneclear", "Lane Clear");
            {
                laneMenu.Add(new MenuBool("useelch", "Use [E]").SetValue(true));
            }

            Config.Add(laneMenu);

            Config.Add(lantMenu);
            var flashMenu = new Menu("flashf", "Flash-Hook Settings");
            {
                flashMenu.Add(new MenuList("flashmodes", "Flash Modes", new[] {"Flash->E->Q", "Flash->Q"}));
                flashMenu.Add(new MenuKeyBind("qflash", "Flash-Hook", Keys.T, KeyBindType.Press));
            }

            Config.Add(flashMenu);

            var miscMenu = new Menu("miscsettings", "Miscellaneous");

            var eventMenu = new Menu("eventssettings", "Events");
            {
                eventMenu.Add(new MenuBool("useW2I", "Interrupt with [W]").SetValue(true));
                eventMenu.Add(new MenuBool("useQW2D", "Use W/Q on Dashing").SetValue(true));
            }

            miscMenu.Add(eventMenu);
            Config.Add(miscMenu);

            Config.Attach();
        }
    }
}