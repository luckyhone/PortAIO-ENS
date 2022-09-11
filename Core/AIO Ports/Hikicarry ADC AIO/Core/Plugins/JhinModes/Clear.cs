using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Champions;
using LeagueSharpCommon;
using SPrediction;

namespace HikiCarry.Core.Plugins.JhinModes
{
    static class Clear
    {
        /// <summary>
        /// Execute Q Clear
        /// </summary>
        private static void ExecuteQ()
        {
            var min = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Jhin.Q.Range).MinOrDefault(x => x.Health);
            Jhin.Q.CastOnUnit(min);
        }

        /// <summary>
        /// Execute W Clear
        /// </summary>
        private static void ExecuteW()
        {
            var min = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Jhin.W.Range);
            if (Jhin.W.GetLineFarmLocation(min).MinionsHit >= Initializer.Config["w.hit.x.minion"].GetValue<MenuSlider>().Value)
            {
                Jhin.W.Cast(Jhin.W.GetLineFarmLocation(min).Position);
            }
        }

        /// <summary>
        /// Execute Clear
        /// </summary>
        public static void ExecuteClear()
        {
            if (ObjectManager.Player.ManaPercent < Initializer.Config["clear.mana"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            if (Jhin.Q.IsReady() && Initializer.Config["q.clear"].GetValue<MenuBool>().Enabled) // done working
            {
                ExecuteQ();
            }

            if (Jhin.W.IsReady() && Initializer.Config["w.clear"].GetValue<MenuBool>().Enabled) // done working
            {
                ExecuteW();
            }
        }
    }
}