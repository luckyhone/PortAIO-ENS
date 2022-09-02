using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace StormAIO
{
    public class MainMenu
    {
        public static Menu Main_Menu, UtilitiesMenu, Emotes, Level, Labeler;
        public static string Key => Labeler.GetValue<MenuKeyBind>("SpellFarmKey").Key.ToString();
        public static string Key2 => Labeler.GetValue<MenuKeyBind>("Test2").Key.ToString();
        public static MenuKeyBind SpellFarm => Labeler.GetValue<MenuKeyBind>("SpellFarmKey");

        public MainMenu()
        {
            CreateMenu();
            CreateUtilitiesMenu();
        }

        #region Functions

        private static void CreateMenu()
        {
            Main_Menu = new Menu("StormAIO", "StormAIO", true);
            Main_Menu.Attach();
        }

        private static void CreateUtilitiesMenu()
        {
            UtilitiesMenu = new Menu("StormAIOUtilities", "StormAIO-Utilities", true);
            Emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            UtilitiesMenu.Add(Emotes);
            Level = new Menu("AutoLevel", "Auto Level Up")
            {
                new MenuBool("autolevel", "Auto Level")
            };
            UtilitiesMenu.Add(Level);
            var Indicator = new Menu("Indicator", "Indicator")
            {
                new MenuBool("Indicator", "Indicator"),
                new MenuColor("SetColor", "SetColor", new ColorBGRA(253, 197, 45, 232))
            };
            UtilitiesMenu.Add(Indicator);
            Labeler = new Menu("L", "Label Menu")
            {
                new MenuKeyBind("SpellFarmKey", "Spell Far mKey", Keys.M, KeyBindType.Toggle),
                new MenuSeparator("C", "Changes Need Reload, Reload Key F5")
            };
            UtilitiesMenu.Add(Labeler);
            UtilitiesMenu.Attach();
            SpellFarm.SetValue(true); // < set it to on by Default 
        }

        #endregion
    }
}