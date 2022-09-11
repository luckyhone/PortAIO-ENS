using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using LeagueSharpCommon;

namespace HikiCarry.Core.Activator
{
    internal class Cleanser
    {
        internal static ItemData QuickSilverSash => ItemData.GetIData(ItemId.Quicksilver_Sash);
        internal static ItemData Mercurial => ItemData.GetIData(ItemId.Mercurial_Scimitar);
        internal static Random Random;

        public Cleanser()
        {
            Console.WriteLine("HikiCarry: Cleanser Initalized");
            Game.OnUpdate += CleanseOnUpdate;
        }

        private void CleanseOnUpdate(EventArgs args)
        {
            if (Initializer.Activator["use.cleanse"].GetValue<MenuBool>().Enabled)
            {
                Cleanse();
            }
        }

        public static int OwnedItemCheck()
        {
            if (ObjectManager.Player.HasItem(QuickSilverSash.Id) && ObjectManager.Player.CanUseItem(QuickSilverSash.Id))
            {
                return 1;
            }

            if (ObjectManager.Player.HasItem(Mercurial.Id) && ObjectManager.Player.CanUseItem(Mercurial.Id))
            {
                return 2;
            }

            return 0;
        }

        public static void CleanseMe()
        {
            switch (OwnedItemCheck())
            {
                case 1:
                    DelayAction.Add(Random.Next(1, Initializer.Activator["cleanse.delay"].GetValue<MenuSlider>().Value), 
                        () => ObjectManager.Player.UseItem((int)QuickSilverSash.Id));

                    break;
                case 2:
                    DelayAction.Add(Random.Next(1, Initializer.Activator["cleanse.delay"].GetValue<MenuSlider>().Value),
                       () => ObjectManager.Player.UseItem((int)Mercurial.Id));
                    break;
                case 0:
                    return;
            }
        }

        public static void Cleanse()
        {
            if (OwnedItemCheck() != 0)
            {
                if ((ObjectManager.Player.HasBuffOfType(BuffType.Charm) &&
                     Initializer.Activator["qss.charm"].GetValue<MenuBool>().Enabled)
                    ||
                    (ObjectManager.Player.HasBuffOfType(BuffType.Flee) &&
                     Initializer.Activator["qss.flee"].GetValue<MenuBool>().Enabled)
                    ||
                    (ObjectManager.Player.HasBuffOfType(BuffType.Snare) &&
                     Initializer.Activator["qss.snare"].GetValue<MenuBool>().Enabled)
                    ||
                    (ObjectManager.Player.HasBuffOfType(BuffType.Polymorph) &&
                     Initializer.Activator["qss.polymorph"].GetValue<MenuBool>().Enabled)
                    ||
                    (ObjectManager.Player.HasBuffOfType(BuffType.Stun) &&
                     Initializer.Activator["qss.stun"].GetValue<MenuBool>().Enabled)
                    ||
                    (ObjectManager.Player.HasBuffOfType(BuffType.Suppression) &&
                     Initializer.Activator["qss.suppression"].GetValue<MenuBool>().Enabled)
                    ||
                    (ObjectManager.Player.HasBuffOfType(BuffType.Taunt) &&
                     Initializer.Activator["qss.taunt"].GetValue<MenuBool>().Enabled)
                    )
                {
                    CleanseMe();
                }

            }
        }
    }
}
