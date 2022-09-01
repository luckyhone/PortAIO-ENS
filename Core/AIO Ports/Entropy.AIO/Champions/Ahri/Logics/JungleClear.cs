using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Ahri.Logics
{
    using static ChampionBase;
    class JungleClear
    {
        public static void Execute()
        {
            if (E.IsReady())
            {
                ExecuteE();
            }

            if (Q.IsReady())
            {
                ExecuteQ();
            }

            if (W.IsReady())
            {
                ExecuteW();
            }
        }

        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        private static void ExecuteQ()
        {
            if (!Components.JungleClearMenu.QSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < Q.GetNeededMana(Components.JungleClearMenu.QSliderBool))
            {
                return;
            }

            var qTarget = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(Q.Range));

            if (qTarget != null)
            {
                var qinput = Q.GetPrediction(qTarget);
                if (qinput.Hitchance >= HitChance.High)
                {
                    Q.Cast(qTarget);
                }
            }
        }

        private static void ExecuteW()
        {
            if (!Components.JungleClearMenu.WSliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < W.GetNeededMana(Components.JungleClearMenu.WSliderBool))
            {
                return;
            }

            var wTarget = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(W.Range - 100));

            if (wTarget != null)
            {
                W.Cast();
            }
            
        }

        private static void ExecuteE()
        {
            if (!Components.JungleClearMenu.ESliderBool.Enabled)
            {
                return;
            }

            if (LocalPlayer.ManaPercent < E.GetNeededMana(Components.JungleClearMenu.ESliderBool))
            {
                return;
            }

            var eTarget = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(E.Range));

            if (eTarget != null)
            {
                E.Cast(eTarget);
            }

        }
    }
}