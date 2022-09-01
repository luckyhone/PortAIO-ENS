using System;
using System.Collections.Generic;
using EnsoulSharp;
using EnsoulSharp.SDK;
using LeagueSharpCommon.Geometry;
using SharpDX;

using Color = System.Drawing.Color;

namespace LeagueSharpCommon
{
    public static class Utility
    {
        /// <summary>
        ///     Draws a "lag-free" circle
        /// </summary>
        [Obsolete("Use Render.Circle", false)]
        public static void DrawCircle(
            Vector3 center,
            float radius,
            Color color,
            int thickness = 5,
            int quality = 30,
            bool onMinimap = false)
        {
            if (!onMinimap)
            {
                Render.Circle.DrawCircle(center, radius, color, thickness);
                return;
            }

            var pointList = new List<Vector3>();
            for (var i = 0; i < quality; i++)
            {
                var angle = i * Math.PI * 2 / quality;
                pointList.Add(
                    new Vector3(
                        center.X + radius * (float)Math.Cos(angle),
                        center.Y + radius * (float)Math.Sin(angle),
                        center.Z));
            }

            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                var aonScreen = Drawing.WorldToMinimap(a);
                var bonScreen = Drawing.WorldToMinimap(b);

                Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, thickness, color);
            }
        }
        
        /// <summary>
        ///     Checks if this position is a wall using NavMesh
        /// </summary>
        public static bool IsWall(this Vector3 position)
        {
            return NavMesh.GetCollisionFlags(position).HasFlag(CollisionFlags.Wall);
        }

        /// <summary>
        ///     Checks if this position is a wall using NavMesh
        /// </summary>
        public static bool IsWall(this Vector2 position)
        {
            return position.To3D().IsWall();
        }
        
        /// <summary>
        ///     Returns the path of the unit appending the ServerPosition at the start, works even if the unit just entered fow.
        /// </summary>
        public static List<Vector2> GetWaypoints(this AIBaseClient unit)
        {
            var result = new List<Vector2>();

            if (unit.IsVisible)
            {
                result.Add(unit.ServerPosition.To2D());
                var path = unit.Path;
                if (path.Length > 0)
                {
                    var first = path[0].To2D();
                    if (first.Distance(result[0], true) > 40)
                    {
                        result.Add(first);
                    }

                    for (int i = 1; i < path.Length; i++)
                    {
                        result.Add(path[i].To2D());
                    }
                }
            }

            return result;
        }

        public static List<Vector2Time> GetWaypointsWithTime(this AIBaseClient unit)
        {
            var wp = unit.GetWaypoints();

            if (wp.Count < 1)
            {
                return null;
            }

            var result = new List<Vector2Time>();
            var speed = unit.MoveSpeed;
            var lastPoint = wp[0];
            var time = 0f;

            foreach (var point in wp)
            {
                time += point.Distance(lastPoint) / speed;
                result.Add(new Vector2Time(point, time));
                lastPoint = point;
            }

            return result;
        }
    }
    public class Vector2Time
    {
        #region Fields

        public Vector2 Position;

        public float Time;

        #endregion

        #region Constructors and Destructors

        public Vector2Time(Vector2 pos, float time)
        {
            this.Position = pos;
            this.Time = time;
        }

        #endregion
    }
}