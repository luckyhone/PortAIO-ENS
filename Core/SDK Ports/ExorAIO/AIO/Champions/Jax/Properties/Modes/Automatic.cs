﻿using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Jax
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
            ///     The Automatic R Logic.
            /// </summary>
            if (Vars.R.IsReady() && Vars.Menu["spells"]["r"]["logical"].GetValue<MenuBool>().Enabled)
            {
                if (GameObjects.Player.HealthPercent < 20 && GameObjects.Player.CountEnemyHeroesInRange(750f) > 0)
                {
                    Vars.R.Cast();
                }
                else if (GameObjects.Player.CountEnemyHeroesInRange(750f) >= 2)
                {
                    Vars.R.Cast();
                }
            }

            /// <summary>
            ///     The Automatic E Logic.
            /// </summary>
            if (Vars.E.IsReady() && !GameObjects.Player.IsUnderEnemyTurret()
                                 && Vars.Menu["spells"]["e"]["logical"].GetValue<MenuBool>().Enabled)
            {
                if (GameObjects.EnemyHeroes.Any(t => !Invulnerable.Check(t) && t.IsValidTarget(Vars.E.Range)))
                {
                    Vars.E.Cast();
                }
            }
        }

        #endregion
    }
}