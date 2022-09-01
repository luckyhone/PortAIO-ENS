
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

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
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void BuildingClear(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!(Orbwalker.GetTarget() is HQClient) && !(Orbwalker.GetTarget() is AITurretClient)
                && !(Orbwalker.GetTarget() is BarracksDampenerClient))
            {
                return;
            }

            /// <summary>
            ///     The W BuildingClear Logic.
            /// </summary>
            if (Vars.W.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["buildings"])
                && Vars.Menu["spells"]["w"]["buildings"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.W.Cast();
            }
        }

        /// <summary>
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Clear(EventArgs args)
        {
            /// <summary>
            ///     The Clear E Logics.
            /// </summary>
            if (Vars.E.IsReady())
            {
                /// <summary>
                ///     The LaneClear E Logic.
                /// </summary>
                if (GameObjects.Player.ManaPercent
                    > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["laneclear"])
                    && Vars.Menu["spells"]["e"]["laneclear"].GetValue<MenuSliderButton>().Enabled
                    && Targets.Minions.Count >= 3 && GameObjects.Player.CountEnemyHeroesInRange(2000f) == 0)
                {
                    Vars.E.Cast();
                }

                /// <summary>
                ///     The JungleClear E Logic.
                /// </summary>
                else if (GameObjects.Player.ManaPercent
                         > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["jungleclear"])
                         && Vars.Menu["spells"]["e"]["jungleclear"].GetValue<MenuSliderButton>().Enabled
                         && Targets.JungleMinions.Any(m => m.IsValidTarget(Vars.E.Range)))
                {
                    Vars.E.Cast();
                }
            }

            /// <summary>
            ///     The Q JungleGrab Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.JungleMinions.Any(m => !m.IsValidTarget(Vars.E.Range))
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["junglegrab"])
                && Vars.Menu["spells"]["q"]["junglegrab"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.Q.CastOnUnit(Targets.JungleMinions.FirstOrDefault(m => !m.IsValidTarget(Vars.E.Range)));
            }
        }

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void Clear(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!(Orbwalker.GetTarget() is AIMinionClient)
                || !Targets.JungleMinions.Contains(Orbwalker.GetTarget() as AIMinionClient))
            {
                return;
            }

            /// <summary>
            ///     The Clear W Logics.
            /// </summary>
            if (Vars.W.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["clear"])
                && Vars.Menu["spells"]["w"]["clear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.W.Cast();
            }
        }

        #endregion
    }
}