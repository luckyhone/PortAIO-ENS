using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Gangplank.Misc
{
    public static class Definitions
    {
        public static float ExplosionRadius = 355f;
        public static float ChainRadius     = 690f;

        public static int LastQCast;
        public static int LastECast;

        public static AIHeroClient Player => ObjectManager.Player;
        //public static bool CanCastE => Game.TickCount - LastECast >= 250;

        public static float GetDecayRate()
        {
            var level = Player.Level;
            return level >= 13 ? 0.5f : level >= 7 ? 1f : 2f;
        }

        public static float DistanceFrom(AIBaseClient target)
        {
            return Player.Distance(target);
        }

        public static List<AIHeroClient> GetAllEnemiesInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && x.InRange(Player, range)).ToList();
        }
    }
}