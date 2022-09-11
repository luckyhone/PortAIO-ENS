using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Darius.Misc
{
    public static class Components
    {
        public static class ComboMenu
        {
            public static readonly MenuBool QBool   = new MenuBool("comboQ", "Use Q in Combo");
            public static readonly MenuBool QAA     = new MenuBool("QAA", "Don't Q if in Auto Attack range", false);
            public static readonly MenuBool QChecks = new MenuBool("QChecks", "Don't cancel Auto Attacks with Q");

            public static readonly MenuKeyBind QLock =
                new MenuKeyBind("QLock", "Q Magnet", Keys.T, KeyBindType.Toggle);

            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W in Combo");
            public static readonly MenuBool WAA   = new MenuBool("WAA", " ^- Only for Auto Attack reset");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E in Combo");
            public static readonly MenuBool EAA   = new MenuBool("EAA", " ^- Only if out of Auto Attack range");
        }

        public static class HarassMenu
        {
            public static readonly MenuBool QBool   = new MenuBool("comboQ", "Use Q in Harass");
            public static readonly MenuBool QAA     = new MenuBool("QAA", "Don't Q if in Auto Attack range", false);
            public static readonly MenuBool QChecks = new MenuBool("QChecks", "Don't cancel Auto Attacks with Q");

            public static readonly MenuKeyBind QLock =
                new MenuKeyBind("QLock", "Q Outside Lock", Keys.T, KeyBindType.Toggle);

            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W in Harass");
            public static readonly MenuBool WAA   = new MenuBool("WAA", " ^- Only for Auto Attack reset");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E in Harass");
            public static readonly MenuBool EAA   = new MenuBool("EAA", " ^- Only if out of Auto Attack range");
        }

        public static class LaneClearMenu
        {
            public static readonly MenuKeyBind farmKey =
                new MenuKeyBind("farmKey", "Farm Toggle", Keys.Z, KeyBindType.Toggle);

            public static readonly MenuBool   QBool = new MenuBool("farmQ", "Use Q in Lane Clear", false);
            public static readonly MenuSlider qHit  = new MenuSlider("minQ", " ^- if Hits X Minions", 3, 0, 6);
            public static readonly MenuBool   WBool = new MenuBool("farmW", "Use W to Last Hit");
        }

        public static class LastHitMenu
        {
            public static readonly MenuBool WBool = new MenuBool("farmW", "Use W to Last Hit");
        }

        public static class KillstealMenu
        {
            public static readonly MenuBool KSR = new MenuBool("KSR", "Auto R on Killable");
        }

        public static class JungleClearMenu
        {
            public static readonly MenuBool QBool = new MenuBool("farmQ", "Use Q in Jungle Clear");
            public static readonly MenuBool WBool = new MenuBool("farmW", "Use W in Jungle Clear");
        }

        public static class GapCloserMenu
        {
            public static readonly MenuBool      EBool      = new MenuBool("enabled", "Enabled");
        }

        public static class InterrupterMenu
        {
            public static readonly MenuBool      EBool        = new MenuBool("enabled", "Enabled");
        }
    }
}