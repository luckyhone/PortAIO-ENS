using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Ahri
{
    public static class Components
    {
        public static class ComboMenu
        {
            public static readonly MenuBool QBool         = new MenuBool("q", "Use Q");
            public static readonly MenuBool WBool         = new MenuBool("w", "Use W");
            public static readonly MenuBool EBool         = new MenuBool("e", "Use E");
            public static readonly MenuBool RBool         = new MenuBool("r", "Use R");
            public static readonly MenuBool RKillAbleBool = new MenuBool("rks", "^ If target Can KillAble");
            public static readonly MenuBool RActiveBool   = new MenuBool("rkeep", "^ Keep R Active");
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("q", "Use Q | If Mana >= x%", 50);
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("w", "Use W | If Mana >= x%", 50);
            public static readonly MenuSliderButton ESliderBool = new MenuSliderButton("e", "Use E | If Mana >= x%", 50);
        }

        public static class KillstealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("q", "Use Q");
            public static readonly MenuBool EBool = new MenuBool("e", "Use E");
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("q", "Use Q | If Mana >= x%", 50);
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("w", "Use W | If Mana >= x%", 50);

            public static readonly MenuSlider QCustomizationSlider =
                new MenuSlider("q", "Use Q if minions hittable >= x", 3, 1, 5);

            public static readonly MenuSlider WCustomizationSlider =
                new MenuSlider("w", "Use W if minions in range >= x", 3, 1, 5);
        }

        public static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("q", "Use Q | If Mana >= x%", 30);
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("w", "Use W | If Mana >= x%", 30);
            public static readonly MenuSliderButton ESliderBool = new MenuSliderButton("e", "Use E | If Mana >= x%", 30);
        }

        public static class GapCloserMenu
        {
            public static readonly MenuBool      EBool      = new MenuBool("enabled", "Enabled");
            public static readonly MenuBool EGapcloser = new MenuBool("E", "Use E Gapcloser");
        }
    }
}