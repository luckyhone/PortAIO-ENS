using System;
using EnsoulSharp;

namespace hCamille
{
    class Program
    {
        public static void Loads()
        {
            OnGameLoad(new EventArgs());
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.CharacterName == "Camille")
            {
                new hCamille.Champions.Camille();
            }
            else
            {
                Console.WriteLine("XD");
                return;
            }
        }
    }
}