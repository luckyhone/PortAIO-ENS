using EnsoulSharp.SDK.MenuUI;

namespace Flowers__Annie
{

    public static class MenuExtensions
    {
        public static bool GetBool(this Menu Menu, string MenuItemName)
        {
            return Menu[MenuItemName].GetValue<MenuBool>().Enabled;
        }

        public static bool GetKey(this Menu Menu, string MenuItemName)
        {
            return Menu[MenuItemName].GetValue<MenuKeyBind>().Active;
        }

        public static int GetSlider(this Menu Menu, string MenuItemName)
        {
            return Menu[MenuItemName].GetValue<MenuSlider>().Value;
        }

        public static int GetList(this Menu Menu, string MenuItemName)
        {
            return Menu[MenuItemName].GetValue<MenuList>().Index;
        }
    }
}