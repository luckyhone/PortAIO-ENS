using EnsoulSharp;

namespace Entropy.Lib.Render
{
    using System;
    using SharpDX;
    using SharpDX.Direct3D9;

    public static class RectangleRendering
    {
        #region Constructors and Destructors

        static RectangleRendering()
        {
            Initialize();
        }

        #endregion

        #region Public Methods and Operators

        public static void Render(Vector2 screenStart, Vector2 screenEnd, float borderWidth, Color color, Color borderColor)
        {
            Render(screenStart, screenEnd.X - screenStart.X, screenEnd.Y - screenStart.Y, borderWidth, color, borderColor);
        }

        public static void Render(Vector2 screenStart, Vector2 screenEnd, Color color)
        {
            Render(screenStart, screenEnd.X - screenStart.X, screenEnd.Y - screenStart.Y, 0f, color, color);
        }

        public static void Render(
            Vector2 screenPosition,
            float   width,
            float   height,
            Color   color)
        {
            Render(screenPosition, width, height, 0f, color, color);
        }

        private static readonly Color blank = new Color(0, 0, 0, 0);

        public static void RenderOutline(
            Vector2 screenPosition,
            float   width,
            float   height,
            float   borderWidth,
            Color   color)
        {
            Render(screenPosition, width, height, borderWidth, blank, color);
        }

