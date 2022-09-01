using SharpDX;

namespace Entropy.Awareness.Models
{
    public class ResolutionOffset
    {
        public Vector2 Resolution { get; set; }

        public Vector2 Offset { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}