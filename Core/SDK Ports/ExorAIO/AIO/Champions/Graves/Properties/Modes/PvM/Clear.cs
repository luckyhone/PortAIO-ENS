
using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using ExorAIO.Utilities;
using SharpDX;

#pragma warning disable 1587

namespace ExorAIO.Champions.Graves
{

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
            if (!(args.Target is HQClient) && !(args.Target is AITurretClient) && !(args.Target is BarracksDampenerClient))
            {
                return;
            }

            /// <summary>
            ///     The E BuildingClear Logic.
            /// </summary>
            if (Vars.E.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["buildings"])
                && Vars.Menu["spells"]["e"]["buildings"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.E.Cast(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, GameObjects.Player.BoundingRadius));
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
                if (Enumerable.Any(Targets.JungleMinions)
                    && GameObjects.Player.ManaPercent
                    > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["jungleclear"])
                    && !Vars.AnyWallInBetween(
                        GameObjects.Player.ServerPosition,
                        Targets.JungleMinions[0].ServerPosition)
                    && Vars.Menu["spells"]["q"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    Vars.Q.Cast(Targets.JungleMinions[0].ServerPosition);
                }

                /// <summary>
                ///     The LaneClear Q Logic.
                /// </summary>
                if (Targets.Minions.Any()
                    && GameObjects.Player.ManaPercent
                    > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["laneclear"])
                    && !Vars.AnyWallInBetween(
                        GameObjects.Player.ServerPosition,
                        (Vector3)Vars.Q.GetLineFarmLocation(Targets.Minions, Vars.Q.Width).Position)
                    && Vars.Menu["spells"]["q"]["laneclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    Vars.Q.Cast(Vars.Q.GetLineFarmLocation(Targets.Minions, Vars.Q.Width).Position);
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
                || !Targets.JungleMinions.Contains((AIMinionClient) (Orbwalker.GetTarget() as AIMinionClient)))
            {
                return;
            }

            /// <summary>
            ///     The E JungleClear Logic.
            /// </summary>
            if (Vars.E.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["jungleclear"])
                && Vars.Menu["spells"]["e"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.E.Cast(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, GameObjects.Player.BoundingRadius));
            }
        }

        #endregion
    }
}