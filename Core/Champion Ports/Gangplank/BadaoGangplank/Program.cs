using System;
using System.Collections.Generic;
using EnsoulSharp;

namespace BadaoGP
{
    static class Program
    {
        public static readonly List<string> SupportedChampion = new List<string>()
        {
            "Gangplank"
        };

        public static void Loads()
        {
            Game_OnGameLoad(new EventArgs());
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!SupportedChampion.Contains(ObjectManager.Player.CharacterName))
            {
                return;
            }
            Game.Print("<font color=\"#24ff24\">Badao </font>" + "<font color=\"#ff8d1a\">" +
                       ObjectManager.Player.CharacterName + "</font>" + "<font color=\"#24ff24\"> loaded!</font>");
            BadaoKingdom.BadaoChampion.BadaoGangplank.BadaoGangplank.BadaoActivate();
        }
    }
}