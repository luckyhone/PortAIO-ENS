using System;
using EnsoulSharp.SDK.MenuUI;

namespace ElSinged
{
    public class ElSingedMenu
    {
        public static Menu Menu;

        public static void Initialize()
        {
            Menu = new Menu("menu","ElSinged", true);

            var cMenu = new Menu("Combo", "Combo");
            {
                cMenu.Add(new MenuBool("ElSinged.Combo.Q", "Use Q").SetValue(true));
                cMenu.Add(new MenuBool("ElSinged.Combo.W", "Use W").SetValue(true));
                cMenu.Add(new MenuBool("ElSinged.Combo.E", "Use E").SetValue(true));
                cMenu.Add(new MenuBool("ElSinged.Combo.R", "Use R").SetValue(true));
                cMenu.Add(new MenuSeparator("ElSinged.Coffasfsafsambo.R", " - "));
                cMenu.Add(new MenuSlider("ElSinged.Combo.R.Count", "Use R enemies >= ",2, 1, 5));
                cMenu.Add(new MenuBool("ElSinged.Combo.Ignite", "Use Ignite").SetValue(true));
                cMenu.Add(new MenuKeyBind("ComboActive", "Combo!",Keys.Space, KeyBindType.Press));
            }
            Menu.Add(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            {
                hMenu.Add(new MenuBool("ElSinged.Harass.Q", "Use Q").SetValue(true));
                hMenu.Add(new MenuBool("ElSinged.Harass.W", "Use W").SetValue(true));
                hMenu.Add(new MenuBool("ElSinged.Harass.E", "Use E").SetValue(true));
            }
            Menu.Add(hMenu);

            var lcMenu = new Menu("Laneclear", "Laneclear");
            {
                lcMenu.Add(new MenuBool("ElSinged.Laneclear.Q", "Use Q").SetValue(true));
                lcMenu.Add(new MenuBool("ElSinged.Laneclear.E", "Use E").SetValue(true));
            }
            Menu.Add(lcMenu);

            //ElSinged.Misc
            var miscMenu = new Menu("Drawings", "Misc");
            {
                miscMenu.Add(new MenuBool("ElSinged.Draw.off", "Turn drawings off").SetValue(false));
                miscMenu.Add(new MenuBool("ElSinged.Draw.Q", "Draw Q").SetValue(true));
                miscMenu.Add(new MenuBool("ElSinged.Draw.W", "Draw W").SetValue(true));
                miscMenu.Add(new MenuBool("ElSinged.Draw.E", "Draw E").SetValue(true));
            }
            Menu.Add(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            {
                credits.Add(new MenuSeparator("ElSinged.Paypal", "if you would like to donate via paypal:"));
                credits.Add(new MenuSeparator("ElSinged.Email", "info@zavox.nl"));
            }
            Menu.Add(credits);

            Menu.Add(new MenuSeparator("422442fsaafs4242f", " - "));
            Menu.Add(new MenuSeparator("422442fsaafsf", "Version: 1.0.0.4"));
            Menu.Add(new MenuSeparator("fsasfafsfsafsa", "Made By jQuery"));

            Menu.Attach();

            Console.WriteLine("Menu Loaded");
        }
    }
}