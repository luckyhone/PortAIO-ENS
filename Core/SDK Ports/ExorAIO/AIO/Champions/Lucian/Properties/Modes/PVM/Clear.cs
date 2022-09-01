using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using SharpDX;

namespace ExorAIO.Champions.Lucian
{
    using ExorAIO.Utilities;
    using Geometry = ExorAIO.Utilities.Geometry;

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
            ///     The E BuildingClear Logic.
            /// </summary>
            if (Vars.E.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["buildings"])
                && Vars.Menu["spells"]["e"]["buildings"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.E.Cast(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, 25));
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
                Vars.W.Cast(Game.CursorPos);
            }
        }

        /// <summary>
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Clear(EventArgs args)
        {
            if (
                !GameObjects.EnemyHeroes.Any(
                    t =>
                    !Invulnerable.Check(t) && !t.IsValidTarget(Vars.Q.Range) && t.IsValidTarget(Vars.Q2.Range - 50f)
                    && Vars.Menu["spells"]["q"]["whitelist"][t.CharacterName.ToLower()].GetValue<MenuBool>().Enabled))
            {
                return;
            }

            /// <summary>
            ///     The Q Minion Harass Logic.
            /// </summary>

            if (Vars.Q.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["extended"]["exlaneclear"])
                && Vars.Menu["spells"]["q"]["extended"]["exlaneclear"].GetValue<MenuSliderButton>().Enabled)
            {
                foreach (var minion 
                    in from minion in Targets.Minions.Where(m => m.IsValidTarget(Vars.Q.Range))
                       let polygon =
                           new Geometry.Rectangle(
                           GameObjects.Player.ServerPosition,
                           GameObjects.Player.ServerPosition.Extend(minion.ServerPosition, Vars.Q2.Range - 50f),
                           Vars.Q2.Width)
                       where
                           !polygon.IsOutside(
                               (Vector2)
                               Vars.Q2.GetPrediction(
                                   GameObjects.EnemyHeroes.FirstOrDefault(
                                       t =>
                                       !Invulnerable.Check(t) && !t.IsValidTarget(Vars.Q.Range)
                                       && t.IsValidTarget(Vars.Q2.Range - 50f)
                                       && Vars.Menu["spells"]["q"]["whitelist"][t.CharacterName.ToLower()]
                                              .GetValue<MenuBool>().Enabled)).UnitPosition)
                       select minion)
                {
                    Vars.Q.CastOnUnit(minion);
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
            if (!(Orbwalker.GetTarget() is AIMinionClient))
            {
                return;
            }

            /// <summary>
            ///     The JungleClear E Logic.
            /// </summary>
            if (Vars.E.IsReady() && Targets.JungleMinions.Any()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["jungleclear"])
                && Vars.Menu["spells"]["e"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.E.Cast(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, 50));
                return;
            }

            /// <summary>
            ///     The JungleClear Q Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.JungleMinions.Any()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["jungleclear"])
                && Vars.Menu["spells"]["q"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.Q.CastOnUnit(Orbwalker.GetTarget() as AIMinionClient);
                return;
            }

            /// <summary>
            ///     The JungleClear W Logic.
            /// </summary>
            if (Vars.W.IsReady() && Targets.JungleMinions.Any()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["jungleclear"])
                && Vars.Menu["spells"]["w"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.W.Cast(((AIMinionClient)Orbwalker.GetTarget()).ServerPosition);
            }
        }

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void LaneClear(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            /// <summary>
            ///     The E LaneClear Logic.
            /// </summary>
            if (Vars.E.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["laneclear"])
                && Vars.Menu["spells"]["e"]["laneclear"].GetValue<MenuSliderButton>().Enabled)
            {
                if (!Targets.Minions.Any(m => m.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange()))
                    && Targets.Minions.Any(
                        m =>
                        m.Distance(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, Vars.E.Range))
                        < GameObjects.Player.GetRealAutoAttackRange()))
                {
                    Vars.E.Cast(Game.CursorPos);
                    return;
                }
            }

            /// <summary>
            ///     The LaneClear Q Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.Minions.Any()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["laneclear"])
                && Vars.Menu["spells"]["q"]["laneclear"].GetValue<MenuSliderButton>().Enabled)
            {
                if (Vars.Q2.GetLineFarmLocation(Targets.Minions, Vars.Q2.Width).MinionsHit >= 3)
                {
                    Vars.Q.CastOnUnit(Targets.Minions[0]);
                    return;
                }
            }

            /// <summary>
            ///     The LaneClear W Logic.
            /// </summary>
            if (Vars.W.IsReady() && Targets.Minions.Any()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["spells"]["w"]["laneclear"])
                && Vars.Menu["spells"]["w"]["laneclear"].GetValue<MenuSliderButton>().Enabled)
            {
                if (Vars.W.GetCircularFarmLocation(Targets.Minions, Vars.W.Width).MinionsHit >= 2)
                {
                    Vars.W.Cast(Vars.W.GetCircularFarmLocation(Targets.Minions, Vars.W.Width).Position);
                }
            }
        }

        #endregion
    }
}