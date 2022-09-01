using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;

namespace PortAIO.Library_Ports.Entropy.Lib.Geometry
{
    public static class ExtendEx
    {
        #region Public Methods and Operators

        public static Vector2 Extend(this Vector2 source, Vector2 target, float distance)
        {
            return source + distance * (target - source).Normalized();
        }

        public static Vector2 Extend(this Vector2 source, Vector3 target, float distance)
        {
            return source + distance * (target.To2D() - source).Normalized();
        }

        public static Vector3 Extend(this Vector3 source, Vector2 target, float distance)
        {
            return source + distance * (target.To3D() - source).Normalized();
        }

        public static Vector3 Extend(this Vector3 vector, Vector3 direction, float distance)
        {
            if (Math.Abs(vector.To2D().DistanceSquared(direction.To2D())) < float.Epsilon)
            {
                return vector;
            }

            var edge = direction.To2D() - vector.To2D();
            edge.Normalize();

            var v = vector.To2D() + edge * distance;
            return new Vector3(v.X, vector.Y, v.Y);
        }

        public static Vector3 Extend(this GameObject source, GameObject target, float range)
        {
            return Extend(source.Position, target.Position, range);
        }

        #endregion
    }
}