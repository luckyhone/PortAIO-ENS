using System;
using BadaoGP;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SPrediction;

namespace BadaoKingdom.BadaoChampion.BadaoGangplank
{
    public static class BadaoGangplankJungleClear
    {
        public static void BadaoActivate ()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode != OrbwalkerMode.LaneClear)
                return;
            if (!BadaoGangplankVariables.JungleQ.GetValue<MenuBool>().Enabled)
                return;
            foreach (var aiBaseClient in MinionManager.GetMinions(BadaoMainVariables.Q.Range,
                         MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.Health))
            {
                var minion = (AIMinionClient) aiBaseClient;
                if (minion.BadaoIsValidTarget() && BadaoMainVariables.Q.GetDamage(minion) >= minion.Health)
                {
                    BadaoMainVariables.Q.Cast(minion);
                }
            }
        }
    }
}