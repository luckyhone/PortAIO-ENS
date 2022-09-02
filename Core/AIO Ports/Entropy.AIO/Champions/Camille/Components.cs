using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Camille
{
    public static class Components
    {
        public static class ComboMenu
        {
            public static readonly MenuBool QBool            = new MenuBool("comboQ", "Use Q");
            public static readonly MenuList QMode            = new MenuList("qMode", "Q Mode", new[] {"Always", "AA Reset"}, 1);
            public static readonly MenuList Q2Mode           = new MenuList("q2Mode", "Q2 Mode", new[] {"Instant", "AA Reset"});
            public static readonly MenuBool WBool            = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool            = new MenuBool("comboE", "Use E");
            public static readonly MenuBool EUnderTurretBool = new MenuBool("comboETurret", "Don't E under turret");
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("harassQ", "Use Q | If Mana >= x%", 50);
            public static readonly MenuList       QMode       = new MenuList("qMode", "Q Mode", new[] {"Always", "AA Reset"}, 1);
            public static readonly MenuList       Q2Mode      = new MenuList("q2Mode", "Q2 Mode", new[] {"Instant", "AA Reset"});
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("harassW", "Use W | If Mana >= x%", 50);
        }

        public static class KillstealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("ksQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("ksW", "Use W");
        }

        public static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("JungleClearQ", "Use Q | If Mana >= x%", 50);
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("LaneClearQ", "Use Q | If Mana >= x%", 50);
        }

        public static class StructureClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("structClearQ", "Use Q | If Mana >= x%", 50);
        }

        public static class MiscMenu
        {
            public static readonly MenuBool WFollowBool = new MenuBool("wFollow", "Auto Position W");
            public static readonly MenuBool WInEBool    = new MenuBool("wInE", "Use W Whilst Using E");
        }
    }
}