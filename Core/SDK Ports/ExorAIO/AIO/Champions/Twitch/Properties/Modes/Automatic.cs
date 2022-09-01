
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Twitch
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
            ///     The Automatic E Logics.
            /// </summary>
            if (Vars.E.IsReady())
            {
                /// <summary>
                ///     The Automatic Enemy E Logic.
                /// </summary>
                if (Vars.Menu["spells"]["e"]["logical"].GetValue<MenuBool>().Enabled)
                {
                    if (
                        GameObjects.EnemyHeroes.Any(
                            t =>
                            !Invulnerable.Check(t) && t.IsValidTarget(Vars.E.Range)
                            && t.GetBuffCount("twitchdeadlyvenom") == 6))
                    {
                        Vars.E.Cast();
                    }
                }

                /// <summary>
                ///     The Automatic JungleSteal E Logic.
                /// </summary>
                if (Vars.Menu["spells"]["e"]["junglesteal"].GetValue<MenuBool>().Enabled)
                {
                    if (
                        Targets.JungleMinions.Any(
                            m =>
                            m.IsValidTarget(Vars.E.Range)
                            && m.Health
                            < (float)GameObjects.Player.GetSpellDamage(m, SpellSlot.E)
                            + (float)GameObjects.Player.GetSpellDamage(m, SpellSlot.E)))
                    {
                        Vars.E.Cast();
                    }
                }
            }
        }

        #endregion
    }
}