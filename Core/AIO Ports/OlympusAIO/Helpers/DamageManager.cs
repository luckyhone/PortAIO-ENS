using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnsoulSharp;
using EnsoulSharp.SDK;

using OlympusAIO.Champions;

namespace OlympusAIO.Helpers
{
    class DamageManager
    {
        public static float GetDamageByChampion(AIBaseClient target)
        {
            float damage = 0f;

            if (target.IsDead || HasResistantBuff(target))
                return 0f;

            switch (OlympusAIO.objPlayer.CharacterName)
            {
                case "AurelionSol":
                    if (OlympusAIO.objPlayer.CanAttack)
                    {
                        damage += (float)OlympusAIO.objPlayer.GetAutoAttackDamage(target);
                    }
                    if (SpellManager.Q.IsReady())
                    {
                        damage += SpellManager.Q.GetDamage(target);
                    }
                    if (SpellManager.W.IsReady())
                    {
                        damage += SpellManager.W.GetDamage(target);
                    }
                    if (SpellManager.R.IsReady())
                    {
                        damage += SpellManager.R.GetDamage(target);
                    }
                    break;
                case "Evelynn":
                    if (OlympusAIO.objPlayer.CanAttack)
                    {
                        damage += (float)OlympusAIO.objPlayer.GetAutoAttackDamage(target);
                    }
                    if (SpellManager.Q.IsReady())
                    {
                        damage += SpellManager.Q.GetDamage(target, Evelynn.Misc.IsWEmpowered() ? 1 : 0);
                    }
                    if (SpellManager.E.IsReady())
                    {
                        damage += SpellManager.E.GetDamage(target);
                    }
                    if (SpellManager.R.IsReady())
                    {
                        damage += SpellManager.R.GetDamage(target, target.HealthPercent < 30 ? 1 : 0);
                    }
                    break;
                case "Heimerdinger":
                    if (OlympusAIO.objPlayer.CanAttack)
                    {
                        damage += (float)OlympusAIO.objPlayer.GetAutoAttackDamage(target);
                    }
                    if (SpellManager.Q.IsReady())
                    {
                        damage += SpellManager.Q.GetDamage(target);
                    }
                    if (SpellManager.W.IsReady())
                    {
                        damage += SpellManager.W.GetDamage(target);
                    }
                    if (SpellManager.E.IsReady())
                    {
                        damage += SpellManager.E.GetDamage(target);
                    }
                    if (SpellManager.R.IsReady())
                    {
                        damage += SpellManager.R.GetDamage(target);
                    }
                    break;
                case "Lissandra":
                    if (OlympusAIO.objPlayer.CanAttack)
                    {
                        damage += (float)OlympusAIO.objPlayer.GetAutoAttackDamage(target);
                    }
                    if (SpellManager.Q.IsReady())
                    {
                        damage += SpellManager.Q.GetDamage(target);
                    }
                    if (SpellManager.W.IsReady())
                    {
                        damage += SpellManager.W.GetDamage(target);
                    }
                    if (SpellManager.E.IsReady())
                    {
                        damage += SpellManager.E.GetDamage(target);
                    }
                    if (SpellManager.R.IsReady())
                    {
                        damage += SpellManager.R.GetDamage(target);
                    }
                    break;
                case "Poppy":
                    if (OlympusAIO.objPlayer.CanAttack)
                    {
                        damage += (float)OlympusAIO.objPlayer.GetAutoAttackDamage(target);
                    }
                    if (SpellManager.Q.IsReady())
                    {
                        damage += SpellManager.Q.GetDamage(target);
                    }
                    if (SpellManager.E.IsReady())
                    {
                        damage += SpellManager.E.GetDamage(target);
                    }
                    if (SpellManager.R.IsReady())
                    {
                        return (float)Damage.CalculateDamage(OlympusAIO.objPlayer, target, DamageType.Physical, (new double[] { 200, 300, 400 }[SpellManager.R.Level - 1] + 0.9f * (OlympusAIO.objPlayer.BaseAttackDamage + OlympusAIO.objPlayer.FlatPhysicalDamageMod)));
                    }
                    break;
                case "Teemo":
                    break;
            }

            if (target.HasBuff("ManaBarrier"))
                damage += target.Mana / 2f;

            if (target.HasBuff("GarenW"))
                damage += damage * 0.7f;

            return damage;
        }
        public static bool HasResistantBuff(AIBaseClient target)
        {
            foreach (var buff in target.Buffs)
            {
                switch (buff.Name)
                {
                    case "Chrono Shift":
                        return true;
                    case "JudicatorIntervention":
                        return true;
                    case "Undying Rage":
                        return true;
                    case "bansheesveil":
                        return true;
                    case "SivirE":
                        return true;
                    case "NocturneW":
                        return true;
                    case "kindrednodeathbuff":
                        return true;
                }
            }

            return target.HasBuffOfType(BuffType.Invulnerability) || target.HasBuffOfType(BuffType.SpellImmunity);
        }
    }
}
