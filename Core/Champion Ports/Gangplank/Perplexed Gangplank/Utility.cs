using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Perplexed_Gangplank
{
    public static class Utility
    {
        public static AIHeroClient Player => ObjectManager.Player;
        public static float GetDecayRate()
        {
            var level = Player.Level;
            return level >= 13 ? 0.5f : Player.Level >= 6 ? 1f : 2f;
        }
        public static float DistanceFrom(AIBaseClient target)
        {
            return Player.Distance(target);
        }
        public static List<AIHeroClient> GetAllEnemiesInRange(float range)
        {
            return ObjectManager.Get<AIHeroClient>().Where(x => x.IsEnemy && x.InRange(range) && x.IsValidTarget()).ToList();
        }
        public static bool CanKillWithQ(AIBaseClient target)
        {
            return Player.GetSpellDamage(target, SpellSlot.Q) >= target.Health;
        }
        public static double GetRDamage(AIBaseClient target, int waves)
        {
            var waveDamage = Player.GetSpellDamage(target, SpellSlot.R);
            var totalDamage = waveDamage * waves;
            return totalDamage;
        }
        public static bool CanKillWithR(AIBaseClient target, int waves)
        {
            return GetRDamage(target, waves) > target.Health;
        }
    }
}