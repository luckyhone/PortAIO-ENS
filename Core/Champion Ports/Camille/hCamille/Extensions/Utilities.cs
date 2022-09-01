using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace hCamille.Extensions
{
    class Utilities
    {
        public static string[] HighChamps =
        {
            "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
            "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
            "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
            "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
            "Zed", "Ziggs","Kindred","Jhin"
        };

        public static string[] HitchanceNameArray = { "Low", "Medium", "High", "Very High", "Only Immobile" };
        public static HitChance[] HitchanceArray = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh, HitChance.Immobile };

        public static HitChance HikiChance(string menuName)
        {
            return HitchanceArray[Menus.Config[menuName].GetValue<MenuList>().Index];
        }

        public static bool Enabled(string menuName)
        {
            return Menus.Config[menuName].GetValue<MenuBool>().Enabled;
        }

        public static int Slider(string menuName)
        {
            return Menus.Config[menuName].GetValue<MenuSlider>().Value;
        }
    }
}