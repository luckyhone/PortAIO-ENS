using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#pragma warning disable 1587

namespace ExorAIO.Champions.Darius
{
    using System;

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
            /// <summary>
            ///     The AoE Q Logic.
            /// </summary>
            if (Vars.Q.IsReady()
                && GameObjects.Player.CountEnemyHeroesInRange(Vars.Q.Range)
                >= Vars.Menu["spells"]["q"]["aoe"].GetValue<MenuSliderButton>().Value
                && Vars.Menu["spells"]["q"]["aoe"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.Q.Cast();
            }
        }

        #endregion
    }
}