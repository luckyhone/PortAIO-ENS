using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

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
        public static void Harass(EventArgs args)
        {
            if (!Targets.Target.IsValidTarget() || Invulnerable.Check(Targets.Target))
            {
                return;
            }

            /// <summary>
            ///     The R Harass Logic.
            /// </summary>
            if (Vars.R.IsReady() && Targets.Target.IsValidTarget(Vars.R.Range)
                                 && GameObjects.Player.ManaPercent
                                 > ManaManager.GetNeededMana(Vars.R.Slot, Vars.Menu["spells"]["r"]["autoharass"])
                                 && Vars.Menu["spells"]["r"]["autoharass"].GetValue<MenuSliderButton>().Enabled
                                 && Vars.Menu["spells"]["r"]["whitelist"][Targets.Target.CharacterName.ToLower()].GetValue<MenuBool>()
                                     .Enabled)
            {
                if (!Vars.R.GetPrediction(Targets.Target).CollisionObjects.Any(c => Targets.Minions.Contains(c)))
                {
                    Vars.R.Cast(Vars.R.GetPrediction(Targets.Target).UnitPosition);
                }
            }
        }

        #endregion
    }
}