using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace ExorAIO.Champions.Lucian
{
    using ExorAIO.Utilities;
    /// <summary>
    ///     The champion class.
    /// </summary>
    internal class Lucian
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called on orbwalker action.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="OrbwalkingActionArgs" /> instance containing the event data.</param>
        public static void OnAction(object sender, BeforeAttackEventArgs args)
        {
            if (GameObjects.Player.HasBuff("LucianR"))
            {
                args.Process = false;
            }
        }

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && !GameObjects.Player.HasBuff("LucianR") && Orbwalker.IsAutoAttack(args.SData.Name))
            {
                /// <summary>
                ///     Initializes the orbwalkingmodes.
                /// </summary>
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        Logics.Weaving(sender, args);
                        break;
                    case OrbwalkerMode.LaneClear:
                        Logics.LaneClear(sender, args);
                        Logics.JungleClear(sender, args);
                        Logics.BuildingClear(sender, args);
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
            if (GameObjects.Player.IsDead)
            {
                return;
            }

            if (Vars.E.IsReady() && sender.IsMelee && args.Type == AntiGapcloser.GapcloserType.Targeted
                && Vars.Menu["spells"]["e"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                if (args.Target.IsMe)
                {
                    Vars.E.Cast(GameObjects.Player.ServerPosition.Extend(sender.ServerPosition, -475f));
                }
            }
        }

        /// <summary>
        ///     Called on animation trigger.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectPlayAnimationEventArgs" /> instance containing the event data.</param>
        public static void OnPlayAnimation(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs args)
        {
            if (sender.IsMe && Orbwalker.ActiveMode != OrbwalkerMode.None)
            {
                if (args.Animation.Equals("Spell1") || args.Animation.Equals("Spell2"))
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
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
            ///     Initializes the Automatic actions.
            /// </summary>
            Logics.Automatic(args);

            /// <summary>
            ///     Initializes the Killsteal events.
            /// </summary>
            Logics.Killsteal(args);
            if (GameObjects.Player.Spellbook.IsAutoAttack || GameObjects.Player.HasBuff("LucianR"))
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
                case OrbwalkerMode.LaneClear:
                    Logics.Clear(args);
                    break;
                case OrbwalkerMode.Harass:
                    Logics.Harass(args);
                    break;
            }
        }

        /// <summary>
        ///     Loads Lucian.
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