using EnsoulSharp;
using EnsoulSharp.SDK;

namespace GangplankBuddy
{
    class SpellManager
    {
        //public static Spell Q = new Spell.Targeted(SpellSlot.Q, 625);
        //public static Spell.Active W = new Spell.Active(SpellSlot.W);
        //public static Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 1200, SkillShotType.Circular);
        //public static Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, uint.MaxValue, SkillShotType.Circular);
        public static Spell Q = new Spell(SpellSlot.Q, 625);
        public static Spell W = new Spell(SpellSlot.W);
        public static Spell E = new Spell(SpellSlot.E, 1200f);
        public static Spell R = new Spell(SpellSlot.R, float.MaxValue);

    }
}