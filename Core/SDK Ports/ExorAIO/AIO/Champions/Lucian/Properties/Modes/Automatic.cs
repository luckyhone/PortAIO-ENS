using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using ExorAIO.Utilities;

namespace ExorAIO.Champions.Lucian
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
        public static void Automatic(EventArgs args)
        {
            if (GameObjects.Player.IsRecalling())
            {
                return;
            }

            /// <summary>
            ///     The Automatic R Orbwalking.
            /// </summary>
            if (Vars.Menu["spells"]["r"]["bool"].GetValue<MenuBool>().Enabled
                && Vars.Menu["spells"]["r"]["key"].GetValue<MenuKeyBind>().Active)
            {
                DelayAction.Add(
                    (int)(100 + Game.Ping / 2f),
                    () => { ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos); });
            }

            /// <summary>
            ///     The Semi-Automatic R Management.
            /// </summary>
            if (Vars.R.IsReady() && Vars.Menu["spells"]["r"]["bool"].GetValue<MenuBool>().Enabled)
            {
                if (!GameObjects.Player.HasBuff("LucianR") && Targets.Target.IsValidTarget(Vars.R.Range)
                    && Vars.Menu["spells"]["r"]["key"].GetValue<MenuKeyBind>().Active)
                {
                    if (!Vars.W.GetPrediction(Targets.Target).CollisionObjects.Any())
                    {
                        Vars.W.Cast(Vars.W.GetPrediction(Targets.Target).UnitPosition);
                    }
                    Vars.R.Cast(Vars.R.GetPrediction(Targets.Target).UnitPosition);
                }
                else if (GameObjects.Player.HasBuff("LucianR")
                         && !Vars.Menu["spells"]["r"]["key"].GetValue<MenuKeyBind>().Active)
                {
                    Vars.R.Cast();
                }
            }
        }

        #endregion
    }
}