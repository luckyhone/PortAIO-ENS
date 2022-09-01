
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
            if (GameObjects.Player.IsRecalling() || Vars.R.Instance.Name.Equals("JhinRShot"))
            {
                return;
            }

            /// <summary>
            ///     The Automatic Q LastHit Logic.
            /// </summary>
            if (Vars.Q.IsReady() && GameObjects.Player.HasBuff("JhinPassiveReload")
                && Orbwalker.ActiveMode != OrbwalkerMode.Combo
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["lasthit"])
                && Vars.Menu["spells"]["q"]["lasthit"].GetValue<MenuSliderButton>().Enabled)
            {
                foreach (var minion in
                    Targets.Minions.Where(
                        m =>
                        m.IsValidTarget(Vars.Q.Range)
                        && Vars.GetRealHealth(m) < (float)GameObjects.Player.GetSpellDamage(m, SpellSlot.Q)))
                {
                    Vars.Q.CastOnUnit(minion);
                }
            }

            /// <summary>
            ///     The Automatic E Logic.
            /// </summary>
            if (Vars.E.IsReady() && Vars.Menu["spells"]["e"]["logical"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in
                    GameObjects.EnemyHeroes.Where(
                        t => Bools.IsImmobile(t) && !Invulnerable.Check(t) && t.IsValidTarget(Vars.E.Range)))
                {
                    Vars.E.Cast(
                        GameObjects.Player.ServerPosition.Extend(
                            target.ServerPosition,
                            GameObjects.Player.Distance(target) + target.BoundingRadius * 2));
                }
            }

            /// <summary>
            ///     The Automatic W Logic.
            /// </summary>
            if (Vars.W.IsReady() && !GameObjects.Player.IsUnderEnemyTurret()
                && Vars.Menu["spells"]["w"]["logical"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in
                    GameObjects.EnemyHeroes.Where(
                        t =>
                        Bools.IsImmobile(t) && !Invulnerable.Check(t) && t.HasBuff("jhinespotteddebuff")
                        && t.IsValidTarget(Vars.W.Range - 150f)
                        && Vars.Menu["spells"]["w"]["whitelist"][t.CharacterName.ToLower()].GetValue<MenuBool>().Enabled))
                {
                    Vars.W.Cast(target.ServerPosition);
                }
            }
        }

        #endregion
    }
}