using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Cassiopeia
{
    using Bases;
    public static class Components
    {
        public static class ComboMenu
        {
            public static readonly MenuBool QBool       = new MenuBool("q", "Use Q in Combo", true);
            public static readonly MenuBool WBool       = new MenuBool("w", "Use W in Combo", true);
            public static readonly MenuBool WStart      = new MenuBool("wStart", "Start Combo with W", false);
            public static readonly MenuBool EBool       = new MenuBool("e", "Use E in Combo", true);
            public static readonly MenuBool EPoisonBool = new MenuBool("ep", " ^- Only if Poisoned", false);
            public static readonly MenuBool RylaisE     = new MenuBool("rylaisE", "Start Combo with E if have Rylai's", true);

            public static readonly MenuList RMode = new MenuList("rusage",
                                                                 "R Usage: ",
                                                                 new[] {"At X Health", "Killable", "Never"});

            public static readonly MenuSlider rRange = new MenuSlider("1v1range",
                                                                      "1v1 mode only if 1 Enemy in X Range",
                                                                      (int) ChampionBase.R.Range,
                                                                      (int) ChampionBase.R.Range - 200,
                                                                      1500);

            public static readonly MenuSlider hpR       = new MenuSlider("hpR", " ^- if <= X HP", 60);
            public static readonly MenuSlider wasteR    = new MenuSlider("wasteR", "Don't waste R if Enemy HP <= X", 5);
            public static readonly MenuBool   FaceR     = new MenuBool("facer", "Use R only if Facing", true);
            public static readonly MenuSlider teamFight = new MenuSlider("hitR", "Min. Enemies to Hit", 2, 2, 5);

            public static readonly MenuBool facingTeam = new MenuBool("facingR", " ^- Only count if Facing", true);

            //
            public static readonly MenuKeyBind rFlash =
                new MenuKeyBind("rFlash", "R-Flash Key", Keys.G, KeyBindType.Press);

            public static readonly MenuBool rFlashFace = new MenuBool("rFlashFace", " ^- Only if Facing", true);

            public static readonly MenuKeyBind semiR =
                new MenuKeyBind("semiR", "Semi-R Key", Keys.T, KeyBindType.Press);
            
        }

        public static class HarassMenu
        {
            public static readonly MenuBool QBool       = new MenuBool("q", "Use Q in Harass", true);
            public static readonly MenuBool WBool       = new MenuBool("w", "Use W in Harass", true);
            public static readonly MenuBool EBool       = new MenuBool("e", "Use E in Harass", true);
            public static readonly MenuBool EPoisonBool = new MenuBool("ep", " ^- Only if Poisoned", false);
            public static readonly MenuBool EBoolLast   = new MenuBool("farmE", "Use E to Last Hit", true);
        }

        public static class LaneClearMenu
        {
            public static readonly MenuKeyBind farmKey =
                new MenuKeyBind("farmKey", "Farm Toggle", Keys.Z, KeyBindType.Toggle, true);

            public static readonly MenuBool QBool = new MenuBool("farmQ", "Use Q in Lane Clear");

            public static readonly MenuSliderButton hitsQ =
                new MenuSliderButton("hitsQ", " ^- if Hits X Minions", 3, 1, 6);

            public static readonly MenuBool EBool       = new MenuBool("farmW", "Use E in Lane Clear");
            public static readonly MenuBool EPoisonBool = new MenuBool("ep", " ^- Only if Poisoned", false);
            public static readonly MenuBool EAA         = new MenuBool("disableAA", "Disable Auto Attacks", false);
            public static readonly MenuBool EPassive    = new MenuBool("passiveE", "Use E to Last Hit");
        }

        public static class LastHitMenu
        {
            public static readonly MenuBool EBool = new MenuBool("farmE", "Use E to Last Hit", true);
        }

        public static class JungleClearMenu
        {
            public static readonly MenuBool QBool = new MenuBool("farmQ", "Use Q in Jungle Clear");
            public static readonly MenuBool EBool = new MenuBool("farmE", "Use E in Jungle Clear");
        }

        public static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("q", "Use Q to Killsteal");
            public static readonly MenuBool EBool = new MenuBool("e", "Use E to Killsteal");
        }

        public static class GapCloserMenu
        {
            public static readonly MenuBool      RBool      = new MenuBool("enabled", "Enabled");
            public static readonly MenuSlider    hpR        = new MenuSlider("hpR", " ^- if <= X HP", 60);
        }

        public static class MiscMenu
        {
            public static readonly MenuBool AutoQ = new MenuBool("dashQ", "Auto Q on Dash");

            public static readonly MenuSlider MaxR = new MenuSlider("MaxR", "Max R Range", 750, 500, 850);
        }

        public static class InterrupterMenu
        {
            public static readonly MenuBool RBool = new MenuBool("enabled", "Enabled");

        }
    }
}