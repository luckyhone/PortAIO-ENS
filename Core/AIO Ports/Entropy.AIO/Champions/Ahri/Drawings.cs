using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.Ahri
{
    class Drawings : DrawingBase
    {
        public Drawings(params Spell[] spells)
        {
            this.Spells = spells;
        }
    }
}