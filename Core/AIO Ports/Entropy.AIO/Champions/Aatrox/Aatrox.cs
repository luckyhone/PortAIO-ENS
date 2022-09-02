using System;
using ADCCOMMON;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Aatrox
{
    using Bases;
    using Logics;
    using static Components;

    sealed class Aatrox : ChampionBase
    {
        public Aatrox()
        {
            new Spells();
            new Menus();
            new Methods();
            new Drawings(Q, W, E, R);
        }

        public static void OnPostAttack(object sender, AfterAttackEventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:

                    if (ComboMenu.QBool.Enabled && Q.IsReady())
                    {
                        if (Distance.DistanceToPlayer(Prediction.GetPrediction((args.Target as AIHeroClient),0.6f).CastPosition) > Q.Range)
                        {
                            return;
                        }


                        Q.Cast(args.Target as AIHeroClient);
                    }

                    break;
                case OrbwalkerMode.Harass:

                    if (HarassMenu.QBool.Enabled && Q.IsReady())
                    {
                        if (Distance.DistanceToPlayer(Prediction.GetPrediction((args.Target as AIHeroClient),0.6f).CastPosition) > Q.Range)
                        {
                            return;
                        }

                        Q.Cast(args.Target as AIHeroClient);
                    }

                    break;
                case OrbwalkerMode.LaneClear:
                    if (LaneClearMenu.farmKey.Active)
                    {
                        var target = args.Target as AIMinionClient;
                        if (target.Health <= LocalPlayer.GetAutoAttackDamage(target))
                        {
                            return;
                        }

                        if (JungleClearMenu.QBool.Enabled && Q.IsReady() && target != null && target.IsJungle())
                        {
                            Q.Cast(target);
                        }

                        if (LaneClearMenu.QBool.Enabled && Q.IsReady() && target != null && target.IsMinion())
                        {
                            Q.Cast(target);
                        }
                    }

                    break;
            }
        }

        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void OnNewGapcloser(AIHeroClient sender, EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (!W.IsReady() ||
                LocalPlayer.IsDead)
            {
                return;
            }

            if (sender == null || !sender.IsValid || !sender.IsEnemy || !sender.IsMelee)
            {
                return;
            }

            AntiGapcloser.ExecuteW(sender,args);
        }

        public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (!sender.Owner.IsMe || args.Slot != SpellSlot.W)
                {
                    return;
                }

                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                    if (target != null)
                    {
                        if (ComboMenu.EBool.Enabled && E.IsReady() && target.DistanceToPlayer() > 150)
                        {
                            E.Cast(target.Position);
                        }
                    }
                }

                if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                    if (target != null)
                    {
                        if (HarassMenu.EBool.Enabled && E.IsReady() && target.DistanceToPlayer() > 150)
                        {
                            E.Cast(target.Position);
                        }
                    }
                }

            }catch(Exception e){}

        }

        public static void OnTick(EventArgs args)
        {
            if (Q.Name == "AatroxQ")
            {
                Q.Range = 650;
                Q.SetSkillshot(0.6f, 100f, float.MaxValue, false,SpellType.Line);
                Spells.DashQ.SetSkillshot(0.8f, 100f, float.MaxValue, false,SpellType.Line);
            }

            if (Q.Name == "AatroxQ2")
            {
                Q.Range = 500;
                Q.SetSkillshot(0.6f, 200f, float.MaxValue, false,SpellType.Line);
                Spells.DashQ.SetSkillshot(0.7f, 200f, float.MaxValue, false,SpellType.Line);
            }

            if (Q.Name == "AatroxQ3")
            {
                Q.Range = 500;
                Q.SetSkillshot(0.6f, 150f, float.MaxValue, false,SpellType.Circle);
                Spells.DashQ.SetSkillshot(0.7f, 150f, float.MaxValue, false,SpellType.Circle);
            }

            if (LocalPlayer.IsDead || LocalPlayer.IsRecalling() || MenuGUI.IsShopOpen)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo.ExecuteE();
                    Combo.ExecuteEGap();
                    Combo.ExecuteQGap();
                    Combo.ExecuteQ();
                    Combo.ExecuteW();
                    Combo.ExecuteEAA();
                    break;
                case OrbwalkerMode.Harass:
                    Harass.ExecuteE();
                    Harass.ExecuteEGap();
                    Harass.ExecuteQGap();
                    Harass.ExecuteQ();
                    Harass.ExecuteW();
                    Harass.ExecuteEAA();
                    break;
                case OrbwalkerMode.LaneClear:
                    if (LaneClearMenu.farmKey.Active)
                    {
                        JungleClear.ExecuteE();
                        JungleClear.ExecuteW();
                    }

                    break;
            }
        }
    }
}