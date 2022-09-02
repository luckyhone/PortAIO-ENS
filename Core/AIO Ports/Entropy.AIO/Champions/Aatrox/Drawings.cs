using EnsoulSharp.SDK;

namespace Entropy.AIO.Aatrox
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