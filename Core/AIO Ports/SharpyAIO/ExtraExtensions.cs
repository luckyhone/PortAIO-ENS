using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Sharpy_AIO
{
    internal static class ExtraExtensions
    {
        internal static bool IsReadyPerfectly(this Spell spell)
        {
            return spell != null && spell.Slot != SpellSlot.Unknown && spell.Instance.State != SpellState.Cooldown &&
                   spell.Instance.State != SpellState.Disabled && spell.Instance.State != SpellState.NoMana &&
                   spell.Instance.State != SpellState.NotLearned && spell.Instance.State != SpellState.Surpressed &&
                   spell.Instance.State != SpellState.Unknown && spell.Instance.State == SpellState.Ready;
        }
        
        internal static bool IsKillableAndValidTarget(this AIHeroClient target, double calculatedDamage,
            DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.Name == "gangplankbarrel")
                return false;

            if (target.HasBuff("kindredrnodeathbuff"))
            {
                return false;
            }

            // Tryndamere's Undying Rage (R)
            if (target.HasBuff("Undying Rage"))
            {
                return false;
            }

            // Kayle's Intervention (R)
            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            // Poppy's Diplomatic Immunity (R)
            if (target.HasBuff("DiplomaticImmunity") && !ObjectManager.Player.HasBuff("poppyulttargetmark"))
            {
                return false;
            }

            // Banshee's Veil (PASSIVE)
            if (target.HasBuff("BansheesVeil"))
            {
                return false;
            }

            // Sivir's Spell Shield (E)
            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            // Nocturne's Shroud of Darkness (W)
            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            if (target.CharacterName == "Blitzcrank")
                if (!target.HasBuff("manabarriercooldown"))
                    if (target.Health + target.HPRegenRate +
                        (damageType == DamageType.Physical ? target.PhysicalShield : target.MagicalShield) +
                        target.Mana * 0.6 + target.PARRegenRate < calculatedDamage)
                        return true;

            if (target.CharacterName == "Garen")
                if (target.HasBuff("GarenW"))
                    calculatedDamage *= 0.7;

            if (target.HasBuff("FerociousHowl"))
                calculatedDamage *= 0.3;

            return target.Health + target.HPRegenRate +
                   (damageType == DamageType.Physical ? target.PhysicalShield : target.MagicalShield) <
                   calculatedDamage - 2;
        }

        internal static bool IsKillableAndValidTarget(this AIMinionClient target, double calculatedDamage,
            DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.Health <= 0 ||
                target.HasBuffOfType(BuffType.SpellImmunity) || target.HasBuffOfType(BuffType.SpellShield) ||
                target.Name == "gangplankbarrel")
                return false;

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            var dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (dragonSlayerBuff != null)
            {
                if (dragonSlayerBuff.Count >= 4)
                    calculatedDamage += dragonSlayerBuff.Count == 5 ? calculatedDamage * 0.30 : calculatedDamage * 0.15;

                if (target.Name.ToLowerInvariant().Contains("dragon"))
                    calculatedDamage *= 1 - dragonSlayerBuff.Count * 0.07;
            }

            if (target.Name.ToLowerInvariant().Contains("baron") &&
                ObjectManager.Player.HasBuff("barontarget"))
                calculatedDamage *= 0.5;

            return target.Health + target.HPRegenRate +
                   (damageType == DamageType.Physical ? target.PhysicalShield : target.MagicalShield) <
                   calculatedDamage - 2;
        }

        internal static bool IsKillableAndValidTarget(this AIBaseClient target, double calculatedDamage,
            DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.Name == "gangplankbarrel")
                return false;

            if (target.HasBuff("kindredrnodeathbuff"))
            {
                return false;
            }

            // Tryndamere's Undying Rage (R)
            if (target.HasBuff("Undying Rage"))
            {
                return false;
            }

            // Kayle's Intervention (R)
            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            // Poppy's Diplomatic Immunity (R)
            if (target.HasBuff("DiplomaticImmunity") && !ObjectManager.Player.HasBuff("poppyulttargetmark"))
            {
                return false;
            }

            // Banshee's Veil (PASSIVE)
            if (target.HasBuff("BansheesVeil"))
            {
                return false;
            }

            // Sivir's Spell Shield (E)
            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            // Nocturne's Shroud of Darkness (W)
            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            if (target.Name == "Blitzcrank")
                if (!target.HasBuff("manabarriercooldown"))
                    if (target.Health + target.HPRegenRate +
                        (damageType == DamageType.Physical ? target.PhysicalShield : target.MagicalShield) +
                        target.Mana * 0.6 + target.PARRegenRate < calculatedDamage)
                        return true;

            if (target.Name == "Garen")
                if (target.HasBuff("GarenW"))
                    calculatedDamage *= 0.7;


            if (target.HasBuff("FerociousHowl"))
                calculatedDamage *= 0.3;

            var dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (dragonSlayerBuff != null)
                if (target.IsMinion())
                {
                    if (dragonSlayerBuff.Count >= 4)
                        calculatedDamage += dragonSlayerBuff.Count == 5 ? calculatedDamage * 0.30 : calculatedDamage * 0.15;

                    if (target.Name.ToLowerInvariant().Contains("dragon"))
                        calculatedDamage *= 1 - dragonSlayerBuff.Count * 0.07;
                }

            if (target.Name.ToLowerInvariant().Contains("baron") &&
                ObjectManager.Player.HasBuff("barontarget"))
                calculatedDamage *= 0.5;

            return target.Health + target.HPRegenRate +
                   (damageType == DamageType.Physical ? target.PhysicalShield : target.MagicalShield) <
                   calculatedDamage - 2;
        }
    }
}