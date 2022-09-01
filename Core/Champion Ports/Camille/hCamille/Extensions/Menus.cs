using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace hCamille.Extensions
{
    public static class Menus
    {
        public static Menu Config { get; set; }

        public static void Initializer()
        {
            Config = new Menu("hCamille", "hCamille", true);
            {
                var combomenu = new Menu("Combo Settings", "Combo Settings");
                {
                    combomenu.Add(new MenuSeparator("q.settings", "[Q] Settings")).SetFontColor(SharpDX.Color.Gold);
                    combomenu.Add(new MenuBool("q.combo", "Use [Q] ").SetValue(true));
                    combomenu.Add(new MenuList("q.mode", "[Q] Type",new[] { "After Attack", "In AA Range" }));

                    combomenu.Add(new MenuSeparator("W.settings", "[W] Settings")).SetFontColor(
                        SharpDX.Color.Gold);
                    combomenu.Add(new MenuBool("w.combo", "Use [W] ").SetValue(true));
                    combomenu.Add(new MenuList("w.mode", " [W Mode]",new[] { "While Dashing", "Always" }, 1));

                    combomenu.Add(new MenuSeparator("E.settings", "[E] Settings")).SetFontColor(
                        SharpDX.Color.HotPink);
                    combomenu.Add(new MenuBool("e.combo", "Use [E] ").SetValue(true));
                    combomenu.Add(new MenuSlider("wall.search.range", "[E] Wall Search Range",1300, 1, 2500)).SetTooltip("1300 is recommenced");
                    combomenu.Add(new MenuSlider("wall.distance.to.enemy", "[E] Max Wall Distance to Enemy",865, 1, 1500)).SetTooltip("865 is recommenced");
                    combomenu.Add(new MenuSlider("enemy.search.range", "[E] Enemy Search Range",1365, 1365, 1900)).SetTooltip("1365 is recommenced (1365 -> E.Range + 500)");
                    combomenu.Add(new MenuSlider("max.enemy.count", "[E] Max Enemy Count",5, 1, 5));
                    Config.Add(combomenu);
                }
                
                var ultimatemenu = new Menu("Ultimate Settings", "Ultimate Settings");
                {
                    ultimatemenu.Add(new MenuBool("r.combo", "Use [R] ").SetValue(true));
                    ultimatemenu.Add(new MenuSlider("enemy.health.percent", "[R] Enemy Health Percentage",30, 1, 99));

                    var whitelist = new Menu("Ultimate Whitelist", "Ultimate Whitelist");
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes)
                        {
                            whitelist.Add(new MenuBool("r."+enemy.CharacterName, "Use [R]:  "+enemy.CharacterName).SetValue(Utilities.HighChamps.Contains(enemy.CharacterName)));
                        }
                        ultimatemenu.Add(whitelist);
                    }
                    ultimatemenu.Add(new MenuList("r.mode", "[R] Type",new[] { "Auto", "Only Selected" }));
                    Config.Add(ultimatemenu);
                }

                var harassmenu = new Menu("Harass Settings", "Harass Settings");
                {
                    harassmenu.Add(new MenuBool("q.harass", "Use [Q] ").SetValue(true));
                    harassmenu.Add(new MenuBool("w.harass", "Use [W] ").SetValue(true));
                    harassmenu.Add(new MenuSlider("harass.mana", "Mana Manager",50, 1, 99)).SetFontColor(SharpDX.Color.Gold);
                    Config.Add(harassmenu);
                }
                
                var clearmenu = new Menu("Wave Settings", "Wave Settings");
                {
                    clearmenu.Add(new MenuBool("q.clear", "Use [Q]").SetValue(true));
                    clearmenu.Add(new MenuBool("w.clear", "Use [W]").SetValue(true));
                    clearmenu.Add(new MenuSlider("min.count", "[W] Min. Minion Count",3, 1, 5));
                    clearmenu.Add(new MenuSlider("clear.mana", "Mana Manager",50, 1, 99)).SetFontColor(SharpDX.Color.Gold);
                    Config.Add(clearmenu);
                }

                var junglemenu = new Menu("Jungle Clear Settings", "Jungle Clear Settings");
                {
                    junglemenu.Add(new MenuBool("q.jungle", "Use [Q]").SetValue(true));
                    junglemenu.Add(new MenuBool("w.jungle", "Use [W]").SetValue(true));
                    junglemenu.Add(new MenuSlider("jungle.mana", "Mana Manager",50, 1, 99)).SetFontColor(SharpDX.Color.Gold);
                    Config.Add(junglemenu);
                }

                var miscmenu = new Menu("Miscellaneous", "Miscellaneous");
                {
                    miscmenu.Add(new MenuBool("e.anti", "[E] -> Antigapcloser ?").SetValue(true));
                    miscmenu.Add(new MenuBool("e.interrupt", "[E] -> Interrupter ?").SetValue(true));
                    Config.Add(miscmenu);
                }
                
                var drawMenu = new Menu("Draw Settings", "Draw Settings");
                {
                    var skillDraw = new Menu("Skill Draws", "Skill Draws");
                    {
                        skillDraw.Add(new MenuBool("q.draw", "Draw Q Range")); // White
                        skillDraw.Add(new MenuBool("w.draw", "Draw W Range")); // White
                        skillDraw.Add(new MenuBool("e.draw", "Draw E Range")); // White
                        skillDraw.Add(new MenuBool("r.draw", "Draw R Range")); // White
                        drawMenu.Add(skillDraw);
                    }

                    Config.Add(drawMenu);

                }
                
                Config.Add(
                    new MenuSeparator("keys", "Keys")).SetFontColor(SharpDX.Color.DodgerBlue);
                Config.Add(
                    new MenuKeyBind("flee", "Flee!",Keys.A,
                        KeyBindType.Press));
                Config.Add(
                    new MenuSeparator("credits.x1", "Developed by Hikigaya")).SetFontColor(SharpDX.Color.Gold);
                Config.Attach();
            }
        }
        
        private static float TotalDamage(AIHeroClient hero)
        {
            var damage = 0d;
            if (Spells.Q.IsReady())
            {
                if (Calculation.HasProtocolOneBuff)
                {
                    damage += hero.ProtocolDamage();
                }
                if (Calculation.HasProtocolTwoBuff)
                {
                    damage += hero.ProtocolTwoDamage();
                }
            }
            if (Spells.W.IsReady())
            {
                damage += hero.TacticalDamage();
            }
            if (Spells.E.IsReady())
            {
                damage += hero.WallDiveDamage();
            }
            if (Spells.R.IsReady())
            {
                damage += hero.HextechDamage()*4;
            }
            return (float)damage;
        }
    }
}