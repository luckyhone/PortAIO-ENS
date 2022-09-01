using System;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace KoreanZed
{
    class ZedPotions
    {
        private readonly ZedMenu zedMenu;

        public ZedPotions(ZedMenu zedMenu)
        {
            this.zedMenu = zedMenu;

            Game.OnUpdate += Game_OnUpdate;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (zedMenu.GetPotActive()
                && ObjectManager.Player.HealthPercent
                < zedMenu.GetPotActiveWhen()
                && !ObjectManager.Player.HasBuff("RegenerationPotion")
                && !ObjectManager.Player.InShop())
            {
                var pot = ItemData.GetIData(ItemId.Health_Potion);
                Items.CanUseItem(GameObjects.Player, pot.SpellName);
            }
        }
    }
}