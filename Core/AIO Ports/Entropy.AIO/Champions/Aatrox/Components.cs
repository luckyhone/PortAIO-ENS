using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Aatrox
{

    public static class Components
    {
        public static class ComboMenu
        {
            public static readonly MenuBool       QBool  = new MenuBool("comboQ", "Use Q in Combo");
            public static readonly MenuBool       WBool  = new MenuBool("comboW", "Use W in Combo");
            public static readonly MenuBool       EBool  = new MenuBool("comboE", "Use E in Combo");
            public static readonly MenuBool       EQBool = new MenuBool("comboEQ", "Dash backwards for Q");
            public static readonly MenuSliderButton RBool  = new MenuSliderButton("comboR", "Use R if Player <= X Health", 40);
        }

        public static class HarassMenu
        {
            public static readonly MenuBool QBool  = new MenuBool("comboQ", "Use Q in Harass");
            public static readonly MenuBool WBool  = new MenuBool("comboW", "Use W in Harass");
            public static readonly MenuBool EBool  = new MenuBool("comboE", "Use E in Harass");
            public static readonly MenuBool EQBool = new MenuBool("comboEQ", "Dash backwards for Q");
        }

        public static class LaneClearMenu
        {
            public static readonly MenuKeyBind farmKey =
                new MenuKeyBind("farmKey", "Farm Toggle", Keys.Z, KeyBindType.Toggle);

            public static readonly MenuBool QBool = new MenuBool("farmQ", "Use Q in Lane Clear");
        }

        public static class JungleClearMenu
        {
            public static readonly MenuBool QBool = new MenuBool("farmQ", "Use Q in Jungle Clear");
            public static readonly MenuBool WBool = new MenuBool("farmW", "Use W in Jungle Clear");
            public static readonly MenuBool EBool = new MenuBool("farmE", "Use E in Jungle Clear");
        }

        public static class GapCloserMenu
        {
            public static readonly MenuBool      WBool      = new MenuBool("enabled", "Enabled");
        }
    }
}