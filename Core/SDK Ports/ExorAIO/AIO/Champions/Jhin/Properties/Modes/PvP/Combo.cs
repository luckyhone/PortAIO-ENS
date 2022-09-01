
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Jhin
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
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Combo(EventArgs args)
        {
            /// <summary>
            ///     The R Logic.
            /// </summary>
            if (Vars.R.IsReady() && Vars.R.Instance.Name.Equals("JhinRShot")
                && Vars.Menu["spells"]["r"]["combo"].GetValue<MenuBool>().Enabled)
            {
                if (
                    GameObjects.EnemyHeroes.Any(
                        t => t.IsValidTarget(Vars.R.Range) && !Jhin.Cone.IsOutside((Vector2)t.ServerPosition)))
                {
                    foreach (var target in
                        GameObjects.EnemyHeroes.Where(
                            t => t.IsValidTarget(Vars.R.Range) && !Jhin.Cone.IsOutside((Vector2)t.ServerPosition)))
                    {
                        if (Vars.Menu["spells"]["r"]["nearmouse"].GetValue<MenuBool>().Enabled)
                        {
                            Vars.R.Cast(
                                Vars.R.GetPrediction(
                                    GameObjects.EnemyHeroes.Where(
                                        t =>
                                        t.IsValidTarget(Vars.R.Range) && !Jhin.Cone.IsOutside((Vector2)t.ServerPosition))
                                        .OrderBy(o => o.Distance(Game.CursorPos))
                                        .First()).UnitPosition);
                            return;
                        }

                        Vars.R.Cast(Vars.R.GetPrediction(target).UnitPosition);
                        return;
                    }
                }

                Vars.R.Cast(Game.CursorPos);
            }

            if (Bools.HasSheenBuff() && Targets.Target.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())
                || !Targets.Target.IsValidTarget() || Vars.R.Instance.Name.Equals("JhinRShot"))
            {
                return;
            }

            /// <summary>
            ///     The Automatic W Logic.
            /// </summary>
            if (Vars.W.IsReady() && !GameObjects.Player.IsUnderEnemyTurret()
                && Vars.Menu["spells"]["w"]["combo"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in
                    GameObjects.EnemyHeroes.Where(
                        t =>
                        !Invulnerable.Check(t, DamageType.Magical, false) && t.HasBuff("jhinespotteddebuff")
                        && t.IsValidTarget(Vars.W.Range - 150f)
                        && Vars.Menu["spells"]["w"]["whitelist"][t.CharacterName.ToLower()].GetValue<MenuBool>().Enabled))
                {
                    Vars.W.Cast(Vars.W.GetPrediction(target).UnitPosition);
                }
            }

            if (!Targets.Target.IsValidTarget() || Invulnerable.Check(Targets.Target))
            {
                return;
            }

            /// <summary>
            ///     The Q Combo Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.Target.IsValidTarget(Vars.Q.Range)
                && GameObjects.Player.HasBuff("JhinPassiveReload")
                && Vars.Menu["spells"]["q"]["combo"].GetValue<MenuBool>().Enabled)
            {
                Vars.Q.CastOnUnit(Targets.Target);
            }
        }

        #endregion
    }
}