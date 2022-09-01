using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Ahri.Logics
{
    using static ChampionBase;
    static class Harass
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteE()
        {
            if (!Components.HarassMenu.ESliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < ManaManager.GetNeededMana(E, Components.HarassMenu.ESliderBool))
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(
                         t => t.IsValidTarget(E.Range)))
            {
                E.Cast(target);
                break;
            }
        }

        public static void ExecuteQ()
        {
            if (!Components.HarassMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < ManaManager.GetNeededMana(Q, Components.HarassMenu.QSliderBool))
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(
                         t => t.IsValidTarget(Q.Range)))
            {
                Q.Cast(target);
                break;
            }
        }

        public static void ExecuteW()
        {
            if (!Components.HarassMenu.WSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < W.GetNeededMana(Components.HarassMenu.WSliderBool))
            {
                return;
            }

            if (GameObjects.EnemyHeroes.Any(t => t.IsValidTarget(W.Range - 50)))
            {
                W.Cast();
            }
        }
    }
}