using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace hikiMarksmanRework.Core.Spells
{
    class EzrealSpells
    {
        public static Spell Q, W, E, R;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 1180f);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 475f);
            R = new Spell(SpellSlot.R, 2500f);

            Q.SetSkillshot(0.25f, 60f, 2000f, true, SpellType.Line);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SpellType.Line);
            R.SetSkillshot(1f, 160f, 2000f, false, SpellType.Line);

        }
    }
}