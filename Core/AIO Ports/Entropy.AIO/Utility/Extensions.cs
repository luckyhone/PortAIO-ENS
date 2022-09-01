using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using EzAIO.Extras;
using PortAIO.Library_Ports.Entropy.Lib.Constants;
using SharpDX;

namespace Entropy.AIO.Utility
{
    static class Extensions
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Gets the valid generic (lane or jungle) minions targets in the game.
        /// </summary>
        public static List<AIMinionClient> GetAllGenericMinionsTargets()
        {
            return GetAllGenericMinionsTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid generic (lane or jungle) minions targets in the game inside a determined range.
        /// </summary>
        public static List<AIMinionClient> GetAllGenericMinionsTargetsInRange(float range)
        {
            return GetEnemyLaneMinionsTargetsInRange(range).Concat(GetGenericJungleMinionsTargetsInRange(range)).ToList();
        }

        /// <summary>
        ///     Gets the valid generic unit targets in the game.
        /// </summary>
        public static List<AIBaseClient> GetAllGenericUnitTargets()
        {
            return GetAllGenericUnitTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid generic unit targets in the game inside a determined range.
        /// </summary>
        public static List<AIBaseClient> GetAllGenericUnitTargetsInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(range)).
                Concat<AIBaseClient>(GetAllGenericMinionsTargetsInRange(range)).
                ToList();
        }

        /// <summary>
        ///     Gets the valid enemy pet targets in the game.
        /// </summary>
        public static List<AIMinionClient> GetEnemyPets()
        {
            return GetEnemyPetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid enemy pets in the game inside a determined range.
        /// </summary>
        public static List<AIMinionClient> GetEnemyPetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(h => h.DistanceToPlayer() < range && ObjectNames.Pets.Contains(h.Name)).
                ToList();
        }

        /// <summary>
        ///     Gets the valid ally pet targets in the game.
        /// </summary>
        public static List<AIMinionClient> GetAllyPets()
        {
            return GetAllyPetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid ally pets in the game inside a determined range.
        /// </summary>
        public static List<AIMinionClient> GetAllyPetsInRange(float range)
        {
            return GameObjects.AllyMinions.Where(h => h.DistanceToPlayer() < range && ObjectNames.Pets.Contains(h.Name)).ToList();
        }

        /// <summary>
        ///     Gets the valid ally heroes targets in the game.
        /// </summary>
        public static List<AIHeroClient> GetAllyHeroesTargets()
        {
            return GetAllyHeroesTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid ally heroes targets in the game inside a determined range.
        /// </summary>
        public static List<AIHeroClient> GetAllyHeroesTargetsInRange(float range)
        {
            return GameObjects.AllyHeroes.Where(h => h.DistanceToPlayer() < range).ToList();
        }

        /// <summary>
        ///     Gets the valid ally lane minions targets in the game.
        /// </summary>
        public static List<AIMinionClient> GetAllyLaneMinionsTargets()
        {
            return GetAllyLaneMinionsTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid ally lane minions targets in the game inside a determined range.
        /// </summary>
        public static List<AIMinionClient> GetAllyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.AllyMinions.Where(m => m.DistanceToPlayer() < range).ToList();
        }

        /// <summary>
        ///     Gets the best valid enemy heroes targets in the game.
        /// </summary>
        public static List<AIHeroClient> GetBestEnemyHeroesTargets()
        {
            return GetBestEnemyHeroesTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the best valid enemy heroes targets in the game inside a determined range.
        /// </summary>
        public static List<AIHeroClient> GetBestEnemyHeroesTargetsInRange(float range)
        {
            return TargetSelector.GetTargets(range,DamageType.Physical).ToList();
        }

        /// <summary>
        ///     Gets the valid enemy heroes targets in the game.
        /// </summary>
        public static List<AIHeroClient> GetEnemyHeroesTargets()
        {
            return GetEnemyHeroesTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid enemy heroes targets in the game inside a determined range.
        /// </summary>
        public static List<AIHeroClient> GetEnemyHeroesTargetsInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(range)).ToList();
        }

        /// <summary>
        ///     Gets the valid lane minions targets in the game.
        /// </summary>
        public static IEnumerable<AIMinionClient> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid lane minions targets in the game inside a determined range.
        /// </summary>
        public static IEnumerable<AIMinionClient> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(h => h.IsValidTarget(range));
        }

        /// <summary>
        ///     Gets the valid generic (All but small) jungle minions targets in the game.
        /// </summary>
        public static List<AIMinionClient> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid generic (All but small) jungle minions targets in the game inside a determined range.
        /// </summary>
        public static List<AIMinionClient> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.
                Jungle.
                Where(m => (!((m.GetJungleType() & JungleType.Small) != 0) || Components.General.CastOnSmallJungleMinionsMenuBool.Enabled) &&
                           m.IsValidTarget(range)).
                ToList();
        }

