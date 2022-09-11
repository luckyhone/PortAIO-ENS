using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Champions;
using HikiCarry.Core.Predictions;
using LeagueSharpCommon;

namespace HikiCarry.Core.Plugins.JhinModes
{
    static class Combo
    {
        /// <summary>
        /// W Minimum Range
        /// </summary>
        private static readonly int MinRange = Initializer.Config["w.combo.min.distance"].GetValue<MenuSlider>().Value;

        /// <summary>
        /// W Maximum Range
        /// </summary>
        private static readonly int MaxRange = Initializer.Config["w.combo.max.distance"].GetValue<MenuSlider>().Value;

        /// <summary>
        /// Basit spell execute
        /// </summary>
        /// <param name="spell">Spell</param>
        public static void Execute(this Spell spell)
        {
            foreach (var enemy in HeroManager.Enemies.Where(o=> o.IsValidTarget(spell.Range)))
            {
                spell.Cast(enemy);
            }
        }

        /// <summary>
        /// Q Logic
        /// </summary>
        public static void ExecuteQ()
        {
            foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Jhin.Q.Range)))
            {
                Jhin.Q.CastOnUnit(enemy);
            }
        }

        /// <summary>
        /// W Logic
        /// </summary>
        public static void ExecuteW()
        {
            if (Initializer.Config["w.passive.combo"].GetValue<MenuBool>().Enabled)
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Jhin.W.Range) && Utilities.Utilities.IsImmobile(x)))
                {
                    Jhin.W.Cast(enemy);
                }
            }
            else
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValid && x.Distance(ObjectManager.Player) < MaxRange
                    && x.Distance(ObjectManager.Player) > MinRange
                    && !x.IsDead && !x.IsZombie()))
                {
                    Jhin.W.Do(enemy, Utilities.Utilities.HikiChance("hitchance"));
                }
            }
        }

        /// <summary>
        /// E Logic
        /// </summary>
        public static void ExecuteE()
        {
            foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Jhin.E.Range) && Utilities.Utilities.IsImmobile(x)))
            {
                Jhin.E.Do(enemy, Utilities.Utilities.HikiChance("hitchance"));
            }
        }

        /// <summary>
        /// Execute all combo
        /// </summary>
        public static void ExecuteCombo()
        {
            if (Jhin.Q.IsReady() && Initializer.Config["q.combo"].GetValue<MenuBool>().Enabled)
            {
                ExecuteQ();
            }
            if (Jhin.W.IsReady() && Initializer.Config["w.combo"].GetValue<MenuBool>().Enabled)
            {
                ExecuteW();
            }
            if (Jhin.E.IsReady() && Initializer.Config["e.combo"].GetValue<MenuBool>().Enabled)
            {
                ExecuteE();
            }
        }

    }
}