using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille.Misc
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharpDX;

    #endregion

    static class Definitions
    {
        #region Properties And Fields

        public static          QState   QState => GetQState();
        public static          int      LastQCast   = 0;
        public static          int      LastWCast   = 0;
        public static          Vector3  WDirection  = Vector3.Zero;
        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static bool CastingW =>
            LocalPlayer.HasBuff("camillewconeslashcharge") || Environment.TickCount - LastWCast <= 1000;

        public static bool CastingR => LocalPlayer.HasBuff("camillerrange");

        public static bool IsOnWall =>
            LocalPlayer.HasBuff("camilleeonwall") || LocalPlayer.HasBuff("camilleedashtoggle");

        public static bool IsDashing =>
            LocalPlayer.HasBuff("camilleedash1") || LocalPlayer.HasBuff("CamilleEDash2");

        public static AIHeroClient LastETarget = null;

        #endregion

        #region Methods And Extensions

        /// <summary>
        ///     Returns the current casting state of Q spell.
        /// </summary>
        /// <returns></returns>
        private static QState GetQState()
        {
            var player = LocalPlayer;
            if (player.HasBuff("CamilleQ") || Environment.TickCount - LastQCast <= 250)
            {
                return QState.Casted;
            }

            if (player.HasBuff("camilleqprimingstart"))
            {
                return QState.Charging;
            }

            if (player.HasBuff("camilleqprimingcomplete"))
            {
                return QState.Charged;
            }

            return QState.None;
        }

        /// <summary>
        ///     Returns the closest wall to our target that we can E with.
        /// </summary>
        /// <param name="walls">Walls to check.</param>
        /// <param name="target">The target we want to E to.</param>
        /// <param name="range">The range to check from the walls.</param>
        /// <returns></returns>
        public static Vector3 GetBestWallToTarget(this IEnumerable<Vector3> walls, AIHeroClient target, float range)
        {
            return walls.Where(wall => wall.Distance(target) <= range).OrderBy(wall => wall.Distance(target)).FirstOrDefault();
        }

        /// <summary>
        ///     Gets all walls that have an enemy target within range.
        /// </summary>
        /// <param name="walls">Walls to check.</param>
        /// <param name="range">Range to check</param>
        /// <returns></returns>
        public static IEnumerable<Vector3> GetWallsWithTargets(this IEnumerable<Vector3> walls, float range)
        {
            return walls.Where(wall => GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(range,true,wall))).ToList();
        }

        /// <summary>
        ///     Returns all first point of contact walls from vectors.
        /// </summary>
        /// <param name="points">The collection of vectors to check.</param>
        /// <returns></returns>
        public static IEnumerable<Vector3> GetWalls(this IEnumerable<Vector3> points)
        {
            return points.Select(x => GetFirstWall(LocalPlayer.Position, x)).Where(x => x != Vector3.Zero);
        }

        /// <summary>
        ///     Returns the first wall position between two vectors.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="end">The end position.</param>
        /// <returns></returns>
        private static Vector3 GetFirstWall(Vector3 start, Vector3 end)
        {
            var       wallPos      = Vector3.Zero;
            const int chunks       = 20;
            var       distDividend = (start - end) / chunks;
            for (var i = 0; i < chunks; i++)
            {
                var tempPos = start + distDividend * i;
                if (!tempPos.IsWall())
                {
                    continue;
                }

                wallPos = tempPos;
                break;
            }

            return wallPos;
        }

        /// <summary>
        ///     Returns a list of positions rotated around a vector within a radius.
        /// </summary>
        /// <param name="pos">The vector to rotate around.</param>
        /// <param name="radius">The radius from <paramref name="pos" /> to rotate.</param>
        /// <param name="points">The amount of positions we want to return. Higher number = less performance.</param>
        /// <returns></returns>
        public static IEnumerable<Vector3> RotateAround(this Vector3 pos, float radius, int points)
        {
            var vectors = new List<Vector3>();
            for (var i = 1; i <= points; i++)
            {
                var angle = i * 2 * Math.PI / points;
                var point = new Vector3(pos.X + radius * (float) Math.Cos(angle),
                                        pos.Y,
                                        pos.Z + radius * (float) Math.Sin(angle));
                vectors.Add(point);
            }

            return vectors;
        }

        #endregion
    }
}