
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

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
            ///     The Q BuildingClear Logic.
            /// </summary>
            if (Vars.Q.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["buildings"])
                && Vars.Menu["spells"]["q"]["buildings"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.Q.Cast();
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
            ///     The LaneClear W Logic.
            /// </summary>
            if (Vars.W.IsReady() && !GameObjects.Player.HasBuff("TwitchFullAutomatic"))
            {
                /// <summary>
                ///     The W LaneClear Logic.
                /// </summary>
                if (GameObjects.Player.ManaPercent
                    > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["laneclear"])
                    && Vars.Menu["spells"]["w"]["laneclear"].GetValue<MenuSliderButton>().Enabled
                    && Vars.W.GetCircularFarmLocation(
                        Targets.Minions.Where(m => m.GetBuffCount("twitchdeadlyvenom") <= 4).ToList(),
                        Vars.W.Width).MinionsHit >= 3)
                {
                    Vars.W.Cast(
                        Vars.W.GetCircularFarmLocation(
                            Targets.Minions.Where(m => m.GetBuffCount("twitchdeadlyvenom") <= 4).ToList(),
                            Vars.W.Width).Position);
                }

                /// <summary>
                ///     The W JungleClear Logic.
                /// </summary>
                else if (GameObjects.Player.ManaPercent
                         > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["jungleclear"])
                         && Vars.Menu["spells"]["w"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
                {
                    var objAiMinion =
                        Targets.JungleMinions.FirstOrDefault(m => m.GetBuffCount("twitchdeadlyvenom") <= 4);
                    if (objAiMinion != null)
                    {
                        Vars.W.Cast(objAiMinion.ServerPosition);
                    }
                }
            }

            /// <summary>
            ///     The LaneClear E Logic.
            /// </summary>
            if (Vars.E.IsReady() && Targets.Minions.Any()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["laneclear"])
                && Vars.Menu["spells"]["e"]["laneclear"].GetValue<MenuSliderButton>().Enabled)
            {
                if (
                    Targets.Minions.Count(
                        m =>
                        m.IsValidTarget(Vars.E.Range)
                        && m.Health
                        < (float)GameObjects.Player.GetSpellDamage(m, SpellSlot.E)
                        + (float)GameObjects.Player.GetSpellDamage(m, SpellSlot.E)) >= 3)
                {
                    Vars.E.Cast();
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
            ///     The Q JungleClear Logic.
            /// </summary>
            if (Vars.Q.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["jungleclear"])
                && Vars.Menu["spells"]["q"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.Q.Cast();
            }
        }

        #endregion
    }
}