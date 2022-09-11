using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius
{
    using Misc;
    using static Bases.ChampionBase;

    class Spells
    {
        public Spells()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Q = new Spell(SpellSlot.Q, 415);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 520);
            R = new Spell(SpellSlot.R, 460);

            E.SetSkillshot(0.27f, 45, float.MaxValue,false, SpellType.Cone);

            W.SetCustomDamage(Damage.GetWDamage);
            R.SetCustomDamage(Damage.GetRDamage);
        }
    }
}