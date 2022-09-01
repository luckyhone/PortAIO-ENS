
using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using ExorAIO.Utilities;

#pragma warning disable 1587

namespace ExorAIO.Champions.Kalista
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
            /// <summary>
            ///     Orbwalk on minions.
            /// </summary>
            if (Items.HasItem(GameObjects.Player,3085)
                && Targets.Minions.Any(m => m.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange()))
                && !GameObjects.EnemyHeroes.Any(t => t.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange()))
                && Vars.Menu["miscellaneous"]["minionsorbwalk"].GetValue<MenuBool>().Enabled)
            {
                GameObjects.Player.IssueOrder(
                    GameObjectOrder.AttackUnit,
                    Targets.Minions.FirstOrDefault(m => m.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())));
            }

            if (Bools.HasSheenBuff() && !Targets.Target.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())
                || !Targets.Target.IsValidTarget() || Invulnerable.Check(Targets.Target)
                || GameObjects.Player.Mana < Vars.E.Instance.ManaCost + Vars.Q.Instance.ManaCost)
            {
                return;
            }

            /// <summary>
            ///     The Q Combo Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Vars.Menu["spells"]["q"]["combo"].GetValue<MenuBool>().Enabled)
            {
                var validTargets = GameObjects.EnemyHeroes.Where(t => t.IsValidTarget());
                if (!Vars.Q.GetPrediction(Targets.Target).CollisionObjects.Any()
                    || Vars.Q.GetPrediction(Targets.Target)
                           .CollisionObjects.All(
                               c =>
                               (validTargets.Contains(c) || Targets.Minions.Contains(c))
                               && c.Health < (float)GameObjects.Player.GetSpellDamage(c, SpellSlot.Q)))
                {
                    Vars.Q.Cast(Vars.Q.GetPrediction(Targets.Target).UnitPosition);
                }
            }
        }

        #endregion
    }
}