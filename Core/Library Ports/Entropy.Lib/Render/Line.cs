using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp.SDK;
using EzAIO.Extras;
using SharpDX;

namespace Entropy.Lib.Render
{
    public sealed class Line : LinearPolygon
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Line" /> class.
        /// </summary>
        /// <param name="rootPoint1">The root point1.</param>
        /// <param name="rootPoint2">The root point2.</param>
        public Line(Vector3 rootPoint1, Vector3 rootPoint2)
        {
            _rootPoint1 = rootPoint1;
            _rootPoint2 = rootPoint2;

            Update();
        }

        #endregion

        #region Methods

        protected override void Update()
        {
            WorldPoints = new List<Vector3>
            {
                RootPoint1, RootPoint2
            };
        }

        #endregion

        #region Fields

        private Vector3 _rootPoint1;

        private Vector3 _rootPoint2;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the first root point.
        /// </summary>
        /// <value>
        ///     The first root point.
        /// </value>
        public Vector3 RootPoint1
        {
            get => _rootPoint1;

            set
            {
                _rootPoint1 = value;
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the second root point.
        /// </summary>
        /// <value>
        ///     The second root point.
        /// </value>
        public Vector3 RootPoint2
        {
            get => _rootPoint2;

            set
            {
                _rootPoint2 = value;
                Update();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Finds the intersection of two lines.
        /// </summary>
        /// <param name="line1">The other line.</param>
        /// <returns>
        ///     The <see cref="Vector2" /> intersection point, or <see cref="Vector2.Zero" /> if the lines are
        ///     parallel.
        /// </returns>
        public Vector2 FindIntersection(Line line1)
        {
            var s1 = RootPoint1.To2D();
            var e1 = RootPoint2.To2D();
            var s2 = line1.RootPoint1.To2D();
            var e2 = line1.RootPoint2.To2D();

            var r = s1.Intersection(e1, s2, e2);
            return r.Intersects ? r.Point : Vector2.Zero;
        }

        public bool IsPointOnLine(Vector2 point)
        {
            var a = (RootPoint2.Y - RootPoint1.Y) / (RootPoint2.X - RootPoint2.X);
            var b = RootPoint1.Y - a * RootPoint1.X;
            return Math.Abs(point.Y - (a * point.X + b)) <= float.Epsilon;
        }

        /// <summary>
        ///     Finds the intersection of the line and a circle.
        /// </summary>
        /// <param name="circle">The circle.</param>
        /// <returns><see cref="Vector2[]" /> of all the intersections found.</returns>
        public Vector2[] FindIntersection(Geometry.Circle circle)
        {
            float t;

            var dx = RootPoint2.X - RootPoint1.X;
            var dy = RootPoint2.Y - RootPoint1.Y;

            var a = dx * dx + dy * dy;
            var b = 2 * (dx * (RootPoint1.X - circle.Center.X) + dy * (RootPoint1.Y - circle.Center.Y));
            var c = (RootPoint1.X - circle.Center.X) * (RootPoint1.X - circle.Center.X) +
                    (RootPoint1.Y - circle.Center.Y) * (RootPoint1.Y - circle.Center.Y) -
                    circle.Radius * circle.Radius;
            var det = b * b - 4 * a * c;

            if (Math.Abs(a) <= float.Epsilon || det < 0)
            {
                return new Vector2[]
                {
                };
            }

            if (det == 0)
            {
                t = -b / (2 * a);
                return new[]
                {
                    new Vector2(RootPoint1.X + t * dx, RootPoint1.Y + t * dy)
                };
            }

            t = (float) ((-b + Math.Sqrt(det)) / (2 * a));
            var int1 = new Vector2(RootPoint1.X + t * dx, RootPoint1.Y + t * dy);
            t = (float) ((-b - Math.Sqrt(det)) / (2 * a));

            return new[]
            {
                int1,
                new Vector2(RootPoint1.X + t * dx, RootPoint1.Y + t * dy)
            };
        }

        public new Vector2[] FindIntersection(LinearPolygon poly)
        {
            var result = new List<Vector2>();
            for (var i = 0; i < poly.WorldPoints.Count - 1; i++)
            {
                var line  = new Line(poly.WorldPoints[i], poly.WorldPoints[i + 1]);
                var inter = FindIntersection(line);
                if (inter != Vector2.Zero)
                {
                    result.Add(inter);
                }
            }

            {
                var line  = new Line(poly.WorldPoints.First(), poly.WorldPoints.Last());
                var inter = FindIntersection(line);
                if (inter != Vector2.Zero)
                {
                    result.Add(inter);
                }
            }

            return result.ToArray();
        }

        public Vector3[] Split(int divisions)
        {
            var list = new List<Vector3>();
            var dist = RootPoint1.Distance(RootPoint2);
            var div  = dist / divisions;
            for (var i = 0; i < divisions; i++)
            {
                list.Add(RootPoint1.Extend(RootPoint2, div * i));
            }

            return list.ToArray();
        }

        /// <summary>
        ///     Renders the line with the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The thickness.</param>
        public override void Render(Color color, float thickness = 1)
        {
            LineRendering.Render(color, thickness, WorldPoints.ToArray());
        }

        #endregion
    }
}