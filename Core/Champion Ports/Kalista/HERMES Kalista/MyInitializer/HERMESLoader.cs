using EnsoulSharp;

namespace HERMES_Kalista.MyInitializer
{
    public static partial class HERMESLoader
    {
        public static void Init()
        {
            if (ObjectManager.Player.CharacterName == "Kalista")
            {
                MyUtils.Cache.Load();
                LoadMenu();
                LoadSpells();
                LoadLogic();
                //ShowNotifications();
                Draw();
            }
        }
    }
}