﻿using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#pragma warning disable 1587

namespace ExorAIO.Champions.Jhin
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
        public static void Harass(EventArgs args)
        {
            if (!Targets.Target.IsValidTarget() || Vars.R.Instance.Name.Equals("JhinRShot"))
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
                Vars.Q.CastOnUnit(Targets.Target);
            }
        }

        #endregion
    }
}