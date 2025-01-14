﻿using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using KoreanZed.Enumerators;
using KoreanZed.QueueActions;
using PortAIO;
using SharpDX;
using SPrediction;
using HealthPrediction = SebbyLib.HealthPrediction;

namespace KoreanZed
{
    class ZedCore
    {
        private Orbwalker ZedOrbwalker { get; set; }

        private readonly ZedMenu zedMenu;

        private readonly ZedSpell q;

        private readonly ZedSpell w;

        private readonly ZedSpell e;

        private readonly ZedSpell r;

        private readonly AIHeroClient player;
        

        private readonly ZedEnergyChecker energy;

        private readonly ActionQueueList harasQueue;

        private readonly ActionQueueList comboQueue;

        private readonly ActionQueueList laneClearQueue;

        private readonly ActionQueueList lastHitQueue;

        private readonly ActionQueueCheckAutoAttack checkAutoAttack;

        private readonly ZedShadows shadows;

        private readonly ActionQueue actionQueue;

        private readonly ZedComboSelector zedComboSelector;
        
        public ZedCore(ZedSpells zedSpells, Orbwalker zedOrbwalker, ZedMenu zedMenu, ZedShadows zedShadows, ZedEnergyChecker zedEnergy)
        {
            q = zedSpells.Q;
            w = zedSpells.W;
            e = zedSpells.E;
            r = zedSpells.R;

            player = ObjectManager.Player;
            ZedOrbwalker = zedOrbwalker;
            this.zedMenu = zedMenu;
            energy = zedEnergy;

            actionQueue = new ActionQueue();
            harasQueue = new ActionQueueList();
            comboQueue = new ActionQueueList();
            laneClearQueue = new ActionQueueList();
            lastHitQueue = new ActionQueueList();
            checkAutoAttack = new ActionQueueCheckAutoAttack();
            shadows = zedShadows;
            zedComboSelector = new ZedComboSelector(zedMenu);

            Game.OnUpdate += Game_OnUpdate;
        }
        
