using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;

namespace Challenger_Series.Utils
{
    public static class Misc
    {
        private static Random _rand = new Random();

        /// <summary>
        ///     Returns true if the unit is under tower range.
        /// </summary>
        public static bool UnderTurret(this AIBaseClient unit)
        {
            return UnderTurret(unit.Position, true);
        }

        /// <summary>
        ///     Returns true if the unit is under turret range.
        /// </summary>
        public static bool UnderTurret(this AIBaseClient unit, bool enemyTurretsOnly)
        {
            return UnderTurret(unit.Position, enemyTurretsOnly);
        }

        public static Vector3 RandomizeToVector3(this Vector2 position, int min, int max)
        {
            return new Vector2(position.X + _rand.Next(min, max), position.Y + _rand.Next(min, max)).ToVector3();
        }
        public static Vector3 Randomize(this Vector3 position, int min, int max)
        {
            return new Vector2(position.X + _rand.Next(min, max), position.Y + _rand.Next(min, max)).ToVector3();
        }

        public static int GiveRandomInt(int min, int max)
        {
            return _rand.Next(min, max);
        }

        public static bool UnderTurret(this Vector3 position, bool enemyTurretsOnly)
        {
            return
                ObjectManager.Get<AITurretClient>().Any(turret => turret.IsValidTarget(950, enemyTurretsOnly, position));
        }
        public static bool UnderAllyTurret(this AIBaseClient unit)
        {
            return UnderAllyTurret(unit.ServerPosition);
        }

        public static bool UnderAllyTurret(this Vector3 position)
        {
            return
                GameObjects.Get<AITurretClient>()
                    .Any(turret => turret.Position.Distance(position) < 950 && turret.IsAlly && turret.Health > 1);
        }
    }
}