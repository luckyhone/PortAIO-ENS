using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Ahri.Logics
{
    using static ChampionBase;
    static class LaneClear
    {
        public static void Execute()
        {
            if (Q.IsReady())
            {
                ExecuteQ();
            }

            if (W.IsReady())
            {
                ExecuteW();
            }
        }

        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        private static void ExecuteQ()
        {
            if (!Components.LaneClearMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < ManaManager.GetNeededMana(Q, Components.LaneClearMenu.QSliderBool))
            {
                return;
            }

            var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)).ToList();

            if (!minions.Any())
            {
                return;
            }

            var qFarm = Q.GetLineFarmLocation(minions);

            if (qFarm.MinionsHit >= Components.LaneClearMenu.QCustomizationSlider.Value && qFarm.Position.IsValid())
            {
                Q.Cast(qFarm.Position);
            }
        }

        private static void ExecuteW()
        {
            if (!Components.LaneClearMenu.WSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < W.GetNeededMana(Components.LaneClearMenu.WSliderBool))
            {
                return;
            }

            var hitCount = GameObjects.EnemyMinions.Count(x => x.IsValidTarget(W.Range - 100));

            if (hitCount >= Components.LaneClearMenu.WCustomizationSlider.Value)
            {
                W.Cast();
            }
        }
    }
}