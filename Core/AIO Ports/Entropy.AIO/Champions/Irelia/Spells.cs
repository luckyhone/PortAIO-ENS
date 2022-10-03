using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.Irelia
{
    using static ChampionBase;
    class Spells
    {
        public Spells()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W, 825f);
            E = new Spell(SpellSlot.E, 850f);
            R = new Spell(SpellSlot.R, 850f);

            W.SetSkillshot(0.25f, 120f, 230f, false,SpellType.Line);
            E.SetSkillshot(0f, 20f, 2000f, false,SpellType.Line);
            R.SetSkillshot(0.40f, 160f, 2000f, false,SpellType.Line);

            Q.SetCustomDamage(Misc.Damage.QDamage);
            W.SetCustomDamage(Misc.Damage.WDamage);
            E.SetCustomDamage(Misc.Damage.EDamage);
            R.SetCustomDamage(Misc.Damage.RDamage);
        }
    }
}