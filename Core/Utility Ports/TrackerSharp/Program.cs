using EnsoulSharp.SDK.MenuUI;

namespace Tracker
{
    internal class Program
    {
        public static Menu Config;

        public static void Loads()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Config = new Menu("Tracker", "Tracker", true);
            HbTracker.AttachToMenu(Config);
            WardTracker.AttachToMenu(Config);
            Config.Attach();
        }
    }
}