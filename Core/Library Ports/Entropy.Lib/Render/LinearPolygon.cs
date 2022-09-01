using System.Collections.Generic;
using System.Linq;
using EnsoulSharp.SDK.Clipper;
using EzAIO.Extras;
using SharpDX;

namespace Entropy.Lib.Render
{
    public class LinearPolygon : IPolygon
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the world points.
        /// </summary>
        /// <value>
        ///     The world points.
        /// </value>
        public List<Vector3> WorldPoints;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LinearPolygon" /> class.
        /// </summary>
        /// <param name="worldPoints">The world points.</param>
        public LinearPolygon(params Vector3[] worldPoints)
        {
            WorldPoints = worldPoints.ToList();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates and calculates this polygon's <see cref="WorldPoints" /> using its simpler to use custom
        ///     implementation.
        /// </summary>
        protected virtual void Update()
        {
        }

        #endregion

        #region Public Methods and Operators

        public void Dispose()
        {
            WorldPoints = null;
        }

        /// <summary>
        ///     Determines whether [is inside polygon] [the specified world point].
        /// </summary>
        /// <param name="worldPoint">The world point.</param>
        /// <returns>
        ///     <c>true</c> if [is inside polygon] [the specified world point]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInsidePolygon(Vector2 worldPoint)
        {
            return !IsInsidePolygon(worldPoint.ToFlat3D());
        }

        /// <summary>
        ///     Determines whether [is inside polygon] [the specified world point].
        /// </summary>
        /// <param name="worldPoint">The world point.</param>
        /// <returns>
        ///     <c>true</c> if [is inside polygon] [the specified world point]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInsidePolygon(Vector3 worldPoint)
        {
            return !IsOutsidePolygon(worldPoint);
        }

        /// <summary>
        ///     Determines whether [is outside polygon] [the specified world point].
        /// </summary>
        /// <param name="worldPoint">The world point.</param>
        /// <returns>
        ///     <c>true</c> if [is outside polygon] [the specified world point]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOutsidePolygon(Vector2 worldPoint)
        {
            return IsOutsidePolygon(worldPoint.ToFlat3D());
        }

        public bool IsOnPolygon(Vector3 worldPoint)
        {
            var point = new IntPoint(worldPoint.X, worldPoint.Z);
            return Clipper.PointInPolygon(point, ToClipperPath()) == -1;
        }

        public bool IsOnPolygon(Vector2 worldPoint)
        {
            var point = new IntPoint(worldPoint.X, worldPoint.Y);
            return Clipper.PointInPolygon(point, ToClipperPath()) == -1;
        }

        public Vector2[] FindIntersection(LinearPolygon poly)
        {
            var result = new List<Vector2>();
            for (var i = 0; i < WorldPoints.Count - 1; i++)
            {
                var l = new Line(WorldPoints[i], WorldPoints[i + 1]);
                result.AddRange(l.FindIntersection(poly));
            }

            result.AddRange(new Line(WorldPoints.First(), WorldPoints.Last()).FindIntersection(poly));
            return result.ToArray();
        }

        /// <summary>
        ///     Determines whether [is outside polygon] [the specified world point].
        /// </summary>
        /// <param name="worldPoint">The world point.</param>
        /// <returns>
        ///     <c>true</c> if [is outside polygon] [the specified world point]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOutsidePolygon(Vector3 worldPoint)
        {
            var point = new IntPoint(worldPoint.X, worldPoint.Z);
            return Clipper.PointInPolygon(point, ToClipperPath()) == 0;
        }

        /// <summary>
        ///     Renders the linear polygon with the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The thickness.</param>
        public virtual void Render(Color color, float thickness = 1f)
        {
            if (WorldPoints == null || !WorldPoints.Any())
            {
                return;
            }

            LineRendering.Render(color, thickness, WorldPoints.ToArray());
            LineRendering.Render(color, thickness, WorldPoints[0], WorldPoints[WorldPoints.Count - 1]);
        }

        /// <summary>
        ///     Converts the polygon to a clipper path, which is a list of <see cref="IntPoint" />
        /// </summary>
        /// <returns><see cref="List{IntPoint}" /> of the world-space points</returns>
        public List<IntPoint> ToClipperPath()
        {
            var result = new List<IntPoint>(WorldPoints.Count);
            result.AddRange(WorldPoints.Select(p => new IntPoint(p.X, p.Z)));
            return result;
        }

        #endregion
    }
}