using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.utilities
{
    public class SkinChanger
    {
        public static Menu SkinSeter;
        private static AIHeroClient Player => ObjectManager.Player;

        public static MenuSliderButton SkinMeun => SkinSeter.GetValue<MenuSliderButton>(Player.CharacterName);
        private static bool theyalltrash => SkinSeter.GetValue<MenuSliderButton>("trash").Enabled;

        public SkinChanger()
        {
            SkinSeter = new Menu("SkinChanger", "Set Skin") {new MenuBool("trash", "They are trash")};
            SkinSeter.Add(new MenuSliderButton(Player.CharacterName, "set skin",
                1)).ValueChanged += OnValueChanged;
            MainMenu.UtilitiesMenu.Add(SkinSeter);
            Player.SetSkin(SkinMeun.ActiveValue);
            Game.OnNotify += delegate(GameNotifyEventArgs args)
            {
                if (args.EventId == GameEventId.OnGameStart)
                    // set Skin upon game start 
                    if (theyalltrash)
                    {
                        var enemies = GameObjects.EnemyHeroes;
                        foreach (var Enemy in enemies) Enemy.SetSkin(0);
                        // set them to trash Xd
                    }

                if (args.EventId == GameEventId.OnReincarnate && SkinMeun.Enabled)
                    Player.SetSkin(SkinMeun.ActiveValue);
                // ^ to Reload Player skin Icon personally I like Skin icons :)
            };
            Game.OnWndProc += delegate(GameWndEventArgs args)
            {
                if (args.Msg == 0x0100 && args.WParam == (int) Keys.Up)
                {
                    if (SkinMeun.ActiveValue == 100) SkinMeun.SetValue(0);
                    if (SkinMeun.ActiveValue != 100)
                        SkinMeun.SetValue(SkinMeun.ActiveValue == -1 ? 0 + 1 : SkinMeun.ActiveValue + 1);
                    Player.SetSkin(SkinMeun.ActiveValue);
                }

                if (args.Msg == 0x0100 && args.WParam == (int) Keys.Down)
                {
                    if (SkinMeun.ActiveValue == 0) return;
                    if (SkinMeun.ActiveValue != 0) SkinMeun.SetValue(SkinMeun.ActiveValue - 1);
                    Player.SetSkin(SkinMeun.ActiveValue);
                }
            };
        }


        private void OnValueChanged(object sender, EventArgs e)
        {
            if (SkinMeun.Enabled) Player.SetSkin(SkinMeun.ActiveValue);
        }
    }
}