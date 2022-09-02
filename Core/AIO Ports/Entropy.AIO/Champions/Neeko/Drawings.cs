using EnsoulSharp.SDK;

namespace Entropy.AIO.Neeko
{
    using Bases;

    class Drawings : DrawingBase
    {
        public Drawings(params Spell[] spells)
        {
            this.Spells = spells;
        }
    }
}