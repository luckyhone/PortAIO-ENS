using EnsoulSharp;
using Challenger_Series.Utils.Plugins;

namespace Challenger_Series
{
    class Program
    {
        public static void Loads()
        {
            switch (ObjectManager.Player.CharacterName)
            {
                case "Soraka":
                    // Soraka();
                    break;
                case "Vayne":
                    //new Vayne();
                    break;
                case "Irelia":
                    //new Irelia();
                    break;
                case "Kalista":
                    new Kalista();
                    break;
                case "KogMaw":
                    //new KogMaw();
                    break;
                case "Ashe":
                    //new Ashe();
                    break;
                case "Caitlyn":
                    new Caitlyn();
                    break;
                case "Lucian":
                    //new Lucian();
                    break;
                case "Ezreal":
                    //new Ezreal();
                    break;
                default:
                    break;
            }
        }
    }
}