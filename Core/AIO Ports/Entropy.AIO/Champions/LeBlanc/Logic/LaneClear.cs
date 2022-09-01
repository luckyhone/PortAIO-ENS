using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.LeBlanc.Misc;
using Entropy.AIO.Utility;
using PortAIO.Library_Ports.Entropy.Lib.Constants;

namespace Entropy.AIO.LeBlanc.Logic
{
    using static Components;
    using static ChampionBase;
    class Laneclear
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        internal static void ExecuteQ()
        {
            if (!LaneClearMenu.QSliderBool.Enabled || LaneClearMenu.QSliderBool.Value > LocalPlayer.ManaPercent)
            {
                return;
            }

            float time(AIBaseClient x)
            {
                return LocalPlayer.Distance(x) / Q.Speed;
            }

            var qTarget = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)).
                FirstOrDefault(x => x.GetPredictedMinionHealth(time(x)) < Q.GetDamage(x));

            if (qTarget != null)
            {
                Q.Cast(qTarget);
            }
        }

        internal static void ExecuteW()
        {
            if (!Definitions.HasW1)
            {
                W.Cast();
                return;
            }

            if (!LaneClearMenu.WSliderBool.Enabled ||
                LocalPlayer.ManaPercent <= ManaManager.GetNeededMana(W, LaneClearMenu.WSliderBool))
            {
                return;
            }

            var farmLoc =
                W.GetCircularFarmLocation(GameObjects.EnemyMinions.Where(x => x.IsValidTarget(W.Range) && !x.IsDead));
            if (farmLoc.MinionsHit >= LaneClearMenu.WCountSliderBool.Value)
            {
                W.Cast(farmLoc.Position);
            }
        }
    }
}