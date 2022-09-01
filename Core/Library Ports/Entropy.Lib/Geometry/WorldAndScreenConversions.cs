using EnsoulSharp;
using SharpDX;

namespace PortAIO.Library_Ports.Entropy.Lib.Geometry
{
    public static class WorldAndScreenConversions
    {
        #region Public Methods and Operators

        public static Vector3 ScreenToWorld(this Vector2 position)
        {
            return Drawing.ScreenToWorld(position);
        }

        public static Vector2 WorldToScreen(this Vector3 position)
        {
            return Drawing.WorldToScreen(position);
        }

        public static Vector2 WorldToTacticalMap(this Vector3 position)
        {
            return Drawing.WorldToMinimap(position);
        }

        #endregion
    }
}