using System;
using System.Linq;
using BadaoGP;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SPrediction;

namespace BadaoKingdom.BadaoChampion.BadaoGangplank
{
    public static class BadaoGangplankLaneClear
    {
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode != OrbwalkerMode.LaneClear)
                return;
            if (!BadaoGangplankVariables.LaneQ.GetValue<MenuBool>().Enabled)
                return;
            foreach (var aiBaseClient in MinionManager.GetMinions(BadaoMainVariables.Q.Range).OrderBy(x => x.Health))
            {
                var minion = (AIMinionClient) aiBaseClient;
                if (minion.BadaoIsValidTarget() && BadaoMainVariables.Q.GetDamage(minion) >= minion.Health && !(ObjectManager.Player.InAutoAttackRange(minion)))
                {
                    BadaoMainVariables.Q.Cast(minion);
                }
            }
        }
    }
}