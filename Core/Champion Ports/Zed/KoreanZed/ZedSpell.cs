using EnsoulSharp;
using EnsoulSharp.SDK;

namespace KoreanZed
{
    public class ZedSpell : Spell
    {
        public bool UseOnCombo { get; set; }

        public bool UseOnHarass { get; set; }

        public bool UseOnLastHit { get; set; }

        public bool UseOnLaneClear { get; set; }

        public ZedSpell(SpellSlot slot, float range)
            : base (slot, range)
        {
            UseOnCombo = false;
            UseOnHarass = false;
            UseOnLastHit = false;
            UseOnLaneClear = false;
        }
    }
}