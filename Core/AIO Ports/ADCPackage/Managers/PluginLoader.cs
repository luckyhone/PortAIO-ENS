using ADCPackage.Plugins;
using EnsoulSharp;

namespace ADCPackage
{
    class PluginLoader
    {
        public static string Champname = ObjectManager.Player.CharacterName;

        public static void Load()
        {
            switch (Champname)
            {
                case "Jinx":
                    Jinx.Load();
                    return;
                case "Tristana":
                    //Tristana.Load();
                    return;
                case "Corki":
                    //Corki.Load();
                    return;
            }
            Game.Print("[<font color='#F8F46D'>ADC Package</font>] by <font color='#79BAEC'>God</font> - <font color='#FFFFFF'>" + Champname + " not supported</font>");
        }
    }
}