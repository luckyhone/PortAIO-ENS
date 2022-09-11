using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace HikiCarry.Core.Activator
{
    internal class Youmuu
    {
        internal static ItemData Youmuus => ItemData.GetIData(ItemId.Youmuus_Ghostblade);
        public Youmuu()
        {
            Console.WriteLine("HikiCarry: Youmuu Initalized");
            Orbwalker.OnAfterAttack += YoumuuAfterAttack;
        }
        

        private void YoumuuAfterAttack(object sender, AfterAttackEventArgs e)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo &&
                Initializer.Activator["youmuu"].GetValue<MenuBool>().Enabled && ObjectManager.Player.HasItem(Youmuus.Id) && ObjectManager.Player.CanUseItem(Youmuus.Id))
            {
                ObjectManager.Player.UseItem((int)Youmuus.Id);
            }
        }
    }
}