using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace ADCCOMMON
{
    using System;

    internal class ItemsManager
    {
        private static Menu itemMenu;

        public static void AddToMenu(Menu mainMenu)
        {
            itemMenu = mainMenu;

            itemMenu.Add(new MenuBool("Youmuus", "Use Youmuu's Ghostblade", true).SetValue(true));

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                var target = TargetSelector.GetTarget(800, DamageType.Physical);

                if (target != null && target.IsHPBarRendered)
                {
                    if (itemMenu["Youmuus"].GetValue<MenuBool>().Enabled && Items.HasItem(ObjectManager.Player,ItemId.Youmuus_Ghostblade) &&
                        target.IsValidTarget(600 + 150))
                    {
                        Items.UseItem(ObjectManager.Player,(int)ItemId.Youmuus_Ghostblade);
                    }
                }
            }
        }
    }
}
