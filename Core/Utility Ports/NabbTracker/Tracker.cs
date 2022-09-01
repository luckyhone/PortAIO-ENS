namespace NabbTracker
{
    class Tracker
    {
        /// <summary>
        /// Called when the game loads itself.
        /// </summary>
        public static void OnLoad()
        {
            Menus.Initialize();
            Drawings.Initialize();
        }
    }
}