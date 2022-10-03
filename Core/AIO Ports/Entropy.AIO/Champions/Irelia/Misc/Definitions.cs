using EnsoulSharp;
using PortAIO.Library_Ports.Entropy.Lib.Extensions;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;

namespace Entropy.AIO.Irelia.Misc
{
    #region

    using System;
    using System.Linq;
    using SharpDX;

    #endregion

    static class Definitions
    {
        public static float wHeld = 200;

        public static float chargeW = 0;

        public static Vector3 PredictedPosition(this AIBaseClient target, float time)
        {
            if (target.Buffs.Any(b => b.IsMovementImpairing() && b.TimeLeft() <= time))
            {
                return target.Position;
            }

            var distance = target.MoveSpeed * (time + Game.Ping / 1000f);
            if (target.Path[target.Path.Length - 1].Distance(target.Position) <= distance)
            {
                distance = target.Path[target.Path.Length - 1].Distance(target.Position);
            }

            return target.Position.Extend(target.Path[target.Path.Length - 1], target.IsMoving ? distance : 0);
        }

        public static Vector3 RayDistance(Vector3 start, Vector3 path, Vector3 center, float dist)
        {
            var a = start.X - center.X;
            var b = start.Y - center.Y;
            var c = start.Z - center.Z;
            var x = path.X;
            var y = path.Y;
            var z = path.Z;

            var n1 = a * x + b * y + c * z;

            var n2 = Math.Pow(z, 2) * Math.Pow(dist, 2) -
                     Math.Pow(a, 2) * Math.Pow(z, 2)    -
                     Math.Pow(b, 2) * Math.Pow(z, 2)    +
                     2 * a * c * x     * z              +
                     2 * b * c * y     * z              +
                     2 * a * b * x     * y              +
                     Math.Pow(dist, 2) * Math.Pow(x, 2) +
                     Math.Pow(dist, 2) * Math.Pow(y, 2) -
                     Math.Pow(a, 2) * Math.Pow(y, 2)    -
                     Math.Pow(b, 2) * Math.Pow(x, 2)    -
                     Math.Pow(c, 2) * Math.Pow(x, 2)    -
                     Math.Pow(c, 2) * Math.Pow(y, 2);
            var n3 = Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2);

            var r1 = -(n1 + Math.Sqrt(n2)) / n3;
            var r2 = -(n1 - Math.Sqrt(n2)) / n3;
            var r  = Math.Max(r1, r2);

            return start + path * (float) r;
        }
    }
}