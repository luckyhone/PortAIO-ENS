using System.Runtime.Serialization;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;
using ObjectManager = EnsoulSharp.ObjectManager;

namespace Entropy.AIO.Irelia
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
            var comboMenu = new Menu("combo", "Combo");
            var QSet = new Menu("QSet", "Q Settings")
            {
                ComboMenu.QBool,
                ComboMenu.QGap,
                ComboMenu.jumpAround,
                ComboMenu.stackPassive,
                ComboMenu.stackMana,
                ComboMenu.priorityMarked,
                new MenuSeparator("--", " ---- "),
                ComboMenu.markedKey
            };
            var WSet = new Menu("WSet", "W Settings")
            {
                ComboMenu.WBool,
                ComboMenu.wRelease
            };
            var ESet = new Menu("ESet", "E Settings")
            {
                ComboMenu.EBool
            };
            var RSet = new Menu("RSet", "R Settings")
            {
                ComboMenu.RMode,
                ComboMenu.hpR,
                ComboMenu.wasteR,
                ComboMenu.forceR,
                new MenuSeparator("--", " ---- "),
                ComboMenu.semiR,
                ComboMenu.RRange
            };
            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QBool,
                HarassMenu.QGap,
                HarassMenu.WBool,
                HarassMenu.EBool
            };
            comboMenu.Add(QSet);
            comboMenu.Add(WSet);
            comboMenu.Add(ESet);
            comboMenu.Add(RSet);

            var farmmenu = new Menu("farming", "Farming")
            {
                LaneClearMenu.farmKey,
                new MenuSeparator("1", " ~~~~ ")
            };
            var laneMenu = new Menu("laneMenu", "Lane Clear")
            {
                LaneClearMenu.QBool,
                LaneClearMenu.QTurret,
                LaneClearMenu.qAA,
                LaneClearMenu.qRange
            };
            var jungleMenu = new Menu("jungleMenu", "Jungle Clear")
            {
                JungleClearMenu.QBool,
                JungleClearMenu.QMarked,
                JungleClearMenu.EBool
            };
            var lastMenu = new Menu("lastMenu", "Last Hit")
            {
                LastHitMenu.QBool,
                LastHitMenu.QTurret,
                LastHitMenu.qAA,
                LastHitMenu.qRange
            };
            farmmenu.Add(laneMenu);
            farmmenu.Add(jungleMenu);
            //farmmenu.Add(lastMenu);
            var killStealMenu = new Menu("killsteal", "Killsteal")
            {
                KillstealMenu.QBool,
                KillstealMenu.EBool
            };
            var fleeMenu = new Menu("flee", "Flee")
            {
                FleeMenu.QBool,
                FleeMenu.marked
            };
            var RInterrupterMenu = new Menu("q", "E Settings")
            {
                InterrupterMenu.EBool,
                new MenuSeparator("sfsdfsdf"," ")
            };


            MenuBase.Interrupter = new Menu($"{ObjectManager.Player.CharacterName}.interrupter", "Interrupter")
            {
                RInterrupterMenu
            };

            var drawingsMenu = MenuBase.Drawings;
            {
                drawingsMenu.Add(Drawing.KillableMinion);
                drawingsMenu.Add(QBool);
                drawingsMenu.Add(WBool);
                drawingsMenu.Add(EBool);
                drawingsMenu.Add(RBool);

                drawingsMenu.Add(new Menu("damage", "Draw Damages")
                {
                    QDamageBool,
                    WDamageBool,
                    EDamageBool,
                    RDamageBool
                });
            }

            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                farmmenu,
                killStealMenu,
                fleeMenu
            };
            MenuBase.Root.Add(MenuBase.Interrupter);
            //ComboMenu.markedKey.Permashow();
            ComboMenu.semiR.AddPermashow();
            LaneClearMenu.farmKey.AddPermashow();
            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }
        }
    }
}