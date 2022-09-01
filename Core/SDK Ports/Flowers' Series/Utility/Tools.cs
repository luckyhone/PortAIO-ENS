using EnsoulSharp.SDK.MenuUI;
using Flowers_Series.Common;

namespace Flowers_Series.Utility
{
    public static class Tools
    {
        public static Menu Menu;
        public static bool EnableActivator = true;

        public static void Inject()
        {
            Menu = Program.Menu.Add(new Menu("Tools", "Tools"));

            var PlugingMenu = Menu.Add(new Menu("PlugingInject", "Pluging Inject"));
            {
                //PlugingMenu.Add(new MenuBool("LoadEvade", "Inject Evade Plugings", true));
                PlugingMenu.Add(new MenuBool("LoadPotions", "Inject Potions Plugings", true));
                //PlugingMenu.Add(new MenuBool("LoadOffensive", "Inject Offensive Plugings", true));
                //PlugingMenu.Add(new MenuBool("LoadDefensive", "Inject Defensive Plugings", true));
                PlugingMenu.Add(new MenuBool("LoadSummoner", "Inject Summoner Plugings", true));
                PlugingMenu.Add(new MenuBool("LoadSkinChance", "Inject SkinChance Plugings", true));
                //PlugingMenu.Add(new MenuBool("LoadAutoLevel", "Inject AutoLevel Plugings", true));
                PlugingMenu.Add(new MenuSeparator(" ", "  "));
                PlugingMenu.Add(new MenuSeparator("ASDASDW", "If you Change Please Press F5 ReLoad!"));
            }

            Menu.Add(new MenuSeparator(" ", "  "));

            Manager.WriteConsole("Tools Inject!");

            if (Menu["PlugingInject"].GetValue<MenuBool>("LoadPotions").Enabled)
            {
                Potions.Inject();
            }

            if (Menu["PlugingInject"].GetValue<MenuBool>("LoadSummoner").Enabled)
            {
                Summoner.Inject();;
            }

            if (Menu["PlugingInject"].GetValue<MenuBool>("LoadSkinChance").Enabled)
            {
                SkinChance.Inject();
            }
            
        }
    }
}