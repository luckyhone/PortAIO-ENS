using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace ADCCOMMON
{
    using System;
    using Color = System.Drawing.Color;

    public static class ManaManager
    {
        private static Menu FarmMenu;
        private static Menu drawMenu;
        public static bool SpellFarm;
        public static bool SpellHarass;

        public static bool HasEnoughMana(int manaPercent)
        {
            return ObjectManager.Player.ManaPercent >= manaPercent && !ObjectManager.Player.IsUnderEnemyTurret();
        }

        public static void AddSpellFarm(Menu mainMenu)
        {
            FarmMenu = mainMenu;

            mainMenu.Add(new MenuBool("SpellFarm", "Use Spell Farm(Mouse Scroll)", true).SetValue(true));
            mainMenu.Add(
                    new MenuKeyBind("SpellHarass", "Use Spell Harass(In LaneClear Mode)",Keys.H,
                        KeyBindType.Toggle, true))
                .ValueChanged += ManaManager_ValueChanged;

            mainMenu["SpellFarm"].GetValue<MenuBool>().AddPermashow( "Use Spell Farm");
            mainMenu["SpellHarass"].GetValue<MenuKeyBind>().AddPermashow( "Use Spell Harass(In LaneClear Mode)");

            SpellFarm = mainMenu["SpellFarm"].GetValue<MenuBool>().Enabled;
            SpellHarass = mainMenu["SpellHarass"].GetValue<MenuKeyBind>().Active;

            Game.OnWndProc += OnWndProc;
        }
        
        private static void ManaManager_ValueChanged(MenuKeyBind menuitem, EventArgs Args)
        {
            SpellHarass = menuitem.GetValue<MenuKeyBind>().Active;
        }

        public static void AddDrawFarm(Menu mainMenu)
        {
            drawMenu = mainMenu;

            mainMenu.Add(new MenuBool("DrawFarm", "Draw Spell Farm Status", true).SetValue(true));
            mainMenu.Add(new MenuBool("DrawHarass", "Draw Spell Harass Status", true).SetValue(true));
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead && !ObjectManager.Player.InShop() && !MenuGUI.IsChatOpen  )
            {
                if (drawMenu["DrawFarm"].GetValue<MenuBool>().Enabled)
                {
                    var MePos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                    Drawing.DrawText(MePos[0] - 57, MePos[1] + 48, Color.FromArgb(242, 120, 34),
                        "Spell Farm:" + (SpellFarm ? "On" : "Off"));
                }

                if (drawMenu["DrawHarass"].GetValue<MenuBool>().Enabled)
                {
                    var MePos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                    Drawing.DrawText(MePos[0] - 57, MePos[1] + 68, Color.FromArgb(242, 120, 34),
                        "Spell Hars:" + (SpellHarass ? "On" : "Off"));
                }
            }
        }

        private static void OnWndProc(GameWndEventArgs Args)
        {
            if (Args.Msg == 0x20a)
            {
                FarmMenu["SpellFarm"].GetValue<MenuBool>().SetValue(!FarmMenu["SpellFarm"].GetValue<MenuBool>().Enabled);
                SpellFarm = FarmMenu["SpellFarm"].GetValue<MenuBool>().Enabled;
            }
        }
    }
}
