using EnsoulSharp;

namespace Entropy.Lib.Render
{
     #region Using Directives

    using System;
    using SharpDX;
    using SharpDX.Direct3D9;

    #endregion

    /// <summary>
    ///     Class Text
    /// </summary>
    public static class TextRendering
    {
        #region Static Fields

        /// <summary>
        ///     The default font used in rendering if no other font is specified
        /// </summary>
        public static readonly Font DefaultFont = FontFactory.CreateNewFont(20, FontWeight.Bold);

        #endregion

        #region Constructors and Destructors

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Renders text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="color">The color.</param>
        /// <param name="worldPositions">The world positions.</param>
        public static void Render(string content, Color color, params Vector3[] worldPositions)
        {
            Render(content, color, Array.ConvertAll(worldPositions, Drawing.WorldToScreen));
        }

        /// <summary>
        ///     Renders text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="color">The color.</param>
        /// <param name="screenPositions">The screen positions.</param>
        public static void Render(string content, Color color, params Vector2[] screenPositions)
        {
            foreach (var screenPosition in screenPositions)
            {
                DefaultFont.DrawText(null, content, (int) screenPosition.X, (int) screenPosition.Y, color);
            }
        }

        /// <summary>
        ///     Renders text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="color">The color.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="rectangles">The rectangles.</param>
        public static void Render(string content, Color color, FontDrawFlags flags, params Rectangle[] rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                DefaultFont.DrawText(null, content, rectangle, flags, color);
            }
        }

        /// <summary>
        ///     Renders text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="color">The color.</param>
        /// <param name="font">The font.</param>
        /// <param name="worldPositions">The world positions.</param>
        public static void Render(string content, Color color, Font font, params Vector3[] worldPositions)
        {
            Render(content, color, font, Array.ConvertAll(worldPositions, Drawing.WorldToScreen));
        }

        /// <summary>
        ///     Renders text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="color">The color.</param>
        /// <param name="font">The font.</param>
        /// <param name="screenPositions">The screen positions.</param>
        public static void Render(string content, Color color, Font font, params Vector2[] screenPositions)
        {
            foreach (var screenPosition in screenPositions)
            {
                font.DrawText(null, content, (int) screenPosition.X, (int) screenPosition.Y, color);
            }
        }

        /// <summary>
        ///     Renders text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="color">The color.</param>
        /// <param name="font">The font.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="rectangles">The rectangles.</param>
        public static void Render(
            string             content,
            Color              color,
            Font               font,
            FontDrawFlags      flags,
            params Rectangle[] rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                font.DrawText(null, content, rectangle, flags, color);
            }
        }

        #endregion
    }
}