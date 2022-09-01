
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using HERMES_Kalista.MyLogic.Others;

#pragma warning disable 1587

namespace ExorAIO.Champions.Cassiopeia
{
    using System;
    using System.Linq;

    using ExorAIO.Utilities;


    /// <summary>
    ///     The champion class.
    /// </summary>
    internal class Cassiopeia
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called on orbwalker action.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="OrbwalkingActionArgs" /> instance containing the event data.</param>
        public static void OnAction(object sender, BeforeAttackEventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:

                    /// <summary>
                    ///     The 'No AA in Combo' Logic.
                    /// </summary>
                    if (Vars.Menu["miscellaneous"]["noaacombo"].GetValue<MenuBool>().Enabled)
                    {
                        if (!Bools.HasSheenBuff()
                            && (Vars.Q.IsReady() || Vars.W.IsReady()
                                || GameObjects.Player.Mana > Vars.E.Instance.ManaCost))
                        {
                            args.Process = false;
                        }
                    }
                    break;
            }

        }

        /// <summary>
        ///     Called upon calling a spellcast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SpellbookCastSpellEventArgs" /> instance containing the event data.</param>
        public static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R
                && Vars.Menu["miscellaneous"]["blockr"].GetValue<MenuBool>().Enabled)
            {
                if (
                    !GameObjects.EnemyHeroes.Any(
                        t =>
                        t.IsValidTarget(Vars.R.Range - 100f) && !Invulnerable.Check(t, DamageType.Magical, false)
                        && t.IsFacing(GameObjects.Player)))
                {
                    args.Process = false;
                }
            }
        }

        /// <summary>
        ///     Fired on an incoming gapcloser.
        /// </summary>
        /// <param name="sender">The object.</param>
        /// <param name="args">The <see cref="Events.GapCloserEventArgs" /> instance containing the event data.</param>
        public static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (GameObjects.Player.IsDead || Invulnerable.Check(sender, DamageType.Magical, false))
            {
                return;
            }

            if (Vars.R.IsReady() && sender.IsValidTarget(Vars.R.Range) && sender.IsFacing(GameObjects.Player)
                && Vars.Menu["spells"]["r"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                Vars.R.Cast(args.EndPosition);
            }
            if (Vars.W.IsReady() && sender.IsValidTarget(Vars.W.Range)
                && GameObjects.Player.Distance(args.EndPosition) > 500
                && Vars.Menu["spells"]["w"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                Vars.W.Cast(args.EndPosition);
            }
        }

        /// <summary>
        ///     Called on interruptable spell.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Events.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        public static void OnInterruptableTarget(object sender, Interrupter.InterruptSpellArgs args)
        {
            if (GameObjects.Player.IsDead || Invulnerable.Check(args.Sender, DamageType.Magical, false))
            {
                return;
            }

            if (Vars.R.IsReady() && args.Sender.IsValidTarget(Vars.R.Range) && args.Sender.IsFacing(GameObjects.Player)
                && Vars.Menu["spells"]["r"]["interrupter"].GetValue<MenuBool>().Enabled)
            {
                Vars.R.Cast(args.Sender.ServerPosition);
            }
        }

        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void OnUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead)
            {
                return;
            }

            /// <summary>
            ///     Initializes the Automatic actions.
            /// </summary>
            Logics.Automatic(args);

            /// <summary>
            ///     Initializes the Killsteal events.
            /// </summary>
            Logics.Killsteal(args);
            if (GameObjects.Player.Spellbook.IsAutoAttack)
            {
                return;
            }

            /// <summary>
            ///     Initializes the orbwalkingmodes.
            /// </summary>
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Logics.Combo(args);
                    break;
                case OrbwalkerMode.Harass:
                    Logics.Harass(args);
                    break;
                case OrbwalkerMode.LastHit:
                    Logics.LastHit(args);
                    break;
                case OrbwalkerMode.LaneClear:
                    Logics.Clear(args);
                    break;
            }
        }

        /// <summary>
        ///     Loads Cassiopeia.
        /// </summary>
        public void OnLoad()
        {
            /// <summary>
            ///     Initializes the menus.
            /// </summary>
            Menus.Initialize();

            /// <summary>
            ///     Initializes the spells.
            /// </summary>
            Spells.Initialize();

            /// <summary>
            ///     Initializes the methods.
            /// </summary>
            Methods.Initialize();

            /// <summary>
            ///     Initializes the drawings.
            /// </summary>
            Drawings.Initialize();
        }

        #endregion
    }
}