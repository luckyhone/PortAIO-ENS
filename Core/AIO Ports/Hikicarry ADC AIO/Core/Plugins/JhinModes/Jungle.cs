using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Champions;
using SPrediction;

namespace HikiCarry.Core.Plugins.JhinModes
{
    static class Jungle
    {
        /// <summary>
        /// Execute Jungle
        /// </summary>
        public static void ExecuteJungle()
        {
            if (ObjectManager.Player.ManaPercent < Initializer.Config["jungle.mana"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Jhin.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);
            
            if (mobs == null || (mobs.Count == 0))
            {
                return;
            }

            if (Jhin.Q.IsReady() && Initializer.Config["q.clear"].GetValue<MenuBool>().Enabled)
            {
                Jhin.Q.Cast(mobs[0]);
            }

            if (Jhin.W.IsReady() && Initializer.Config["w.clear"].GetValue<MenuBool>().Enabled)
            {
                Jhin.W.Cast(mobs[0]);
            }
        }
    }
}