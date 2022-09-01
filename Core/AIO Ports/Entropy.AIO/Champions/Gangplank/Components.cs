using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Gangplank
{
    public static class Components
    {
        public static class ComboMenu
        {
            public static readonly MenuBool   EFirst           = new MenuBool("eFirst", "Place first barrel", false);
            public static readonly MenuBool   EMax             = new MenuBool("eMax", "Always cast E at max range", false);
            public static readonly MenuBool   Triple           = new MenuBool("triple", "Triple barrel when available");
            public static readonly MenuSlider ExtraTripleRange = new MenuSlider("extraTriple", "Extra Triple Range", 20, 0, 100);

            public static readonly MenuSliderButton AADecayLevel =
                new MenuSliderButton("aaDecayLevel", "Decay barrel with AA | If Level < x", 13, 2, 19);

            public static readonly MenuBool ExplodeAA = new MenuBool("explodeAA", "Explode barrel with AA when Q on cooldown");

            public static readonly MenuKeyBind ComboToMouse =
                new MenuKeyBind("comboToMouse", "Combo To Mouse", Keys.A, KeyBindType.Press);

            public static readonly MenuKeyBind ExplodeNearestBarrel =
                new MenuKeyBind("explodeNearestBarrel", "Explode Nearest Barrel", Keys.T, KeyBindType.Press);
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("harassQ", "Use Q | If Mana >= x%", 50);

        }
    }
}