using System;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace PortAIO.Library_Ports.Entropy.Lib.Constants
{
    public static class Extensions
    {
        public static float GetPredictedMinionHealth(this AIMinionClient minion, float time = -1f)
        {
            try
            {
                var rtime = time < 0f ? minion.TimeForAutoAttackToReachTarget() : time;
                return HealthPrediction.GetPrediction(minion,(int)rtime);
            }
            catch (Exception e)
            {
                // Ignore
            }

            return 999f;
        }
        public static float TimeForAutoAttackToReachTarget(this AttackableUnit target, AIBaseClient source = null)
        {
            if (target == null)
            {
                return 0;
            }
            
            var realSource    = source ?? ObjectManager.Player;
            var animationTime = realSource.AttackCastDelay * 1000f;

            if (realSource.IsMelee)
            {
                return animationTime;
            }
                
            var dist         = realSource.Distance(target) - target.BoundingRadius /2f;
            var missileSpeed = realSource.BasicAttack.MissileSpeed;

            var travelTime = 1000f * dist / missileSpeed;

            return animationTime + travelTime;
        }
    }
}