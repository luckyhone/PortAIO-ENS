using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace ADCCOMMON
{
    using System;

    public static class SkinManager
    {
        private static Menu skinMenu;
        private static readonly int OwnSkinID;

        static SkinManager()
        {
            OwnSkinID = ObjectManager.Player.SkinId;

            Game.OnUpdate += OnUpdate;
        }

        public static void AddToMenu(Menu mainMenu, int SkinCount = 15)
        {
            skinMenu = mainMenu;

            mainMenu.Add(new MenuBool("EnabledSkin", "Enabled", true).SetValue(false));
            mainMenu.Add(new MenuSlider("SelectSkin", "Select Skin ID: ", 0, 0, SkinCount));

            mainMenu["EnabledSkin"].GetValue<MenuBool>().ValueChanged += delegate(MenuBool obj, EventArgs Args)
            {
                if (!obj.GetValue<MenuBool>().Enabled)
                {
                    ObjectManager.Player.SetSkin(OwnSkinID);
                }
            };
        }

        private static void OnUpdate(EventArgs args)
        {
            if (skinMenu["EnabledSkin"].GetValue<MenuBool>().Enabled)
            {
                ObjectManager.Player.SetSkin(
                    skinMenu["SelectSkin"].GetValue<MenuSlider>().Value);
            }
        }
    }
}