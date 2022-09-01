using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;

namespace Entropy.AIO.Utility
{
    using static Components;
    static class ManaManager
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The minimum mana needed to cast the Spell from the 'slot' SpellSlot.
        /// </summary>
        public static int GetNeededMana(this Spell spell, MenuSliderButton menuComponent)
        {
            var ignoreManaManagerMenu = General.IgnoreManaManagerBlue;
            
            if (ignoreManaManagerMenu == null)
            {
                return 0;
            }

            if (ignoreManaManagerMenu.Enabled && Player.HasBuff("crestoftheancientgolem"))
            {
                return 0;
            }

            var cost = spell.Mana;
            return (int) (menuComponent.Value + cost / Player.MaxMana * 100);
        }

        public static int GetNeededMana(this SpellSlot slot, MenuSliderButton menuComponent)
        {
            var ignoreManaManagerMenu = General.IgnoreManaManagerBlue;

            if (ignoreManaManagerMenu == null)
            {
                return 0;
            }

            if (ignoreManaManagerMenu.Enabled && Player.HasBuff("crestoftheancientgolem"))
            {
                return 0;
            }

            var cost = 0f;

            switch (slot)
            {
                case SpellSlot.Q:
                    cost = ChampionBase.Q.Mana;
                    break;
                case SpellSlot.W:
                    cost = ChampionBase.W.Mana;
                    break;
                case SpellSlot.E:
                    cost = ChampionBase.E.Mana;
                    break;
                case SpellSlot.R:
                    cost = ChampionBase.R.Mana;
                    break;
            }

            return (int) (menuComponent.Value + cost / Player.MaxMana * 100);
        }

        public static bool ManaCheck(this Spell spell, MenuSliderButton menuComponent)
        {
            return Player.ManaPercent > spell.GetNeededMana(menuComponent);
        }

        private static AIHeroClient Player => ObjectManager.Player;

        #endregion
    }
}