using EnsoulSharp;

namespace Sharpy_AIO
{
    class Program
    {
        public static void Game_OnGameLoad()
        {
            PluginLoader.LoadPlugin(ObjectManager.Player.CharacterName);
        }    
    }
}