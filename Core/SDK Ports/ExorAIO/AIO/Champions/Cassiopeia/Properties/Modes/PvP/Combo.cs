﻿
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Cassiopeia
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
            if (Bools.HasSheenBuff())
            {
                return;
            }

            /// <summary>
            ///     The E Combo Logic.
            /// </summary>
            if (Vars.E.IsReady() && Vars.Menu["spells"]["e"]["combo"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in
                    GameObjects.EnemyHeroes.Where(
                        t =>
                        t.IsValidTarget(Vars.E.Range)
                        && (t.HasBuffOfType(BuffType.Poison)
                            || !Vars.Menu["spells"]["e"]["combopoison"].GetValue<MenuBool>().Enabled)
                        && !Invulnerable.Check(t, DamageType.Magical, false)))
                {
                    DelayAction.Add(
                        Vars.Menu["spells"]["e"]["delay"].GetValue<MenuSlider>().Value,
                        () => { Vars.E.CastOnUnit(target); });
                }
            }

            if (!Targets.Target.IsValidTarget() || Invulnerable.Check(Targets.Target, DamageType.Magical, false))
            {
                return;
            }

            /// <summary>
            ///     The R Combo Logic.
            /// </summary>
            if (Vars.R.IsReady() && Vars.Menu["spells"]["r"]["combo"].GetValue<MenuSliderButton>().Enabled
                && Vars.Menu["spells"]["r"]["combo"].GetValue<MenuSliderButton>().Value <= Targets.RTargets.Count)
            {
                Vars.R.Cast(Targets.RTargets[0].ServerPosition);
            }
            if (Targets.Target.HasBuffOfType(BuffType.Poison))
            {
                return;
            }

            /// <summary>
            ///     The Q Combo Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.Target.IsValidTarget(Vars.Q.Range)
                && Vars.Menu["spells"]["q"]["combo"].GetValue<MenuBool>().Enabled)
            {
                Vars.Q.Cast(Vars.Q.GetPrediction(Targets.Target).CastPosition);
                return;
            }

            DelayAction.Add(
                1000,
                () =>
                    {
                        /// <summary>
                        ///     The W Combo Logic.
                        /// </summary>
                        if (Vars.W.IsReady() && Targets.Target.IsValidTarget(Vars.W.Range - 100f)
                            && !Targets.Target.IsValidTarget(500f)
                            && Vars.Menu["spells"]["w"]["combo"].GetValue<MenuBool>().Enabled)
                        {
                            Vars.W.Cast(Vars.W.GetPrediction(Targets.Target).CastPosition);
                        }
                    });
        }

        #endregion
    }
}