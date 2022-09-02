using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Neeko
{
    using Bases;
    using Logics;
    using Misc;
    using static Components;

    sealed class Neeko : ChampionBase
    {
        public Neeko()
        {
            new Spells();
            new Menus();
            new Methods();
            new Drawings(Q, E, R);
        }

        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static void OnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (LocalPlayer.IsDead || !GameObjects.EnemyHeroes.Contains(sender))
            {
                return;
            }

            if (E.IsReady() && ComboMenu.EBool.Enabled)
            {
                UponDash.ExecuteE(args,sender);
            }
        }

        public static void OnCustomTick(EventArgs args)
        {
            if (LocalPlayer.IsDead)
            {
                return;
            }

            if (Q.IsReady())
            {
                if (KillstealMenu.QBool.Enabled)
                {
                    Killsteal.ExecuteQ();
                    return;
                }
            }

            if (E.IsReady())
            {
                if (KillstealMenu.EBool.Enabled)
                {
                    Killsteal.ExecuteE();
                }
            }
        }

        public static void OnTick(EventArgs args)
        {
            if (LocalPlayer.IsDead)
            {
                return;
            }

            if (E.IsReady())
            {
                Automatic.EOnImmobile();
            }

            if (R.IsReady())
            {
                Automatic.ExecuteR();
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (E.IsReady())
                    {
                        Combo.ExecuteE();
                    }

                    if (Q.IsReady())
                    {
                        Combo.ExecuteQ();
                    }

                    break;

                case OrbwalkerMode.Harass:
                    if (Q.IsReady())
                    {
                        Harass.ExecuteQ();
                    }

                    break;

                case OrbwalkerMode.LaneClear:
                    if (E.IsReady())
                    {
                        LaneClear.ExecuteE();
                        JungleClear.ExecuteE();
                    }

                    if (Q.IsReady())
                    {
                        LaneClear.ExecuteQ();
                        JungleClear.ExecuteQ();
                    }

                    break;
            }
        }

        public static void OnPreAttack(object sender, BeforeAttackEventArgs args)
        {
            if (Definitions.InShape()                  &&
                Orbwalker.ActiveMode == OrbwalkerMode.Combo &&
                MiscellaneousMenu.BlockAAInComboIfShape.Enabled)
            {
                args.Process = true;
            }
        }

        public static void GapCloser(AIHeroClient sender, EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (LocalPlayer.IsDead)
            {
                return;
            }

            if (sender == null || !sender.IsValid || !sender.IsEnemy || !sender.IsMelee)
            {
                return;
            }

            if (E.IsReady() && !Invulnerable.Check(sender))
            {
                AntiGapcloser.ExecuteE(args,sender);
                return;
            }

            if (W.IsReady())
            {
                AntiGapcloser.ExecuteW(args,sender);
            }
        }
    }
}