using System.Runtime.Serialization;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;
using ObjectManager = EnsoulSharp.ObjectManager;

namespace Entropy.AIO.Neeko
{
    using static ChampionBase;
    using static Bases.Components.DrawingMenu;
    using static Components;
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
                ComboMenu.QOnlyIfEnemyCCed,
                ComboMenu.EBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool,
                HarassMenu.QOnlyIfEnemyCCed
            };

            var killstealMenu = new Menu("killsteal", "Kill Steal")
            {
                KillstealMenu.QBool,
                KillstealMenu.EBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.ESliderBool,

                new Menu("customization", "Customization")
                {
                    LaneClearMenu.QMinionSlider,
                    LaneClearMenu.EMinionSlider
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.ESliderBool
            };

            var automaticMenu = new Menu("automatic", "Automatic Actions")
            {
                new Menu("e", "E Logics")
                {
                    AutomaticMenu.EImmobileBool
                },
                new Menu("r", "R Logics")
                {
                    AutomaticMenu.RTargetsNear,
                    AutomaticMenu.ROnlyInShape
                }
            };

            var EGapCloserMenu = new Menu("e", "E")
            {
                GapCloserMenu.EBool,
                new MenuSeparator("aaaaaasfasdfasdf"," - ")
            };

            MenuBase.Gapcloser = new Menu($"{ObjectManager.Player.CharacterName.ToLower()}.antigapcloser", "Anti-Gapcloser")
            {
                EGapCloserMenu
            };

            var miscellaneousMenu = new Menu("miscellaneous", "Miscellaneous")
            {
                MiscellaneousMenu.BlockAAInComboIfShape
            };

            var drawingMenu = MenuBase.Drawings;
            {
                drawingMenu.Add(QBool);
                drawingMenu.Add(EBool);
                drawingMenu.Add(RBool);

                drawingMenu.Add(new Menu("damage", "Draw Damages")
                {
                    QDamageBool,
                    EDamageBool,
                    RDamageBool,
                    AutoDamageSliderBool
                });
            }

            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killstealMenu,
                laneClearMenu,
                jungleClearMenu,
                automaticMenu,
                miscellaneousMenu
            };

            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }

            MenuBase.Root.Add(MenuBase.Gapcloser);
        }
    }
}