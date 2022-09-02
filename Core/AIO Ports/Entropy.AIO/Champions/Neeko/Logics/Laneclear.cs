using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Neeko.Logics
{
    using System.Linq;
    using Utility;
    using static Components;
    using static Bases.ChampionBase;

    static class LaneClear
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteE()
        {
            if (!LaneClearMenu.ESliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent <= ManaManager.GetNeededMana(E, LaneClearMenu.ESliderBool))
            {
                return;
            }

            var minions      = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(E.Range)).ToList();
            var farmLocation = E.GetLineFarmLocation(minions, E.Width);

            if (farmLocation.MinionsHit < LaneClearMenu.EMinionSlider.Value)
            {
                return;
            }

            E.Cast(farmLocation.Position);
        }

        public static void ExecuteQ()
        {
            if (!LaneClearMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent <= ManaManager.GetNeededMana(Q, LaneClearMenu.QSliderBool))
            {
                return;
            }

            var minions      = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(Q.Range));
            var farmLocation = Q.GetCircularFarmLocation(minions, Q.Width);

            if (farmLocation.MinionsHit < LaneClearMenu.QMinionSlider.Value)
            {
                return;
            }

            Q.Cast(farmLocation.Position);
        }
    }
}