using EnsoulSharp;
using EnsoulSharp.SDK;

namespace hikiMarksmanRework.Core.Spells
{
    class VayneSpells
    {
        public static Spell Q, W, E, R;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 545f);
            R = new Spell(SpellSlot.R);
        }
    }
}