        /// <summary>
        ///     Gets the valid large jungle minions targets in the game.
        /// </summary>
        public static List<AIMinionClient> GetLargeJungleMinionsTargets()
        {
            return GetLargeJungleMinionsTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid large jungle minions targets in the game inside a determined range.
        /// </summary>
        public static List<AIMinionClient> GetLargeJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.JungleLarge.Where(m => m.IsValidTarget(range)).ToList();
        }

        /// <summary>
        ///     Gets the valid legendary jungle minions targets in the game.
        /// </summary>
        public static List<AIMinionClient> GetLegendaryJungleMinionsTargets()
        {
            return GetLegendaryJungleMinionsTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid legendary jungle minions targets in the game inside a determined range.
        /// </summary>
        public static List<AIMinionClient> GetLegendaryJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.JungleLegendary.Where(m => m.IsValidTarget(range)).ToList();
        }

        /// <summary>
        ///     Gets the valid small jungle minions targets in the game.
        /// </summary>
        public static List<AIMinionClient> GetSmallJungleMinionsTargets()
        {
            return GetSmallJungleMinionsTargetsInRange(float.MaxValue);
        }

        /// <summary>
        ///     Gets the valid small jungle minions targets in the game inside a determined range.
        /// </summary>
        public static List<AIMinionClient> GetSmallJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.JungleSmall.Where(m => m.IsValidTarget(range)).ToList();
        }

        public static float Distance(
            this Vector2 point,
            Vector2 segmentStart,
            Vector2 segmentEnd,
            bool onlyIfOnSegment = false,
            bool squared = false)
        {
            var objects = point.ProjectOn(segmentStart, segmentEnd);

            if (objects.IsOnSegment || onlyIfOnSegment == false)
            {
                return squared
                    ? Vector2.DistanceSquared(objects.SegmentPoint, point)
                    : Vector2.Distance(objects.SegmentPoint, point);
            }

            return float.MaxValue;
        }

        public static float Times(this Vector3 vec, Vector3 vec2)
        {
            return vec.X * vec2.X + vec.Y * vec2.Y + vec.Z * vec2.Z;
        }

        public static float Length2D(this Vector3 vec)
        {
            return (float) Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);
        }

        public static Vector3 RotateZ(Vector3 target, Vector3 Origin, float deg)
        {
            var x = (float) ((target.X - Origin.X) * Math.Cos(deg) - (Origin.Z - target.Z) * Math.Sin(deg));
            var z = (float) ((Origin.Z - target.Z) * Math.Cos(deg) - (target.X - Origin.X) * Math.Sin(deg));
            return new Vector3(x, target.Y, z);
        }

        public static float GetDistanceSqr2D(this Vector3 from, Vector3 to)
        {
            return (from - to).LengthSqr2D();
        }

        public static float LengthSqr2D(this Vector3 vec)
        {
            return vec.X * vec.X + vec.Z + vec.Z;
        }

        public static int CountInRange(this AIBaseClient unit, float range, IEnumerable<AIBaseClient> units)
        {
            return GetInRange(unit.Position.To2D(), range, units).Count();
        }

        public static int CountInRange(this Vector2 pos, float range, IEnumerable<AIBaseClient> units)
        {
            return GetInRange(pos, range, units).Count();
        }

        public static float Dist2D(this Vector2 from, Vector3 to)
        {
            return (from - to.To2D()).Length();
        }

        public static float Magnitude(this Vector3 a)
        {
            return (float) Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        }

        public static float DirectionF(this Vector2 a, Vector2 b)
        {
            return (float) Math.Atan2(b.Y - a.Y, b.X - a.X);
        }

        public static bool CanExecute(this Spell spell, AIBaseClient target)
        {
            return spell.GetDamage(target) >= target.Health;
        }

        public static float SegmentDistance(
            this Vector2 point,
            Vector2 segmentStart,
            Vector2 segmentEnd,
            bool onlyIfOnSegment,
            bool squared)
        {
            var objects = point.ProjectOn(segmentStart, segmentEnd);
            if (objects.IsOnSegment || onlyIfOnSegment == false)
            {
                return squared
                    ? objects.SegmentPoint.DistanceSquared(point)
                    : objects.SegmentPoint.Distance(point);
            }

            return float.MaxValue;
        }

        private static IEnumerable<AIBaseClient> GetInRange(Vector2 pos, float range, IEnumerable<AIBaseClient> units)
        {
            return units.Where(unit => unit != null && unit.IsValid && unit.Position.To2D().Distance(pos) <= range).ToList();
        }

        public static MinionTypes GetMinionType(this AIMinionClient minion)
        {
            if (minion == null)
            {
                return MinionTypes.Unknown;
            }

            var meleeMinions = new List<string>
            {
                "sru_chaosminionmelee",
                "sru_orderminionmelee",
                "ha_chaosminionmelee",
                "ha_orderminionmelee"
            };
            var rangedMinions = new List<string>
            {
                "sru_chaosminionranged",
                "sru_orderminionranged",
                "ha_chaosminionranged",
                "ha_orderminionranged"
            };
            var siegeMinions = new List<string>
            {
                "sru_chaosminionsiege", "sru_orderminionsiege",
                "ha_chaosminionsiege", "ha_orderminionsiege"
            };
            var superMinions = new List<string>
            {
                "sru_chaosminionsuper", "sru_orderminionsuper",
                "ha_chaosminionsuper", "ha_orderminionsuper"
            };
            var smallJungleCreeps = new List<string>
            {
                "sru_razorbeakmini", "sru_murkwolfmini",
                "sru_krugmini", "sru_krugminimini"
            };
            var bigJungleCreeps = new List<string>
            {
                "sru_razorbeak", "sru_murkwolf", "sru_gromp",
                "sru_krug", "sru_red", "sru_blue", "sru_crab"
            };
            var epicJungleCreeps = new List<string>
            {
                "sru_dragon_air", "sru_dragon_earth", "sru_dragon_fire",
                "sru_dragon_water", "sru_dragon_elder", "sru_riftherald",
                "sru_baron"
            };

            var baseSkinName = Regex.Replace(minion.Name, @"[^a-zA-Z_]+", "").ToLower();
            if (baseSkinName.Contains("ward") || baseSkinName.Contains("trinket"))
            {
                return MinionTypes.Ward;
            }

            if (meleeMinions.Contains(baseSkinName))
            {
                return MinionTypes.Melee;
            }

            if (rangedMinions.Contains(baseSkinName))
            {
                return MinionTypes.Ranged;
            }

            if (siegeMinions.Contains(baseSkinName))
            {
                return MinionTypes.Siege;
            }

            if (superMinions.Contains(baseSkinName))
            {
                return MinionTypes.Super;
            }

            return MinionTypes.Unknown;
        }

        #endregion
    }
}