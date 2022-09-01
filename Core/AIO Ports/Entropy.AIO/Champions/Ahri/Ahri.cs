using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Ahri.Logics;
using Entropy.AIO.Bases;
using AntiGapcloser = Entropy.AIO.Ahri.Logics.AntiGapcloser;

namespace Entropy.AIO.Ahri
{
    sealed class Ahri : ChampionBase
    {
        public Ahri()
        {
            new Spells();
            new Menus();
            new Methods();
            new Drawings(Q, W, E);
        }

        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void OnTick(EventArgs args)
        {
            if (LocalPlayer.IsDead || LocalPlayer.IsRecalling())
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (R.IsReady())
                    {
                        Combo.ExecuteR();
                    }

                    if (E.IsReady())
                    {
                        Combo.ExecuteE();
                    }

                    if (Q.IsReady())
                    {
                        Combo.ExecuteQ();
                    }

                    if (W.IsReady())
                    {
                        Combo.ExecuteW();
                    }

                    break;
                case OrbwalkerMode.Harass:
                    if (E.IsReady())
                    {
                        Harass.ExecuteE();
                    }

                    if (Q.IsReady())
                    {
                        Harass.ExecuteQ();
                    }

                    if (W.IsReady())
                    {
                        Harass.ExecuteW();
                    }

                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear.Execute();
                    JungleClear.Execute();
                    break;
            }
        }

        public static void OnCustomTick(EventArgs args)
        {
            if (LocalPlayer.IsDead || LocalPlayer.IsRecalling())
            {
                return;
            }

            if (Q.IsReady())
            {
                Killsteal.ExecuteQ();
            }

            if (E.IsReady())
            {
                Killsteal.ExecuteE();
            }
        }

        public static void OnNewGapcloser(AIHeroClient sender, EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (!E.IsReady() ||
                LocalPlayer.IsDead)
            {
                return;
            }
            

            if (sender == null || !sender.IsValid || !sender.IsEnemy || !sender.IsMelee)
            {
                return;
            }

            AntiGapcloser.ExecuteE(args);
        }
    }
}