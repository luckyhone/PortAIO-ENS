using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;
using static Entropy.AIO.Gangplank.Components;
using static Entropy.AIO.Bases.Components.DrawingMenu;

namespace Entropy.AIO.Gangplank
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
                ComboMenu.EFirst,
                ComboMenu.EMax,
                ComboMenu.Triple,
                ComboMenu.ExtraTripleRange,
                ComboMenu.AADecayLevel,
                ComboMenu.ExplodeAA,
                ComboMenu.ComboToMouse,
                ComboMenu.ExplodeNearestBarrel
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool
            };


            var drawingMenu = MenuBase.Drawings;
            {
                drawingMenu.Add(QBool);
                drawingMenu.Add(WBool);
                drawingMenu.Add(EBool);
                drawingMenu.Add(RBool);

                drawingMenu.Add(new Menu("damage", "Draw Damages")
                {
                    QDamageBool,
                    WDamageBool,
                    EDamageBool,
                    RDamageBool,
                    AutoDamageSliderBool
                });
            }

            var menuList = new[]
            {
                comboMenu,
                harassMenu
            };

            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }
        }
    }
}