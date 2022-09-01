
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Jinx
{
    using System;
    using System.Linq;

    using ExorAIO.Utilities;


    /// <summary>
    ///     The champion class.
    /// </summary>
    internal class Jinx
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called on orbwalker action.
        /// </summary>
        /// <param name="sender">The object.</param>
        /// <param name="args">The <see cref="OrbwalkingActionArgs" /> instance containing the event data.</param>
        public static void OnAction(object sender, BeforeAttackEventArgs args)
        {
            
            const float SplashRange = 160f;
            var isUsingFishBones = GameObjects.Player.HasBuff("JinxQ");

            if (Vars.Q.IsReady())
            {
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.LastHit:
                    case OrbwalkerMode.LaneClear:
                        var minionTarget = args.Target as AIMinionClient;
                        var canLastHit = Vars.Menu["spells"]["q"]["lasthit"].GetValue<MenuSliderButton>().Enabled
                                         && GameObjects.Player.ManaPercent
                                         > ManaManager.GetNeededMana(
                                             Vars.W.Slot,
                                             Vars.Menu["spells"]["q"]["lasthit"]);

                        var canLaneClear = Vars.Menu["spells"]["q"]["clear"].GetValue<MenuSliderButton>().Enabled
                                           && GameObjects.Player.ManaPercent
                                           > ManaManager.GetNeededMana(
                                               Vars.W.Slot,
                                               Vars.Menu["spells"]["q"]["lasthit"]);
                        if (minionTarget != null)
                        {
                            var minionsInRange =
                                GameObjects.EnemyMinions.Count(m => m.Distance(minionTarget) < SplashRange);
                            if (isUsingFishBones)
                            {
                                if (minionsInRange < 3)
                                {
                                    Vars.Q.Cast();
                                }
                            }
                            else
                            {
                                if (minionsInRange >= 3
                                    && (Orbwalker.ActiveMode == OrbwalkerMode.LastHit && canLastHit
                                        || Orbwalker.ActiveMode == OrbwalkerMode.LaneClear
                                        && canLaneClear))
                                {
                                    Vars.Q.Cast();
                                }
                            }
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

            if (Vars.E.IsReady() && sender.IsValidTarget(Vars.E.Range)
                && Vars.Menu["spells"]["e"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                Vars.E.Cast(args.EndPosition);
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
            ///     Initializes the Killsteal events.
            /// </summary>
            Logics.Killsteal(args);
            if (GameObjects.Player.Spellbook.IsAutoAttack)
            {
                return;
            }

            /// <summary>
            ///     Initializes the Automatic actions.
            /// </summary>
            Logics.Automatic(args);

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
            }
        }

        /// <summary>
        ///     Loads Jinx.
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