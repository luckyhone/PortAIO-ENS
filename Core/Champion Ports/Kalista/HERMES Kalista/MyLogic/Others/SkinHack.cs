using System;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace HERMES_Kalista.MyLogic.Others
{
    public static class SkinHack
    {
        public static bool Died = false;

        public static void OnUpdate(EventArgs args)
        {
            var skinid = ObjectManager.Player.SkinId;
            if (Program.SkinhackMenu.GetValue<MenuList>("skin").Index != skinid)
            {
                ObjectManager.Player.SetSkin(Program.SkinhackMenu.GetValue<MenuList>("skin").Index);
            }
        }
    }
}