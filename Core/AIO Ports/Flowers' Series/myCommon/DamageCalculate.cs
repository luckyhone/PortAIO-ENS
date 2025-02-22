﻿using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ADCCOMMON
{

    public static class DamageCalculate
    {
        public static float GetComboDamage(AIBaseClient target)
        {
            if (target == null || target.IsDead )
            {
                return 0;
            }

            var damage = 0d;

            damage += GetQDamage(target);
            damage += GetWDamage(target);
            damage += GetEDamage(target);
            damage += GetRDamage(target);

            damage += ObjectManager.Player.GetAutoAttackDamage(target, true);

            if (ObjectManager.Player.HasBuff("SummonerExhaust"))
            {
                damage = damage * 0.6f;
            }

            if (target.CharacterName == "Moredkaiser")
            {
                damage -= target.Mana;
            }

            if (target.HasBuff("GarenW"))
            {
                damage = damage * 0.7f;
            }

            if (target.HasBuff("ferocioushowl"))
            {
                damage = damage * 0.7f;
            }

            if (target.HasBuff("BlitzcrankManaBarrierCD") && target.HasBuff("ManaBarrier"))
            {
                damage -= target.Mana / 2f;
            }

            return (float)damage;
        }

        public static float GetQDamage(AIBaseClient target)
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0 ||
                !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).IsReady())
            {
                return 0f;
            }

            return (float)ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
        }

        public static float GetWDamage(AIBaseClient target)
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0 ||
                !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).IsReady())
            {
                return 0f;
            }

            return (float)ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
        }

        public static float GetEDamage(AIBaseClient target)
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0 ||
                !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).IsReady())
            {
                return 0f;
            }

            return (float)ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
        }

        public static float GetRDamage(AIBaseClient target)
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level == 0 ||
                !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).IsReady())
            {
                return 0f;
            }

            return (float)ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
        }

        public static float GetIgniteDmage(AIBaseClient target)
        {
            if (ObjectManager.Player.GetSpellSlot("SummonerDot") == SpellSlot.Unknown ||
                !ObjectManager.Player.GetSpellSlot("SummonerDot").IsReady())
            {
                return 0f;
            }

            return 50 + 20*ObjectManager.Player.Level - target.HPRegenRate/5*3;
        }
    }
}
