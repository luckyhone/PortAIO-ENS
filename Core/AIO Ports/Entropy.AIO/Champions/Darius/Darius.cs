using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Darius.Misc.Logics;
using ExorAIO.Champions.Darius;
using AntiGapcloser = Entropy.AIO.Darius.Misc.Logics.AntiGapcloser;
using Components = Entropy.AIO.Darius.Misc.Components;

namespace Entropy.AIO.Darius
{
    using Bases;
    using static Components;

    sealed class Darius : ChampionBase
    {
        public Darius()
        {
            new Spells();
            new Menus();
            new Methods();
            new Drawings(Q, W, E, R);
        }

        public static void OnPostAttack(object sender, AfterAttackEventArgs args)
        {
            Definitions.AllowQ = true;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:

                    if (ComboMenu.WBool.Enabled && ComboMenu.WAA.Enabled && W.IsReady())
                    {
                        if (W.Cast())
                        {
                            Definitions.LastW = Game.Time + 0.5f;
                        }
                    }

                    break;
                case OrbwalkerMode.Harass:

                    if (HarassMenu.WBool.Enabled && HarassMenu.WAA.Enabled && W.IsReady())
                    {
                        if (W.Cast())
                        {
                            Definitions.LastW = Game.Time + 0.5f;
                        }
                    }

                    break;
                case OrbwalkerMode.LaneClear:
                    if (LaneClearMenu.farmKey.Active)
                    {
                        var target = args.Target as AIMinionClient;
                        if (JungleClearMenu.WBool.Enabled && W.IsReady() && target != null && target.IsJungle())
                        {
                            if (W.Cast())
                            {
                                Definitions.LastW = Game.Time + 0.5f;
                            }
                        }
                    }

                    break;
            }
        }

        public static void PreAttack(object sender, BeforeAttackEventArgs e)
        {
            Definitions.AllowQ = false;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:

                    if (ComboMenu.WBool.Enabled && !ComboMenu.WAA.Enabled && W.IsReady())
                    {
                        if (W.Cast())
                        {
                            Definitions.LastW = Game.Time + 0.5f;
                        }
                    }

                    break;
                case OrbwalkerMode.Harass:

                    if (HarassMenu.WBool.Enabled && !HarassMenu.WAA.Enabled && W.IsReady())
                    {
                        if (W.Cast())
                        {
                            Definitions.LastW = Game.Time + 0.5f;
                        }
                    }

                    break;
            }
        }

        public static void OnNewGapcloser(AIHeroClient sender, EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (!E.IsReady() ||
                ObjectManager.Player.IsDead)
            {
                return;
            }


            if (sender == null || !sender.IsValid || !sender.IsEnemy || !sender.IsMelee)
            {
                return;
            }

            AntiGapcloser.ExecuteE(sender,args);
        }

        public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Slot == SpellSlot.W)
            {
                Definitions.AllowQ = false;
                Definitions.LastW  = Game.Time + 0.5f;
            }
        }

        public static void OnInterruptableSpell(AIBaseClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            var heroSender = sender as AIHeroClient;

            if (heroSender == null || !heroSender.IsEnemy)
            {
                return;
            }

            if (Invulnerable.Check(heroSender))
            {
                return;
            }

            if (E.IsReady())
            {
                OnInterruptable.ExecuteE(sender, args);
            }
        }

        public static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            Killsteal.ExecuteR();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Other.Magnet();
                    Combo.ExecuteE();
                    Combo.ExecuteQ();
                    break;
                case OrbwalkerMode.Harass:
                    Other.Magnet();
                    Harass.ExecuteE();
                    Harass.ExecuteQ();
                    break;
                case OrbwalkerMode.LaneClear:
                    if (LaneClearMenu.farmKey.Active)
                    {
                        JungleClear.ExecuteQ();

                        LaneClear.ExecuteW();
                    }

                    break;
                case OrbwalkerMode.LastHit:
                    if (LaneClearMenu.farmKey.Active)
                    {
                        LaneClear.ExecuteWLast();
                    }

                    break;
            }
        }
    }
}