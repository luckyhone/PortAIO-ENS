using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Champions;
using HikiCarry.Core.Predictions;
namespace HikiCarry.Core.Plugins.JhinModes
{
    static class None
    {
        public static void ImmobileExecute()
        {
            if (Jhin.E.IsReady() && Initializer.Config["auto.e.immobile"].GetValue<MenuBool>().Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Jhin.E.Range) && Utilities.Utilities.IsImmobile(x)))
                {
                    Jhin.E.Do(enemy, Utilities.Utilities.HikiChance("hitchance"));
                }
            }
        }

        public static void KillSteal()
        {
            if (Jhin.Q.IsReady() && Initializer.Config["q.ks"].GetValue<MenuBool>().Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Jhin.Q.Range) &&
                                                                     x.Health < Jhin.Q.GetDamage(x)))
                {
                    Jhin.Q.CastOnUnit(enemy);
                }
            }
            if (Jhin.W.IsReady() && Initializer.Config["w.ks"].GetValue<MenuBool>().Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.Distance(ObjectManager.Player) < Initializer.Config["w.combo.max.distance"].GetValue<MenuSlider>().Value
                                                                     && x.Distance(ObjectManager.Player) > Initializer.Config["w.combo.min.distance"].GetValue<MenuSlider>().Value
                                                                     && x.IsValid && x.Health < Jhin.W.GetDamage(x) && !x.IsDead && !x.IsZombie() && x.IsValid))
                {
                    Jhin.W.Do(enemy,Utilities.Utilities.HikiChance("hitchance"));
                }
            }
        }

       
    }
}