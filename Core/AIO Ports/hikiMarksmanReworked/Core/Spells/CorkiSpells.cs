using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace hikiMarksmanRework.Core.Spells
{
    class CorkiSpells
    {
        public static Spell Q, W, E, R,BIG;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 800);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 1300);
            BIG = new Spell(SpellSlot.R, 1500);

            Q.SetSkillshot(0.50f, 250f, 1135f, false, SpellType.Circle);
            W.SetSkillshot(0.25f, 450, 1200, false, SpellType.Line);
            E.SetSkillshot(0f, (float)(45 * Math.PI / 180), 1500, false, SpellType.Cone);
            R.SetSkillshot(0.2f, 40f, 2000f, true, SpellType.Line);
            BIG.SetSkillshot(0.25f, 100f, 2000f, true, SpellType.Line);

        }
    }
}