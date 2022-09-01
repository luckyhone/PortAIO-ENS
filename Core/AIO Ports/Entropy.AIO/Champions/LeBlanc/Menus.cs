using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;

namespace Entropy.AIO.LeBlanc
{
    using static Components;
    using static Bases.Components.DrawingMenu;
    class Menus
    {
        public Menus()
        {
            Initialize();
        }

        private static void Initialize()
        {
            var comboMenu = new Menu("combo", "Combo")
            {
                ComboMenu.QBool,
                ComboMenu.WBool,
                ComboMenu.WGCBool,
                ComboMenu.WReturnBool,
                ComboMenu.EBool,
                ComboMenu.RBool,
                ComboMenu.RModelist,
                ComboMenu.RModeKey
            };
            ComboMenu.RModelist.AddPermashow();
            ComboMenu.RModeKey.AddPermashow();

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool,
                HarassMenu.WSliderBool,
                HarassMenu.ESliderBool
            };

            var killStealMenu = new Menu("killsteal", "KillSteal")
            {
                KillstealMenu.QBool,
                KillstealMenu.WBool,
                KillstealMenu.EBool
            };

            var LaneclearMenu = new Menu("Laneclear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.WSliderBool,
                LaneClearMenu.WCountSliderBool
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QBool
            };

            var drawingsMenu = MenuBase.Drawings;
            {
                drawingsMenu.Add(QBool);
                drawingsMenu.Add(WBool);
                drawingsMenu.Add(EBool);
                drawingsMenu.Add(RBool);

                drawingsMenu.Add(new Menu("damage", "Draw Damages")
                {
                    QDamageBool,
                    WDamageBool,
                    EDamageBool,
                    RDamageBool,
                    AutoDamageSliderBool
                });

                drawingsMenu.Add(DrawsMenu.RootDanage);
                drawingsMenu.Add(DrawsMenu.RModeBool);
            }

            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                LaneclearMenu,
                jungleClearMenu
            };

            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }
        }
    }
}