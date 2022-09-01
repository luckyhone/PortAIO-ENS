
using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using ExorAIO.Utilities;

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
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Combo(EventArgs args)
        {
            if ((Bools.HasSheenBuff() && Targets.Target.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange()))
                || !Targets.Target.IsValidTarget() || Invulnerable.Check(Targets.Target))
            {
                return;
            }

            /// <summary>
            ///     The E Combo Logic.
            /// </summary>
            if (Vars.E.IsReady() && Targets.Target.IsValidTarget(Vars.E.Range)
                && !Targets.Target.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())
                && Vars.Menu["spells"]["e"]["engager"].GetValue<MenuBool>().Enabled)
            {
                if (GameObjects.Player.Distance(Game.CursorPos) > GameObjects.Player.GetRealAutoAttackRange()
                    && GameObjects.Player.ServerPosition.Extend(
                        Game.CursorPos,
                        Vars.E.Range - GameObjects.Player.GetRealAutoAttackRange()).CountEnemyHeroesInRange(1000f) < 3
                    && Targets.Target.Distance(
                        GameObjects.Player.ServerPosition.Extend(
                            Game.CursorPos,
                            Vars.E.Range - GameObjects.Player.GetRealAutoAttackRange()))
                    < GameObjects.Player.GetRealAutoAttackRange())
                {
                    Vars.E.Cast(Game.CursorPos);
                }
            }

            /// <summary>
            ///     The Q Combo Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.Target.IsValidTarget(Vars.Q.Range)
                && !Vars.AnyWallInBetween(
                    GameObjects.Player.ServerPosition,
                    Vars.Q.GetPrediction(Targets.Target).UnitPosition)
                && Vars.Menu["spells"]["q"]["combo"].GetValue<MenuBool>().Enabled)
            {
                Vars.Q.Cast(Vars.Q.GetPrediction(Targets.Target).UnitPosition);
            }
        }

        #endregion
    }
}