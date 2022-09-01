using System;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace BasicChatBlock
{
    class Program
    {
        public static Menu main;

        public static MenuBool enabledItem;
        public static bool isEnabled => enabledItem.GetValue<MenuBool>().Enabled;

        public static void Loads()
        {
            Game_OnGameLoad(new EventArgs());
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            main = new Menu("com.chatblocker","BasicChatBlocker", true);
            enabledItem = new MenuBool("com.chatblocker.enabled", "Enabled?", true).SetValue(true);
            main.Add(enabledItem);
            main.Attach();

            Game.Say("/mute all",false);

            Game.OnSendChat += Game_OnChat;
        }

        private static void Game_OnChat(GameSendChatEventArgs args)
        {
            if (!isEnabled) return;
            args.Process = false;
        }
    }
}