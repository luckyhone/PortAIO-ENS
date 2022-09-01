using EnsoulSharp;

namespace KoreanZed
{
    internal class Program
    {
        public static void Game_OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName.ToLowerInvariant() == "zed")
            {
                var ZedZeppelin = new Zed();
            }
        }
    }
}