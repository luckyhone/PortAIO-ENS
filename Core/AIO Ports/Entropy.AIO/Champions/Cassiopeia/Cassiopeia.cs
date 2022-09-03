using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Cassiopeia
{
    using Bases;
    using Logics;
    using Misc;
    using static Components;

    sealed class Cassiopeia : ChampionBase
    {
        public Cassiopeia()
        {
            new Spells();
            new Menus();
            new Methods();
            new Drawings(Q, W, E, R);
        }

        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static void OnTick(EventArgs args)
        {
            if (LocalPlayer.IsDead)
            {
                return;
            }

            if (E.IsReady() &&
                KillStealMenu.EBool.Enabled)
            {
                Killsteal.ExecuteE();
            }

            if (Q.IsReady() &&
                KillStealMenu.QBool.Enabled)
            {
                Killsteal.ExecuteQ();
            }

            if (ComboMenu.RMode.Index != 0)
            {
                ComboMenu.hpR.Visible = false;
            }

            if (ComboMenu.RMode.Index == 0)
            {
                ComboMenu.hpR.Visible = true;
            }

            MinionManager.MinionList();
            R.Range = MiscMenu.MaxR.Value;
            Other.SemiR();
            Other.FlashR();
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo.ExecuteR();
                    Combo.TeamfightR();
                    Combo.ExecuteCombo();
                    break;

                case OrbwalkerMode.Harass:
                    Harass.ExecuteE();
                    Harass.ExecuteQ();
                    Harass.ExecuteW();
                    Harass.ExecuteELast();
                    break;

                case OrbwalkerMode.LastHit:
                    Lasthit.ExecuteE();
                    break;

                case OrbwalkerMode.LaneClear:
                    JungleClear.ExecuteQPush();
                    JungleClear.ExecuteEPushLast();
                    JungleClear.ExecuteEPush();
                    if (LaneClearMenu.farmKey.Active)
                    {
                        LaneClear.ExecuteEPassive();
                    }

                    if (!LaneClearMenu.farmKey.Active)
                    {
                        LaneClear.ExecuteQPush();
                        LaneClear.ExecuteEPushLast();
                        LaneClear.ExecuteEPush();
                    }

                    break;
            }
        }

        public static void OnPreAttack(object sender, BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                if (LaneClearMenu.EAA.Enabled && args.Target is AIMinionClient)
                {
                    if (LocalPlayer.ManaPercent > 100 && E.Level > 0)
                    {
                        args.Process = true;
                    }
                }
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit   ||
                Orbwalker.ActiveMode == OrbwalkerMode.LaneClear ||
                Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                if (Lasthit.possibleTarget != null && args.Target == Lasthit.possibleTarget)
                {
                    args.Process = true;
                }
            }
        }

        public static void OnNewGapcloser(AIHeroClient sender, EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (LocalPlayer.IsDead)
            {
                return;
            }

            if (sender == null || !sender.IsEnemy || !sender.IsMelee)
            {
                return;
            }

            if (R.IsReady())
            {
                AntiGapcloser.ExecuteR(sender,args);
            }
        }

        public static void OnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (LocalPlayer.IsDead || !GameObjects.EnemyHeroes.Contains(sender))
            {
                return;
            }

            if (Q.IsReady())
            {
                Other.ExecuteQ(sender,args);
            }
        }

        public static void OnInterruptableSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (LocalPlayer.IsDead)
            {
                return;
            }

            var heroSender = sender as AIHeroClient;

            if (heroSender == null || !heroSender.IsEnemy)
            {
                return;
            }

            if (Invulnerable.Check(heroSender,DamageType.Magical,false))
            {
                return;
            }

            if (R.IsReady())
            {
                OnInterruptable.ExecuteR(sender, args);
            }
        }
    }
}