using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Evade
{
    static class Helpers
    {
        public static bool IsSpellShielded(this AIHeroClient unit)
        {
            if (ObjectManager.Player.HasBuffOfType(BuffType.SpellShield))
            {
                return true;
            }

            if (ObjectManager.Player.HasBuffOfType(BuffType.SpellImmunity))
            {
                return true;
            }

            //Sivir E
            if (unit.GetLastCastedSpell().Name == "SivirE" && (Utils.TickCount - unit.GetLastCastedSpell().StartTime) < 300)
            {
                return true;
            }

            //Morganas E
            if (unit.GetLastCastedSpell().Name == "BlackShield" && (Utils.TickCount - unit.GetLastCastedSpell().StartTime) < 300)
            {
                return true;
            }

            //Nocturnes E
            if (unit.GetLastCastedSpell().Name == "NocturneShit" && (Utils.TickCount - unit.GetLastCastedSpell().StartTime) < 300)
            {
                return true;
            }

            return false;
        }
    }
}