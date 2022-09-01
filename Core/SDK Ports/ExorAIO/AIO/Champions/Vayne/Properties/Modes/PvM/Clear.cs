
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#pragma warning disable 1587

namespace ExorAIO.Champions.Vayne
{
    using System.Linq;

    using ExorAIO.Utilities;


    using SharpDX;

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
        public static void BuildingClear(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!(Orbwalker.GetTarget() is HQClient) && !(Orbwalker.GetTarget() is AITurretClient)
                && !(Orbwalker.GetTarget() is BarracksDampenerClient))
            {
                return;
            }

            /// <summary>
            ///     The Q BuildingClear Logic.
            /// </summary>
            if (Vars.Q.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["buildings"])
                && Vars.Menu["spells"]["q"]["buildings"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.Q.Cast(Game.CursorPos);
            }
        }

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void Clear(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            /// <summary>
            ///     The Q FarmHelper Logic.
            /// </summary>
            if (Vars.Q.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["farmhelper"])
                && Vars.Menu["spells"]["q"]["farmhelper"].GetValue<MenuSliderButton>().Enabled)
            {
                if (Targets.Minions.Any()
                    && Targets.Minions.Count(
                        m =>
                        Vars.GetRealHealth(m)
                        < GameObjects.Player.GetAutoAttackDamage(m)
                        + (float)GameObjects.Player.GetSpellDamage(m, SpellSlot.Q)
                        && m.Distance(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, 300f))
                        < GameObjects.Player.GetRealAutoAttackRange()) > 1)
                {
                    Vars.Q.Cast(Game.CursorPos);
                }
            }
        }

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void JungleClear(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!(Orbwalker.GetTarget() is AIMinionClient)
                || !Targets.JungleMinions.Contains(Orbwalker.GetTarget() as AIMinionClient))
            {
                return;
            }

            /// <summary>
            ///     The E JungleClear Logic.
            /// </summary>
            if (Vars.E.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.E.Slot, Vars.Menu["spells"]["e"]["jungleclear"])
                && Vars.Menu["spells"]["e"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
            {
                var target = (AIMinionClient)Orbwalker.GetTarget();
                if (!target.Name.Contains("SRU_Dragon")
                    && !target.Name.Equals("SRU_Baron")
                    && !target.Name.Equals("SRU_Riftherald"))
                {
                    for (var i = 1; i < 10; i++)
                    {
                        var position = target.ServerPosition;
                        var prediction = Vars.E.GetPrediction(target).UnitPosition;
                        var prediction2 = Vars.E2.GetPrediction(target).UnitPosition;
                        var vector = Vector3.Normalize(target.ServerPosition - GameObjects.Player.ServerPosition);
                        if ((position + vector * (i * 42)).IsWall() && (position + vector * (i * 45)).IsWall()
                            && (prediction + vector * (i * 42)).IsWall() && (prediction + vector * (i * 45)).IsWall()
                            && (prediction2 + vector * (i * 42)).IsWall() && (prediction2 + vector * (i * 45)).IsWall())
                        {
                            Vars.E.CastOnUnit(target);
                            return;
                        }
                    }
                }
            }

            /// <summary>
            ///     The Q JungleClear Logic.
            /// </summary>
            if (Vars.Q.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["spells"]["q"]["jungleclear"])
                && Vars.Menu["spells"]["q"]["jungleclear"].GetValue<MenuSliderButton>().Enabled)
            {
                Vars.Q.Cast(Game.CursorPos);
            }
        }

        #endregion
    }
}