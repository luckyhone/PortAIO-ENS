using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using PortAIO.Library_Ports.Entropy.Lib.Constants;

namespace PortAIO.Library_Ports.Entropy.Lib.Extensions
{
    public static class MinionExtensions
    {
        public static bool CanKillMinionWithDamage(this AIMinionClient minion, float damage, float delay = -1f)
        {
            try
            {
                var pred = GetPredictedMinionHealth(minion, delay);

                return pred <= damage && pred > -damage;
            }
            catch (Exception e)
            {
            }

            return false;
        }

        /// <summary>
        ///     If time == 0, it will use Time that AA will hit the target
        /// </summary>
        /// <param name="minion"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float GetPredictedMinionHealth(this AIMinionClient minion, float time = -1f)
        {
            try
            {
                var rtime = time < 0f ? minion.TimeForAutoAttackToReachTarget() : time;
                return HealthPrediction.GetPrediction(minion, (int)rtime);
            }
            catch (Exception e)
            {
            }

            return 999f;
        }
        public static bool IsLaneMinion(this AIMinionClient minion)
        {
            return minion.Name.Contains("OrderMinion") || minion.Name.Contains("ChaosMinion");
        }
    }
}