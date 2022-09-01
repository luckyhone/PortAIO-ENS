using System;
using System.Linq;
using EnsoulSharp;
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
        public static void Killsteal(EventArgs args)
        {
            /// <summary>
            ///     The Q Killsteal Logic.
            /// </summary>
            if (Vars.Q.IsReady())
            {
                /// <summary>
                ///     Normal Q KilLSteal Logic.
                /// </summary>
                if (Vars.Menu["spells"]["q"]["killsteal"].GetValue<MenuBool>().Enabled)
                {
                    foreach (var target in
                        GameObjects.EnemyHeroes.Where(
                            t =>
                            !Invulnerable.Check(t) && t.IsValidTarget(Vars.Q.Range)
                            && Vars.GetRealHealth(t) < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q)))
                    {
                        Vars.Q.CastOnUnit(target);
                    }
                }

                if (
                    !GameObjects.EnemyHeroes.Any(
                        t =>
                        !Invulnerable.Check(t) && !t.IsValidTarget(Vars.Q.Range) && t.IsValidTarget(Vars.Q2.Range - 50f)
                        && Vars.GetRealHealth(t) < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q)))
                {
                    return;
                }

                /// <summary>
                ///     Extended Q KilLSteal Logic.
                /// </summary>
                if (Vars.Menu["spells"]["q"]["extended"]["exkillsteal"].GetValue<MenuBool>().Enabled)
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
                                           && Vars.GetRealHealth(t)
                                           < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q))).UnitPosition)
                           select minion)
                    {
                        Vars.Q.CastOnUnit(minion);
                    }

                    /// <summary>
                    ///     Through enemy heroes.
                    /// </summary>
                    foreach (var target
                        in from target in GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(Vars.Q.Range))
                           let polygon =
                               new Geometry.Rectangle(
                               GameObjects.Player.ServerPosition,
                               GameObjects.Player.ServerPosition.Extend(target.ServerPosition, Vars.Q2.Range - 50f),
                               Vars.Q2.Width)
                           where
                               !polygon.IsOutside(
                                   (Vector2)
                                   Vars.Q2.GetPrediction(
                                       GameObjects.EnemyHeroes.FirstOrDefault(
                                           t =>
                                           !Invulnerable.Check(t) && !t.IsValidTarget(Vars.Q.Range)
                                           && t.IsValidTarget(Vars.Q2.Range - 50f)
                                           && Vars.GetRealHealth(t)
                                           < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q))).UnitPosition)
                           select target)
                    {
                        Vars.Q.CastOnUnit(target);
                    }
                }
            }

            /// <summary>
            ///     The KillSteal W Logic.
            /// </summary>
            if (Vars.W.IsReady() && Vars.Menu["spells"]["w"]["killsteal"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in
                    GameObjects.EnemyHeroes.Where(
                        t =>
                        !Invulnerable.Check(t) && t.IsValidTarget(Vars.W.Range) && !t.IsValidTarget(Vars.Q.Range)
                        && Vars.GetRealHealth(t) < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.W)))
                {
                    if (!Vars.W.GetPrediction(target).CollisionObjects.Any())
                    {
                        Vars.W.Cast(Vars.W.GetPrediction(target).UnitPosition);
                    }
                }
            }
        }

        #endregion
    }
}