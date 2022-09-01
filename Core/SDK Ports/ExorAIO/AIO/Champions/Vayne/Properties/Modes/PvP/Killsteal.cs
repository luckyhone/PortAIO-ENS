
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#pragma warning disable 1587

namespace ExorAIO.Champions.Vayne
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
        public static void Killsteal(EventArgs args)
        {
            /// <summary>
            ///     The Q KillSteal Logic.
            /// </summary>
            if (Vars.Q.IsReady() && !GameObjects.Player.Spellbook.IsAutoAttack
                && Vars.Menu["spells"]["q"]["killsteal"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in
                    GameObjects.EnemyHeroes.Where(
                        t =>
                        t.IsValidTarget(Vars.Q.Range) && !t.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())
                        && t.CountEnemyHeroesInRange(700f) <= 2
                        && Vars.GetRealHealth(t)
                        < GameObjects.Player.GetAutoAttackDamage(t)
                        + (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q)
                        + (t.GetBuffCount("vaynesilvereddebuff") == 2
                               ? (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.W)
                               : 0)))
                {
                    Vars.Q.Cast(target.ServerPosition);
                    Orbwalker.ForceTarget = target;
                }
            }

            /// <summary>
            ///     The E KillSteal Logic.
            /// </summary>
            if (Vars.E.IsReady() && !GameObjects.Player.Spellbook.IsAutoAttack
                && Vars.Menu["spells"]["e"]["killsteal"].GetValue<MenuBool>().DontSave)
            {
                foreach (var target in
                    GameObjects.EnemyHeroes.Where(
                        t =>
                        t.IsValidTarget(Vars.E.Range)
                        && Vars.GetRealHealth(t)
                        < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.E)
                        + (t.GetBuffCount("vaynesilvereddebuff") == 2
                               ? (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.W)
                               : 0)))
                {
                    Vars.E.CastOnUnit(target);
                }
            }
        }

        #endregion
    }
}