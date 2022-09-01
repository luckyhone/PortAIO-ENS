using System;
using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using SharpDX;

namespace ExorAIO.Champions.Lucian
{
    using Geometry = ExorAIO.Utilities.Geometry;
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
            if (
                !GameObjects.EnemyHeroes.Any(
                    t =>
                    !Invulnerable.Check(t) && !t.IsValidTarget(Vars.Q.Range) && t.IsValidTarget(Vars.Q2.Range - 50f)
                    && Vars.Menu["spells"]["q"]["whitelist"][t.CharacterName.ToLower()].GetValue<MenuBool>().Enabled))
            {
                return;
            }

            /// <summary>
            ///     The Extended Q Mixed Harass Logic.
            /// </summary>
            if (Vars.Q.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["extended"]["mixed"])
                && Vars.Menu["spells"]["q"]["extended"]["mixed"].GetValue<MenuSliderButton>().Enabled)
            {
                /// <summary>
                ///     Through enemy minions.
                /// </summary>
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

        #endregion
    }
}