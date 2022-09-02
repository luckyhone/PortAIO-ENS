using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Aatrox
{
    using Bases;
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
                new MenuSeparator("1", " -- Q Settings --"),
                ComboMenu.QBool,
                new MenuSeparator("2", " -- W Settings --"),
                ComboMenu.WBool,
                new MenuSeparator("3", " -- E Settings --"),
                ComboMenu.EBool,
                ComboMenu.EQBool,
                new MenuSeparator("4", " -- R Settings --"),
                ComboMenu.RBool
            };
            var harassMenu = new Menu("harass", "Harass")
            {
                new MenuSeparator("1", " -- Q Settings --"),
                HarassMenu.QBool,
                new MenuSeparator("2", " -- W Settings --"),
                HarassMenu.WBool,
                HarassMenu.EQBool,
                new MenuSeparator("3", " -- E Settings --"),
                HarassMenu.EBool
            };
            var farmmenu = new Menu("farming", "Farming")
            {
                LaneClearMenu.farmKey,
                new MenuSeparator("1", " ~~~~ ")
            };
            var laneMenu = new Menu("laneMenu", "Lane Clear")
            {
                LaneClearMenu.QBool
            };
            var jungleMenu = new Menu("jungleMenu", "Jungle Clear")
            {
                JungleClearMenu.QBool,
                JungleClearMenu.WBool,
                JungleClearMenu.EBool
            };

            farmmenu.Add(laneMenu);
            farmmenu.Add(jungleMenu);


            var drawingMenu = MenuBase.Drawings;
            {
                drawingMenu.Add(QBool);
                drawingMenu.Add(WBool);
                drawingMenu.Add(EBool);
                drawingMenu.Add(new Menu("damage", "Draw Damages")
                {
                    QDamageBool,
                    WDamageBool
                });
            }
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                farmmenu
            };

           
            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }

            var WGapCloserMenu = new Menu("wGapClose", "W Settings")
            {
                GapCloserMenu.WBool,
            };


            MenuBase.Gapcloser = new Menu($"{ObjectManager.Player.CharacterName.ToLower()}.antigapcloser", "Anti-Gapcloser")
            {
                WGapCloserMenu
            };


            MenuBase.Root.Add(MenuBase.Gapcloser);
            LaneClearMenu.farmKey.AddPermashow();
        }
    }
}