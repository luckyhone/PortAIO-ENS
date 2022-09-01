using System;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace NabbTracker
{
    class Program
    {
        public static void Loads()
        {
            GameEvent.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad()
        {
            Tracker.OnLoad();
            Game.Print("Nabb<font color=\"#228B22\">Tracker</font>: Ultima - Loaded!");
        }
    }
}