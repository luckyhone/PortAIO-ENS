using EnsoulSharp;
using EnsoulSharp.SDK;
using OlympusAIO.Champions;

namespace Zypppy_AurelionSol
{

    class Program
    {
        static void Loads()
        {
            GameEvent.OnGameLoad += GameEvents_GameStart;
        }

        public static void GameEvents_GameStart()
        {
            if (ObjectManager.Player.CharacterName != "AurelionSol")
                return;

            var AurelionSol = new AurelionSol();
        }
    }
}