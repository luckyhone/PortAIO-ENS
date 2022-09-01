using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace HERMES_Kalista
{
    public class Program
    {
        #region Fields and Objects

        //public static MActivator Activator;
        //public static Or Orbwalker;

        #region Menu

        public static Menu MainMenu;
        public static Menu ComboMenu;
        public static Menu LaneClearMenu;
        public static Menu EscapeMenu;
        public static Menu ActivatorMenu;
        public static Menu DrawingsMenu;
        public static Menu SkinhackMenu;
        public static Menu OrbwalkerMenu;

        #endregion Menu

        #region Spells

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        #endregion Spells

        #endregion

        public static void Loads()
        {
            MyInitializer.HERMESLoader.Init();
        }
    }
}