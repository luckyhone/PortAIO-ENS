namespace iDZed
{
    static class Program
    {
        public static void Loads()
        {
            Game_OnGameLoad();
        }

        static void Game_OnGameLoad()
        {
            Zed.OnLoad();
        }
    }
}