using EnsoulSharp;
using EnsoulSharp.SDK;

namespace KoreanZed
{
    class ZedSpells
    {
        public ZedSpell Q { get; set; }
        public ZedSpell W { get; set; }
        public ZedSpell E { get; set; }
        public ZedSpell R { get; set; }

        public ZedSpells()
        {
            Q = new ZedSpell(SpellSlot.Q, 925F);
            Q.SetSkillshot(0.25F, 50F, 1600F, false, SpellType.Line);

            float wRange = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange;
            W = new ZedSpell(SpellSlot.W, wRange);
            W.SetSkillshot(0.75F, 75F, 1000F, false, SpellType.Line);

            float eRange = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange - 10;
            E = new ZedSpell(SpellSlot.E, eRange);

            float rRange = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange;
            R = new ZedSpell(SpellSlot.R, rRange);
        }
    }
}