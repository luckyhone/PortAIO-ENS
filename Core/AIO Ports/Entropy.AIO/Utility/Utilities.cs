using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Utility
{
    static class Utilities
    {
        public static void AddAllSpellMenu(this Menu menu, bool onlyAntiMelees = false)
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy =>
                enemy.IsMelee || !onlyAntiMelees))
            {
                var subSpellMenu = new Menu(enemy.CharacterName.ToLower(), enemy.CharacterName);
                {
                    foreach (var spell in enemy.Spellbook.Spells.
                        Where(spell =>
                            spell.SData != null && spell.Slot == SpellSlot.Q ||
                            spell.Slot == SpellSlot.W ||
                            spell.Slot == SpellSlot.E ||
                            spell.Slot == SpellSlot.R))
                    {
                        subSpellMenu.Add(new MenuBool($"{enemy.CharacterName.ToLower()}.{spell.SData.Name.ToLower()}",
                            $"Slot: {spell.Slot} ({spell.SData.Name})",
                            false));
                    }
                }

                menu.Add(subSpellMenu);
            }
        }

        public static AIHeroClient GetSemiAutoTarget(
            this Spell spell,
            DamageType type,
            float customRange = -1f)
        {
            var killableTarget = GameObjects.EnemyHeroes.FirstOrDefault(
                e => e.IsValidTarget(customRange < 0 ? spell.Range : customRange) &&
                     !e.IsInvulnerable &&
                     e.Health < spell.GetDamage(e));
            if (killableTarget != null)
            {
                return killableTarget;
            }

            var selectedTarget = TargetSelector.SelectedTarget;
            if (selectedTarget != null &&
                selectedTarget.IsValidTarget(spell.Range) &&
                !selectedTarget.IsInvulnerable)
            {
                return selectedTarget;
            }

            var nearestTarget = GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(spell.Range) &&
                                                                   !e.IsInvulnerable).
                MinBy(o => o.Distance(Game.CursorPos));
            if (nearestTarget != null)
            {
                return nearestTarget;
            }

            return null;
        }

        #region Public Methods and Operators

        /// <returns>
        ///     true if the sender is a hero, a turret or an important jungle monster; otherwise, false.
        /// </returns>
        public static bool ShouldShieldAgainstSender(AIBaseClient sender)
        {
            return GameObjects.EnemyHeroes.Contains(sender) ||
                   GameObjects.EnemyTurrets.Contains(sender) ||
                   GameObjects.JungleLarge.Concat(GameObjects.JungleLegendary).Contains(sender);
        }

        /// <summary>
        ///     The PreserveMana Dictionary.
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static Dictionary<SpellSlot, int> PreserveManaData = new Dictionary<SpellSlot, int>();


        /// <summary>
        ///     Gets the angle by 'degrees' degrees.
        /// </summary>
        /// <param name="degrees">
        ///     The angle degrees.
        /// </param>
        /// <returns>
        ///     The angle by 'degrees' degrees.
        /// </returns>
        public static float GetAngleByDegrees(float degrees)
        {
            return (float) (degrees * Math.PI / 180);
        }

        public static float RangeMultiplier(float range)
        {
            if (ObjectManager.Player.HasItem(ItemId.Rapid_Firecannon) &&
                ObjectManager.Player.GetBuffCount("itemstatikshankcharge") == 100)
            {
                return Math.Min(range * 0.35f, 150);
            }

            return 0;
        }

        #endregion
    }
}