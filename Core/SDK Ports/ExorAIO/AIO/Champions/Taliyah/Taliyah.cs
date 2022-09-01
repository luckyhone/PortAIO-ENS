
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using HERMES_Kalista.MyLogic.Others;

#pragma warning disable 1587

namespace ExorAIO.Champions.Taliyah
{
    using System;
    using System.Linq;

    using ExorAIO.Utilities;


    /// <summary>
    ///     The champion class.
    /// </summary>
    internal class Taliyah
    {
        #region Static Fields

        /// <summary>
        ///     Defines the Terrain object.
        /// </summary>
        public static GameObject TerrainObject;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Called when an object gets created by the game.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.Name.Equals("Taliyah_Base_Q_aoe_bright.troy"))
            {
                TerrainObject = obj;
            }
        }

        /// <summary>
        ///     Called when an object gets deleted by the game.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.Name.Equals("Taliyah_Base_Q_aoe_bright.troy"))
            {
                DelayAction.Add(
                    500,
                    () =>
                        {
                            if (
                                !ObjectManager.Get<GameObject>()
                                     .Any(
                                         o =>
                                         o.IsAlly && o.Distance(GameObjects.Player) < 412.5f
                                         && o.Name.Equals("Taliyah_Base_Q_aoe_bright.troy")))
                            {
                                TerrainObject = null;
                            }
                        });
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

            if (Vars.E.IsReady() && GameObjects.Player.Distance(args.EndPosition) < Vars.E.Range - 50f
                && Vars.Menu["spells"]["e"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                Vars.E.Cast(args.EndPosition);
            }

            if (Vars.W.IsReady() && sender.IsValidTarget(Vars.W.Range)
                && Vars.Menu["spells"]["w"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                if (sender.CharacterName.Equals("MasterYi"))
                {
                    DelayAction.Add(250, () => { Vars.W.Cast(GameObjects.Player.ServerPosition, args.StartPosition); });
                    return;
                }

                Vars.W.Cast(
                    args.EndPosition,
                    sender.IsMelee
                        ? GameObjects.Player.ServerPosition.Extend(args.EndPosition, GameObjects.Player.Distance(args.EndPosition) * 2)
                        : GameObjects.Player.ServerPosition);
            }
        }

        /// <summary>
        ///     Called on interruptable spell.
        /// </summary>
        /// <param name="sender">The object.</param>
        /// <param name="args">The <see cref="Events.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        public static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (GameObjects.Player.IsDead || Invulnerable.Check(args.Sender, DamageType.Magical, false))
            {
                return;
            }

            if (Vars.W.IsReady() && args.Sender.IsValidTarget(Vars.W.Range)
                && Vars.Menu["spells"]["w"]["interrupter"].GetValue<MenuBool>().Enabled)
            {
                Vars.W.Cast(args.Sender.ServerPosition, GameObjects.Player.ServerPosition);
            }
        }

        /// <summary>
        ///     Called while processing spellcast operations.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            /// <summary>
            ///     Automatically Mount on R Logic.
            /// </summary>
            if (Vars.R.IsReady() && sender != null && sender.IsMe && args.Slot != SpellSlot.Unknown
                && args.Slot.Equals(SpellSlot.R) && Vars.Menu["miscellaneous"]["mountr"].GetValue<MenuBool>().Enabled)
            {
                Vars.R.Cast();
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
            ///     Initializes the spells.
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
        ///     Loads Taliyah.
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