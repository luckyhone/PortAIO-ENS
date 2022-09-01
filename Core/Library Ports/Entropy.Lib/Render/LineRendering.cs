using EnsoulSharp;

namespace Entropy.Lib.Render
{
    #region Using Directives

    using System;
    using SharpDX;
    using SharpDX.Direct3D9;
    using SharpDX.Mathematics.Interop;

    #endregion

    /// <summary>
    ///     Class Line
    /// </summary>
    public static class LineRendering
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes the <see cref="LineRendering" /> class.
        /// </summary>
        static LineRendering()
        {
            Initialize();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the cached line.
        /// </summary>
        /// <value>
        ///     The cached line.
        /// </value>
        private static SharpDX.Direct3D9.Line CachedLine;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is drawing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is drawing; otherwise, <c>false</c>.
        /// </value>
        private static bool IsDrawing;

        #endregion

        #region Public Methods and Operators

        public static void Render(Color color, float thickness, params Vector2[] screenPositions)
        {
            Render(color, thickness, Array.ConvertAll(screenPositions, v => new RawVector2(v.X, v.Y)));
        }

        /// <summary>
        ///     Renders a line.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="screenPositions">The screen positions.</param>
        public static void Render(Color color, float thickness, params RawVector2[] screenPositions)
        {
            // Make sure we can draw the line
            if (screenPositions.Length < 2 || thickness < float.Epsilon)
            {
                return;
            }

            // Check if the line is still being drawn
            if (!IsDrawing)
            {
                // If not, set the line parameters
                CachedLine.Width = thickness;

                // And begin the drawing process
                CachedLine.Begin();
                IsDrawing = true;
            }

            // Draw the cached line
            CachedLine.Draw(screenPositions, color);

            // Finish drawing and release control of the object
            IsDrawing = false;
            CachedLine.End();
        }

        /// <summary>
        ///     Renders a line.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="worldPositions">The world positions.</param>
        public static void Render(Color color, float thickness, params Vector3[] worldPositions)
        {
            // Convert the world space positions to screen space positions
            var screenPositions = Array.ConvertAll(worldPositions, Drawing.WorldToScreen);

            // Draw the screen space positions
            Render(color, thickness, screenPositions);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        private static void Dispose()
        {
            CachedLine?.Dispose();
            CachedLine = null;
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        private static void Initialize()
        {
            // Set the cached line so we don't have to intiialize it every frame
            CachedLine = new SharpDX.Direct3D9.Line(Drawing.Direct3DDevice9);

            // Listen to events
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => Dispose();
            AppDomain.CurrentDomain.ProcessExit  += (sender, args) => Dispose();

            Drawing.OnPreReset     += args => CachedLine?.OnLostDevice();
            Drawing.OnPostReset += args => CachedLine?.OnResetDevice();
        }

        #endregion
    }
}