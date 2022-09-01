using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SebbyLib;
using SPrediction;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Varus : Base
    {
        public float AArange = ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius * 2;
        float CastTime = Game.Time;
        bool CanCast = true;

        public Varus()
        {
            Q = new Spell(SpellSlot.Q, 925);
            W = new Spell(SpellSlot.Q, 0);
            E = new Spell(SpellSlot.E, 975);
            R = new Spell(SpellSlot.R, 1050);

            Q.SetSkillshot(0.25f, 70, 1650, false, SpellType.Line);
            E.SetSkillshot(0.35f, 120, 1500, false, SpellType.Circle);
            R.SetSkillshot(0.25f, 120, 1950, false, SpellType.Line);
            Q.SetCharged("VarusQ","VarusQ",925, 1600, 1.5f);

            var draw = Local.Add(new Menu("draw", "Draw"));
            draw.Add(new MenuBool("onlyRdy", "Draw only ready spells", true).SetValue(true));
            draw.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            draw.Add(new MenuBool("eRange", "E range", true).SetValue(false));
            draw.Add(new MenuBool("rRange", "R range", true).SetValue(false));

            var qConfig = Local.Add(new Menu("qConfig", "Q Config"));
            qConfig.Add(new MenuBool("autoQ", "Auto Q", true).SetValue(true));
            qConfig.Add(new MenuBool("maxQ", "Cast Q only max range", true).SetValue(true));
            qConfig.Add(new MenuBool("fastQ", "Fast cast Q", true).SetValue(false));

            var eConfig = Local.Add(new Menu("eConfig", "E Config"));
            eConfig.Add(new MenuBool("autoE", "Auto E", true).SetValue(true));

            var rConfig = Local.Add(new Menu("rConfig", "R Config"));
            rConfig.Add(new MenuBool("autoR", "Auto R", true).SetValue(true));
            rConfig.Add(new MenuSlider("rCount", "Auto R if enemies in range (combo mode)", 3, 0, 5));
            rConfig.Add(new MenuKeyBind("useR", "Semi-manual cast R key", Keys.T, KeyBindType.Press));

            var gapcloserR = rConfig.Add(new Menu("gapcloserR", "GapCloser R"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                gapcloserR.Add(new MenuBool("GapCloser" + enemy.CharacterName, enemy.CharacterName).SetValue(false));
            
            var farm = FarmMenu.Add(new Menu("farm", "Farm"));
            farm.Add(new MenuBool("farmQ", "Lane clear Q", true).SetValue(true));
            farm.Add(new MenuBool("farmE", "Lane clear E", true).SetValue(true));
            
            Game.OnUpdate += Game_OnGameUpdate;

            Drawing.OnDraw += Drawing_OnDraw;
            //Orbwalking.BeforeAttack += BeforeAttack;
            //Orbwalking.AfterAttack += afterAttack;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            //Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Local["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (Q.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }

            if (Local["eRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (E.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }
            if (Local["rRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (R.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (R.IsReady() && Local["GapCloser" + sender.CharacterName].GetValue<MenuBool>().Enabled)
            {
                var Target = sender;
                if (Target.IsValidTarget(R.Range))
                {
                    R.Cast(Target.ServerPosition);
                    //Program.debug("AGC " );
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "VarusQ" || args.SData.Name == "VarusE" || args.SData.Name == "VarusR")
                {
                    CastTime = Game.Time;
                    CanCast = false;
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (R.IsReady())
            {
                if (Local["useR"].GetValue<MenuKeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                    if (t.IsValidTarget())
                        R.Cast(t);
                }
            }
            if (Program.LagFree(0))
            {
                SetMana();
                if (!CanCast)
                {
                    if (Game.Time - CastTime > 1)
                    {
                        CanCast = true;
                        return;
                    }
                    var t = Orbwalker.GetTarget() as AIBaseClient;
                    if (t.IsValidTarget())
                    {
                        if (OktwCommon.GetBuffCount(t, "varuswdebuff") < 3)
                            CanCast = true;
                    }
                    else
                    {
                        CanCast = true;
                    }
                }
            }

            if (Program.LagFree(1) && E.IsReady() && Local["autoQ"].GetValue<MenuBool>().Enabled && !ObjectManager.Player.Spellbook.IsAutoAttack)
                LogicE();
            if (Program.LagFree(2) && Q.IsReady() && Local["autoE"].GetValue<MenuBool>().Enabled && !ObjectManager.Player.Spellbook.IsAutoAttack)
                try
                {
                    LogicQ();
                }
                catch (Exception)
                {
                }

            if (Program.LagFree(3) && R.IsReady() && Local["autoR"].GetValue<MenuBool>().Enabled)
                LogicR();
            if (Program.LagFree(4))
                Farm();
        }
        
        private void Farm()
        {
            if (Program.LaneClear && E.IsReady() && FarmMenu["farmE"].GetValue<MenuBool>().Enabled)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, E.Range, MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0 && Player.Mana > RMANA + EMANA + QMANA && OktwCommon.GetBuffCount(mobs[0], "varuswdebuff") == 3)
                {
                    E.Cast(mobs[0]);
                    return;
                }

                if (FarmSpells)
                {
                    var allMinionsE = Cache.GetMinions(Player.ServerPosition, E.Range);
                    var Efarm = Q.GetCircularFarmLocation(allMinionsE, E.Width);
                    if (Efarm.MinionsHit > 3)
                    {
                        E.Cast(Efarm.Position);
                        return;
                    }
                }
            }
        }
        
        private void LogicR()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(R.Range)))
            {

                if (enemy.CountEnemyHeroesInRange(400) >= Local["rCount"].GetValue<MenuSlider>().Value && Local["rCount"].GetValue<MenuSlider>().Value > 0)
                {
                    R.Cast(enemy, true, true);
                    Program.debug("R AOE");
                }
                if ((enemy.CountEnemyHeroesInRange(600) == 0 || Player.Health < Player.MaxHealth * 0.5) && R.GetDamage(enemy) + GetWDmg(enemy) + Q.GetDamage(enemy) > enemy.Health && OktwCommon.ValidUlt(enemy))
                {
                    Program.CastSpell(R, enemy);
                    Program.debug("R KS");
                }
            }
            if (Player.Health < Player.MaxHealth * 0.5)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(target => target.IsValidTarget(270) && target.IsMelee && Local["GapCloser" + target.CharacterName].GetValue<MenuBool>().Enabled))
                {
                    Program.CastSpell(R, target);
                }
            }
        }
        
        private void LogicQ()
        {

            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(1600) && Q.GetDamage(enemy) + GetWDmg(enemy) > enemy.Health))
            {
                if (enemy.IsValidTarget(R.Range))
                    CastQ(enemy);
                return;
            }

            if (Local["maxQ"].GetValue<MenuBool>().Enabled && (Q.Range < 1500) && Player.CountEnemyHeroesInRange(AArange) == 0)
                return;

            var t = Orbwalker.GetTarget() as AIHeroClient;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (t.IsValidTarget() && t != null)
            {
                if (Q.IsCharging)
                {
                    if (Local["fastQ"].GetValue<MenuBool>().Enabled)
                        Q.Cast(Q.GetPrediction(t).CastPosition);

                    if (GetQEndTime() > 2)
                        Program.CastSpell(Q, t);
                    else
                        Q.Cast(Q.GetPrediction(t).CastPosition);
                    return;
                }

                if ((OktwCommon.GetBuffCount(t, "varuswdebuff") == 3 && CanCast && !E.IsReady()) || !ObjectManager.Player.InAutoAttackRange(t))
                {
                    if ((Program.Combo || (OktwCommon.GetBuffCount(t, "varuswdebuff") == 3 && Program.Harass)) && Player.Mana > RMANA + QMANA)
                    {
                        CastQ(t);
                    }
                    else if (Program.Harass && Player.Mana > RMANA + EMANA + QMANA + QMANA && HarassMenu["Harass" + t.CharacterName].GetValue<MenuBool>().Enabled && !Player.IsUnderEnemyTurret() && OktwCommon.CanHarras())
                    {
                        CastQ(t);
                    }
                    else if (!Program.None && Player.Mana > RMANA + WMANA)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                            CastQ(enemy);
                    }
                }
            }
            
            else if (FarmSpells && FarmMenu["farm"]["farmQ"].GetValue<MenuBool>().Enabled && Q.Range > 1500 && Player.CountEnemyHeroesInRange(1450) == 0 &&  (Q.IsCharging || (Player.ManaPercent > FarmMenu["Mana"].GetValue<MenuSlider>().Value)))
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, Q.Width);
                if (Qfarm.MinionsHit > 3 || (Q.IsCharging && Qfarm.MinionsHit > 0))
                    Q.Cast(Qfarm.Position);
            }
        }

        private void LogicE()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(E.Range) && E.GetDamage(enemy) + GetWDmg(enemy) > enemy.Health))
            {
                Program.CastSpell(E, enemy);
            }
            var t = Orbwalker.GetTarget() as AIHeroClient;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (t.IsValidTarget())
            {
                if ((OktwCommon.GetBuffCount(t, "varuswdebuff") == 3 && CanCast) || !ObjectManager.Player.InAutoAttackRange(t))
                {
                    if (Program.Combo && Player.Mana > RMANA + QMANA)
                    {
                        Program.CastSpell(E, t);
                    }
                    else if (!Program.None && Player.Mana > RMANA + WMANA)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                            E.Cast(enemy);
                    }
                }
            }
        }
        
        private float GetQEndTime()
        {
            return
                Player.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == "VarusQ")
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault() - Game.Time;
        }

        private float GetWDmg(AIBaseClient target)
        {
            return (OktwCommon.GetBuffCount(target, "varuswdebuff") * W.GetDamage(target, 1));
        }

        private void CastQ(AIBaseClient target)
        {
            if (!Q.IsCharging)
            {
                if (target.IsValidTarget(Q.Range - 300))
                    Q.StartCharging();
            }
            else
            {
                if (GetQEndTime() > 1)
                    Program.CastSpell(Q, target);
                else
                    Q.Cast(Q.GetPrediction(target).CastPosition);
                return;
            }
        }

        private void SetMana()
        {
            if (( Config["manaDisable"].GetValue<MenuBool>().Enabled && Program.Combo) || Player.HealthPercent < 20)
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