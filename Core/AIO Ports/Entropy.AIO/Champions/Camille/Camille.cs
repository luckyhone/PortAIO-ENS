using System;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille
{
    #region

    using System.Linq;
    using Bases;
    using Logics;
    using Misc;
    using SharpDX;
    using static Components;

    #endregion

    sealed class Camille : ChampionBase
    {
        public Camille()
        {
            new Spells();
            new Menus();
            new Methods();
            new Drawings(Q, W, E, R);
        }

        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static void OnTick(EventArgs args)
        {
            if (LocalPlayer.IsDead || LocalPlayer.IsRecalling())
            {
                return;
            }

            if (Definitions.CastingW && MiscMenu.WFollowBool.Enabled)
            {
                Automatic.WMagnet();
                return;
            }

            Killsteal.ExecuteW();
            Killsteal.ExecuteQ();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo.ExecuteQ();
                    Combo.ExecuteW();
                    Combo.ExecuteE();
                    break;
                case OrbwalkerMode.Harass:
                    Harass.ExecuteQ();
                    Harass.ExecuteW();
                    break;
                case OrbwalkerMode.LaneClear:
                    StructureClear.ExecuteQ();
                    break;
            }
        }

        public static void OnCustomTick(EventArgs args)
        {
            if (!Definitions.CastingW)
            {
                Definitions.WDirection = Vector3.Zero;
            }
        }

        public static void OnPreMove(object sender, BeforeMoveEventArgs args)
        {
            if (Definitions.IsOnWall)
            {
                //Cancel any orbwalker movements when on a wall, so we don't jump to our cursor.
                args.Process = false;
            }
        }

        public static void OnGainBuff(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            if (args.Buff.Name == "CamilleEDash2" && MiscMenu.WInEBool.Enabled)
            {
                if (Definitions.LastETarget == null)
                {
                    return;
                }

                if (!Definitions.LastETarget.IsValidTarget(E.Range))
                {
                    return;
                }

                W.Cast(Definitions.LastETarget.Position);
                Definitions.LastETarget = null;
            }
        }

        public static void OnPostAttack(object sender, AfterAttackEventArgs args)
        {
            if (!Q.IsReady())
            {

                if (Orbwalker.ActiveMode != OrbwalkerMode.Combo  &&
                    Orbwalker.ActiveMode != OrbwalkerMode.Harass &&
                    Orbwalker.ActiveMode != OrbwalkerMode.LaneClear)
                {
                    return;
                }
                
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                case OrbwalkerMode.Harass:
                    Weaving.ExecuteQ();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear.ExecuteQ();
                    JungleClear.ExecuteQ();
                    Weaving.ExecuteQ();
                    break;
            }
        }

        public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            switch (args.Slot)
            {
                case SpellSlot.Q:
                    Definitions.LastQCast = Environment.TickCount;
                    break;
                case SpellSlot.W:
                    Definitions.LastWCast = Environment.TickCount;
                    var dir = (args.End - args.Start).Normalized();
                    Definitions.WDirection = dir;
                    break;
            }
        }
    }
}