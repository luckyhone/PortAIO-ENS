using EnsoulSharp;

namespace Entropy.Lib.Render
{
    #region Using Directives

    using System;
    using SharpDX;
    using SharpDX.Direct3D9;

    #endregion

    /// <summary>
    ///     Class CircleRendering2D
    /// </summary>
    public static class CircleRendering2D
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes the <see cref="CircleRendering2D" /> class.
        /// </summary>
        static CircleRendering2D()
        {
            Initialize();
        }

        #endregion

        #region Public Methods and Operators

        public static void Render(
            Vector2 screenPosition,
            float   radius,
            Color   color,
            float   borderWidth,
            bool    filled = false)
        {
            // Check if the effect is disposed
            if (Effect.IsDisposed)
            {
                return;
            }

            // Save the current VertexDecleration for restoring later
            var decleration = Drawing.Direct3DDevice9.VertexDeclaration;

            // Begin the shading process
            Effect.Begin();

            // Send data to the GPU using the Direct3DDevice
            Drawing.Direct3DDevice9.SetStreamSource(0, VertexBuffer, 0, Utilities.SizeOf<Vector4>());
            Drawing.Direct3DDevice9.VertexDeclaration = VertexDeclaration;

            var renderTarget = Drawing.Direct3DDevice9.GetRenderTarget(0);

            // Send all the global variables to the shader
            Effect.BeginPass(0);
            Effect.SetValue("Transform", Matrix.OrthoOffCenterLH(0, renderTarget.Description.Width, renderTarget.Description.Height, 0, -1, 1));
            Effect.SetValue("Color", new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));

            Effect.SetValue("Radius", radius);
            Effect.SetValue("Filled", filled);
            Effect.SetValue("BorderWidth", borderWidth);
            Effect.SetValue("Center", screenPosition);

            Effect.CommitChanges();

            Effect.EndPass();

            // Draw the primitives in the shader
            Drawing.Direct3DDevice9.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            // End the shading process
            Effect.End();
            
            // Restore the previous VertexDecleration
            Drawing.Direct3DDevice9.VertexDeclaration = decleration;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the effect.
        /// </summary>
        /// <value>
        ///     The effect.
        /// </value>
        private static Effect Effect;

        /// <summary>
        ///     Gets or sets the technique.
        /// </summary>
        /// <value>
        ///     The technique.
        /// </value>
        private static EffectHandle Technique;

        /// <summary>
        ///     Gets or sets the vertex buffer.
        /// </summary>
        /// <value>
        ///     The vertex buffer.
        /// </value>
        private static VertexBuffer VertexBuffer;

        /// <summary>
        ///     Gets or sets the vertex declaration.
        /// </summary>
        /// <value>
        ///     The vertex declaration.
        /// </value>
        private static VertexDeclaration VertexDeclaration;

        /// <summary>
        ///     Gets or sets the vertex elements.
        /// </summary>
        /// <value>
        ///     The vertex elements.
        /// </value>
        private static VertexElement[] VertexElements;

        #endregion

        #region Methods

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        private static void Dispose()
        {
            Effect?.Dispose();
            Effect = null;

            Technique?.Dispose();
            Technique = null;

            VertexBuffer?.Dispose();
            VertexBuffer = null;

            VertexDeclaration?.Dispose();
            VertexDeclaration = null;
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        private static void Initialize()
        {
            // Initialize the vertex buffer, specifying its size, usage, format and pool
            VertexBuffer = new VertexBuffer(Drawing.Direct3DDevice9,
                                            Utilities.SizeOf<Vector4>() * 4,
                                            Usage.WriteOnly,
                                            VertexFormat.None,
                                            Pool.Managed);

            // Lock and write the vertices onto the vertex buffer
            VertexBuffer.Lock(0, 0, LockFlags.None).
                         WriteRange(new[]
                         {
                             new Vector4(0, Drawing.Height, 1, 1),                          // bottom left
                             new Vector4(0, 0, 1, 1),                                                   // top left
                             new Vector4(Drawing.Width, Drawing.Height, 1, 1), // bottom right
                             new Vector4(Drawing.Width, 0, 1, 1)                           // top right
                         });
            VertexBuffer.Unlock();

            // Specify the vertex elements to be used by the shader
            VertexElements = new[]
            {
                new VertexElement(0,
                                  0,
                                  DeclarationType.Float4,
                                  DeclarationMethod.Default,
                                  DeclarationUsage.Position,
                                  0),
                VertexElement.VertexDeclarationEnd
            };

            // Initialize the vertex decleration using the previously created vertex elements
            VertexDeclaration = new VertexDeclaration(Drawing.Direct3DDevice9, VertexElements);

            #region Effect binary

            var compiledEffect = new byte[]
            {
            };

            const string effectSource = @"
                        struct VS_OUTPUT
                        {
                            float4 Position   : POSITION;
                            float4 Color      : COLOR0;
                        	float4 Position2D : TEXCOORD0;
                        };
                        
                        // Globals passed
                        float4 Color;
                        float4x4 Transform;
                        float Radius;
                        float BorderWidth;
                        float2 Center;
                        bool Filled;
                        // Vertex Shader
                        VS_OUTPUT VS(VS_OUTPUT input)
                        {
                            VS_OUTPUT output = (VS_OUTPUT) 0;
                            output.Position = mul(input.Position, Transform);
                            output.Color = input.Color;
                        	output.Position2D = input.Position;
                            return output;
                        }
                        
                        // Pixel Shader
                        float4 PS(VS_OUTPUT input) : COLOR
                        {
                            VS_OUTPUT output = (VS_OUTPUT) 0;
                            output = input;
                            
                            output = input;
                            output.Color.x = Color.x;
                            output.Color.y = Color.y;
                            output.Color.z = Color.z;
                            output.Color.w = 0;
                        
                        	float true_radius_outer = Radius + BorderWidth / 2;
                        	float true_radius_inner = true_radius_outer - BorderWidth;
                        	
                        	float4 cpos = input.Position2D;
                        	float dist_sq = pow(cpos.x-Center.x, 2) + pow(cpos.y-Center.y, 2);
                        	
                        	if (dist_sq > pow(true_radius_outer, 2)) {
                        		return output.Color;
                        	}
                        	if (dist_sq < pow(true_radius_inner, 2) && !Filled) {
                        		return output.Color;
                        	}
                        	output.Color.w = Color.w;
                        	return output.Color;
                        }
                        
                        technique Movement
                        {
                            pass P0
                            {
                                ZEnable = FALSE;
                                AlphaBlendEnable = TRUE;
                                DestBlend = INVSRCALPHA;
                                SrcBlend = SRCALPHA;
                                VertexShader = compile vs_2_0 VS();
                                PixelShader  = compile ps_2_0 PS();
                            }
                        }";

            #endregion

            // Load the effect from memory
            //Effect = Effect.FromMemory(Renderer.Direct3DDevice, compiledEffect, ShaderFlags.None);
            Effect = Effect.FromString(Drawing.Direct3DDevice9, effectSource, ShaderFlags.None);

            // Set the only technique in the shaders
            Technique = Effect.GetTechnique(0);

            // Listen to events
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => Dispose();
            AppDomain.CurrentDomain.ProcessExit  += (sender, args) => Dispose();

            Drawing.OnPostReset     += args => Effect?.OnLostDevice();
            Drawing.OnPostReset += args => Effect?.OnResetDevice();
        }

        #endregion
    }
}