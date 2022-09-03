using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Cassiopeia
{
    using static Components;
    using static Bases.Components;
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
                new MenuSeparator("1", " -- Q Settings -- "),
                ComboMenu.QBool,
                new MenuSeparator("2", " -- W Settings -- "),
                ComboMenu.WBool,
                ComboMenu.WStart,
                new MenuSeparator("3", " -- E Settings -- "),
                ComboMenu.EBool,
                ComboMenu.EPoisonBool,
                ComboMenu.RylaisE,
                new MenuSeparator("4", " -- R Settings -- ")
            };

            var r1v1 = new Menu("r1v1", "1v1 Settings")
            {
                ComboMenu.RMode,
                ComboMenu.hpR,
                ComboMenu.wasteR,
                ComboMenu.FaceR,
                ComboMenu.rRange
            };

            var rtf = new Menu("rtf", "Teamfight settings")
            {
                ComboMenu.teamFight,
                ComboMenu.facingTeam
            };
            comboMenu.Add(r1v1);
            comboMenu.Add(rtf);

            var harassMenu = new Menu("harass", "Harass")
            {
                new MenuSeparator("1", " -- Q Settings -- "),
                HarassMenu.QBool,
                new MenuSeparator("2", " -- W Settings -- "),
                HarassMenu.WBool,
                new MenuSeparator("3", " -- E Settings -- "),
                HarassMenu.EBool,
                HarassMenu.EPoisonBool,
                new MenuSeparator("--", " ~~~~ "),
                HarassMenu.EBoolLast
            };

            var killStealMenu = new Menu("killsteal", "Killsteal")
            {
                KillStealMenu.QBool,
                KillStealMenu.EBool
            };
            var farmmenu = new Menu("farming", "Farming");

            var laneMenu = new Menu("laneMenu", "Lane Clear")
            {
                LaneClearMenu.farmKey,
                new MenuSeparator("1", " ~~~~ ")
            };
            var pushMenu = new Menu("pushMenu", "Pushing")
            {
                LaneClearMenu.QBool,
                LaneClearMenu.hitsQ,
                LaneClearMenu.EBool,
                LaneClearMenu.EPoisonBool,
                LaneClearMenu.EAA
            };
            var passiveMenu = new Menu("passiveMenu", "Passive")
            {
                LaneClearMenu.EPassive
            };
            var jungleMenu = new Menu("jungleMenu", "Jungle Clear")
            {
                JungleClearMenu.QBool,
                JungleClearMenu.EBool
            };
            laneMenu.Add(pushMenu);
            laneMenu.Add(passiveMenu);
            var lastMenu = new Menu("lastMenu", "Last Hit")
            {
                LastHitMenu.EBool
            };
            farmmenu.Add(laneMenu);
            farmmenu.Add(jungleMenu);
            farmmenu.Add(lastMenu);

            var RGapCloserMenu = new Menu("RGapClose", "R Settings")
            {
                GapCloserMenu.RBool,
                GapCloserMenu.hpR,
                new MenuSeparator("adfadfasdf"," - ")
            };

            MenuBase.Gapcloser = new Menu($"{ObjectManager.Player.CharacterName.ToLower()}.antigapcloser", "Anti-Gapcloser")
            {
                RGapCloserMenu
            };

            var drawingMenu = MenuBase.Drawings;
            {
                drawingMenu.Add(DrawingMenu.QBool);
                drawingMenu.Add(DrawingMenu.WBool);
                drawingMenu.Add(DrawingMenu.EBool);
                drawingMenu.Add(DrawingMenu.RBool);

                drawingMenu.Add(new Menu("damage", "Draw Damages")
                {
                    DrawingMenu.QDamageBool,
                    DrawingMenu.EDamageBool,
                    DrawingMenu.RDamageBool
                });
            }

            var miscMenu = new Menu("misc", "Misc.")
            {
                MiscMenu.AutoQ,
                MiscMenu.MaxR
            };
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                farmmenu,
                killStealMenu,
                miscMenu
            };

            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }


            var RInterrupterMenu = new Menu("q", "R Settings")
            {
                InterrupterMenu.RBool,
                new MenuSeparator("adsfkauhfdkajh1"," - ")
            };



            MenuBase.Interrupter = new Menu($"{ObjectManager.Player.CharacterName.ToLower()}.interrupter", "Interrupter")
            {
                RInterrupterMenu
            };


            LaneClearMenu.farmKey.AddPermashow();
            ComboMenu.semiR.AddPermashow();
            ComboMenu.rFlash.AddPermashow();
            MenuBase.Champion.Add(new MenuSeparator("--", " ~~~~ "));
            MenuBase.Champion.Add(ComboMenu.rFlash);
            MenuBase.Champion.Add(ComboMenu.rFlashFace);
            MenuBase.Champion.Add(ComboMenu.semiR);
            MenuBase.Root.Add(MenuBase.Gapcloser);
            MenuBase.Root.Add(MenuBase.Interrupter);
        }
    }
}