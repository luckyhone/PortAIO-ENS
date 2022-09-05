using EnsoulSharp;
using EnsoulSharp.SDK;

namespace NickyJinx
{
    class Program
    {
        public static void Loads()
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Jinx")
                return;

            JinxLoading.OnLoad();
            Game.Print("<font color = \"#6B9FE3\">EnsoulSharp.Jinx</font><font color = \"#E3AF6B\"> by Nicky</font>");
           
        }
    }
}