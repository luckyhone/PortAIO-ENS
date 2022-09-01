using EnsoulSharp;

namespace Flowers_Yasuo
{
    public class MyLoader
    {
        public static void Loads()
        {
            if (ObjectManager.Player.CharacterName != "Yasuo")
            {
                return;
            }

            new MyBase.MyChampions();
        }
    }
}