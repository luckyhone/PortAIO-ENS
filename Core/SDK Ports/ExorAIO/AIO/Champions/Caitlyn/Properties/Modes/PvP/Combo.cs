﻿using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Caitlyn
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
        public static void Combo(EventArgs args)
        {
            /// <summary>
            ///     The W Combo Logic.
            /// </summary>
            if (Vars.W.IsReady() && Vars.W.GetPrediction(Targets.Target).Hitchance >= HitChance.VeryHigh
                                 && Vars.Menu["spells"]["w"]["combo"].GetValue<MenuBool>().Enabled)
            {
                Vars.W.Cast(Vars.W.GetPrediction(Targets.Target).CastPosition);
            }

            /// <summary>
            ///     The E Combo Logic.
            /// </summary>
            if (Vars.E.IsReady() && Vars.Menu["spells"]["e"]["combo"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in
                         GameObjects.EnemyHeroes.Where(
                             t => t.IsValidTarget(600f) && !Invulnerable.Check(t) && !t.HasBuff("caitlynyordletrapinternal"))
                        )
                {
                    if (!Vars.E.GetPrediction(target).CollisionObjects.Any()
                        && Vars.E.GetPrediction(target).Hitchance >= HitChance.Medium)
                    {
                        Vars.E.Cast(Vars.E.GetPrediction(target).UnitPosition);
                    }
                }
            }
        }

        #endregion
    }
}