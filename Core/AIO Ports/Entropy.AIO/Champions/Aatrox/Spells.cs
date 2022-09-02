using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Aatrox
{
    using static Bases.ChampionBase;

    class Spells
    {
        public Spells()
        {
            Initialize();
        }

        public static Spell DashQ { get; set; }

        private static void Initialize()
        {
            Q     = new Spell(SpellSlot.Q, 650);
            DashQ = new Spell(SpellSlot.Q, 650);
            W     = new Spell(SpellSlot.W, 800);
            E     = new Spell(SpellSlot.E, 300);
            R     = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.6f, 120f, float.MaxValue, false,SpellType.Line);
            W.SetSkillshot(0.25f, 80f, 1800f,true,SpellType.Line);

            Q.SetCustomDamage(Misc.Damage.GetQDamage);
            W.SetCustomDamage(Misc.Damage.GetWDamage);
        }
    }
}