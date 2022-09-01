using System;
using SharpDX;

namespace Entropy.Lib.Render
{
    public interface IPolygon : IDisposable
    {
        #region Public Methods and Operators

        bool IsInsidePolygon(Vector3 worldPoint);

        bool IsInsidePolygon(Vector2 worldPoint);

        bool IsOutsidePolygon(Vector3 worldPoint);

        bool IsOutsidePolygon(Vector2 worldPoint);

        void Render(Color color, float thickness = 1f);

        #endregion
    }
}