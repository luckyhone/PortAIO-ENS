
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using HERMES_Kalista.MyLogic.Others;

#pragma warning disable 1587

namespace ExorAIO.Champions.Vayne
{
    using System;
    using System.Linq;

    using ExorAIO.Utilities;


    using SharpDX;

    /// <summary>
    ///     The champion class.
    /// </summary>
    internal class Vayne
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called on orbwalker action.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="OrbwalkingActionArgs" /> instance containing the event data.</param>
        public static void OnAction(object sender, BeforeAttackEventArgs args)
        {
            /// <summary>
            ///     The Automatic Stealth Logics.
            /// </summary>
            if (!GameObjects.Player.IsUnderEnemyTurret() && GameObjects.Player.HasBuff("vaynetumblefade"))
            {
                /// <summary>
                ///     The Automatic Stealth Logic.
                /// </summary>
                if (GameObjects.Player.GetBuff("vaynetumblefade").EndTime - Game.Time
                    > GameObjects.Player.GetBuff("vaynetumblefade").EndTime
                    - GameObjects.Player.GetBuff("vaynetumblefade").StartTime
                    - Vars.Menu["miscellaneous"]["stealthtime"].GetValue<MenuSlider>().Value / 1000f)
                {
                    args.Process = false;
                }

                /// <summary>
                ///     The Automatic Stealth Logic.
                /// </summary>
                else if (GameObjects.Player.HasBuff("summonerexhaust")
                         || GameObjects.Player.HasBuffOfType(BuffType.Blind))
                {
                    args.Process = false;
                }
            }

            /// <summary>
            ///     The Target Forcing Logic.
            /// </summary>
            var hero = args.Target as AIHeroClient;
            var bestTarget =
                GameObjects.EnemyHeroes.FirstOrDefault(
                    t =>
                    t.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange())
                    && t.HasBuff("vaynesilvereddebuff"));
            if (hero != null && bestTarget?.NetworkId != hero.NetworkId
                && Vars.GetRealHealth(hero) > GameObjects.Player.GetAutoAttackDamage(hero) * 3)
            {
                Orbwalker.ForceTarget = bestTarget;
                return;
            }

            Orbwalker.ForceTarget = null;
        }

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Orbwalker.IsAutoAttack(args.SData.Name))
            {
                /// <summary>
                ///     Initializes the orbwalkingmodes.
                /// </summary>
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        Logics.Weaving(sender, args);
                        break;
                    case OrbwalkerMode.LastHit:
                        Logics.Clear(sender, args);
                        break;
                    case OrbwalkerMode.LaneClear:
                        Logics.Clear(sender, args);
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
            if (GameObjects.Player.IsDead || Invulnerable.Check(sender, DamageType.Magical, false))
            {
                return;
            }

            if (Vars.E.IsReady() && sender.IsValidTarget(Vars.E.Range))
            {
                /// <summary>
                ///     The Anti-GapCloser E Logic.
                /// </summary>
                if (sender.IsMelee 
                    && Vars.Menu["spells"]["e"]["gapcloser"].GetValue<MenuBool>().Enabled)
                {
                    Vars.E.CastOnUnit(sender);
                }

                /// <summary>
                ///     The Dash-Condemn Prediction Logic.
                /// </summary>
                if (!GameObjects.Player.IsDashing()
                    && GameObjects.Player.Distance(args.EndPosition) > GameObjects.Player.BoundingRadius
                    && Vars.Menu["spells"]["e"]["dashpred"].GetValue<MenuBool>().Enabled
                    && Vars.Menu["spells"]["e"]["whitelist"][sender.CharacterName.ToLower()].GetValue<MenuBool>()
                           .Enabled)
                {
                    for (var i = 1; i < 10; i++)
                    {
                        var vector = Vector3.Normalize(args.EndPosition - GameObjects.Player.ServerPosition);
                        if ((args.EndPosition + vector * (float)(i * 42.5)).IsWall()
                            && (args.EndPosition + vector * (float)(i * 44.5)).IsWall())
                        {
                            Vars.E.CastOnUnit(sender);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Called on interruptable spell.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Events.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        public static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (GameObjects.Player.IsDead || Invulnerable.Check(args.Sender, DamageType.Magical, false))
            {
                return;
            }

            if (Vars.E.IsReady() && args.Sender.IsValidTarget(Vars.E.Range)
                && Vars.Menu["spells"]["e"]["interrupter"].GetValue<MenuBool>().Enabled)
            {
                Vars.E.CastOnUnit(args.Sender);
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
            }
        }

        /// <summary>
        ///     Loads Tryndamere.
        /// </summary>
        public void OnLoad()
        {
            /// <summary>
            ///     Initializes the menus.
            /// </summary>
            Menus.Initialize();

            /// <summary>
            ///     Updates the spells.
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
            ///     Initializes the prediction drawings.
            /// </summary>
            PredictionDrawings.Initialize();
        }

        #endregion
    }
}