using System;
using EnsoulSharp.SDK.MenuUI;

namespace ElVarus
{
    public class ElVarusMenu
    {
        #region Static Fields

        public static Menu Menu;

        #endregion
        
        #region Public Methods and Operators

        public static void Initialize()
        {
            Menu = new Menu("menu", "ElVarus", true);
            
            var cMenu = new Menu("Combo", "Combo");

            cMenu.Add(new MenuBool("ElVarus.Combo.Q", "Use Q").SetValue(true));
            cMenu.Add(new MenuBool("ElVarus.combo.always.Q", "always Q").SetValue(false));
            cMenu.Add(new MenuBool("ElVarus.Combo.E", "Use E").SetValue(true));
            cMenu.Add(new MenuBool("ElVarus.Combo.R", "Use R").SetValue(true));
            cMenu.Add(new MenuBool("ElVarus.Combo.W.Focus", "Focus W target").SetValue(false));
            cMenu.Add(new MenuSeparator("ElVarus.sssss", " "));
            cMenu.Add(new MenuSlider("ElVarus.Combo.R.Count", "R when enemies >= ",1, 1, 5));
            cMenu.Add(new MenuSlider("ElVarus.Combo.Stack.Count", "Q when stacks >= ",3, 1, 3));
            cMenu.Add(new MenuSeparator("ElVarus.sssssssss", " "));
            cMenu.Add(
                new MenuKeyBind("ElVarus.SemiR", "Semi-manual cast R key",Keys.T, KeyBindType.Press));

            cMenu.Add(new MenuSeparator("ElVarus.ssssssssssss", " "));
            cMenu.Add(new MenuKeyBind("ComboActive", "Combo!",Keys.Space, KeyBindType.Press));
            Menu.Add(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.Add(new MenuBool("ElVarus.Harass.Q", "Use Q").SetValue(true));
            hMenu.Add(new MenuBool("ElVarus.Harass.E", "Use E").SetValue(true));
            hMenu.Add(new MenuSeparator("ElVarus.Harasssfsass.E", " "));
            hMenu.Add(new MenuSlider("minmanaharass", "Mana needed to clear ",55));

            Menu.Add(hMenu);

            var itemMenu = new Menu("Items", "Items");
            itemMenu.Add(new MenuBool("ElVarus.Items.Youmuu", "Use Youmuu's Ghostblade").SetValue(true));
            Menu.Add(itemMenu);

            var lMenu = new Menu("Clear", "Clear");
            lMenu.Add(new MenuBool("useQFarm", "Use Q").SetValue(true));
            lMenu.Add(
                new MenuSlider("ElVarus.Count.Minions", "Killable minions with Q >=",2, 1, 5));
            lMenu.Add(new MenuBool("useEFarm", "Use E").SetValue(true));
            lMenu.Add(
                new MenuSlider("ElVarus.Count.Minions.E", "Killable minions with E >=",2, 1, 5));
            lMenu.Add(new MenuBool("useEFarmddsddaadsd", " "));
            lMenu.Add(new MenuBool("useQFarmJungle", "Use Q in jungle").SetValue(true));
            lMenu.Add(new MenuBool("useEFarmJungle", "Use E in jungle").SetValue(true));
            lMenu.Add(new MenuSeparator("useEFarmddssd", " "));
            lMenu.Add(new MenuSlider("minmanaclear", "Mana needed to clear ",55));

            Menu.Add(lMenu);

            //ElSinged.Misc
            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.Add(new MenuBool("ElVarus.Draw.off", "Turn drawings off").SetValue(true));
            miscMenu.Add(new MenuBool("ElVarus.Draw.Q", "Draw Q"));
            miscMenu.Add(new MenuBool("ElVarus.Draw.W", "Draw W"));
            miscMenu.Add(new MenuBool("ElVarus.Draw.E", "Draw E"));

            miscMenu.Add(new MenuBool("ElVarus.KSSS", "Killsteal").SetValue(true));

            Menu.Add(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.Add(new MenuSeparator("ElSinged.Paypal", "if you would like to donate via paypal:"));
            credits.Add(new MenuSeparator("ElSinged.Email", "info@zavox.nl"));
            Menu.Add(credits);

            Menu.Add(new MenuSeparator("422442fsaafs4242f", " "));
            Menu.Add(new MenuSeparator("422442fsaafsf", "Version: 1.0.2.2"));
            Menu.Add(new MenuSeparator("fsasfafsfsafsa", "Made By jQuery"));

            Menu.Attach();

            Console.WriteLine("Menu Loaded");
        }
        #endregion
    }
}