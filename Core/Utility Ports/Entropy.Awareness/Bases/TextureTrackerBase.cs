using EnsoulSharp;
using Entropy.Lib.Render;

namespace Entropy.Awareness.Bases
{
    using System;
    using SharpDX;
    using SharpDX.Direct3D9;
    using SharpDX.Mathematics.Interop;

    public abstract class TextureTrackerBase : ITracker
    {
        public string Name { get; }

        public bool TextureWasCreated;
        
        public TextureTrackerBase(string name)
        {
            Name = name;
        }

        public void CreateTexture(int width, int height)
        {
            Width  = width;
            Height = height;
            

            Texture = new Texture(Drawing.Direct3DDevice9,
                                  Width,
                                  Height,
                                  0,
                                  Usage.RenderTarget,
                                  Format.A8R8G8B8,
                                  Pool.Default);
            
            Surface = Texture.GetSurfaceLevel(0);

            TextureWasCreated = true;
        }
        
        private Texture Texture;

        public int Width;
        public int Height;

        private Surface Surface;

        public Vector2 TexturePosition = new Vector2(0, 0);
        
        public abstract void Initialize();

        public void WorldRender()
        {

        }

        public void Render()
        {
            if (!TextureWasCreated)
            {
                return;
            }
            
            Clip(BuildTexture);
            
            TextureRendering.Render(TexturePosition, Texture, 1f);
            
            Clear();
        }

        public void Clear()
        {
            //Cleaning the texture
            Clip(() =>
            {
                Drawing.Direct3DDevice9.Clear(ClearFlags.Target, new RawColorBGRA(0, 0, 0, 0), 1.0f, 0);
            });
        }
        
        public void Clip(Action func)
        {
            Drawing.Direct3DDevice9.SetRenderTarget(0, Surface);

            func();

            Drawing.Direct3DDevice9.SetRenderTarget(0, TrackersCommon.DefaultSurface);
        }

        public abstract void BuildTexture();
        
    }
}