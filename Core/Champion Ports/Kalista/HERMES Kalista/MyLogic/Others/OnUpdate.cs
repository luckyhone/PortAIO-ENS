using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace HERMES_Kalista.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnUpdate(EventArgs args)
        {
            if (Map.GetMap().Type != MapType.SummonersRift) return;
            if (GameObjects.Player.HasBuff("rengarralertsound"))
            {
                if (Items.HasItem(ObjectManager.Player, ItemId.Oracle_Lens) && Items.CanUseItem(ObjectManager.Player,(int)ItemId.Oracle_Lens))
                {
                    Items.UseItem(ObjectManager.Player,(int)ItemId.Oracle_Lens);
                }
                else if (Items.HasItem(ObjectManager.Player,(int)ItemId.Control_Ward))
                {
                    Items.UseItem(ObjectManager.Player,(int)ItemId.Control_Ward);
                }
            }

            var enemyVayne = GameObjects.EnemyHeroes.FirstOrDefault(e => e.CharacterName == "Vayne");
            if (enemyVayne != null && enemyVayne.Distance(GameObjects.Player) < 700 && enemyVayne.HasBuff("VayneInquisition"))
            {
                if (Items.HasItem(ObjectManager.Player,ItemId.Oracle_Lens) && Items.CanUseItem(ObjectManager.Player,(int)ItemId.Oracle_Lens))
                {
                    Items.UseItem(ObjectManager.Player,(int)ItemId.Oracle_Lens);
                }
                else if (Items.HasItem(ObjectManager.Player,(int)ItemId.Control_Ward))
                {
                    Items.UseItem(ObjectManager.Player,(int)ItemId.Control_Ward);
                }
            }

            if (GameObjects.Player.InFountain() && GameObjects.Player.InShop())
            {
                if (Program.ComboMenu.GetValue<MenuBool>("AutoBuy").Enabled &&
                    !Items.HasItem(ObjectManager.Player,ItemId.Oracle_Lens) && ObjectManager.Player.Level >= 9 &&
                    GameObjects.EnemyHeroes.Any(
                        h =>
                            h.CharacterName == "Rengar" || h.CharacterName == "Talon" ||
                            h.CharacterName == "Vayne"))
                {
                    //Heroes.Shop.BuyItem((ItemId)ItemData.Oracle_Alteration.Id); soon
                }
            }
        }
    }
}