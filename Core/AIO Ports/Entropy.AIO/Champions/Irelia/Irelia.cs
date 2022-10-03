using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Irelia
{
    #region

    using System.Linq;
    using Logics;
    using Misc;
    using SharpDX;
    using static Components;
    using Champion = Bases.ChampionBase;

    #endregion

    class Irelia : Champion
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static float sheenTimer;

        public Irelia()
        {
            new Spells();
            new Menus();
            new Methods();
            new Drawings(Q, W, E, R);
        }

        public static void OnTick(EventArgs args)
        {
            if (LocalPlayer.IsDead)
            {
                return;
            }

            MinionManager.MinionList();

            Killsteal.ExecuteE();
            Killsteal.ExecuteQ();
            Combo.AutoW();

            if (ComboMenu.semiR.Active)
            {
                var target = TargetSelector.GetTarget(R.Range,DamageType.Physical);
                if (target != null)
                {
                    R.Cast(target);
                }
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:

                    Combo.ExecuteCombo();
                    break;

                case OrbwalkerMode.Harass:
                    Harass.ExecuteHarass();
                    break;
                case OrbwalkerMode.LastHit:
                    if (LaneClearMenu.farmKey.Active)
                    {
                        Lasthit.ExecuteQ();
                    }

                    break;

                case OrbwalkerMode.LaneClear:
                    if (LaneClearMenu.farmKey.Active)
                    {
                        Lasthit.ExecuteQ();
                        JungleClear.ExecuteE();
                        JungleClear.ExecuteQ();
                    }

                    break;
                case OrbwalkerMode.Flee:
                    Flee.Execute();
                    break;
            }
        }

        public static void OnInterruptableSpell(AIBaseClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (LocalPlayer.IsDead)
            {
                return;
            }

            if (E.IsReady())
            {
                OnInterruptable.ExecuteE(sender, args);
            }
        }

        public static void OnDelete(GameObject sender, EventArgs args)
        {
            var obj = sender;
            if (obj == null)
            {
                return;
            }

            if (obj.Name.Contains("E_Blades"))
            {
                Combo.e1Pos = Vector3.Zero;
            }
        }

        public static void BuffManagerOnOnLoseBuff(AIBaseClient sender, AIBaseClientBuffRemoveEventArgs args)
        {
            if (sender == LocalPlayer)
            {
                if (sender.Name == "sheen" || sender.Name == "TrinityForce")
                {
                    sheenTimer = Game.Time + 1.7f;
                }
            }
        }

        public static void OnWndProc(GameWndEventArgs args)
        {
            //if (args.WParam == )
            //{
                if (args.Msg == (ulong)WindowsKeyMessages.KEYFIRST)
                {
                    Definitions.wHeld = Game.Time;
                }
            //}
        }

        public static void OnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (LocalPlayer.IsDead || !GameObjects.EnemyHeroes.Contains(sender))
            {
                return;
            }

            if (!Combo.e1Pos.IsZero && E.Name == "IreliaE2")
            {
                if (args.EndPos.DistanceToPlayer() <= E.Range)
                {
                    E.Cast(args.EndPos);
                    Combo.e1Pos = Vector3.Zero;
                }
            }
        }

        public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Slot == SpellSlot.E)
            {
                Combo.e1Pos = args.End.DistanceToPlayer() <= E.Range
                    ? args.End
                    : LocalPlayer.Position.Extend(args.End, E.Range);
            }

            if (args.Slot == SpellSlot.W)
            {
                Definitions.chargeW = Game.Time;
            }
        }
    }
}