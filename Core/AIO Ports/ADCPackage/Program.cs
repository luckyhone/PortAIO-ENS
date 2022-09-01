using System;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace ADCPackage
{
    class Program
    {
        public static void Game_OnGameLoad()
        {
            Menu.Initialize();
            PluginLoader.Load();

            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            OrbwalkerSwitch.Update();
        }
    }
}