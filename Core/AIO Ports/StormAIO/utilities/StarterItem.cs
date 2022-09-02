using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace StormAIO.utilities
{
    public class StarterItem
    {
        private static AIHeroClient Player => ObjectManager.Player;

        public StarterItem()
        {
            CreateMenu();
            DelayAction.Add(500, BuyItem);
        }

        private static void BuyItem()
        {
            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = MainMenu.UtilitiesMenu.GetValue<MenuList>("selectitem").SelectedValue;

            if (item != null && Game.MapId == GameMapId.SummonersRift)
                switch (item)
                {
                    case "Dorans Blade":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 450 && !Player.HasItem(ItemId.Dorans_Blade))
                                Player.BuyItem(ItemId.Dorans_Blade);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                    case "Dorans Ring":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 400 && !Player.HasItem(ItemId.Dorans_Ring)) Player.BuyItem(ItemId.Dorans_Ring);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                    case "Dorans Shield":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 450 && !Player.HasItem(ItemId.Dorans_Shield))
                                Player.BuyItem(ItemId.Dorans_Shield);
                        if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                            Player.BuyItem(ItemId.Health_Potion);
                        break;
                    }
                    case "Long Sword":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 350 && !Player.HasItem(ItemId.Long_Sword))
                                Player.BuyItem(ItemId.Long_Sword);
                        if (gold >= 150 && !Player.HasItem(ItemId.Refillable_Potion))
                            Player.BuyItem(ItemId.Refillable_Potion);
                        break;
                    }
                    case "Corrupting Potion":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Corrupting_Potion))
                                Player.BuyItem(ItemId.Corrupting_Potion);
                        break;
                    }
                }
        }

        private static void CreateMenu()
        {
            var Champ = Player.CharacterName;
            switch (Champ)
            {
                case "Yone":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "StarterItem",
                        new[] {"Dorans Blade", "none"}));
                    break;
                case "Warwick":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Hunters Machete", "none"}));
                    break;
                case "Akali":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Dorans Ring", "Dorans Shield", "Long Sword", "none"}));
                    break;
                case "Yorick":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Dorans Blade", "Corrupting Potion", "none"}));
                    break;
                case "KogMaw":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "StarterItem",
                        new[] {"Dorans Blade", "none"}));
                    break;
                case "DrMundo":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Dorans Shield", "none"}));
                    break;
                case "Rengar":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Hunters Machete", "Dorans Blade", "none"}));
                    break;
                case "Garen":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Dorans Shield", "Long Sword", "none"}));
                    break;
                case "Urgot":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Dorans Blade", "Long Sword", "Long Sword", "none"}));
                    break;
                case "Lucian":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Dorans Blade", "Long Sword", "none"}));
                    break;
                case "Chogath":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Dorans Ring", "Dorans Shield", "none"}));
                    break;
                case "Zed":
                    MainMenu.UtilitiesMenu.Add(new MenuList(Champ, "Select Item",
                        new[] {"Long Sword", "none"}));
                    break;
            }
        }
    }
}