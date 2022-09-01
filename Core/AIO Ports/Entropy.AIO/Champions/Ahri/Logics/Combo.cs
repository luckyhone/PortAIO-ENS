using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Ahri.Misc;
using Damage = Entropy.AIO.Ahri.Misc.Damage;
using static Entropy.AIO.Ahri.Components;
using static Entropy.AIO.Bases.ChampionBase;

namespace Entropy.AIO.Ahri.Logics
{
    static class Combo
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteR()
        {
            if (!R.IsReady())
            {
                return;
            }
    
            if (ComboMenu.RActiveBool.Enabled)
            {
                if (Environment.TickCount - Definitions.lastRTime > 9500 && LocalPlayer.HasBuff("AhriTumble"))
                {
                    if (R.Cast(LocalPlayer.Position.Extend(Game.CursorPos, R.Range)))
                    {
                        Definitions.lastRTime = Environment.TickCount;
                        return;
                    }
                }
            }

            if (!ComboMenu.RBool.Enabled)
            {
                return;
            }

            var rTarget = TargetSelector.GetTarget(R.Range * 2,DamageType.Magical);

            if (rTarget == null)
            {
                return;
            }

            var comboDMG = Damage.R(rTarget);

            if (Q.IsReady())
            {
                comboDMG += Damage.Q(rTarget);
            }

            if (W.IsReady())
            {
                comboDMG += Damage.W(rTarget);
            }

            if (rTarget.IsValidTarget(LocalPlayer.GetCurrentAutoAttackRange()))
            {
                comboDMG += LocalPlayer.GetAutoAttackDamage(rTarget);
            }

            if (!rTarget.IsInvulnerable)
            {
                return;
            }

            if (R.Cast(LocalPlayer.Position.Extend(Game.CursorPos, R.Range)))
            {
                Definitions.lastRTime = Environment.TickCount;
            }
        }

        public static void ExecuteE()
        {
            if (!ComboMenu.EBool.Enabled || !E.IsReady())
            {
                return;
            }

            var eTarget = TargetSelector.GetTarget(E.Range,DamageType.Magical);

            if (eTarget == null)
            {
                return;
            }

            E.Cast(eTarget);
        }

        public static void ExecuteQ()
        {
            if (!Components.ComboMenu.QBool.Enabled || !Q.IsReady())
            {
                return;
            }

            var qTarget = TargetSelector.GetTarget(Q.Range,DamageType.Physical);

            if (qTarget == null)
            {
                return;
            }

            Q.Cast(qTarget);
        }

        public static void ExecuteW()
        {
            if (!ComboMenu.WBool.Enabled || !W.IsReady())
            {
                return;
            }

            if (GameObjects.EnemyHeroes.Any(t => t.IsValidTarget(W.Range)))
            {
                W.Cast();
            }
        }
    }
}