        private void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
                default:
                    return;
            }
        }
        
        private void Combo()
        {
            if (actionQueue.ExecuteNextAction(comboQueue))
            {
                return;
            }

            shadows.Combo();

            if (w.UseOnCombo && shadows.CanCast && player.HasBuff("zedr2"))
            {
                AIHeroClient target = TargetSelector.GetTarget(w.Range + e.Range, DamageType.Physical);

                if (target != null)
                {
                    actionQueue.EnqueueAction(comboQueue,
                        () => true,
                        () => shadows.Cast(w.GetPrediction(target).CastPosition),
                        () => true);
                    return;
                }
            }

            float maxRange = float.MaxValue;

            if (r.UseOnCombo && r.IsReady() && r.Instance.ToggleState == 0)
            {
                AIHeroClient target = null;

                maxRange = Math.Min(maxRange, r.Range);

                if (zedMenu.GetParamBool("koreanzed.combo.ronselected"))
                {
                    if (TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget(maxRange))
                    {
                        target = TargetSelector.SelectedTarget;
                    }
                }
                else
                {
                    List<AIHeroClient> ignoredChamps = zedMenu.GetBlockList(BlockListType.Ultimate);
                    target = TargetSelector.GetTarget(maxRange, r.DamageType, true,null, ignoredChamps);
                }

                if (target != null)
                {
                    switch (zedMenu.GetCombo())
                    {
                        case ComboType.AllStar:
                            AllStarCombo(target);
                            break;
                        case ComboType.TheLine:
                            TheLineCombo(target);
                            break;
                    }
                    return;
                }
            }
            else if (w.UseOnCombo && shadows.CanCast && (!r.UseOnCombo || (r.UseOnCombo && !r.IsReady()))
                && (player.Mana > w.Mana + (q.UseOnCombo && q.IsReady() ? q.Mana : 0F) + (e.UseOnCombo && e.IsReady() ? e.Mana : 0F)))
            {
                maxRange = Math.Min(maxRange, w.Range + e.Range);
                AIHeroClient target = TargetSelector.GetTarget(maxRange, DamageType.Physical);
                if (target != null)
                {
                    actionQueue.EnqueueAction(
                        comboQueue,
                        () => shadows.CanCast,
                        () => shadows.Cast(w.GetPrediction(target).CastPosition),
                        () => !shadows.CanCast);
                    actionQueue.EnqueueAction(
                        comboQueue,
                        () => w.Instance.ToggleState != 0,
                        () => shadows.Combo(),
                        () => true);
                    actionQueue.EnqueueAction(
                        comboQueue,
                        () => shadows.CanSwitch && target.Distance(shadows.Instance.Position) <= player.AttackRange,
                        () => shadows.Switch(),
                        () => !shadows.CanSwitch || target.Distance(shadows.Instance.Position) > player.AttackRange || !w.IsReady());
                    actionQueue.EnqueueAction(
                        comboQueue,
                        () => player.Distance(target) <= ObjectManager.Player.GetRealAutoAttackRange(target),
                        () => ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target),
                        () => target.IsDead || target.IsZombie() || player.Distance(target) > ObjectManager.Player.GetRealAutoAttackRange(target) || checkAutoAttack.Status);
                    return;
                }
            }

            if (q.UseOnCombo && q.IsReady() && player.Mana > q.Mana)
            {
                maxRange = Math.Min(maxRange, q.Range);
                AIHeroClient target = TargetSelector.GetTarget(maxRange, q.DamageType);

                PredictionOutput predictionOutput = q.GetPrediction(target);

                if (predictionOutput.Hitchance >= HitChance.Medium)
                {
                    q.Cast(predictionOutput.CastPosition);
                }
            }

            if (e.UseOnCombo && e.IsReady() && player.Mana > e.Mana)
            {
                maxRange = Math.Min(maxRange, e.Range);
                AIHeroClient target = TargetSelector.GetTarget(maxRange, e.DamageType);
                if (target != null)
                {
                    actionQueue.EnqueueAction(comboQueue,
                        () => e.IsReady(),
                        () => e.Cast(),
                        () => true);
                    return;
                }
            }

            if (w.UseOnCombo && w.IsReady() && shadows.CanSwitch)
            {
                List<AIBaseClient> shadowList = shadows.GetShadows();

                foreach (AIBaseClient objAiBase in shadowList)
                {
                    AIHeroClient target = TargetSelector.GetTarget(2000F, DamageType.Physical);

                    if (target != null && player.Distance(target) > ObjectManager.Player.GetRealAutoAttackRange(target) + 50F &&
                        objAiBase.Distance(target) < player.Distance(target))
                    {
                        shadows.Switch();
                    }
                }
            }
        }
        private void AllStarCombo(AIHeroClient target)
        {
            actionQueue.EnqueueAction(
                comboQueue,
                () => r.IsReady() && r.Instance.ToggleState == 0 && player.IsVisible,
                () =>
                    {
                        zedComboSelector.AllStarAnimation();
                        r.Cast(target);
                    },
                () => r.IsReady() && r.Instance.ToggleState != 0 && player.IsVisible);
            actionQueue.EnqueueAction(
                comboQueue,
                () => w.UseOnCombo && shadows.CanCast && player.Mana > w.Mana,
                () => shadows.Cast(target.Position),
                () => target.IsDead || target.IsZombie() || w.Instance.ToggleState != 0 || !w.UseOnCombo || player.Mana <= w.Mana);
            actionQueue.EnqueueAction(
                comboQueue,
                () => q.UseOnCombo && q.IsReady(),
                () => ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Q, q.GetPrediction(target).CastPosition),
                () => target.IsDead || target.IsZombie() || !q.IsReady() || !q.UseOnCombo);
            actionQueue.EnqueueAction(
                comboQueue,
                () => w.Instance.ToggleState != 0 && e.UseOnCombo && e.IsReady() && e.CanCast(target),
                () => e.Cast(),
                () => target.IsDead || target.IsZombie() || w.Instance.ToggleState == 0 || !e.IsReady() || !e.UseOnCombo || !e.CanCast(target));
        }

        private void TheLineCombo(AIHeroClient target)
        {
            actionQueue.EnqueueAction(
                comboQueue,
                () => r.IsReady() && r.Instance.ToggleState == 0 && player.IsVisible,
                () =>
                {
                    zedComboSelector.TheLineAnimation();
                    r.Cast(target);
                },
                () => r.IsReady() && r.Instance.ToggleState != 0 && player.IsVisible);
            actionQueue.EnqueueAction(
                comboQueue,
                () => w.UseOnCombo && shadows.CanCast && player.Mana > w.Mana,
                () => shadows.Cast(target.Position.Extend(shadows.Instance.Position, -1000F)),
                () => target.IsDead || target.IsZombie() || w.Instance.ToggleState != 0 || !w.UseOnCombo || player.Mana <= w.Mana);
            actionQueue.EnqueueAction(
                comboQueue,
                () => e.UseOnCombo && e.IsReady() && e.CanCast(target),
                () => e.Cast(),
                () => target.IsDead || target.IsZombie() || !e.IsReady() || !e.UseOnCombo || !e.CanCast(target));
            actionQueue.EnqueueAction(
                comboQueue,
                () => q.UseOnCombo && q.IsReady() && q.CanCast(target),
                () => q.Cast(q.GetPrediction(target).CastPosition),
                () => target.IsDead || target.IsZombie() || !q.IsReady() || !q.UseOnCombo || !q.CanCast(target) || player.Mana <= q.Mana);
        }
        
        private void Harass()
        {
            if (actionQueue.ExecuteNextAction(harasQueue))
            {
                return;
            }

            shadows.Harass();

            float maxRange = float.MaxValue;

            if (!energy.ReadyToHaras)
            {
                return;
            }

            List<AIHeroClient> blackList = zedMenu.GetBlockList(BlockListType.Harass);

            if ((w.UseOnHarass && w.IsReady() && w.Instance.ToggleState == 0)
                && (player.HealthPercent
                    > zedMenu.GetDontUseLowLife()
                    && GameObjects.EnemyHeroes.Count(
                        hero => !hero.IsDead && !hero.IsZombie() && player.Distance(hero) < 2000F)
                    < zedMenu.Getdontuseagainst()))
            {
                if (q.UseOnHarass && q.IsReady() && (player.Mana > q.Mana + w.Mana))
                {
                    switch ((ShadowHarassTrigger)zedMenu.Gettrigger())
                    {
                        case ShadowHarassTrigger.MaxRange:
                            maxRange = Math.Min(maxRange, q.Range + w.Range);
                            break;

                        case ShadowHarassTrigger.MaxDamage:
                            maxRange = Math.Min(maxRange, w.Range + e.Range);
                            break;
                    }

                    AIHeroClient target = TargetSelector.GetTarget(maxRange, q.DamageType, true,null, blackList);

                    if (target != null)
                    {
                        actionQueue.EnqueueAction(
                            harasQueue,
                            () => shadows.CanCast,
                            () => shadows.Cast(target),
                            () => !shadows.CanCast);
                        actionQueue.EnqueueAction(
                            harasQueue,
                            () => true,
                            () => shadows.Harass(),
                            () => true);
                        return;
                    }
                }

                else if (e.UseOnHarass && e.IsReady() && (player.Mana > e.Mana + w.Mana))
                {
                    maxRange = Math.Min(maxRange, e.Range + w.Range);
                    AIHeroClient target = TargetSelector.GetTarget(maxRange, e.DamageType, true,null, blackList);

                    if (target != null)
                    {
                        actionQueue.EnqueueAction(
                            harasQueue,
                            () => shadows.CanCast,
                            () => shadows.Cast(target),
                            () => !shadows.CanCast);
                        actionQueue.EnqueueAction(
                            harasQueue,
                            () => true,
                            () => shadows.Harass(),
                            () => true);
                        return;
                    }
                }
            }

            if (q.UseOnHarass && energy.ReadyToHaras)
            {
                maxRange = Math.Min(maxRange, q.Range);
                AIHeroClient target = TargetSelector.GetTarget(maxRange, q.DamageType, true ,null, blackList);

                if (target != null)
                {
                    PredictionOutput predictionOutput = q.GetPrediction(target);

                    bool checkColision = zedMenu.GetParamBool("koreanzed.harasmenu.checkcollisiononq");

                    if (predictionOutput.Hitchance >= HitChance.High
                        && (!checkColision
                            || !q.GetCollision(
                                player.Position.To2D(),
                                new List<Vector2>() { predictionOutput.CastPosition.To2D() }).Any()))
                    {
                        q.Cast(predictionOutput.CastPosition);
                    }
                }
            }

            if (e.UseOnHarass && energy.ReadyToHaras)
            {
                if (TargetSelector.GetTarget(e.Range, e.DamageType) != null)
                {
                    e.Cast();
                }
            }
            

            LastHit();
        }
        
        private void JungleClear()
        {
            if (q.UseOnLaneClear && q.IsReady() && energy.ReadyToLaneClear)
            {
                AIBaseClient jungleMob =
                    MinionManager.GetMinions(
                        q.Range / 1.5F,
                        MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral,
                        MinionManager.MinionOrderTypes.MaxHealth).FirstOrDefault();

                if (jungleMob != null)
                {
                    q.Cast(q.GetPrediction(jungleMob).CastPosition);
                }
            }

            if (e.UseOnLaneClear && e.IsReady() && energy.ReadyToLaneClear)
            {
                if (
                    MinionManager.GetMinions(e.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                    .Any())
                {
                    e.Cast();
                }
            }
        }
        
        private void LaneClear()
        {
            if (actionQueue.ExecuteNextAction(laneClearQueue))
            {
                return;
            }

            LastHit();

            JungleClear();
            

            if (shadows.GetShadows().Any())
            {
                shadows.LaneClear(actionQueue, laneClearQueue);
                return;
            }

            if (w.UseOnLaneClear)
            {
                WlaneClear();
                return;
            }
            else
            {
                if (e.UseOnLaneClear && e.IsReady())
                {
                    int willHit = MinionManager.GetMinions(e.Range).Count;
                    int param = zedMenu.GetParamSlider("koreanzed.laneclearmenu.useeif");

                    if (willHit >= param)
                    {
                        actionQueue.EnqueueAction(
                            laneClearQueue,
                            () => true,
                            () => e.Cast(),
                            () => !e.IsReady());
                        return;
                    }
                }

                if (q.UseOnLaneClear && q.IsReady())
                {
                    var farmLocation = q.GetLineFarmLocation(MinionManager.GetMinions(q.Range));

                    int willHit = farmLocation.MinionsHit;
                    int param = zedMenu.GetParamSlider("koreanzed.laneclearmenu.useqif");

                    if (willHit >= param)
                    {
                        actionQueue.EnqueueAction(
                            laneClearQueue,
                            () => q.IsReady(),
                            () => q.Cast(farmLocation.Position),
                            () => !q.IsReady());
                        return;
                    }
                }
            }
        }
        
        private void WlaneClear()
        {
            if (!energy.ReadyToLaneClear)
            {
                return;
            }

            var minionsLong = MinionManager.GetMinions(w.Range + q.Range);
            var minionsShort = minionsLong.Where(minion => player.Distance(minion) <= w.Range + e.Range).ToList();
            bool attackingMinion = minionsShort.Any(minion => player.Distance(minion) <= player.AttackRange);

            if (!attackingMinion)
            {
                return;
            }

            var theChosen =
                MinionManager.GetMinions(e.Range + w.Range)
                    .OrderByDescending(
                        minion =>
                        MinionManager.GetMinions(player.Position.Extend(minion.Position, e.Range + 130F), e.Range)
                            .Count())
                    .FirstOrDefault();
            if (theChosen == null)
            {
                return;
            }

            Vector3 shadowPosition = player.Position.Extend(theChosen.Position, e.Range + 130F);

            if (player.Distance(shadowPosition) <= w.Range - 100F)
            {
                shadowPosition = Vector3.Zero;
            }

            bool canUse = GameObjects.EnemyHeroes.Count(enemy => !enemy.IsDead && !enemy.IsZombie() && enemy.Distance(player) < 2500F)
                          <= zedMenu.GetParamSlider("koreanzed.laneclearmenu.dontuseeif");

            if (e.UseOnLaneClear && e.IsReady())
            {
                int extendedWillHit = MinionManager.GetMinions(shadowPosition, e.Range).Count();
                int shortenWillHit = MinionManager.GetMinions(e.Range).Count;
                int param = zedMenu.GetParamSlider("koreanzed.laneclearmenu.useeif");

                if (canUse && shadows.CanCast && shadowPosition != Vector3.Zero && extendedWillHit >= param
                    && player.Mana > w.Mana + e.Mana)
                {
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => shadows.CanCast,
                        () => shadows.Cast(shadowPosition),
                        () => !shadows.CanCast);
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => w.Instance.ToggleState != 0,
                        () => e.Cast(),
                        () => !e.IsReady());
                    return;
                }
                else if (shortenWillHit >= param)
                {
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => e.IsReady(),
                        () => e.Cast(),
                        () => !e.IsReady());
                    return;
                }
            }

            if (q.UseOnLaneClear && q.IsReady())
            {
                int extendedWillHit = 0;
                Vector3 extendedFarmLocation = Vector3.Zero;
                foreach (AIBaseClient objAiBase in MinionManager.GetMinions(shadowPosition, q.Range))
                {
                    var colisionList = q.GetCollision(
                        shadowPosition.To2D(),
                        new List<Vector2>() { objAiBase.Position.To2D() },
                        w.Delay);

                    if (colisionList.Count > extendedWillHit)
                    {
                        extendedFarmLocation =
                            colisionList.OrderByDescending(c => c.Distance(shadowPosition)).FirstOrDefault().Position;
                        extendedWillHit = colisionList.Count;
                    }
                }

                var shortenFarmLocation = q.GetLineFarmLocation(MinionManager.GetMinions(q.Range));

                int shortenWillHit = shortenFarmLocation.MinionsHit;
                int param = zedMenu.GetParamSlider("koreanzed.laneclearmenu.useqif");

                if (canUse && shadows.CanCast && shadowPosition != Vector3.Zero && extendedWillHit >= param
                    && player.Mana > w.Mana + q.Mana)
                {
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => shadows.CanCast,
                        () => shadows.Cast(shadowPosition),
                        () => !shadows.CanCast);
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => w.Instance.ToggleState != 0,
                        () => q.Cast(extendedFarmLocation),
                        () => !q.IsReady());
                    return;
                }
                else if (shortenWillHit >= param)
                {
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => q.IsReady(),
                        () => q.Cast(shortenFarmLocation.Position),
                        () => !q.IsReady());
                    return;
                }
            }
        }
        
        private void LastHit()
        {
            if (actionQueue.ExecuteNextAction(lastHitQueue))
            {
                return;
            }

            if (q.UseOnLastHit && q.IsReady() && energy.ReadyToLastHit)
            {
                AIBaseClient target =
                    MinionManager.GetMinions(q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Enemy, MinionManager.MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            minion =>
                                minion.Distance(player) > ObjectManager.Player.GetRealAutoAttackRange(minion) + 10F && !minion.IsDead
                                && q.GetDamage(minion) / 2
                                >= HealthPrediction.GetHealthPrediction(
                                    minion,
                                    (int)(player.Distance(minion) / q.Speed) * 1000,
                                    (int)q.Delay * 1000));

                if (target != null)
                {
                    PredictionOutput predictionOutput = q.GetPrediction(target);
                    actionQueue.EnqueueAction(lastHitQueue, () => q.IsReady(), () => q.Cast(predictionOutput.CastPosition), () => !q.IsReady());
                    return;
                }
            }

            if (e.UseOnLastHit && e.IsReady() && energy.ReadyToLastHit)
            {
                if (MinionManager.GetMinions(e.Range).Count(minion => e.GetDamage(minion) >= minion.Health)
                    >= zedMenu.GetParamSlider("koreanzed.lasthitmenu.useeif"))
                {
                    actionQueue.EnqueueAction(lastHitQueue, () => e.IsReady(), () => e.Cast(), () => !e.IsReady());
                    return;
                }
            }
        }
        public float ComboDamage(AIHeroClient target)
        {
            float result = q.UseOnCombo && q.IsReady()
                ? (q.GetCollision(player.Position.To2D(), new List<Vector2>() { target.Position.To2D() })
                    .Any()
                    ? q.GetDamage(target) / 2
                    : q.GetDamage(target))
                : 0F;

            result += e.UseOnCombo && e.IsReady() ? e.GetDamage(target) : 0F;

            result += w.UseOnCombo && w.IsReady() && player.Distance(target) < w.Range + ObjectManager.Player.GetRealAutoAttackRange(target)
                ? (float)player.GetAutoAttackDamage(target, true)
                : 0F;

            float multiplier = 0.3F;
            if (r.Instance.Level == 2) multiplier = 0.4F;
            else if (r.Instance.Level == 3) multiplier = 0.5F;

            result += r.UseOnCombo && r.IsReady()
                ? (float)
                (r.GetDamage(target) + player.GetAutoAttackDamage(target, true)
                                     + (q.IsReady() ? q.GetDamage(target) * multiplier : 0F)
                                     + (e.IsReady() ? e.GetDamage(target) * multiplier : 0F))
                : 0F;

            return result;
        }

        public void ForceUltimate(AIHeroClient target = null)
        {
            if (target != null && r.CanCast(target))
            {
                r.Cast(target);
            }
            else
            {
                r.Cast(TargetSelector.GetTarget(r.Range, r.DamageType));
            }
        }
    }
}