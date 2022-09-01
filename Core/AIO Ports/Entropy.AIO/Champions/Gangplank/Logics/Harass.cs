using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Gangplank.Logics
{
    using static ChampionBase;
    using static Components;
    static class Harass
    {
        public static void ExecuteQ()
        {
            if (!Q.IsReady())
            {
                return;
            }

            if (!HarassMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (GameObjects.Player.ManaPercent < ManaManager.GetNeededMana(Q, HarassMenu.QSliderBool))
            {
                return;
            }

            var qTarget = TargetSelector.GetTarget(Q.Range,DamageType.Physical);

            if (qTarget == null)
            {
                return;
            }
            

            Q.Cast(qTarget);
        }
    }
}