using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Neeko.Logics
{
    using Utility;
    using static Components;
    using static Bases.ChampionBase;

    static class JungleClear
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteE()
        {
            if (!JungleClearMenu.ESliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent <= ManaManager.GetNeededMana(E, JungleClearMenu.ESliderBool))
            {
                return;
            }

            var target = GameObjects.Jungle.Where(m => m.IsValidTarget(E.Range)).MaxBy(h => h.MaxHealth);

            if (target == null)
            {
                return;
            }

            E.Cast(target);
        }

        public static void ExecuteQ()
        {
            if (!JungleClearMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent <= ManaManager.GetNeededMana(Q, JungleClearMenu.QSliderBool))
            {
                return;
            }

            var target = GameObjects.Jungle.Where(m => m.IsValidTarget(Q.Range)).MaxBy(h => h.MaxHealth);

            if (target == null)
            {
                return;
            }

            Q.Cast(target);
        }
    }
}