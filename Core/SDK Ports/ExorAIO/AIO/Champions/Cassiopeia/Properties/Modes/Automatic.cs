
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Cassiopeia
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
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Automatic(EventArgs args)
        {
            if (GameObjects.Player.IsRecalling())
            {
                return;
            }

            /// <summary>
            ///     The Tear Stacking Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Bools.HasTear(GameObjects.Player)
                && Orbwalker.ActiveMode == OrbwalkerMode.None
                && GameObjects.Player.CountEnemyHeroesInRange(1500) == 0
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["miscellaneous"]["tear"])
                && Vars.Menu["miscellaneous"]["tear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.Q.Cast(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, Vars.Q.Range - 5f));
            }

            /// <summary>
            ///     The Automatic Logics.
            /// </summary>
            foreach (var target in
                GameObjects.EnemyHeroes.Where(
                    t => Bools.IsImmobile(t) && !Invulnerable.Check(t, DamageType.Magical, false)))
            {
                /// <summary>
                ///     The Automatic W Logic.
                /// </summary>
                if (Vars.W.IsReady() && target.IsValidTarget(Vars.W.Range) && !target.IsValidTarget(500f)
                    && Vars.Menu["spells"]["w"]["logical"].GetValue<MenuBool>().Enabled)
                {
                    Vars.W.Cast(target.ServerPosition);
                }
            }

            /// <summary>
            ///     The Semi-Automatic R Logic.
            /// </summary>
            if (Vars.R.IsReady() && Vars.Menu["spells"]["r"]["bool"].GetValue<MenuBool>().Enabled
                && Vars.Menu["spells"]["r"]["key"].GetValue<MenuKeyBind>().Active)
            {
                var target =
                    GameObjects.EnemyHeroes.Where(
                        t =>
                        t.IsValidTarget(Vars.R.Range - 100f) && t.IsFacing(GameObjects.Player)
                        && !Invulnerable.Check(t, DamageType.Magical, false)
                        && Vars.Menu["spells"]["r"]["whitelist"][t.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                        .OrderBy(o => o.Health)
                        .FirstOrDefault();
                if (target != null)
                {
                    Vars.R.Cast(target.ServerPosition);
                }
            }
        }

        #endregion
    }
}