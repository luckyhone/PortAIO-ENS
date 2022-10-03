using EnsoulSharp;
using EnsoulSharp.SDK;

namespace hikiMarksmanRework.Core.Spells
{
    class SivirSpells
    {
        public static Spell Q, W, E, R;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 1245f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 1000f);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SpellType.Line);

        }
    }
}