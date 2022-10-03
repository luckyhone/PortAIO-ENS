using EnsoulSharp;
using EnsoulSharp.SDK;
using SpellType = EnsoulSharp.SDK.SpellType;

namespace hikiMarksmanRework.Core.Spells
{
    class DravenSpells
    {
        public static Spell Q, W, E, R;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 20000);

            E.SetSkillshot(0.25f, 130f, 1400f, false,SpellType.Line);
            R.SetSkillshot(0.4f, 160f, 2000f, true,SpellType.Line);

        }
    }
}