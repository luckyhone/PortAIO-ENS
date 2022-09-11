using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;

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
        public static int EnemyMinionsCount(this AIBaseClient target, float range)
        {
            return GameObjects.EnemyMinions.Count(m => m.IsValidTarget(range));
        }
        public static Vector3 PosAfterTime(AIBaseClient unit, float time)
        {
            return Prediction.GetPrediction(unit, time).CastPosition;
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