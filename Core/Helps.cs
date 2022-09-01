using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;

namespace PortAIO
{
    public static class Helps
    {
        /// <summary>
        ///     Counts the hits.
        /// </summary>
        /// <param name="units">The units.</param>
        /// <param name="castPosition">The cast position.</param>
        /// <returns>System.Int32.</returns>
        public static  int CountHits(List<AIBaseClient> units, Vector3 castPosition)
        {
            var points = units.Select(unit => Prediction.GetPrediction(unit,0f).UnitPosition).ToList();
            return CountHits(points, castPosition);
        }

        /// <summary>
        ///     Counts the hits.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="castPosition">The cast position.</param>
        /// <returns>System.Int32.</returns>
        public static  int CountHits(List<Vector3> points, Vector3 castPosition)
        {
            return points.Count(point => WillHit(point, castPosition));
        }
        
        /// <summary>
        ///     Returns if the spell will hit the unit when casted on castPosition.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="castPosition">The cast position.</param>
        /// <param name="extraWidth">Added width to the spell.</param>
        /// <param name="minHitChance">The minimum hit chance.</param>
        /// <returns><c>true</c> if the spell will hit the unit, <c>false</c> otherwise.</returns>
        public static  bool WillHit(AIBaseClient unit,
            Vector3 castPosition,
            int extraWidth = 0,
            HitChance minHitChance = HitChance.High)
        {
            var unitPosition = Prediction.GetPrediction(unit,0f);
            return unitPosition.Hitchance >= minHitChance &&
                   WillHit(unitPosition.UnitPosition, castPosition, extraWidth);
        }
        
        public static SpellType Type { get; set; }
        public static float WidthSqr { get; private set; }
        public static float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                WidthSqr = value*value;
            }
        }
        private static float _width;
        private static Vector3 _from;
        
        /// <summary>
        ///     Gets or sets from position.
        /// </summary>
        /// <value>From position.</value>
        public static Vector3 From
        {
            get { return !_from.To2D().IsValid() ? ObjectManager.Player.ServerPosition : _from; }
            set { _from = value; }
        }
        
        private static float _range;
        
        public static SpellSlot Slot { get; set; }
        
        public static string ChargedBuffName { get; set; }
        
        private static int _chargedCastedT;
        
        /// <summary>
        ///     Gets a value indicating whether this instance is charging a charged spell.
        /// </summary>
        /// <value><c>true</c> if this instance is charging  a charged spell; otherwise, <c>false</c>.</value>
        public static bool IsCharging
        {
            get
            {
                if (!Slot.IsReady())
                    return false;

                return ObjectManager.Player.HasBuff(ChargedBuffName) ||
                       Environment.TickCount - _chargedCastedT < 300 + Game.Ping;
            }
        }
        
        /// <summary>
        ///     Gets or sets a value indicating whether this instance is charged spell.
        /// </summary>
        /// <value><c>true</c> if this instance is charged spell; otherwise, <c>false</c>.</value>
        public static bool IsChargedSpell { get; set; }
        
        /// <summary>
        ///     Gets or sets the range.
        /// </summary>
        /// <value>The range.</value>
        public static float Range
        {
            get
            {
                if (!IsChargedSpell)
                {
                    return _range;
                }

                if (IsCharging)
                {
                    return ChargedMinRange +
                           Math.Min(
                               ChargedMaxRange - ChargedMinRange,
                               (Environment.TickCount - _chargedCastedT)*(ChargedMaxRange - ChargedMinRange)/
                               ChargeDuration - 150);
                }

                return ChargedMaxRange;
            }
            set { _range = value; }
        }
        
        public static int ChargedMinRange { get; set; }
        
        public static int ChargedMaxRange { get; set; }
        
        public static int ChargeDuration { get; set; }
        
        /// <summary>
        ///     Gets the range squared.
        /// </summary>
        /// <value>The range squared.</value>
        public static float RangeSqr
        {
            get { return Range*Range; }
        }
        
        /// <summary>
        ///     Returns if the spell will hit the point when casted on castPosition.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="castPosition">The cast position.</param>
        /// <param name="extraWidth">Width of the extra.</param>
        /// <returns><c>true</c> if the spell will hit the location, <c>false</c> otherwise.</returns>
        public static  bool WillHit(Vector3 point, Vector3 castPosition, int extraWidth = 0)
        {
            switch (Type)
            {
                case SpellType.Circle:
                    if (point.To2D().Distance(castPosition) < WidthSqr)
                    {
                        return true;
                    }
                    break;

                case SpellType.Line:
                    if (Vector2.Distance(castPosition.To2D(), From.To2D()) <
                        Math.Pow(Width + extraWidth, 2))
                    {
                        return true;
                    }
                    break;
                case SpellType.Cone:
                    var edge1 = (castPosition.To2D() - From.To2D()).Rotated(-Width/2);
                    var edge2 = edge1.Rotated(Width);
                    var v = point.To2D() - From.To2D();
                    if (point.To2D().Distance(From) < RangeSqr && edge1.CrossProduct(v) > 0 &&
                        v.CrossProduct(edge2) > 0)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }
        public static Vector2 To2D(this Vector3 vector3)
        {
            return new Vector2(vector3.X, vector3.Z);
        }

        public static Vector3 To3D(this Vector2 vector2)
        {
            return new Vector3(vector2.X, GameObjects.Player.Position.Z, vector2.Y);
        }

        public static int TotalMaxEXP(this AIHeroClient hero, int level)
        {
            var levelMul = 0;
            for (var i = level; i > 0; i--)
                levelMul += i;
            return levelMul * 100 + level * 180;
        }
        public static Vector3 ToFlat3D(this Vector2 vector2)
        {
            return new Vector3(vector2.X, 0, vector2.Y);
        }

        public static Vector3 ToFlat3D(this Vector3 vector3)
        {
            return new Vector3(vector3.X, 0, vector3.Y);
        }
        
        public static List<AIHeroClient> GetAlliesInRange(this AIBaseClient unit, float range)
        {
            return GetAlliesInRange(unit.Position, range, unit);
        }

        public static List<AIHeroClient> GetAlliesInRange(
            this Vector3 point,
            float range,
            AIBaseClient originalunit = null)
        {
            if (originalunit != null)
            {
                return
                    GameObjects.AllyHeroes.Where(
                        x =>
                            x.NetworkId != originalunit.NetworkId && point.Distance(x.Position) <= range * range).ToList();
            }
            return GameObjects.AllyHeroes.Where(x => point.Distance(x.Position) <= range * range).ToList();
        }
        
        public static List<AIHeroClient> GetEnemiesInRange(this AIBaseClient unit, float range)
        {
            return GetEnemiesInRange(unit.Position, range);
        }

        public static List<AIHeroClient> GetEnemiesInRange(this Vector3 point, float range)
        {
            return GameObjects.EnemyHeroes.Where(x => point.Distance(x.Position) <= range * range).ToList();
        }
        
        public static string FormatTime(this float value, bool totalSeconds = false)
        {
            var ts = TimeSpan.FromSeconds(value);
            if (!totalSeconds && ts.TotalSeconds > 60)
            {
                return string.Format("{0}:{1:00}", (int) ts.TotalMinutes, ts.Seconds);
            }
            return string.Format("{0:0}", ts.TotalSeconds);
        }
    }
}