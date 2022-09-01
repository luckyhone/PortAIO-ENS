using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using OneKeyToWin_AIO_Sebby.Trackers;
using SebbyLib;
using ShadowTracker;
using SPrediction;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Karthus : Base
    {
        public Karthus()
        {
            Q = new Spell(SpellSlot.Q, 890);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 20000);

            Q.SetSkillshot(1.15f, 160f, float.MaxValue, false,SpellType.Circle);
            W.SetSkillshot(0.5f, 50f, float.MaxValue, false,SpellType.Circle);

            R.DamageType = DamageType.Magical;

            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("noti", "Show R notification", true).SetValue(true));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            drawMenu.Add(new MenuBool("wRange", "W range", true).SetValue(false));
            drawMenu.Add(new MenuBool("eRange", "E range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRange", "R range", true).SetValue(false));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw when skill rdy", true).SetValue(true));

            var qConfig = Local.Add(new Menu("qConfig", "Q Config"));
            qConfig.Add(new MenuBool("autoQ", "Auto Q", true).SetValue(true));
            qConfig.Add(new MenuBool("harassQ", "Harass Q", true).SetValue(true));
            qConfig.Add(new MenuSlider("QHarassMana", "Harass Mana",30, 0, 100));
            var useOn = qConfig.Add(new Menu("useOn", "Use on:"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                useOn.Add(new MenuBool("Qon" + enemy.CharacterName, enemy.CharacterName).SetValue(true));

            var wConfig = Local.Add(new Menu("wConfig", "W Config"));
            wConfig.Add(new MenuBool("autoW", "Auto W", true).SetValue(true));
            wConfig.Add(new MenuBool("harassW", "Harass W", true).SetValue(false));
            wConfig.Add(new MenuList("WmodeCombo", "W combo mode", new[] { "always", "run - cheese" }, 1));
            var wGapCloser = wConfig.Add( new Menu("wGapCloser", "W Gap Closer"));
            wGapCloser.Add(new MenuList("WmodeGC", "Gap Closer position mode",new[] { "Dash end position", "My hero position" }, 0));
            var castOnEnemy = wGapCloser.Add(new Menu("castOnEnemy", "Cast on enemy:"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                castOnEnemy.Add(new MenuBool("WGCchampion" + enemy.CharacterName, enemy.CharacterName, true).SetValue(true));

            var eConfig = Local.Add(new Menu("eConfig", "E Config"));
            eConfig.Add(new MenuBool("autoE", "Auto E if enemy in range", true).SetValue(true));
            eConfig.Add(new MenuSlider("Emana", "E % minimum mana",20));
            
            var rConfig = Local.Add(new Menu("rConfig", "R Config"));
            rConfig.Add(new MenuBool("autoR", "Auto R", true).SetValue(true));
            rConfig.Add(new MenuBool("autoRzombie", "Auto R upon dying if can help team", true).SetValue(true));
            rConfig.Add(new MenuSlider("Renemy", "Don't R if enemy in x range", 1500, 0, 2000));
            rConfig.Add(new MenuSlider("RenemyA", "Don't R if ally in x range near target", 800, 0, 2000));
            rConfig.Add(new MenuBool("Rturrent", "Don't R under turret", true).SetValue(true));
            
            FarmMenu.Add(new MenuBool("farmQout", "Last hit Q minion out range AA", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmQ", "Lane clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmE", "Lane clear E", true).SetValue(true));

            FarmMenu.Add(new MenuSlider("QLCminions", " QLaneClear minimum minions", 2, 0,10));
            FarmMenu.Add(new MenuSlider("ELCminions", " ELaneClear minimum minions", 5, 0,10));
            FarmMenu.Add(new MenuBool("jungleQ", "Jungle clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleE", "Jungle clear E", true).SetValue(true));

            Local.Add(new MenuBool("autoZombie", "Auto zombie mode COMBO / LANECLEAR"));
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsRecalling())
                return;

            if (Player.IsZombie())
            {
                if (Local["autoZombie"].GetValue<MenuBool>().Enabled)
                {
                    if (Player.CountEnemyHeroesInRange(Q.Range) > 0)
                        Orbwalker.ActiveMode = OrbwalkerMode.Combo;
                    else
                        Orbwalker.ActiveMode = OrbwalkerMode.LaneClear;
                }
                if (R.IsReady() && Local["autoRzombie"].GetValue<MenuBool>().Enabled)
                {
                    float timeDeadh = 8;
                    timeDeadh = OktwCommon.GetPassiveTime(Player, "KarthusDeathDefiedBuff");
                    Program.debug("Time " + timeDeadh);
                    if (timeDeadh < 4)
                    {
                        foreach (var target in GameObjects.EnemyHeroes.Where(target => target.IsValidTarget() && OktwCommon.ValidUlt(target)) )
                        {
                            var rDamage = R.GetDamage(target);
                            if (target.Health < 3 * rDamage && target.CountAllyHeroesInRange(800) > 0)
                                R.Cast();
                            if (target.Health < rDamage * 1.5 && target.Distance(Player.Position) < 900)
                                R.Cast();
                            if (target.Health + target.HPRegenRate * 5 < rDamage)
                                R.Cast();
                        }
                    }
                }
            }
            else
            {
                Orbwalker.ActiveMode = OrbwalkerMode.None;
            }

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }
            if (Program.LagFree(1) && Q.IsReady() && Local["autoQ"].GetValue<MenuBool>().Enabled)
                try{
                    LogicQ();
                }catch(Exception){}
            if (Program.LagFree(2) && E.IsReady() && Local["autoE"].GetValue<MenuBool>().Enabled)
                try{
                    LogicE();
                }catch(Exception){}
            if (Program.LagFree(3) && R.IsReady())
                try{
                    LogicR();
                }catch(Exception){}
            if (Program.LagFree(4) && W.IsReady() && Local["autoW"].GetValue<MenuBool>().Enabled)
                try{
                    LogicW();
                }catch(Exception){}
        }
        
        private void LogicR()
        {

            if (Local["autoR"].GetValue<MenuBool>().Enabled && Player.CountEnemyHeroesInRange(Local["Renemy"].GetValue<MenuSlider>().Value) == 0)
            {
                if (Player.IsUnderEnemyTurret() && Local["Rturrent"].GetValue<MenuBool>().Enabled)
                    return;

                foreach (var target in GameObjects.EnemyHeroes.Where(target => target.IsValid && !target.IsDead))
                {
                    if (target.IsValidTarget() && target.CountAllyHeroesInRange(Local["RenemyA"].GetValue<MenuSlider>().Value) == 0)
                    {
                        float predictedHealth = target.Health + target.HPRegenRate * 4;
                        float Rdmg = OktwCommon.GetKsDamage(target, R);

                        if (target.HealthPercent > 30)
                        {
                            /*if (Items.HasItem(3155, target))
                            {
                                Rdmg = Rdmg - 250;
                            }

                            if (Items.HasItem(3156, target))
                            {
                                Rdmg = Rdmg - 400;
                            }*/
                        }

                        if (Rdmg > predictedHealth && OktwCommon.ValidUlt(target))
                        {
                            R.Cast();
                            Program.debug("R normal");
                        }
                    }
                    else if(!target.IsVisible)
                    {
                        var ChampionInfoOne = Core.OKTWtracker.ChampionInfoList.Find(x => x.Hero.NetworkId == target.NetworkId);
                        if (ChampionInfoOne != null )
                        {
                            var timeInvisible = Game.Time - ChampionInfoOne.LastVisableTime;
                            if (timeInvisible > 3 && timeInvisible < 10)
                            {
                                float predictedHealth = target.Health + target.HPRegenRate * (4+ timeInvisible);
                                if (R.GetDamage(target) > predictedHealth)
                                    R.Cast();
                            }   
                        }
                    }
                }
            }
        }
        
        private float GetQDamage(AIBaseClient t)
        {
            var minions = Cache.GetMinions(t.Position, Q.Width + 20);

            if(minions.Count > 1)
                return Q.GetDamage(t, 1);
            else
                return Q.GetDamage(t);
        }
        
        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (t.IsValidTarget() && Local["qConfig"]["useOn"]["Qon" + t.CharacterName].GetValue<MenuBool>().Enabled)
            {
                
                if (Program.Combo && Player.Mana > RMANA + QMANA + WMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Harass && OktwCommon.CanHarras() && Local["qConfig"]["harassQ"].GetValue<MenuBool>().Enabled 
                    && Config["Harass" + t.CharacterName].GetValue<MenuBool>().Enabled && Player.ManaPercent > Local["qConfig"]["QHarassMana"].GetValue<MenuSlider>().Value)
                    Program.CastSpell(Q, t);
                else if (OktwCommon.GetKsDamage(t, Q) > t.Health)
                    Program.CastSpell(Q, t);

                foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                    Program.CastSpell(Q, t);
            }
            if (!OktwCommon.CanHarras())
                return;

            if (!Program.None && !Program.Combo && Player.Mana > RMANA + QMANA * 2)
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, Q.Range);

                if (FarmMenu["farmQout"].GetValue<MenuBool>().Enabled)
                {
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range) && (!ObjectManager.Player.InAutoAttackRange(minion) || (!minion.IsUnderEnemyTurret() && minion.IsUnderEnemyTurret()))))
                    {
                        var hpPred = SebbyLib.HealthPrediction.GetHealthPrediction(minion, 1100);
                        if (hpPred < GetQDamage(minion) * 0.9 && hpPred > minion.Health - hpPred * 2)
                        {
                            Q.Cast(minion);
                            return;
                        }
                    }
                }

                if (FarmMenu["farmQ"].GetValue<MenuBool>().Enabled && FarmSpells)
                {
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range) && ObjectManager.Player.InAutoAttackRange(minion)))
                    {    
                        var hpPred = SebbyLib.HealthPrediction.GetHealthPrediction(minion, 1100);
                        if (hpPred < GetQDamage(minion) * 0.9 && hpPred > minion.Health - hpPred * 2)
                        {
                            Q.Cast(minion);
                            return;
                        }
                    }
                    var farmPos = Q.GetCircularFarmLocation(allMinions, Q.Width);
                    if (farmPos.MinionsHit >= FarmMenu["QLCminions"].GetValue<MenuSlider>().Value)
                        Q.Cast(farmPos.Position);
                }
            }
        }
        
        private void LogicW()
        {
            if ((Program.Combo || (Program.Harass && Local["harassW"].GetValue<MenuBool>().Enabled)) && Player.Mana > RMANA + WMANA)
            {
                if (Local["WmodeCombo"].GetValue<MenuList>().Index == 1)
                {
                    var t = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                    if (t.IsValidTarget(W.Range) && W.GetPrediction(t).CastPosition.Distance(t.Position) > 100)
                    {
                        if (Player.Position.Distance(t.ServerPosition) > Player.Position.Distance(t.Position))
                        {
                            if (t.Position.Distance(Player.ServerPosition) < t.Position.Distance(Player.Position))
                                Program.CastSpell(W, t);
                        }
                        else
                        {
                            if (t.Position.Distance(Player.ServerPosition) > t.Position.Distance(Player.Position))
                                Program.CastSpell(W, t);
                        }
                    }
                }
                else
                {
                    var t = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                    if (t.IsValidTarget())
                    {
                        Program.CastSpell(W, t);
                    }
                }
            }
        }
        
        private void LogicE()
        {
            if (Program.None)
                return;

            if (Player.HasBuff("KarthusDefile"))
            {
                if (Program.LaneClear)
                {
                    if(OktwCommon.CountEnemyMinions(Player, E.Range) < FarmMenu["ELCminions"].GetValue<MenuSlider>().Value || Player.ManaPercent < FarmMenu["LCmana"].GetValue<MenuSlider>().Value)
                        E.Cast();
                }
                else if (Local["autoE"].GetValue<MenuBool>().Enabled)
                {
                    if (Player.ManaPercent < Local["eConfig"]["Emana"].GetValue<MenuSlider>().Value || Player.CountEnemyHeroesInRange(E.Range) == 0)
                        E.Cast();
                }
            }
            else 
            {
                if (Program.LaneClear)
                {
                    if (OktwCommon.CountEnemyMinions(Player, E.Range) >= FarmMenu["ELCminions"].GetValue<MenuSlider>().Value && FarmSpells)
                        E.Cast();
                }
                else if (Local["autoE"].GetValue<MenuBool>().Enabled && Player.ManaPercent > Local["eConfig"]["Emana"].GetValue<MenuSlider>().Value && Player.CountEnemyHeroesInRange(E.Range) > 0)
                {
                    E.Cast();
                }
            }
        }
        
        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + QMANA )
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && FarmMenu["jungleQ"].GetValue<MenuBool>().Enabled)
                    {
                        Q.Cast(mob.ServerPosition);
                        return;
                    }
                    if (E.IsReady() && FarmMenu["jungleE"].GetValue<MenuBool>().Enabled && mob.IsValidTarget(E.Range))
                    {
                        E.Cast(mob.ServerPosition);
                        return;
                    }
                }
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

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Local["draw"]["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (Q.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }
            if (Local["draw"]["wRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (W.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }
            if (Local["draw"]["eRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (E.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }
            if (Local["draw"]["rRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (R.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
            if (R.IsReady() && Local["draw"]["noti"].GetValue<MenuBool>().Enabled)
            {
                var t = TargetSelector.GetTarget(R.Range, DamageType.Magical);

                if (t.IsValidTarget() && OktwCommon.GetKsDamage(t, R) > t.Health)
                {
                    Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.CharacterName + " Heal - damage =  " + (t.Health - OktwCommon.GetKsDamage(t, R)) + " hp");
                }
            }
        }
    }
}