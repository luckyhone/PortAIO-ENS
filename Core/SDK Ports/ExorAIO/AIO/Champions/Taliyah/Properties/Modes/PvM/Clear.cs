
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#pragma warning disable 1587

namespace ExorAIO.Champions.Taliyah
{
    using System;
    using System.Linq;

    using ExorAIO.Utilities;


    using SharpDX;

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
        public static void Clear(EventArgs args)
        {
            if (Bools.HasSheenBuff())
            {
                return;
            }

            /// <summary>
            ///     The Clear W Logic.
            /// </summary>
            if (Vars.W.IsReady() && Vars.E.IsReady())
            {
                /// <summary>
                ///     The LaneClear W Logic.
                /// </summary>
                if (Targets.Minions.Any()
                    && GameObjects.Player.ManaPercent
                    > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["laneclear"])
                    && Vars.Menu["spells"]["w"]["laneclear"].GetValue<MenuSliderButton>().Enabled
                    && Vars.W.GetCircularFarmLocation(Targets.Minions, Vars.W.Width).MinionsHit >= 3
                    && Vars.E.GetCircularFarmLocation(Targets.Minions, Vars.E.Width).MinionsHit >= 4)
                {
                    Vars.W.Cast(
                        (Vector3)Vars.W.GetCircularFarmLocation(Targets.Minions, Vars.W.Width).Position,
                        GameObjects.Player.ServerPosition);
                }

                /// <summary>
                ///     The JungleClear W Logic.
                /// </summary>
                else if (Targets.JungleMinions.Any(m => m.IsValidTarget(Vars.W.Range))
                         && GameObjects.Player.ManaPercent
                         > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["jungleclear"])
                         && Vars.Menu["spells"]["w"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    Vars.W.Cast(Targets.JungleMinions[0].ServerPosition, GameObjects.Player.ServerPosition);
                }
            }

            /// <summary>
            ///     The Clear E Logic.
            /// </summary>
            if (Vars.E.IsReady())
            {
                /// <summary>
                ///     The LaneClear E Logic.
                /// </summary>
                if (Targets.Minions.Any()
                    && GameObjects.Player.ManaPercent
                    > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["laneclear"])
                    && Vars.Menu["spells"]["e"]["laneclear"].GetValue<MenuSliderButton>().Enabled
                    && Vars.E.GetCircularFarmLocation(Targets.Minions, Vars.E.Width).MinionsHit >= 4)
                {
                    Vars.E.Cast(Vars.E.GetCircularFarmLocation(Targets.Minions, Vars.E.Width).Position);
                }

                /// <summary>
                ///     The JungleClear E Logic.
                /// </summary>
                else if (Targets.JungleMinions.Any(m => m.IsValidTarget(Vars.E.Range))
                         && GameObjects.Player.ManaPercent
                         > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["jungleclear"])
                         && Vars.Menu["spells"]["e"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    Vars.E.Cast(Targets.JungleMinions[0].ServerPosition);
                }
            }

            /// <summary>
            ///     The Clear Q Logic.
            /// </summary>
            if (Vars.Q.IsReady())
            {
                /// <summary>
                ///     The LaneClear Q Logic.
                /// </summary>
                if (Targets.Minions.Any()
                    && GameObjects.Player.ManaPercent
                    > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["laneclear"])
                    && Vars.Menu["spells"]["q"]["laneclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    if (Taliyah.TerrainObject != null
                        && Vars.Menu["spells"]["q"]["laneclearfull"].GetValue<MenuBool>().Enabled)
                    {
                        return;
                    }

                    if (Vars.Q.GetLineFarmLocation(Targets.Minions, Vars.Q.Width).MinionsHit >= 3)
                    {
                        Vars.Q.Cast(Vars.Q.GetLineFarmLocation(Targets.Minions, Vars.Q.Width).Position);
                    }
                    else if (Vars.Q.GetCircularFarmLocation(Targets.Minions, Vars.Q.Width).MinionsHit >= 3)
                    {
                        Vars.Q.Cast(Vars.Q.GetCircularFarmLocation(Targets.Minions, Vars.Q.Width).Position);
                    }
                }

                /// <summary>
                ///     The JungleClear Q Logic.
                /// </summary>
                else if (Targets.JungleMinions.Any()
                         && GameObjects.Player.ManaPercent
                         > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["jungleclear"])
                         && Vars.Menu["spells"]["q"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    if (Taliyah.TerrainObject != null
                        && Vars.Menu["spells"]["q"]["jungleclearfull"].GetValue<MenuBool>().Enabled)
                    {
                        return;
                    }

                    Vars.Q.Cast(Targets.JungleMinions[0].ServerPosition);
                }
            }
        }

        #endregion
    }
}