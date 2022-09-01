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
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void LastHit(EventArgs args)
        {
            /// <summary>
            ///     The E LastHit Logic.
            /// </summary>
            if (Vars.E.IsReady() && Vars.Menu["spells"]["e"]["lasthit"].GetValue<MenuSliderButton>().Enabled)
            {
                DelayAction.Add(
                    Vars.Menu["spells"]["e"]["delay"].GetValue<MenuSlider>().Value,
                    () =>
                    {
                        foreach (var minion in
                                 Targets.Minions.Where(
                                     m =>
                                         Vars.GetRealHealth(m)
                                         < (float)GameObjects.Player.GetSpellDamage(m, SpellSlot.E)
                                         + (m.HasBuffOfType(BuffType.Poison)
                                             ? (float)
                                             GameObjects.Player.GetSpellDamage(m, SpellSlot.E)
                                             : 0)))
                        {
                            Vars.E.CastOnUnit(minion);
                        }
                    });
            }
        }

        #endregion
    }
}