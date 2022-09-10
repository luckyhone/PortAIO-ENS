using EnsoulSharp.SDK.MenuUI;

namespace Perplexed_Gangplank
{
    public static class MenuManager
    {
        public static Menu Root;
        public static Menu Keys;
        public static Menu Combo;
        public static Menu W, CCTypes;
        public static Menu Harass;
        public static Menu LastHit;
        public static Menu Killsteal;
        public static Menu Misc;
        public static Menu Drawing;

        public static void Initialize()
        {
            //Root
            {
                Root = new Menu("Gangplank", "Perplexed Gangplank", true);
            }
            //Keys
            {
                Keys = new Menu("keys", "Keys")
                {
                    new MenuKeyBind("comboToMouse", "Combo To Mouse", EnsoulSharp.SDK.MenuUI.Keys.A, KeyBindType.Press),
                    new MenuKeyBind("qBarrel", "Q nearest barrel", EnsoulSharp.SDK.MenuUI.Keys.T, KeyBindType.Press)
                };
                Root.Add(Keys);
            }
            //Combo
            {
                Combo = new Menu("combo", "Combo")
                {
                    //new MenuBool("dontEIfKillsteal", "Don't use barrels if target can die to Q"),
                    new MenuBool("explodeQCooldown","Explode barrel with auto attack if Q is on cooldown")
                    //new MenuBool("qCantCombo", "Q enemy if can't barrel combo whatsoever")
                };
                Root.Add(Combo);
            }
            //W Settings
            {
                W = new Menu("wSettings", "W Settings")
                {
                    new MenuBool("wEnable","Enabled")
                };
                CCTypes = new Menu("ccTypes", "Crowd Control")
                {
                    new MenuBool("blind", "Blind"),
                    new MenuBool("charm", "Charm"),
                    new MenuBool("fear", "Fear"),
                    new MenuBool("flee", "Flee"),
                    new MenuBool("polymorph", "Polymorph"),
                    new MenuBool("silence", "Silence"),
                    new MenuBool("slow", "Slow"),
                    new MenuBool("snare", "Snare"),
                    new MenuBool("stun", "Stun"),
                    new MenuBool("suppression", "Suppression"),
                    new MenuBool("taunt", "Taunt"),
                };
                W.Add(CCTypes);
                Root.Add(W);
            }
            //Harass
            {
                Harass = new Menu("harass", "Harass")
                {
                    new MenuBool("harassQ", "Use Q"),
                    new MenuSlider("harassManaPct", "Mana % >=", 30, 1, 99)
                };
                Root.Add(Harass);
            }
            //LastHit
            {
                LastHit = new Menu("lasthit", "Last Hitting")
                {
                    new MenuBool("lasthitQ", "Use Q"),
                    new MenuSlider("lasthitManaPct", "Mana % >=", 30, 1, 99)
                };
                Root.Add(LastHit);
            }
            //Killsteal
            {
                Killsteal = new Menu("killsteal", "Killsteal")
                {
                    new MenuBool("killstealQ", "Killsteal with Q"),
                    new MenuBool("killstealR", "Killsteal with R"),
                    new MenuSlider("killstealRWaves", "R if # of waves can kill", 6, 1, 12)
                };
                Root.Add(Killsteal);
            }
            //Misc
            {
                Misc = new Menu("misc", "Misc")
                {
                    new MenuBool("aaBarrel", "Auto attack barrels to decay health")
                };
                Root.Add(Misc);
            }
            //Drawing
            {
                Drawing = new Menu("drawing", "Drawing")
                {
                    new MenuBool("drawQ", "Draw Q Range"),
                    new MenuBool("drawE", "Draw E Range"),
                    new MenuBool("drawBarrelExplode", "Draw Barrel Explosion Radius"),
                    new MenuBool("drawBarrelChain", "Draw Barrel Chain Radius")
                };
                Root.Add(Drawing);
            }
            //Finally
            {
                Root.Attach();
            }
        }
    }
}