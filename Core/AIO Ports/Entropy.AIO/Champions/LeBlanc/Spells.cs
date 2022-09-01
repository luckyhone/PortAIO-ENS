using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.LeBlanc
{
    using static ChampionBase;
    class Spells
    {
        public Spells()
        {
            Initialize();
        }

        public static Spell RQ { get; set; }
        public static Spell RW { get; set; }
        public static Spell RE { get; set; }

        private static void Initialize()
        {
            Q = new Spell(SpellSlot.Q, 700f);
            W = new Spell(SpellSlot.W, 600f);
            E = new Spell(SpellSlot.E, 925F);
            R = new Spell(SpellSlot.R);

            RQ = new Spell(SpellSlot.R, 700f);
            RW = new Spell(SpellSlot.R, 600f);
            RE = new Spell(SpellSlot.R, 925F);

            W.SetSkillshot(0.25f, 215f, 1450f, false,SpellType.Circle);
            E.SetSkillshot(0.25f, 110f, 1750f,true,SpellType.Line);

            RW.SetSkillshot(0.25f, 215f, 1450f, false,SpellType.Circle);
            RE.SetSkillshot(0.25f, 110f, 1750f,true,SpellType.Line);

            Q.SetCustomDamage(Misc.Damage.QDamage);
            W.SetCustomDamage(Misc.Damage.WDamage);
            E.SetCustomDamage(Misc.Damage.EDamage);
            R.SetCustomDamage(Misc.Damage.RDamage);
        }
    }
}