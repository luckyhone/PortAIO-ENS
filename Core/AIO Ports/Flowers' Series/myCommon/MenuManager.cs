using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace ADCCOMMON
{
    using System.Drawing;

    public static class MenuManager
    {
        public static bool GetBool(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu[MenuItemName].GetValue<MenuBool>().Enabled;
        }

        public static bool GetKey(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu[MenuItemName].GetValue<MenuKeyBind>().Active;
        }

        public static int GetSlider(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu[MenuItemName].GetValue<MenuSlider>().Value;
        }

        public static int GetList(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu[MenuItemName].GetValue<MenuList>().Index;
        }

        public static Color GetColor(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu[MenuItemName].GetValue<MenuColor>().Color.ToSystemColor();
        }

    }
}