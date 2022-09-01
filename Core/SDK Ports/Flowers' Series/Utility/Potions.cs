using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace Flowers_Series.Utility
{
    internal class Potions
    {
        private static AIHeroClient Me => Program.Me;
        private static Menu Menu => Tools.Menu;

        public static void Inject()
        {
            var PotionsMenu = Menu.Add(new Menu("Potions", "Auto Potions"));
            {
                PotionsMenu.Add(new MenuBool("Enable", "Enabled", Tools.EnableActivator));
                PotionsMenu.Add(new MenuSlider("Hp", "When Player HealthPercent <= %", 35));
            }

            Common.Manager.WriteConsole("PotionsMenu Load!");

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (Me.IsDead || Me.InFountain() || Me.Buffs.Any(x => x.Name.ToLower().Contains("recall") || x.Name.ToLower().Contains("teleport")))
            {
                return;
            }

            if (Menu["Potions"].GetValue<MenuBool>("Enable").Enabled && Menu["Potions"].GetValue<MenuSlider>("Hp").Value >= Me.HealthPercent)
            {
                if (Me.Buffs.Any(x => x.Name.Equals("RegenerationPotion", StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
                
            }
        }
    }
}