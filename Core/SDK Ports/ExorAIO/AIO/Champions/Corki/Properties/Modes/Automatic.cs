
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#pragma warning disable 1587

namespace ExorAIO.Champions.Corki
{
    using System;
    using System.Linq;

    using ExorAIO.Utilities;


    /// <summary>
    ///     The logics class.
    /// </summary>
    internal partial class Logics
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Automatic(EventArgs args)
        {
            if (GameObjects.Player.IsRecalling())
            {
                return;
            }

            /// <summary>
            ///     The Automatic R LastHit Logics.
            /// </summary>
            if (Vars.R.IsReady() && Orbwalker.ActiveMode != OrbwalkerMode.Combo
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.R.Slot, Vars.Menu["spells"]["r"]["farmhelper"])
                && Vars.Menu["spells"]["r"]["farmhelper"].GetValue<MenuSliderButton>().Enabled)
            {
                foreach (var minion in
                    GameObjects.EnemyMinions.Where(
                        m =>
                        m.IsValidTarget(Vars.R.Range) && !m.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())))
                {
                    if (Vars.GetRealHealth(minion)
                        < (float)
                          GameObjects.Player.GetSpellDamage(
                              minion,
                              SpellSlot.R))
                    {
                        if (!Vars.R.GetPrediction(minion).CollisionObjects.Any(c => Targets.Minions.Contains(c)))
                        {
                            Vars.R.Cast(minion.ServerPosition);
                        }
                    }
                }
            }
        }

        #endregion
    }
}