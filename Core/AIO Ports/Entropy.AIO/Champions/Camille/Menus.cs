using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Camille
{
    #region

    using Bases;
    using static Components;
    using static Bases.Components.DrawingMenu;

    #endregion

    class Menus
    {
        public Menus()
        {
            Initialize();
        }

        private void Initialize()
        {
            var comboMenu = new Menu("combo", "Combo")
            {
                ComboMenu.QBool,
                ComboMenu.QMode,
                ComboMenu.Q2Mode,
                ComboMenu.WBool,
                ComboMenu.EBool,
                ComboMenu.EUnderTurretBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool,
                HarassMenu.QMode,
                HarassMenu.Q2Mode,
                HarassMenu.WSliderBool
            };

            var killStealMenu = new Menu("killsteal", "KillSteal")
            {
                KillstealMenu.QBool,
                KillstealMenu.WBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool
            };

            var structureClearMenu = new Menu("structureclear", "Structure Clear")
            {
                StructureClearMenu.QSliderBool
            };

            var miscMenu = new Menu("misc", "Misc")
            {
                MiscMenu.WFollowBool,
                MiscMenu.WInEBool
            };

            var drawingMenu = MenuBase.Drawings;
            {
                drawingMenu.Add(QBool);
                drawingMenu.Add(WBool);
                drawingMenu.Add(EBool);

                drawingMenu.Add(new Menu("damage", "Draw Damages")
                {
                    QDamageBool,
                    WDamageBool,
                    AutoDamageSliderBool
                });
            }

            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                structureClearMenu,
                miscMenu
            };

            foreach (var menu in menuList)
            {
                MenuBase.Champion.Add(menu);
            }
        }
    }
}