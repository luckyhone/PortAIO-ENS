﻿using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Taliyah
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
        public static void Killsteal(EventArgs args)
        {
            /// <summary>
            ///     The KillSteal Q Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Vars.Menu["spells"]["q"]["killsteal"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in
                         GameObjects.EnemyHeroes.Where(
                             t =>
                                 t.IsValidTarget(Vars.Q.Range) && !Invulnerable.Check(t, DamageType.Magical, false)
                                                               && Vars.GetRealHealth(t)
                                                               < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q)
                                                               * (Taliyah.TerrainObject != null ? 1 : 3)))
                {
                    if (!Vars.Q.GetPrediction(target).CollisionObjects.Any())
                    {
                        Vars.Q.Cast(Vars.Q.GetPrediction(target).UnitPosition);
                    }
                }
            }
        }

        #endregion
    }
}