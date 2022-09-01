using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;

namespace Entropy.AIO.Ahri
{
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
                Components.ComboMenu.QBool,
                Components.ComboMenu.WBool,
                Components.ComboMenu.EBool,
                Components.ComboMenu.RBool,
                Components.ComboMenu.RKillAbleBool,
                Components.ComboMenu.RActiveBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                Components.HarassMenu.QSliderBool,
                Components.HarassMenu.WSliderBool,
                Components.HarassMenu.ESliderBool
            };

            var killStealMenu = new Menu("killsteal", "KillSteal")
            {
                Components.KillstealMenu.QBool,
                Components.KillstealMenu.EBool
            };

            var LaneClearMenuMenu = new Menu("LaneClear", "Lane Clear")
            {
                Components.LaneClearMenu.QSliderBool,
                Components.LaneClearMenu.WSliderBool,

                new Menu("customization", "Customization")
                {
                    Components.LaneClearMenu.QCustomizationSlider,
                    Components.LaneClearMenu.WCustomizationSlider
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                Components.JungleClearMenu.QSliderBool,
                Components.JungleClearMenu.WSliderBool,
                Components.JungleClearMenu.ESliderBool
            };

            var EGapCloserMenu = new Menu("eGapClose", "E")
            {
                Components.GapCloserMenu.EBool,
                //new MenuSeparator(string.Empty,string.Empty)
            };

            EGapCloserMenu.Add(Components.GapCloserMenu.EGapcloser);

            MenuBase.Gapcloser = new Menu($"{ObjectManager.Player.CharacterName.ToLower()}.antigapcloser", "Anti-Gapcloser")
            {
                EGapCloserMenu
            };

            var drawingMenu = MenuBase.Drawings;
            {
                drawingMenu.Add(Bases.Components.DrawingMenu.QBool);
                drawingMenu.Add(Bases.Components.DrawingMenu.WBool);
                drawingMenu.Add(Bases.Components.DrawingMenu.EBool);

                drawingMenu.Add(new Menu("damage", "Draw Damages")
                {
                    Bases.Components.DrawingMenu.QDamageBool,
                    Bases.Components.DrawingMenu.WDamageBool,
                    Bases.Components.DrawingMenu.EDamageBool,
                    Bases.Components.DrawingMenu.RDamageBool,
                    Bases.Components.DrawingMenu.AutoDamageSliderBool
                });
            }

            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                LaneClearMenuMenu,
                jungleClearMenu
            };

            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }

            MenuBase.Root.Add(MenuBase.Gapcloser);
        }
    }
}