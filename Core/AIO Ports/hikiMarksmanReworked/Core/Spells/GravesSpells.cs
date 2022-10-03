using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Color = System.Drawing.Color;

namespace hikiMarksmanRework.Core.Spells
{
    class GravesSpells
    {
        public static Spell Q, W, E, R;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, 1100f);

            Q.SetSkillshot(0.25f, 45f, 2000f, false, SpellType.Line);
            W.SetSkillshot(0.25f, 250f, 1650f, false, SpellType.Circle);
            R.SetSkillshot(0.25f, 100f, 2100f, false, SpellType.Line);

        }

    }
}