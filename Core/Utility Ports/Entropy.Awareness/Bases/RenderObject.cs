using Entropy.Lib.Render;
using SharpDX;
using SharpDX.Direct3D9;

namespace Entropy.Awareness.Bases
{
    public abstract class RenderObject
    {
        public Vector2 Offset;

        public string Name { get; }

        public RenderObject(string name, Vector2 offset, int width, int height)
        {
            Width  = width;
            Height = height;
            Name = name;
            Offset = offset;
        }

        public bool IsTexture;

        public Texture Texture;

        public int Width;
        public int Height;

        public abstract void BuildRenderObject(Vector2 position);
        public abstract void UpdateInformation();

        public void Render(Vector2 basePosition)
        {
            if (IsTexture) 
            {
                TextureRendering.Render(basePosition + Offset, Texture);
                return;
            }
            
            BuildRenderObject(basePosition + Offset);
        }
    }
}