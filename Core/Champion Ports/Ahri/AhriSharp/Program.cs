

namespace AhriSharp
{
    class Program
    {
        public static Helper Helper;

        public static void Game_OnGameLoad()
        {
            Helper = new Helper();
            new Ahri();
        }
        
    }
}