        public static void Render(
            Vector2 screenPosition,
            float   width,
            float   height,
            float   borderWidth,
            Color   color,
            Color   borderColor)
        {
            // Check if the effect is disposed
            if (effect.IsDisposed)
            {
                return;
            }

            // Save the current VertexDeclaration for restoring later
            var declaration = Drawing.Direct3DDevice9.VertexDeclaration;

            // Begin the shading process
            effect.Begin();

            // Send data to the GPU using the Direct3DDevice
            Drawing.Direct3DDevice9.SetStreamSource(0, vertexBuffer, 0, Utilities.SizeOf<Vector4>());
            Drawing.Direct3DDevice9.VertexDeclaration = vertexDeclaration;

            var renderTarget = Drawing.Direct3DDevice9.GetRenderTarget(0);

            // Send all the global variables to the shader
            effect.BeginPass(0);
            effect.SetValue("Transform", Matrix.OrthoOffCenterLH(0, renderTarget.Description.Width, renderTarget.Description.Height, 0, -1, 1));
            effect.SetValue("Color", new Vector4(color.R             / 255f, color.G       / 255f, color.B       / 255f, color.A       / 255f));
            effect.SetValue("BorderColor", new Vector4(borderColor.R / 255f, borderColor.G / 255f, borderColor.B / 255f, borderColor.A / 255f));

            effect.SetValue("Width", width);
            effect.SetValue("Height", height);
            effect.SetValue("Position", screenPosition);
            effect.SetValue("BorderWidth", borderWidth);

            effect.CommitChanges();

            effect.EndPass();

            // Draw the primitives in the shader
            Drawing.Direct3DDevice9.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            // End the shading process
            effect.End();

            // Restore the previous VertexDeclaration
            Drawing.Direct3DDevice9.VertexDeclaration = declaration;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the effect.
        /// </summary>
        /// <value>
        ///     The effect.
        /// </value>
        private static Effect effect;

        /// <summary>
        ///     Gets or sets the technique.
        /// </summary>
        /// <value>
        ///     The technique.
        /// </value>
        private static EffectHandle technique;

        /// <summary>
        ///     Gets or sets the vertex buffer.
        /// </summary>
        /// <value>
        ///     The vertex buffer.
        /// </value>
        private static VertexBuffer vertexBuffer;

        /// <summary>
        ///     Gets or sets the vertex declaration.
        /// </summary>
        /// <value>
        ///     The vertex declaration.
        /// </value>
        private static VertexDeclaration vertexDeclaration;

        /// <summary>
        ///     Gets or sets the vertex elements.
        /// </summary>
        /// <value>
        ///     The vertex elements.
        /// </value>
        private static VertexElement[] vertexElements;

        #endregion

        #region Methods

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        private static void Dispose()
        {
            effect?.Dispose();
            effect = null;

            technique?.Dispose();
            technique = null;

            vertexBuffer?.Dispose();
            vertexBuffer = null;

            vertexDeclaration?.Dispose();
            vertexDeclaration = null;
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        private static void Initialize()
        {
            // Initialize the vertex buffer, specifying its size, usage, format and pool
            vertexBuffer = new VertexBuffer(Drawing.Direct3DDevice9,
                                            Utilities.SizeOf<Vector4>() * 4,
                                            Usage.WriteOnly,
                                            VertexFormat.None,
                                            Pool.Managed);

            // Lock and write the vertices onto the vertex buffer
            vertexBuffer.Lock(0, 0, LockFlags.None).
                         WriteRange(new[]
                         {
                             new Vector4(0, Drawing.Height, 1, 1),                          // bottom left
                             new Vector4(0, 0, 1, 1),                                                   // top left
                             new Vector4(Drawing.Width, Drawing.Height, 1, 1), // bottom right
                             new Vector4(Drawing.Width, 0, 1, 1)                           // top right
                         });
            vertexBuffer.Unlock();

            // Specify the vertex elements to be used by the shader
            vertexElements = new[]
            {
                new VertexElement(0,
                                  0,
                                  DeclarationType.Float4,
                                  DeclarationMethod.Default,
                                  DeclarationUsage.Position,
                                  0),
                VertexElement.VertexDeclarationEnd
            };

            // Initialize the vertex declaration using the previously created vertex elements
            vertexDeclaration = new VertexDeclaration(Drawing.Direct3DDevice9, vertexElements);

            #region Effect binary

            const string effectSource = @"struct VS_OUTPUT {
  float4 Position: POSITION;
  float4 Color: COLOR0;
  float4 Position2D: TEXCOORD0;
};
// Globals passed
float4 Color;
float4 BorderColor;
float4x4 Transform;
float Width;
float Height;
float BorderWidth;
float2 Position;
// Vertex Shader
VS_OUTPUT VS(VS_OUTPUT input) {
  VS_OUTPUT output = (VS_OUTPUT) 0;
  output.Position = mul(input.Position, Transform);
  output.Color = input.Color;
  output.Position2D = input.Position;
  return output;
}
float4 PS(VS_OUTPUT input): COLOR {
  VS_OUTPUT output = (VS_OUTPUT) 0;
  output = input;
  float4 cpos = input.Position2D;
  //Inside rectangle
  if (cpos.x < Position.x + Width &&
      cpos.x > Position.x &&
      cpos.y < Position.y + Height &&
      cpos.y > Position.y)
  {
    output.Color.x = Color.x;
    output.Color.y = Color.y;
    output.Color.z = Color.z;
    output.Color.w = Color.w;
    return output.Color;
  }
  //Outside rectangle
  if (cpos.x < Position.x + Width + BorderWidth &&
      cpos.x > Position.x - BorderWidth &&
      cpos.y < Position.y + Height + BorderWidth &&
      cpos.y > Position.y - BorderWidth)
  {
    output.Color.x = BorderColor.x;
    output.Color.y = BorderColor.y;
    output.Color.z = BorderColor.z;
    output.Color.w = BorderColor.w;
    return output.Color;
  }
  output.Color.x = 0;
  output.Color.y = 0;
  output.Color.z = 0;
  output.Color.w = 0;
  return output.Color;
}
technique Movement {
  pass P0 {
    ZEnable = FALSE;
    AlphaBlendEnable = TRUE;
    DestBlend = InvSrcAlpha;
    SrcBlend = SrcAlpha;
    VertexShader = compile vs_2_0 VS();
    PixelShader = compile ps_2_0 PS();
  }
}";

            #endregion

            // Load the effect from memory
            //Effect = Effect.FromMemory(Renderer.Direct3DDevice, compiledEffect, ShaderFlags.None);
            effect = Effect.FromString(Drawing.Direct3DDevice9, effectSource, ShaderFlags.None);

            // Set the only technique in the shaders
            technique = effect.GetTechnique(0);

            // Listen to events
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => Dispose();
            AppDomain.CurrentDomain.ProcessExit  += (sender, args) => Dispose();

            Drawing.OnPreReset     += args => effect?.OnLostDevice();
            Drawing.OnPostReset += args => effect?.OnResetDevice();
        }

        #endregion
    }
}