using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace GangplankBuddy
{
    internal class StateHandler
    {
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(SpellManager.E.Range + 175, DamageType.Physical);
            Orbwalker.ForceTarget = null;

            if (target == null || !target.IsValidTarget()) return;

            var barrel = BarrelManager.KillableBarrelAroundUnit(target);
            if (Program.ComboMenu["useQBarrels"].GetValue<MenuBool>().Enabled && barrel != null &&
                SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(barrel);
            } else if (barrel != null && barrel.Distance(ObjectManager.Player) < ObjectManager.Player.GetRealAutoAttackRange(barrel))
            {
                Orbwalker.ForceTarget = barrel;
            }
            else
            {
                var maxChain = Program.ComboMenu["useEMaxChain"].GetValue<MenuSlider>().Value;
                if (Program.ComboMenu["useQBarrels"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() &&
                    maxChain > 1)
                {
                    foreach (var barrels in BarrelManager.Barrels.Where(a => a.Distance(target) < 350))
                    {
                        var killableBarrel =
                            BarrelManager.Killablebarrels.Where(a => a.Distance(ObjectManager.Player) < SpellManager.Q.Range)
                                .FirstOrDefault(a => a.Distance(barrels) < 700);
                        if (killableBarrel != null)
                        {
                            if (SpellManager.Q.IsReady())
                            {
                                SpellManager.Q.Cast(killableBarrel);
                                return;
                            }
                            else if (killableBarrel.Distance(ObjectManager.Player) < ObjectManager.Player.GetRealAutoAttackRange(killableBarrel))
                            {
                                Orbwalker.ForceTarget = killableBarrel;
                                return;
                            }
                        }
                        if (maxChain > 2)
                        {
                            foreach (var barrels2 in BarrelManager.Barrels.Where(a => a.Distance(barrels) < 350))
                            {
                                var killableBarrel2 =
                                    BarrelManager.Killablebarrels.Where(
                                        a => a.Distance(ObjectManager.Player) < SpellManager.Q.Range)
                                        .FirstOrDefault(a => a.Distance(barrels2) < 700);
                                if (killableBarrel2 != null)
                                {
                                    if (SpellManager.Q.IsReady())
                                    {
                                        SpellManager.Q.Cast(killableBarrel2);
                                        return;
                                    }
                                    else if (killableBarrel2.Distance(ObjectManager.Player) < ObjectManager.Player.GetRealAutoAttackRange(killableBarrel2))
                                    {
                                        Orbwalker.ForceTarget = killableBarrel2;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                if (Program.ComboMenu["useE"].GetValue<MenuBool>().Enabled &&
                    !BarrelManager.Barrels.Any(a => target.Distance(a) < 350) && SpellManager.E.IsReady())
                {
                    SpellManager.E.Cast(target);
                    return;
                }
            }
            if (Program.ComboMenu["useQCombo"].GetValue<MenuBool>().Enabled &&
                target.IsValidTarget(SpellManager.Q.Range) && SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(target);
            }
        }
        
        public static void Harass()
        {
            var target = TargetSelector.GetTarget(SpellManager.E.Range + 175, DamageType.Physical);
            Orbwalker.ForceTarget = null;

            if (target == null || !target.IsValidTarget()) return;

            var barrel = BarrelManager.KillableBarrelAroundUnit(target);
            if (Program.HarassMenu["useQBarrelsHarass"].GetValue<MenuBool>().Enabled && barrel != null &&
                SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(barrel);
            }
            else if (barrel != null && barrel.Distance(ObjectManager.Player) < ObjectManager.Player.GetCurrentAutoAttackRange(barrel))
            {
                Orbwalker.ForceTarget = barrel;
            }
            else
            {
                var maxChain = Program.HarassMenu["useEMaxChainHarass"].GetValue<MenuSlider>().Value;
                if (Program.HarassMenu["useQBarrelsHarass"].GetValue<MenuBool>().Enabled &&
                    maxChain > 1)
                {
                    foreach (var barrels in BarrelManager.Barrels.Where(a => a.Distance(target) < 350))
                    {
                        var killableBarrel =
                            BarrelManager.Killablebarrels.Where(a => a.Distance(ObjectManager.Player) < SpellManager.Q.Range)
                                .FirstOrDefault(a => a.Distance(barrels) < 700);
                        if (killableBarrel != null)
                        {
                            if (SpellManager.Q.IsReady())
                            {
                                SpellManager.Q.Cast(killableBarrel);
                                return;
                            }
                            else if (killableBarrel.Distance(ObjectManager.Player) < ObjectManager.Player.GetCurrentAutoAttackRange(killableBarrel))
                            {
                                Orbwalker.ForceTarget = killableBarrel;
                                return;
                            }
                            return;
                        }
                        if (maxChain > 2)
                        {
                            foreach (var barrels2 in BarrelManager.Barrels.Where(a => a.Distance(barrels) < 350))
                            {
                                var killableBarrel2 =
                                    BarrelManager.Killablebarrels.Where(
                                        a => a.Distance(ObjectManager.Player) < SpellManager.Q.Range)
                                        .FirstOrDefault(a => a.Distance(barrels2) < 700);
                                if (killableBarrel2 != null)
                                {
                                    if (SpellManager.Q.IsReady())
                                    {
                                        SpellManager.Q.Cast(killableBarrel2);
                                        return;
                                    }
                                    else if (killableBarrel2.Distance(ObjectManager.Player) < ObjectManager.Player.GetCurrentAutoAttackRange(killableBarrel2))
                                    {
                                        Orbwalker.ForceTarget = killableBarrel2;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                if (Program.HarassMenu["useEHarass"].GetValue<MenuBool>().Enabled &&
                    !BarrelManager.Barrels.Any(a => target.Distance(a) < 350) && SpellManager.E.IsReady())
                {
                    SpellManager.E.Cast(target);
                    return;
                }
            }
            if (Program.HarassMenu["useQHarass"].GetValue<MenuBool>().Enabled &&
                target.IsValidTarget(SpellManager.Q.Range) && SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(target);
            }
        }

        public static void LastHit()
        {
            if (!SpellManager.Q.IsReady() || !Program.FarmingMenu["useQLastHit"].GetValue<MenuBool>().Enabled) return;

            var minion =
                ObjectManager.Get<AIMinionClient>()
                    .FirstOrDefault(
                        a =>
                            a.Distance(ObjectManager.Player) > ObjectManager.Player.GetCurrentAutoAttackRange(a) &&
                            !a.Name.ToLower().Contains("gang") &&
                            ObjectManager.Player.Distance(a) <= SpellManager.Q.Range && a.IsEnemy &&
                            a.Health <= GPDmg.QDamage(a));
            if (minion == null) return;
            SpellManager.Q.Cast(minion);
        }
        
        public static void Waveclear()
        {
            if (Program.FarmingMenu["useEQKill"].GetValue<MenuBool>().Enabled)
            {
                foreach (
                    var killableBarrel in
                        BarrelManager.Killablebarrels.Where(
                            killableBarrel =>
                                killableBarrel.IsValidTarget(SpellManager.Q.Range) &&
                                GameObjects.EnemyMinions.Count(
                                    b =>
                                        b.Distance(killableBarrel) < 350 &&
                                        b.Health < GPDmg.EDamage(b, GPDmg.QDamage(b))) >
                                Program.FarmingMenu["useEQKillMin"].GetValue<MenuSlider>().Value))
                {
                    SpellManager.Q.Cast(killableBarrel);
                    return;
                }
            }
            if (Program.FarmingMenu["useEWaveClear"].GetValue<MenuBool>().Enabled)
            {
                var count = 0;
                AIMinionClient target = null;
                foreach (
                    var source in
                        GameObjects.EnemyMinions.Where(
                            a => a.Distance(ObjectManager.Player) < SpellManager.E.Range))
                {
                    var count2 = GameObjects.EnemyMinions.Count(a => a.Distance(source) < 350);
                    if (count2 > count)
                    {
                        count = count2;
                        target = source;
                    }
                }
                if (target != null && count >= Program.FarmingMenu["useEWaveClearMin"].GetValue<MenuSlider>().Value &&
                    !BarrelManager.Killablebarrels.Any(a => a.Distance(target.Position) < 350) &&
                    !BarrelManager.Barrels.Any(a => a.Distance(target.Position) < 350))
                {
                    SpellManager.E.Cast(target.Position);
                    return;
                }
            }
            var minion =
                GameObjects.EnemyMinions.FirstOrDefault(
                    a =>
                        a.Health < GPDmg.QDamage(a) &&
                        a.IsValidTarget(SpellManager.Q.Range));
            if (Program.FarmingMenu["useQWaveClear"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() &&
                minion != null)
            {
                SpellManager.Q.Cast(minion);
            }
        }

        public static void Healing()
        {
            if (!SpellManager.W.IsReady() || !Program.HealingMenu["enableHeal"].GetValue<MenuBool>().Enabled) return;
            if (ObjectManager.Player.HealthPercent <= Program.HealingMenu["healMin"].GetValue<MenuSlider>().Value)
            {
                SpellManager.W.Cast();
                return;
            }
            if (Program.HealingMenu["healStun"].GetValue<MenuBool>().Enabled && ObjectManager.Player.HasBuffOfType(BuffType.Stun)
                || Program.HealingMenu["healRoot"].GetValue<MenuBool>().Enabled && ObjectManager.Player.HasBuffOfType(BuffType.Snare))
            {
                SpellManager.W.Cast();
            }
        }
    }
}