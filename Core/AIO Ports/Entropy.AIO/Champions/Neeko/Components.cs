using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Neeko
{
    public static class Components
    {
        public static class ComboMenu
        {
            public static readonly MenuBool QBool            = new MenuBool("q", "Use Q");
            public static readonly MenuBool QOnlyIfEnemyCCed = new MenuBool("qonlyifcc", "^ Only if Enemy is CC'd");

            public static readonly MenuBool EBool = new MenuBool("e", "Use E");

        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool      = new MenuSliderButton("q", "Use Q | If Mana >= x%", 50,0,100,false);
            public static readonly MenuBool       QOnlyIfEnemyCCed = new MenuBool("qonlyifcc", "^ Only if Enemy is CC'd");
        }

        public static class KillstealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("q", "Use Q");
            public static readonly MenuBool EBool = new MenuBool("e", "Use E", false);
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("q", "Use Q | If Mana >= x%", 50);
            public static readonly MenuSliderButton ESliderBool = new MenuSliderButton("e", "Use E | If Mana >= x%", 50);

            public static readonly MenuSlider QMinionSlider = new MenuSlider("q", "Use Q if can hit >= x minions", 3, 1, 5);
            public static readonly MenuSlider EMinionSlider = new MenuSlider("e", "Use E if can hit >= x minions", 3, 1, 5);
        }

        public static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("q", "Use Q | If Mana >= x%", 50);
            public static readonly MenuSliderButton ESliderBool = new MenuSliderButton("e", "Use E | If Mana >= x%", 50);
        }

        public static class AutomaticMenu
        {
            public static readonly MenuBool EImmobileBool = new MenuBool("eImmobile", "Cast E on CC'ed targets");

            public static readonly MenuSliderButton RTargetsNear =
                new MenuSliderButton("rHittable", "Automatically cast R if can Hit >= X targets", 3, 1, 6);

            public static readonly MenuBool ROnlyInShape = new MenuBool("rShape", "^ Only if In Shape");
        }

        public static class MiscellaneousMenu
        {
            public static readonly MenuBool BlockAAInComboIfShape = new MenuBool("blockAAShape", "Block AA in Combo if in Shape");
        }

        public static class GapCloserMenu
        {
            public static readonly MenuBool WBool = new MenuBool("enabled", "Enabled");
            public static readonly MenuBool EBool = new MenuBool("enabled", "Enabled");
        }
    }
}