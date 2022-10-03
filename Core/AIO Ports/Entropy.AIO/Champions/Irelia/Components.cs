using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Irelia
{
    public static class Components
    {
        public static class ComboMenu
        {
            //Q
            public static readonly MenuBool   QBool          = new MenuBool("comboQ", "Use Q in Combo");
            public static readonly MenuBool   QGap           = new MenuBool("gapQ", "Use Q for Gapclose on Minions");
            public static readonly MenuBool   jumpAround     = new MenuBool("jumparound", "Use Q to Jump-Around on Minions");
            public static readonly MenuBool   stackPassive   = new MenuBool("passiveStack", " ^- Only to stack passive");
            public static readonly MenuSlider stackMana      = new MenuSlider("manaStack", " ^- Stop if Mana <= X", 50);
            public static readonly MenuBool   priorityMarked = new MenuBool("prioMarked", "Priority Marked Enemies");

            public static readonly MenuKeyBind markedKey =
                new MenuKeyBind("markedKey", "Only if Marked Toggle", Keys.A, KeyBindType.Toggle);

            //W
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W in Combo", false);

            public static readonly MenuSlider wRelease =
                new MenuSlider("autoRelease", "Automatically release after (ms)", 100, 1, 1500);

            //E
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E in Combo");

            //R
            public static readonly MenuList RMode =
                new MenuList("rUsage", "R Usage: ", new[] {"At X Health", "Killable", "Never"});

            public static readonly MenuSlider hpR    = new MenuSlider("hpR", " ^- if <= X HP", 60);
            public static readonly MenuSlider wasteR = new MenuSlider("wasteR", "Don't waste R if Enemy HP <= X", 5);
            public static readonly MenuSlider forceR = new MenuSlider("forceR", "Force R if Hits X", 2, 0, 5);

            public static readonly MenuKeyBind
                semiR = new MenuKeyBind("semiR", "Semi-R", Keys.T, KeyBindType.Press);

            public static readonly MenuSlider RRange = new MenuSlider("RRange", "R Range", 850, 0, 850);

        }

        public static class HarassMenu
        {
            //Q
            public static readonly MenuBool QBool = new MenuBool("comboQ", "Use Q in Harass");
            public static readonly MenuBool QGap  = new MenuBool("gapQ", "Use Q for Gapclose on Minions");

            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W in Harass", false);

            //E
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E in Harass");
        }

        public static class KillstealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("ksQ", "Killsteal with Q");
            public static readonly MenuBool EBool = new MenuBool("ksE", "Killsteal with E");
        }

        public static class LaneClearMenu
        {
            public static readonly MenuKeyBind farmKey =
                new MenuKeyBind("farmKey", "Farm Toggle", Keys.Z, KeyBindType.Toggle);

            public static readonly MenuBool QBool   = new MenuBool("farmQ", "Use Q to Last Hit");
            public static readonly MenuBool QTurret = new MenuBool("turretQ", "Don't use Under-Turret");
            public static readonly MenuBool qAA     = new MenuBool("qaa", "Don't Q in Auto Attack Range", false);

            public static readonly MenuSlider qRange =
                new MenuSlider("qRange", "Don't Q Minion if Enemy is near it by X Range", 0, 0, 500);
        }

        public static class LastHitMenu
        {
            public static readonly MenuBool QBool   = new MenuBool("farmQ", "Use Q to Last Hit");
            public static readonly MenuBool QTurret = new MenuBool("turretQ", "Don't use Under-Turret");
            public static readonly MenuBool qAA     = new MenuBool("qaa", "Don't Q in Auto Attack Range", false);

            public static readonly MenuSlider qRange =
                new MenuSlider("qRange", "Don't Q Minion if Enemy is near it by X Range", 0, 0, 500);
        }

        public static class JungleClearMenu
        {
            public static readonly MenuBool QBool   = new MenuBool("farmQ", "Use Q in Jungle Clear");
            public static readonly MenuBool QMarked = new MenuBool("qMarked", " ^- Only if Marked");
            public static readonly MenuBool EBool   = new MenuBool("farmE", "Use E in Jungle Clear");
        }

        public static class InterrupterMenu
        {
            public static readonly MenuBool      EBool        = new MenuBool("enabled", "Enabled");
        }

        public static class Drawing
        {
            public static readonly MenuBool KillableMinion = new MenuBool("qMinions", "Killable Minions with Q");
        }

        public static class FleeMenu
        {
            public static readonly MenuBool QBool  = new MenuBool("enabled", "Use Q to FleeMenu");
            public static readonly MenuBool marked = new MenuBool("marked", " ^- Only if minion Killable / Marked");
        }
    }
}