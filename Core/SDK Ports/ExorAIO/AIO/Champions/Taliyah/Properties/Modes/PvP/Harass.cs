﻿using EnsoulSharp.SDK;
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
        public static void Harass(EventArgs args)
        {
            if (!Targets.Target.IsValidTarget() || Invulnerable.Check(Targets.Target, DamageType.Magical, false))
            {
                return;
            }

            /// <summary>
            ///     The Q Harass Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.Target.IsValidTarget(Vars.Q.Range)
                                 && GameObjects.Player.ManaPercent
                                 > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["harass"])
                                 && Vars.Menu["spells"]["q"]["harass"].GetValue<MenuSliderButton>().Enabled)
            {
                if (Taliyah.TerrainObject != null && Vars.Menu["spells"]["q"]["harassfull"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if (!Vars.Q.GetPrediction(Targets.Target).CollisionObjects.Any())
                {
                    Vars.Q.Cast(Vars.Q.GetPrediction(Targets.Target).UnitPosition);
                }
            }
        }

        #endregion
    }
}