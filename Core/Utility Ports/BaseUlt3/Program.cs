using System;

namespace BaseUlt3
{
    class Program
    {
        public static BaseUlt BaseUlt; 

        public static void Loads()
        {
            Game_OnGameLoad(new EventArgs());
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            BaseUlt = new BaseUlt();
        }
    }
}