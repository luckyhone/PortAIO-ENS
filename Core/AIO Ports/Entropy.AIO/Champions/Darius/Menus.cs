using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Darius.Misc;
using Entropy.Awareness.Bases;

namespace Entropy.AIO.Darius
{
    using static Components;
    using static Bases.Components.DrawingMenu;
    using Bases;

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
                ComboMenu.QAA,
                ComboMenu.QChecks,
                new MenuSeparator("2", " -- W Settings --"),
                ComboMenu.WBool,
                ComboMenu.WAA,
                new MenuSeparator("3", " -- E Settings --"),
                ComboMenu.EBool,
                ComboMenu.EAA
            };
            var harassMenu = new Menu("harass", "Harass")
            {
                new MenuSeparator("1", " -- Q Settings --"),
                HarassMenu.QBool,
                HarassMenu.QAA,
                HarassMenu.QChecks,
                HarassMenu.QLock,
                new MenuSeparator("2", " -- W Settings --"),
                HarassMenu.WBool,
                HarassMenu.WAA,
                new MenuSeparator("3", " -- E Settings --"),
                HarassMenu.EBool,
                HarassMenu.EAA
            };
            var ksMenu = new Menu("killsteal", "Killsteal")
            {
                KillstealMenu.KSR
            };
            var farmmenu = new Menu("farming", "Farming")
            {
                LaneClearMenu.farmKey,
                new MenuSeparator("1", " ~~~~ ")
            };
            var laneMenu = new Menu("laneMenu", "Lane Clear")
            {
                LaneClearMenu.QBool,
                LaneClearMenu.qHit,
                LaneClearMenu.WBool
            };
            var lastMenu = new Menu("lastMenu", "Last Hit")
            {
                LastHitMenu.WBool
            };
            var jungleMenu = new Menu("jungleMenu", "Jungle Clear")
            {
                JungleClearMenu.QBool,
                JungleClearMenu.WBool
            };

            farmmenu.Add(laneMenu);
            farmmenu.Add(jungleMenu);
            farmmenu.Add(lastMenu);

            var drawingMenu = MenuBase.Drawings;
            {
                drawingMenu.Add(QBool);
                drawingMenu.Add(EBool);
                drawingMenu.Add(RBool);
                drawingMenu.Add(new Menu("damage", "Draw Damages")
                {
                    RDamageBool
                });
            }
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                farmmenu,
                ksMenu
            };

            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }

            var EGapCloserMenu = new Menu("rGapClose", "E Settings")
            {
                GapCloserMenu.EBool,
            };


            MenuBase.Gapcloser = new Menu($"{ObjectManager.Player.CharacterName.ToLower()}.antigapcloser", "Anti-Gapcloser")
            {
                EGapCloserMenu
            };

            var QInterrupterMenu = new Menu("e", "E Settings")
            {
                InterrupterMenu.EBool,
            };


            MenuBase.Interrupter = new Menu($"{ObjectManager.Player.CharacterName.ToLower()}.interrupter", "Interrupter")
            {
                QInterrupterMenu
            };


            MenuBase.Root.Add(MenuBase.Gapcloser);
            MenuBase.Root.Add(MenuBase.Interrupter);
            LaneClearMenu.farmKey.Permashow();
            MenuBase.Champion.Add(new MenuSeparator("--", " ~~~~ "));
            MenuBase.Champion.Add(ComboMenu.QLock);
            ComboMenu.QLock.Permashow();
        }
    }
}