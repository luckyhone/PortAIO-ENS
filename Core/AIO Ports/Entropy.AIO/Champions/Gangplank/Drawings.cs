using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.Gangplank
{
    class Drawings : DrawingBase
    {
        public Drawings(params Spell[] spells)
        {
            this.Spells = spells;
        }
    }
}