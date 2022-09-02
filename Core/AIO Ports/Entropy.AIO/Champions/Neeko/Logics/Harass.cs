using EnsoulSharp;

namespace Entropy.AIO.Neeko.Logics
{
    using System.Linq;
    using Utility;
    using static Components;
    using static Bases.ChampionBase;

    static class Harass
    {
        public static void ExecuteQ()
        {
            if (!HarassMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (ObjectManager.Player.ManaPercent <= ManaManager.GetNeededMana(Q, HarassMenu.QSliderBool))
            {
                return;
            }

            foreach (var target in Extensions.GetBestEnemyHeroesTargetsInRange(Q.Range))
            {
                if (HarassMenu.QOnlyIfEnemyCCed.Enabled && !target.IsStunned)
                {
                    continue;
                }

                Q.Cast(target);
                break;
            }
        }
    }
}