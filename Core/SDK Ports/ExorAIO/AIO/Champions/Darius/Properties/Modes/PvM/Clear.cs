
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#pragma warning disable 1587

namespace ExorAIO.Champions.Darius
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
            if (Bools.HasSheenBuff())
            {
                return;
            }

            /// <summary>
            ///     The Clear Q Logics.
            /// </summary>
            if (Vars.Q.IsReady())
            {
                /// <summary>
                ///     The JungleClear Q Logic.
                /// </summary>
                if (Targets.JungleMinions.Any()
                    && GameObjects.Player.ManaPercent
                    > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["jungleclear"])
                    && Vars.Menu["spells"]["q"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    Vars.Q.Cast();
                }

                /// <summary>
                ///     The LaneClear Q Logic.
                /// </summary>
                else if (Targets.Minions.Any()
                         && GameObjects.Player.ManaPercent
                         > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["laneclear"])
                         && Vars.Menu["spells"]["q"]["laneclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    Vars.Q.Cast();
                }
            }
        }

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void JungleClear(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!(Orbwalker.GetTarget() is AIMinionClient)
                || !Targets.JungleMinions.Contains(Orbwalker.GetTarget() as AIMinionClient))
            {
                return;
            }

            /// <summary>
            ///     The W JungleClear Logic.
            /// </summary>
            if (Vars.W.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["jungleclear"])
                && Vars.Menu["spells"]["w"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.W.Cast();
            }
        }

        #endregion
    }
}