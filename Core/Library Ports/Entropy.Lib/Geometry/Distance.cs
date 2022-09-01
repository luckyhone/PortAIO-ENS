using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;

namespace PortAIO.Library_Ports.Entropy.Lib.Geometry
{
    public static class DistanceEx
    {
        #region Public Methods and Operators

        public static float Distance(this Vector2 from, Vector2 to)
        {
            return Vector2.Distance(from, to);
        }

        public static float Distance(this Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }

        public static float Distance(this Vector3 from, Vector2 to)
        {
            return Vector2.Distance(from.To2D(), to);
        }

        public static float Distance(this Vector2 from, Vector3 to)
        {
            return Vector3.Distance(from.To3D(), to);
        }

        public static float Distance(this Vector3 from, GameObject to)
        {
            return Vector3.Distance(from, to.Position);
        }

        public static float Distance(this GameObject from, Vector3 to)
        {
            return Vector3.Distance(from.Position, to);
        }

        public static float Distance(this GameObject from, GameObject to)
        {
            return Vector3.Distance(from.Position, to.Position);
        }

        public static float DistanceSquared(this Vector3 from, Vector3 to)
        {
            return Vector3.DistanceSquared(from, to);
        }

        public static float DistanceSquared(this Vector3 from, GameObject to)
        {
            return Vector3.DistanceSquared(from, to.Position);
        }

        public static float DistanceSquared(this GameObject from, Vector3 to)
        {
            return Vector3.DistanceSquared(from.Position, to);
        }

        public static float DistanceSquared(this GameObject from, Vector2 to)
        {
            return Vector3.DistanceSquared(from.Position, to.To3D());
        }

        public static float DistanceSquared(this GameObject from, GameObject to)
        {
            return Vector3.DistanceSquared(from.Position, to.Position);
        }

        public static float DistanceSquared(this Vector2 v1, Vector2 v2)
        {
            return Vector2.DistanceSquared(v1, v2);
        }

        public static float DistanceSquared(this Vector2 v1, Vector3 v2)
        {
            return Vector2.DistanceSquared(v1, (Vector2) v2);
        }

        public static float DistanceSquared(this Vector3 v1, Vector2 v2)
        {
            return Vector2.DistanceSquared((Vector2) v1, v2);
        }

        /// <summary>
        ///     Gets distance squared from the segments.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="segmentStart">The segment start.</param>
        /// <param name="segmentEnd">The segment end.</param>
        /// <param name="onlyIfOnSegment">if set to <c>true</c> [only if on segment].</param>
        /// <returns>System.Single.</returns>
        public static float DistanceSquared(
            this Vector2 point,
            Vector2      segmentStart,
            Vector2      segmentEnd,
            bool         onlyIfOnSegment = false)
        {
            var objects = point.ProjectOn(segmentStart, segmentEnd);

            return objects.IsOnSegment || onlyIfOnSegment == false
                ? Vector2.DistanceSquared(objects.SegmentPoint, point)
                : float.MaxValue;
        }

        /// <summary>
        ///     Returns the distance to the line segment.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="segmentStart">The segment start.</param>
        /// <param name="segmentEnd">The segment end.</param>
        /// <param name="onlyIfOnSegment">if set to <c>true</c> [only if on segment].</param>
        /// <param name="squared">if set to <c>true</c> [squared].</param>
        /// <returns></returns>
        public static float Distance(
            this Vector2 point,
            Vector2      segmentStart,
            Vector2      segmentEnd,
            bool         onlyIfOnSegment = false,
            bool         squared         = false)
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

        public static float DistanceToPlayer(this Vector3 to)
        {
            return ObjectManager.Player.Distance(to);
        }

        public static float DistanceToPlayer(this GameObject to)
        {
            return ObjectManager.Player.Distance(to);
        }

        public static bool IsInRange(this Vector3 source, Vector3 target, float range)
        {
            return Distance(source.To2D(), target.To2D()) < range;
        }

        public static bool IsInRange(this GameObject source, GameObject target, float range)
        {
            return IsInRange(source.Position, target.Position, range);
        }

        public static bool IsInRange(this GameObject source, Vector3 target, float range)
        {
            return IsInRange(source.Position, target, range);
        }

        public static bool IsInRange(this Vector3 source, GameObject target, float range)
        {
            return IsInRange(source, target.Position, range);
        }

        #endregion
    }
}