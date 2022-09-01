

using EnsoulSharp;
using EnsoulSharp.SDK;
using Damage = Entropy.AIO.Ahri.Misc.Damage;
using static Entropy.AIO.Bases.ChampionBase;

namespace Entropy.AIO.Ahri
{
    class Spells
    {
        public Spells()
        {
            Initialize();
        }
        

        private static void Initialize()
        {
            Q = new Spell(SpellSlot.Q, 880f);
            W = new Spell(SpellSlot.W, 725f);
            E = new Spell(SpellSlot.E, 975f);
            R = new Spell(SpellSlot.R, 450f);

            Q.SetSkillshot(0.25f, 100f, 2500,false,SpellType.Line);
            E.SetSkillshot(0.25f, 60f, 1550,true,SpellType.Line);

            Q.SetCustomDamage(Damage.Q);
            W.SetCustomDamage(Damage.W);
            E.SetCustomDamage(Damage.E);
            R.SetCustomDamage(Damage.R);
        }
    }
}