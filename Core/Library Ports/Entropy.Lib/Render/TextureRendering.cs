using EnsoulSharp;

namespace Entropy.Lib.Render
{
    #region Using Directives

    using System;
    using SharpDX;
    using SharpDX.Direct3D9;

    #endregion

    /// <summary>
    ///     Class Texture
    /// </summary>
    public static class TextureRendering
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes the <see cref="Sprite" /> class.
        /// </summary>
        static TextureRendering()
        {
            Initialize();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the cached sprite.
        /// </summary>
        /// <value>
        ///     The cached sprite.
        /// </value>
        private static Sprite CachedSprite;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is drawing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is drawing; otherwise, <c>false</c>.
        /// </value>
        private static bool IsDrawing;

        #endregion

        #region Public Methods and Operators

        private static readonly Color DefaultColor = Color.White;

        /// <summary>
        ///     Renders a sprite.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texture">The texture.</param>
        public static void Render(
            Vector2 position,
            Texture texture)
        {
            Render(position, texture, DefaultColor);
        }

        /// <summary>
        ///     Draws the sprite on the screen
        /// </summary>
        /// <param name="position">The position to draw at</param>
        /// <param name="texture"></param>
        /// <param name="rectangle">The rectangle to draw from</param>
        public static void Render(Vector2 position, Texture texture, Rectangle? rectangle)
        {
            Render(position, texture, DefaultColor, null, rectangle);
        }

        /// <summary>
        ///     Renders a sprite.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="scale">The scale.</param>
        public static void Render(
            Vector2 position,
            Texture texture,
            float   scale)
        {
            // Make sure we can render
            if (CachedSprite == null || CachedSprite.IsDisposed)
            {
                return;
            }

            // Check if the sprite is currently being drawn
            if (IsDrawing)
            {
                return;
            }

            IsDrawing = true;
            // If not, begin the drawing process
            CachedSprite.Begin(SpriteFlags.AlphaBlend);

            // Save the old tranformation for restoring later
            var oldTransform = CachedSprite.Transform;

            CachedSprite.Transform *= Matrix.Translation(new Vector3(0, 0, 0));
            CachedSprite.Transform *= Matrix.Scaling(scale);
            CachedSprite.Transform *= Matrix.Translation(new Vector3(position, 0));

            // Transform the sprite and draw it
            CachedSprite.Draw(texture, DefaultColor);

            // Restore the previous transform
            CachedSprite.Transform = oldTransform;

            // Finish the drawing sequence and release control of the object
            IsDrawing = false;
            CachedSprite.End();
        }
        

        /// <summary>
        ///     Renders a sprite.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="backgroundColor">The color.</param>
        /// <param name="center">The center.</param>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        public static void Render(
            Vector2    position,
            Texture    texture,
            Color      backgroundColor,
            Vector3?   center          = null,
            Rectangle? rectangle       = null,
            float?     rotation        = null,
            Vector2?   scale           = null)
        {
            // Make sure we can render
            if (CachedSprite == null || CachedSprite.IsDisposed)
            {
                return;
            }

            // Check if the sprite is currently being drawn
            if (IsDrawing)
            {
                return;
            }

            IsDrawing = true;
            // If not, begin the drawing process
            CachedSprite.Begin(SpriteFlags.AlphaBlend);

            // If there is no need to apply and properties, draw the sprite
            if (rotation == null || scale == null)
            {
                CachedSprite.Draw(texture,
                                  backgroundColor,
                                  rectangle,
                                  center,
                                  new Vector3(position, 0) + (center ?? Vector3.Zero));

                // Finish the drawing sequence and release control of the object
                IsDrawing = false;
                CachedSprite.End();

                // Return from the function
                return;
            }

            // Save the old tranformation for restoring later
            var oldTransform = CachedSprite.Transform;

            // Transform the sprite and draw it
            CachedSprite.Transform *= Matrix.Scaling(new Vector3((Vector2) scale, 0)) *
                                      Matrix.RotationZ((float) rotation)              *
                                      Matrix.Translation(new Vector3(position, 0) + (center ?? Vector3.Zero));

            CachedSprite.Draw(texture, backgroundColor, rectangle, center);

            // Restore the previous transform
            CachedSprite.Transform = oldTransform;

            // Finish the drawing sequence and release control of the object
            IsDrawing = false;
            CachedSprite.End();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        private static void Dispose()
        {
            CachedSprite?.Dispose();
            CachedSprite = null;
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        private static void Initialize()
        {
            // Intialize the sprite now so we don't have to intialize it with every frame
            CachedSprite = new Sprite(Drawing.Direct3DDevice9);

            // Listen to events
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => Dispose();
            AppDomain.CurrentDomain.ProcessExit  += (sender, args) => Dispose();

            Drawing.OnPreReset     += args => CachedSprite?.OnLostDevice();
            Drawing.OnPostReset += args => CachedSprite?.OnResetDevice();
        }

        #endregion
    }
}