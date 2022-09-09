using EnsoulSharp;
using EnsoulSharp.SDK;
using PortAIO;

namespace ElUtilitySuite.Vendor.SFX
{
    using System;
    using System.Linq;

    using global::SharpDX;

    using LeagueSharpCommon;

    public static class VectorExtensions
    {
        public static bool IsOnScreen(this Vector3 position, float radius)
        {
            var pos = Drawing.WorldToScreen(position);
            return !(pos.X + radius < 0) && !(pos.X - radius > Drawing.Width) && !(pos.Y + radius < 0) &&
                   !(pos.Y - radius > Drawing.Height);
        }

        public static bool IsOnScreen(this Vector2 position, float radius)
        {
            return position.To3D().IsOnScreen(radius);
        }

        public static bool IsOnScreen(this Vector2 start, Vector2 end)
        {
            if (start.X > 0 && start.X < Drawing.Width && start.Y > 0 && start.Y < Drawing.Height && end.X > 0 &&
                end.X < Drawing.Width && end.Y > 0 && end.Y < Drawing.Height)
            {
                return true;
            }
            if (start.Intersection(end, new Vector2(0, 0), new Vector2(Drawing.Width, 0)).Intersects)
            {
                return true;
            }
            if (start.Intersection(end, new Vector2(0, 0), new Vector2(0, Drawing.Height)).Intersects)
            {
                return true;
            }
            if (
                start.Intersection(end, new Vector2(0, Drawing.Height), new Vector2(Drawing.Width, Drawing.Height))
                    .Intersects)
            {
                return true;
            }
            return
                start.Intersection(end, new Vector2(Drawing.Width, 0), new Vector2(Drawing.Width, Drawing.Height))
                    .Intersects;
        }

        public static Vector2 FindNearestLineCircleIntersections(this Vector2 start,
            Vector2 end,
            Vector2 circlePos,
            float radius)
        {
            float t;
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            var a = dx * dx + dy * dy;
            var b = 2 * (dx * (start.X - circlePos.X) + dy * (start.Y - circlePos.Y));
            var c = (start.X - circlePos.X) * (start.X - circlePos.X) +
                    (start.Y - circlePos.Y) * (start.Y - circlePos.Y) - radius * radius;

            var det = b * b - 4 * a * c;
            if ((a <= 0.0000001) || (det < 0))
            {
                return Vector2.Zero;
            }
            if (det.Equals(0f))
            {
                t = -b / (2 * a);
                return new Vector2(start.X + t * dx, start.Y + t * dy);
            }

            t = (float)((-b + Math.Sqrt(det)) / (2 * a));
            var intersection1 = new Vector2(start.X + t * dx, start.Y + t * dy);
            t = (float)((-b - Math.Sqrt(det)) / (2 * a));
            var intersection2 = new Vector2(start.X + t * dx, start.Y + t * dy);

            return Vector2.Distance(intersection1, ObjectManager.Player.Position.To2D()) >
                   Vector2.Distance(intersection2, ObjectManager.Player.Position.To2D())
                ? intersection2
                : intersection1;
        }

        public static bool IsInsideCircle(this Vector2 point, Vector2 circlePos, float circleRad)
        {
            return Math.Sqrt(Math.Pow(circlePos.X - point.X, 2) + Math.Pow(circlePos.Y - point.Y, 2)) < circleRad;
        }

        public static bool IsIntersecting(this Vector2 lineStart, Vector2 lineEnd, Vector2 circlePos, float circleRadius)
        {
            return IsInsideCircle(lineStart, circlePos, circleRadius) ^ IsInsideCircle(lineEnd, circlePos, circleRadius);
        }

        public static AIMinionClient GetNearestMinionByNames(this Vector3 position, string[] names)
        {
            var nearest = float.MaxValue;
            AIMinionClient sMinion = null;
            foreach (var minion in
                GameObjects.Jungle.Where(
                    minion => names.Any(name => minion.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))))
            {
                var distance = Vector3.Distance(position, minion.ServerPosition);
                if (nearest > distance || nearest.Equals(float.MaxValue))
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }

        public static AIMinionClient GetMinionFastByNames(this Vector3 position, float range, string[] names)
        {
            return GameObjects.Jungle.FirstOrDefault(m => names.Any(n => m.Name.Equals(n)) && m.IsValidTarget(range));
        }

        public static AIMinionClient GetNearestMinionByNames(this Vector2 position, string[] names)
        {
            return GetNearestMinionByNames(position.To3D(), names);
        }
    }
}