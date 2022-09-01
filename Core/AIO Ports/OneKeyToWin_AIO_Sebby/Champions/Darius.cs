using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SebbyLib;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Darius : Base
    {
        public Darius()
        {
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 145);
            E = new Spell(SpellSlot.E, 540);
            R = new Spell(SpellSlot.R, 460);

            E.SetSkillshot(0.01f, 100f, float.MaxValue, false, SpellType.Line);

            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            drawMenu.Add(new MenuBool("eRange", "E range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRange", "R range", true).SetValue(false));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw when skill rdy", true).SetValue(true));

            var qConfig = Local.Add(new Menu("qConfig", "Q config"));
            qConfig.Add(new MenuBool("Harass", "Harass Q", true).SetValue(true));
            qConfig.Add(new MenuBool("qOutRange", "Auto Q only out range AA", true).SetValue(true));

            var eConfig = Local.Add(new Menu("eConfig", "E config"));
            var useon = eConfig.Add(new Menu("useon", "Use E on:"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                useon.Add(new MenuBool("Eon" + enemy.CharacterName, enemy.CharacterName, true).SetValue(true));

            var rConfig = Local.Add(new Menu("rConfig", "R config"));
            rConfig.Add(new MenuBool("autoR", "Auto R", true).SetValue(true));
            rConfig.Add(new MenuKeyBind("useR", "Semi-manual cast R key", Keys.T, KeyBindType.Press)); //32 == space
            rConfig.Add(new MenuBool("autoRbuff", "Auto R if darius execute multi cast time out ", true).SetValue(true));
            rConfig.Add(new MenuBool("autoRdeath", "Auto R if darius execute multi cast and under 10 % hp", true).SetValue(true));

            FarmMenu.Add(new MenuBool("farmW", "Farm W", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmQ", "Farm Q", true).SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnBeforeAttack += BeforeAttack;
            Orbwalker.OnAfterAttack += AfterAttack;
            Interrupter.OnInterrupterSpell += OnInterruptableSpell;
        }

        private void OnInterruptableSpell(AIHeroClient unit, Interrupter.InterruptSpellArgs args)
        {
            if (E.IsReady()  && unit.IsValidTarget(E.Range))
                E.Cast(unit);
        }

        private void AfterAttack(object unit, AfterAttackEventArgs target)
        {
            if (Player.Mana < RMANA + WMANA || !W.IsReady())
                return;

            var t = target.Target as AIHeroClient;

            if (t.IsValidTarget())
                W.Cast();
        }

        private void BeforeAttack(object sender, BeforeAttackEventArgs e)
        {
            
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (R.IsReady() && Local["rConfig"]["useR"].GetValue<MenuKeyBind>().Active )
            {
                var targetR = TargetSelector.GetTarget(R.Range, DamageType.True);
                if (targetR.IsValidTarget())
                    R.Cast(targetR, true);
            }

            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (Program.LagFree(1) && W.IsReady())
                LogicW();
            if (Program.LagFree(2) && Q.IsReady() && Orbwalker.CanMove() && !ObjectManager.Player.Spellbook.IsAutoAttack)
                LogicQ();
            if (Program.LagFree(3) && E.IsReady())
                LogicE();
            if (Program.LagFree(4) && R.IsReady() && Local["rConfig"]["autoR"].GetValue<MenuBool>().Enabled)
                LogicR();
        }
        
        private void LogicW()
        {
            if (!ObjectManager.Player.Spellbook.IsAutoAttack && FarmMenu["farmW"].GetValue<MenuBool>().Enabled && Program.Farm)
            {
                var minions = Cache.GetMinions(Player.Position, Player.AttackRange);

                int countMinions = 0;

                foreach (var minion in minions.Where(minion => minion.Health < W.GetDamage(minion)))
                {
                    countMinions++;
                }

                if (countMinions > 0)
                    W.Cast();
            }
        }

        private void LogicE()
        {
            if (Player.Mana > RMANA + EMANA )
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                if (target.IsValidTarget() && Local["eConfig"]["useon"]["Eon" + target.CharacterName].GetValue<MenuBool>().Enabled && ((Player.IsUnderEnemyTurret() && !Player.IsUnderEnemyTurret()) || Program.Combo) )
                {
                    if (!ObjectManager.Player.InAutoAttackRange(target))
                    {
                        E.Cast(target);
                    }
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (!Local["qConfig"]["qOutRange"].GetValue<MenuBool>().Enabled || ObjectManager.Player.InAutoAttackRange(t))
                {
                    if (Player.Mana > RMANA + QMANA && Program.Combo)
                        Q.Cast();
                    else if (Program.Harass && Player.Mana > RMANA + QMANA + EMANA + WMANA && Config["Harass"].GetValue<MenuBool>().Enabled && Config["harass" + t.CharacterName].GetValue<MenuBool>().Enabled)
                        Q.Cast();
                }

                if (!R.IsReady() && OktwCommon.GetKsDamage(t, Q) > t.Health)
                    Q.Cast();
            }
            
            else if (FarmMenu["farmQ"].GetValue<MenuBool>().Enabled && FarmSpells)
            {
                var minionsList = Cache.GetMinions(Player.ServerPosition, Q.Range);

                if (minionsList.Any(x => Player.Distance(x.ServerPosition) > 300 && x.Health < Q.GetDamage(x) * 0.6))
                    Q.Cast();
                        
            }
        }
        

        private void LogicR()
        {
            var targetR = TargetSelector.GetTarget(R.Range, DamageType.True);
            if (targetR.IsValidTarget() && OktwCommon.ValidUlt(targetR) && Local["rConfig"]["autoRbuff"].GetValue<MenuBool>().Enabled)
            {
                var buffTime = OktwCommon.GetPassiveTime(Player, "dariusexecutemulticast");
                if((buffTime < 2 || (Player.HealthPercent < 10 && Local["rConfig"]["autoRdeath"].GetValue<MenuBool>().Enabled)) && buffTime > 0)
                    R.Cast(targetR, true);
            }

            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && OktwCommon.ValidUlt(target)))
            {
                var dmgR = OktwCommon.GetKsDamage(target, R, false);

                if (target.HasBuff("dariushemo"))
                    dmgR += R.GetDamage(target) * target.GetBuff("dariushemo").Count * 0.2f;

                if (dmgR > target.Health)
                {
                    R.Cast(target);
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Local["draw"]["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled && Q.IsReady())
                    if (Q.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                    else
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }

            if (Local["draw"]["eRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled && E.IsReady())
                    if (E.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1);
                    else
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1);
            } 
            if (Local["draw"]["rRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled && R.IsReady())
                    if (R.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Red, 1);
                    else
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Red, 1);
            }
        }
        private void SetMana()
        {
            if ((Config["manaDisable"].GetValue<MenuBool>().Enabled && Program.Combo) || Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }
    }
}