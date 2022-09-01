using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.LeBlanc
{
    public static class Components
    {
        public static class ComboMenu
        {
            public static readonly MenuBool QBool       = new MenuBool("q", "Use Q");
            public static readonly MenuBool WBool       = new MenuBool("w", "Use W1", false);
            public static readonly MenuBool WGCBool     = new MenuBool("wgapclose", "^ To Gapclose");
            public static readonly MenuBool WReturnBool = new MenuBool("wreturn", "^ W2 If All Spells On CD");
            public static readonly MenuBool EBool       = new MenuBool("e", "Use E");
            public static readonly MenuBool RBool       = new MenuBool("r", "Use R");
            public static readonly MenuList RModelist   = new MenuList("rmode", "R Mode", new[] {"RQ", "RE"});

            public static readonly MenuKeyBind RModeKey =
                new MenuKeyBind("rswitch", "Switch R Mode", Keys.G, KeyBindType.Press);
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("harassQ", "Use Q | If Mana >= x%", 50);
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("harassW", "Use W | If Mana >= x%", 50);
            public static readonly MenuSliderButton ESliderBool = new MenuSliderButton("harassE", "Use E | If Mana >= x%", 50);

        }

        public static class KillstealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killstealQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("killstealW", "Use W", false);
            public static readonly MenuBool EBool = new MenuBool("killstealE", "Use E");
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("laneClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSlider WCountSliderBool =
                new MenuSlider("laneClearWCount", "Use W if hittable minions >= x", 3, 1, 5);
        }

        public static class JungleClearMenu
        {
            public static readonly MenuBool QBool = new MenuBool("jungleClearQ", "Use Q");
        }

        public static class DrawsMenu
        {
            public static readonly MenuBool RootDanage = new MenuBool("e2", "^ Include Root Damage");
            public static readonly MenuBool RModeBool  = new MenuBool("rmode", "Draw R Mode and State");
        }
    }
}