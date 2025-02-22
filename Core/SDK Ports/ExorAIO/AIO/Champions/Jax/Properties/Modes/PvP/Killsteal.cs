﻿using EnsoulSharp;
using EnsoulSharp.SDK;
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
                                 !Invulnerable.Check(t) && t.IsValidTarget(Vars.Q.Range)
                                                        && !t.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())
                                                        && Vars.GetRealHealth(t)
                                                        < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q)
                                                        + (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.W)))
                {
                    if (Vars.W.IsReady()
                        && Vars.GetRealHealth(target) > (float)GameObjects.Player.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Vars.W.Cast();
                    }
                    Vars.Q.CastOnUnit(target);
                }
            }
        }

        #endregion
    }
}