using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Champions;
using HikiCarry.Core.Predictions;
namespace HikiCarry.Core.Plugins.JhinModes
{
    static class Mixed
    {
        /// <summary>
        /// Execute Harass W
        /// </summary>
        private static void ExecuteW()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Jhin.W.Range)))
            {
                Jhin.W.Do(enemy, Utilities.Utilities.HikiChance("hitchance"));
            }
        }

        /// <summary>
        /// Execute Harass
        /// </summary>
        public static void ExecuteHarass()
        {
            if (ObjectManager.Player.ManaPercent < Initializer.Config["harass.mana"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            if (Jhin.W.IsReady() && Initializer.Config["w.harass"].GetValue<MenuBool>().Enabled)
            {
                ExecuteW();
            }
        }
    }
}