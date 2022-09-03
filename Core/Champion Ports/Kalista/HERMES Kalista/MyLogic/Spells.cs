using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HERMES_Kalista.MyLogic.Others;

namespace HERMES_Kalista.MyLogic
{
    public static class Spells
    {
        public static void OnLoad(EventArgs args)
        {
            Game.OnUpdate += OnUpdate;
            Orbwalker.OnNonKillableMinion += OnNonKillableMinion;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Program.E.IsReady())
            {
                if (
                    GameObjects.EnemyHeroes.Any(
                        en => Program.E.IsInRange(en) && en.IsRendKillable() && en.Health > 0))
                {
                    Program.E.Cast();
                }

                if (
                    ObjectManager.Get<AIMinionClient>().Any(
                        m => m.Distance(ObjectManager.Player) < 1000 && m.IsRendKillable() &&
                             (m.Name == "SRU_Dragon" || m.Name == "SRU_Baron" || m.Name == "SRRU_Red"
                              || m.Name == "SRU_Blue")))
                {
                    Program.E.Cast();
                }
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (Program.E.IsReady())
                    {
                        if (Program.ComboMenu["EComboMinionReset"].GetValue<MenuBool>().Enabled &&
                            GameObjects.GetMinions(Program.E.Range).Any(m => m.IsRendKillable()))
                        {
                            if (
                                GameObjects.EnemyHeroes.Where(e => !e.HaveSpellShield())
                                    .Select(en => en.GetRendBuff())
                                    .Any(buf => buf != null &&
                                                buf.Count >= Program.ComboMenu
                                                    .GetValue<MenuSlider>("EComboMinionResetStacksNew").Value))
                            {
                                Program.E.Cast();
                                return;
                            }

                            if (Program.ComboMenu["UseE2Tilt"].GetValue<MenuBool>().Enabled)
                            {
                                if (ObjectManager.Player.CountEnemyHeroesInRange(300) == 0)
                                {
                                    if (
                                        GameObjects.EnemyHeroes.Where(e => !e.HaveSpellShield())
                                            .Select(en => en.GetRendBuff())
                                            .Any(buf => buf != null &&
                                                        buf.Count >= 1))
                                    {
                                        Program.E.Cast();
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    if (Program.ComboMenu["QCombo"].GetValue<MenuBool>().Enabled &&
                        ObjectManager.Player.ManaPercent > Program.ComboMenu["QMinMana"].GetValue<MenuSlider>().Value &&
                        Program.Q.IsReady())
                    {
                        var target = TargetSelector.GetTarget(Program.Q.Range, DamageType.Physical);
                        if (target.IsValidTarget())
                        {
                            Program.Q.Cast(target);
                            return;
                        }
                    }
                    if(Orbwalker.GetTarget() == null && Orbwalker.CanAttack() && Program.ComboMenu["MinionOrbwalking"].GetValue<MenuBool>().Enabled){
                        var minions = GameObjects.EnemyMinions.Where(e => e.InAutoAttackRange()).OrderByDescending(e => e.Health);

                        if (minions.Count() > 0)
                        {
                            Orbwalker.Attack(minions.First());
                        }
                    }
                    break;
                case OrbwalkerMode.LaneClear:
                    //E Laneclear
                    if (Program.LaneClearMenu.GetValue<MenuBool>("LaneclearE").Enabled &&
                        (ObjectManager.Player.ManaPercent <
                         Program.LaneClearMenu.GetValue<MenuSlider>("LaneclearEMinMana").Value &&
                         GameObjects.GetMinions(1000f).Count(m => m.IsRendKillable()) >
                         Program.LaneClearMenu.GetValue<MenuSlider>("LaneclearEMinions").Value) ||
                        GameObjects.GetMinions(1000f, MinionTypes.All)
                            .Any(m => m.IsRendKillable()) || (ObjectManager.Player.IsUnderEnemyTurret() && GameObjects.GetMinions(1000f).Any(m => m.IsRendKillable())))
                    {
                        Program.E.Cast();
                    }
                    //E poke, slow
                    if ((from enemy in GameObjects.EnemyHeroes.Where(e => Program.E.IsInRange(e))
                        let buff = enemy.GetRendBuff()
                        where Program.E.IsReady() && buff != null && Program.E.IsInRange(enemy)
                        where buff.Count >= Program.ComboMenu.GetValue<MenuSlider>("EComboMinStacks").Value
                        where (enemy.Distance(ObjectManager.Player) > Math.Pow(Program.E.Range*0.80, 2) ||
                               buff.EndTime - Game.Time < 0.3)
                        select enemy).Any())
                    {
                        Program.E.Cast();
                    }
                    break;
                default:
                    //E poke, slow
                    if ((from enemy in GameObjects.EnemyHeroes.Where(e => Program.E.IsInRange(e))
                        let buff = enemy.GetRendBuff()
                        where Program.E.IsReady() && buff != null && Program.E.IsInRange(enemy)
                        where buff.Count >= Program.ComboMenu.GetValue<MenuSlider>("EComboMinStacks").Value
                        where (enemy.Distance(ObjectManager.Player) > Math.Pow(Program.E.Range*0.80, 2) ||
                               buff.EndTime - Game.Time < 0.3)
                        select enemy).Any())
                    {
                        Program.E.Cast();
                    }
                    break;
            }
            
        }

        private static void OnNonKillableMinion(object sender, NonKillableMinionEventArgs args)
        {
            var objaiminion = args.Target as AIMinionClient;
            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && objaiminion.IsRendKillable())
            {
                Program.E.Cast();
            }
        }
    }
}