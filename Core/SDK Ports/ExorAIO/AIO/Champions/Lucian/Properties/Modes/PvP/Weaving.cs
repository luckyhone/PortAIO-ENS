using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace ExorAIO.Champions.Lucian
{
    using ExorAIO.Utilities;
    /// <summary>
    ///     The logics class.
    /// </summary>
    internal partial class Logics
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void Weaving(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!(args.Target is AIHeroClient) || Invulnerable.Check((AIHeroClient)args.Target))
            {
                return;
            }

            /// <summary>
            ///     The E Combo Logic.
            /// </summary>
            if (Vars.E.IsReady())
            {
                if (!Game.CursorPos.IsUnderEnemyTurret()
                    || ((AIHeroClient)args.Target).Health
                    < GameObjects.Player.GetAutoAttackDamage((AIHeroClient)args.Target) * 2)
                {
                    switch (Vars.Menu["spells"]["e"]["mode"].GetValue<MenuList>().Index)
                    {
                        case 0:
                            Vars.E.Cast(
                                GameObjects.Player.ServerPosition.Extend(
                                    Game.CursorPos,
                                    GameObjects.Player.Distance(Game.CursorPos)
                                    < GameObjects.Player.GetRealAutoAttackRange()
                                        ? GameObjects.Player.BoundingRadius
                                        : 475f));
                            break;
                        case 1:
                            Vars.E.Cast(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, 475f));
                            break;
                        case 2:
                            Vars.E.Cast(
                                GameObjects.Player.ServerPosition.Extend(
                                    Game.CursorPos,
                                    GameObjects.Player.BoundingRadius));
                            break;
                    }

                    return;
                }
            }

            /// <summary>
            ///     The Q Combo Logic.
            /// </summary>
            if (Vars.Q.IsReady() && ((AIHeroClient)args.Target).IsValidTarget(Vars.Q.Range)
                && Vars.Menu["spells"]["q"]["combo"].GetValue<MenuBool>().Enabled)
            {
                Vars.Q.CastOnUnit((AIHeroClient)args.Target);
                return;
            }

            /// <summary>
            ///     The W Combo Logic.
            /// </summary>
            if (Vars.W.IsReady() && ((AIHeroClient)args.Target).IsValidTarget(Vars.W.Range)
                && Vars.Menu["spells"]["w"]["combo"].GetValue<MenuBool>().Enabled)
            {
                Vars.W.Cast(Vars.W.GetPrediction((AIHeroClient)args.Target).UnitPosition);
            }
        }

        #endregion
    }
}