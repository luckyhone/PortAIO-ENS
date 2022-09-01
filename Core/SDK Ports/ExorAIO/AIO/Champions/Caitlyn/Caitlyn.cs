
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using HERMES_Kalista.MyLogic.Others;

#pragma warning disable 1587

namespace ExorAIO.Champions.Caitlyn
{
    using System;
    using System.Linq;

    using ExorAIO.Utilities;


    /// <summary>
    ///     The champion class.
    /// </summary>
    internal class Caitlyn
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Fired on spell cast.
        /// </summary>
        /// <param name="spellbook">The spellbook.</param>
        /// <param name="args">The <see cref="SpellbookCastSpellEventArgs" /> instance containing the event data.</param>
        public static void OnCastSpell(Spellbook spellbook, SpellbookCastSpellEventArgs args)
        {
            if (spellbook.Owner.IsMe)
            {
                switch (args.Slot)
                {
                    case SpellSlot.W:

                        /// <summary>
                        ///     Blocks trap cast if there is another trap nearby.
                        /// </summary>
                        if (
                            ObjectManager.Get<AIMinionClient>()
                                .Any(
                                    m =>
                                    m.Distance(args.EndPosition) < 200 && m.Name.Equals("caitlyntrap")))
                        {
                            args.Process = false;
                        }
                        break;
                    case SpellSlot.E:
                        if (Environment.TickCount - Vars.LastTick < 1000)
                        {
                            return;
                        }

                        /// <summary>
                        ///     The Dash to CursorPos Option.
                        /// </summary>
                        if (Orbwalker.ActiveMode == OrbwalkerMode.None
                            && Vars.Menu["miscellaneous"]["reversede"].GetValue<MenuBool>().Enabled)
                        {
                            Vars.LastTick = Environment.TickCount;
                            Vars.E.Cast(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, -Vars.E.Range));
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                /// <summary>
                ///     Initializes the orbwalkingmodes.
                /// </summary>
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        switch (args.SData.Name)
                        {
                            case "CaitlynEntrapment":
                            case "CaitlynEntrapmentMissile":
                                if (Vars.W.IsReady()
                                    && Vars.Menu["spells"]["w"]["triplecombo"].GetValue<MenuBool>().Enabled)
                                {
                                    foreach (var target in
                                        GameObjects.EnemyHeroes.Where(
                                            t => t.IsValidTarget(Vars.W.Range) && !Invulnerable.Check(t)))
                                    {
                                        Vars.W.Cast(target.ServerPosition);
                                    }
                                }
                                break;
                        }

                        break;
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

            if (Vars.E.IsReady()  && sender.IsValidTarget(Vars.E.Range)
                && Vars.Menu["spells"]["e"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                if (!Vars.E.GetPrediction(sender).CollisionObjects.Any())
                {
                    Vars.E.Cast(sender.ServerPosition);
                }
            }

            if (Vars.W.IsReady() && sender.IsValidTarget(Vars.W.Range)
                && Vars.Menu["spells"]["w"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                Vars.W.Cast(args.EndPosition);
            }
        }

        /// <summary>
        ///     Called on interruptable spell.
        /// </summary>
        /// <param name="sender">The object.</param>
        /// <param name="args">The <see cref="Events.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        public static void OnInterruptableTarget(object sender, Interrupter.InterruptSpellArgs args)
        {
            if (GameObjects.Player.IsDead || Invulnerable.Check(args.Sender, DamageType.Magical, false))
            {
                return;
            }

            if (Vars.E.IsReady() && args.Sender.IsValidTarget(Vars.E.Range)
                && Vars.Menu["spells"]["e"]["interrupter"].GetValue<MenuBool>().Enabled)
            {
                if (!Vars.E.GetPrediction(args.Sender).CollisionObjects.Any())
                {
                    Vars.E.Cast(Vars.E.GetPrediction(args.Sender).UnitPosition);
                }
            }

            if (Vars.W.IsReady() && args.Sender.IsValidTarget(Vars.W.Range)
                && Vars.Menu["spells"]["w"]["interrupter"].GetValue<MenuBool>().Enabled)
            {
                Vars.W.Cast(Vars.W.GetPrediction(args.Sender).CastPosition);
            }
        }

        /// <summary>
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void OnUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead)
            {
                return;
            }

            /// <summary>
            ///     Updates the spells.
            /// </summary>
            Spells.Initialize();

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
                case OrbwalkerMode.LaneClear:
                    Logics.Clear(args);
                    break;
            }
        }

        /// <summary>
        ///     Loads Caitlyn.
        /// </summary>
        public void OnLoad()
        {
            /// <summary>
            ///     Initializes the menus.
            /// </summary>
            Menus.Initialize();

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