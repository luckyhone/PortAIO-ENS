using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Z.aio.Common
{
    internal static class Extensions
    {
        public static AIHeroClient GetBestEnemyHeroTargetInRange(this float range,DamageType type = DamageType.Mixed)
        {
            var target = TargetSelector.GetTarget(range,type);
            if (target != null && target.IsValidTarget() && !target.IsInvulnerable)
            {
                return target;
            }
            return null;
        }
        
        public static AIHeroClient GetBestKillableHero(this Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.GetTargets(spell.Range,damageType).FirstOrDefault(t => t.IsValidTarget());
        }
    }
}