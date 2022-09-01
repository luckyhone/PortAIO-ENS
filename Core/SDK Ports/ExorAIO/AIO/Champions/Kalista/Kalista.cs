
using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using ExorAIO.Utilities;
using SharpDX;

#pragma warning disable 1587

namespace ExorAIO.Champions.Kalista
{

    /// <summary>
    ///     The champion class.
    /// </summary>
    internal class Kalista
    {
        #region Static Fields

        /// <summary>
        ///     Gets all the important jungle locations.
        /// </summary>
        internal static readonly List<Vector3> Locations = new List<Vector3>
                                                               {
                                                                   new Vector3(9827.56f, 4426.136f, -71.2406f),
                                                                   new Vector3(4951.126f, 10394.05f, -71.2406f),
                                                                   new Vector3(10998.14f, 6954.169f, 51.72351f),
                                                                   new Vector3(7082.083f, 10838.25f, 56.2041f),
                                                                   new Vector3(3804.958f, 7875.456f, 52.11121f),
                                                                   new Vector3(7811.249f, 4034.486f, 53.81299f)
                                                               };

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the SoulBound.
        /// </summary>
        public static AIHeroClient SoulBound { get; set; } = null;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns true if the target is a perfectly valid rend target.
        /// </summary>
        public static bool IsPerfectRendTarget(AIBaseClient target)
        {
            var hero = target as AIHeroClient;
            return (hero == null || !Invulnerable.Check(hero)) && target.IsValidTarget(Vars.E.Range)
                   && target.HasBuff("kalistaexpungemarker");
        }

        /// <summary>
        ///     Called on orbwalker action.
        /// </summary>
        /// <param name="sender">The object.</param>
        /// <param name="args">The <see cref="OrbwalkingActionArgs" /> instance containing the event data.</param>
        public static void OnAction(object sender, BeforeAttackEventArgs args)
        {
            /// <summary>
            ///     The Target Forcing Logic.
            /// </summary>
            var hero = args.Target as AIHeroClient;
            var bestTarget =
                GameObjects.EnemyHeroes.Where(
                    t =>
                    t.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())
                    && t.HasBuff("kalistacoopstrikemarkally"))
                    .OrderByDescending(TargetSelector.GetPriority)
                    .FirstOrDefault();
            if (hero != null && bestTarget?.NetworkId != hero.NetworkId
                && Vars.GetRealHealth(hero) > GameObjects.Player.GetAutoAttackDamage(hero) * 3)
            {
                Orbwalker.ForceTarget = bestTarget;
                return;
            }

            Orbwalker.ForceTarget = null;
        }

        public static void OnNonKillableMinion(object sender, NonKillableMinionEventArgs args)
        {
            if (Vars.E.IsReady() && IsPerfectRendTarget(args.Target as AIMinionClient)
                                 && Vars.GetRealHealth(args.Target as AIMinionClient)
                                 < (float)GameObjects.Player.GetSpellDamage(args.Target as AIMinionClient, SpellSlot.E)
                                 + (float)
                                 GameObjects.Player.GetSpellDamage(args.Target as AIMinionClient, SpellSlot.E))
            {
                Vars.E.Cast();
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
            if (GameObjects.Player.Spellbook.IsAutoAttack || GameObjects.Player.IsDashing())
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
        ///     Loads Kalista.
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

            /// <summary>
            ///     Initializes the damage drawings.
            /// </summary>
            Healthbars.Initialize();
        }

        #endregion
        
    }
}