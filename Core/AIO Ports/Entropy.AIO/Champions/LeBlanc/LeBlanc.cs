using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using LeagueSharpCommon;

namespace Entropy.AIO.LeBlanc
{
    using Logic;
    using static Components;
    sealed class LeBlanc : ChampionBase
    {
        public LeBlanc()
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

            if (Q.IsReady())
            {
                Killsteal.ExecuteQ();
            }

            if (W.IsReady())
            {
                Killsteal.ExecuteW();
            }

            if (E.IsReady())
            {
                Killsteal.ExecuteE();
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
                    if (Q.IsReady())
                    {
                        Laneclear.ExecuteQ();
                    }

                    if (W.IsReady())
                    {
                        Laneclear.ExecuteW();
                    }

                    break;
            }
        }

        public static void OnWndProc(GameEvent.WindowndEventArgs args)
        {
            if (args.Msg != (ulong)WindowsMessages.WM_KEYUP)
            {
                return;
            }

            if (args.WParam != (ulong)ComboMenu.RModeKey.Key)
            {
                return;
            }

            var menu = ComboMenu.RModelist; //MenuBase.Champion.Children["combo"].Children["rmode"].As<MenuList>();
            if (menu.Index == 1)
            {
                menu.Index = 0;
            }
            else
            {
                menu.Index++;
                if (menu.Index > 1)
                {
                    menu.Index = 1;
                }
            }
        }
    }
}