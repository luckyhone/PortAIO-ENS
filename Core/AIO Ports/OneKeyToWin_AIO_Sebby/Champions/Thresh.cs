using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SebbyLib;
using SharpDX;
using HitChance = EnsoulSharp.SDK.HitChance;
using Prediction = SebbyLib.Prediction;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Thresh : Base
    {
        private Spell Epush;
        private static AIBaseClient Marked;

        private readonly MenuBool ts = new MenuBool("ts", "Use common TargetSelector");
        private readonly MenuSeparator ts1 = new MenuSeparator("ts1", "ON - only one target");
        private readonly MenuSeparator ts2 = new MenuSeparator("ts2", "OFF - all grab-able targets");
        private readonly MenuBool qCC = new MenuBool("qCC", "Auto Q cc");
        private readonly MenuBool qDash = new MenuBool("qDash", "Auto Q dash");
        private readonly MenuSlider minGrab = new MenuSlider("minGrab", "Min range grab", 250, 125, 1075);
        private readonly MenuSlider maxGrab = new MenuSlider("maxGrab", "Max range grab", 1075, 125, 1075);
        private readonly List<MenuBool> championsGrab = new List<MenuBool>();
        private readonly MenuBool GapQ = new MenuBool("GapQ", "OnEnemyGapcloser Q");

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W");
        private readonly MenuSlider Wdmg = new MenuSlider("Wdmg", "W dmg % hp",10,100,0);
        private readonly MenuBool autoW3 = new MenuBool("autoW3", "Auto W shield bif dmg");
        private readonly MenuBool autoW2 = new MenuBool("autoW2", "Auto W if Q succesfull");
        private readonly MenuBool autoW4 = new MenuBool("autoW4", "Auto W vs Blitz Hook");
        private readonly MenuBool autoW5 = new MenuBool("autoW5", "Auto W if jungler pings");
        private readonly MenuBool autoW6 = new MenuBool("autoW6", "Auto W on gapCloser");
        private readonly MenuBool autoW7 = new MenuBool("autoW7", "Auto W on Slows/Stuns");
        private readonly MenuSlider wCount = new MenuSlider("wCount", "Auto W if x enemies near ally",3,0,5);

        private readonly MenuBool autoE = new MenuBool("autoE", "Auto E");
        private readonly MenuBool pushE = new MenuBool("pushE", "Auto push");
        private readonly MenuBool pulldashE = new MenuBool("pulldashE", "Auto pull on dash");
        private readonly MenuBool inter = new MenuBool("inter", "OnPossibleToInterrupt");
        private readonly MenuBool Gap = new MenuBool("Gap", "OnEnemyGapcloser");
        private readonly MenuSlider Emin = new MenuSlider("Emin", "Min pull range E",200,0,(int) E.Range);

        private readonly MenuSlider rCount = new MenuSlider("rCount", "Auto R if x enemies in range", 2, 0, 5);
        private readonly MenuBool rKs = new MenuBool("rKs", "R ks");
        private readonly MenuBool comboR = new MenuBool("comboR", "always R in combo");
        
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);

        private readonly MenuBool AACombo = new MenuBool("AACombo", "Disable AA if can use E");

        public Thresh()
        {
            Q = new Spell(SpellSlot.Q, 1075);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 450);
            Epush = new Spell(SpellSlot.E, 450);

            Q.SetSkillshot(0.5f, 70, 1900f, true, SpellType.Line);
            W.SetSkillshot(0.2f, 10, float.MaxValue, false, SpellType.Circle);
            E.SetSkillshot(0.25f, 50, 2000, false, SpellType.Line);
            Epush.SetSkillshot(0f, 50, float.MaxValue, false, SpellType.Line);

            Local.Add(new Menu("draw", "Draw")
            {
                onlyRdy,
                qRange,
                wRange,
                eRange,
                rRange
            });

            var grabList = new Menu("grabList", "Grab");
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                var championGrab = new MenuBool("grab" + enemy.CharacterName, enemy.CharacterName);
                //Game.Print(championGrab.Name);
                
                championsGrab.Add(championGrab);
                grabList.Add(championGrab);
            }

            Local.Add(new Menu("qConfig", "Q Config")
            {
                ts,
                ts1,
                ts2,
                qCC,
                qDash,
                minGrab,
                maxGrab,
                grabList,
                GapQ
            });

            Local.Add(new Menu("wConfig", "W Config")
            {
                autoW,
                Wdmg,
                autoW3,
                autoW2,
                autoW4,
                autoW5,
                autoW6,
                autoW7,
                wCount
            });

            Local.Add(new Menu("eConfig", "E Config")
            {
                autoE,
                pushE,
                pulldashE,
                inter,
                Gap,
                Emin
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                rCount,
                rKs,
                comboR
            });

            Local.Add(AACombo);
            
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
            AIBaseClient.OnBuffRemove += Obj_AI_Base_OnBuffRemove;
            
        }

        private void Obj_AI_Base_OnBuffRemove(AIBaseClient sender, AIBaseClientBuffRemoveEventArgs args)
        {
            if (sender.IsEnemy && args.Buff.Name == "ThreshQ")
            {
                Marked = null;
            }
        }

        private void Obj_AI_Base_OnBuffAdd(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            if (sender.IsEnemy && args.Buff.Name == "ThreshQ")
            {
                Marked = sender;
            }
        }

        private void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (E.IsReady() && inter.Enabled && sender.IsValidTarget(E.Range))
            {
                E.Cast(sender.Position);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if(sender.IsAlly)return;

            if (autoW6.Enabled)
            {
                var allyHero =
                    GameObjects.AllyHeroes.Where(ally => ally.Distance(Player) <= W.Range + 550 && !ally.IsMe)
                        .OrderBy(ally => ally.Distance(args.EndPosition))
                        .FirstOrDefault();
                if (allyHero != null)
                {
                    CastW(allyHero.Position);
                }
            }
            if (E.IsReady() && Gap.Enabled && sender.IsValidTarget(E.Range) && !Marked.IsValidTarget())
            {
                E.Cast(sender);
            }
            else if (Q.IsReady() && GapQ.Enabled && sender.IsValidTarget(Q.Range))
            {
                Q.Cast(sender);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.Combo && AACombo.Enabled)
            {
                if (!E.IsReady())
                    Orbwalker.AttackEnabled = true;
                else
                    Orbwalker.AttackEnabled = false;
            }
            else
                Orbwalker.AttackEnabled = true;

            if (Marked.IsValidTarget())
            {
                if (Program.Combo)
                {
                    if (OktwCommon.GetPassiveTime(Marked, "ThreshQ") < 0.3)
                        Q.Cast();
                    if (W.IsReady() && autoW2.Enabled)
                    {
                        var allyW = Player;
                        foreach (var ally in GameObjects.AllyHeroes.Where(ally =>
                                     ally.IsValid && !ally.IsDead && Player.Distance(ally.Position) < W.Range + 500))
                        {
                            if (Marked.Distance(ally.Position) > 800 && Player.Distance(ally.Position) > 600)
                            {
                                CastW(Prediction.GetPrediction(ally, 1f).CastPosition);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Program.LagFree(1) && Q.IsReady())
                    LogicQ();

                if (Program.LagFree(2) && E.IsReady() && autoE.Enabled)
                    LogicE();
            }

            if (Program.LagFree(3) && W.IsReady())
                LogicW();
            if (Program.LagFree(4) && R.IsReady())
                LogicR();
        }
        
        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (t.IsValidTarget()  && OktwCommon.CanMove(t) && !Marked.IsValidTarget())
            {
                if (Program.Combo)
                {
                    if (Player.Distance(t) > Emin.Value)
                        CastE(false, t);
                }
                else if (pushE.Enabled)
                {
                    CastE(true, t);
                }
                else if (pulldashE.Enabled && t.IsDashing())
                {
                    var pred =  Prediction.GetPrediction(t, 0.15f);
                    if(pred.CastPosition.Distance(Player.Position) < E.Range)
                        E.Cast(pred.CastPosition);
                }
            }
        }
        
        private void CastE(bool push, AIBaseClient target)
        {
            if (push)
            {
                var eCastPosition = E.GetPrediction(target).CastPosition;
                E.Cast(eCastPosition);
            }
            else
            {
                var eCastPosition = Epush.GetPrediction(target).CastPosition;
                var distance = Player.Distance(eCastPosition);
                var ext = Player.Position.Extend(eCastPosition, -distance);
                E.Cast(ext);
            }
        }
        
        private void LogicQ()
        {
            //float maxGrab = MainMenu.Item("maxGrab", true).GetValue<Slider>().Value;
            //float minGrab = MainMenu.Item("minGrab", true).GetValue<Slider>().Value;

            if (Program.Combo && ts.Enabled)
            {
                var t = TargetSelector.GetTarget(maxGrab.Value, DamageType.Physical);

                if (t.IsValidTarget(maxGrab.Value) && !t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) &&  championsGrab.Any(e => e.Enabled && e.Name == "grab"+t.CharacterName) /*Local.GetValue<MenuBool>("grab" + t.CharacterName).Enabled*/ && Player.Distance(t.Position) > minGrab.Value)
                    Program.CastSpell(Q, t);
            }

            foreach (var t in GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(maxGrab.Value) && championsGrab.Any(e => e.Enabled && e.Name == "grab"+t.CharacterName) /*Local.GetValue<MenuBool>("grab" + t.CharacterName).Enabled&*/ && Player.Distance(t.Position) > minGrab.Value))
            {
                if (!t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) )
                {
                    if (Program.Combo && !ts.Enabled)
                        Program.CastSpell(Q, t);

                    if (qCC.Enabled)
                    {
                        if (!OktwCommon.CanMove(t))
                            Q.Cast(t);

                        Q.CastIfHitchanceEquals(t, HitChance.Immobile);
                    }
                    if (qDash.Enabled)
                    {
                        Q.CastIfHitchanceEquals(t, HitChance.Dash);
                    }
                }
            }
        }
        
        private void LogicR()
        {

            var rCountOut = Player.CountEnemyHeroesInRange(R.Range);
            var rCountIn = Player.CountEnemyHeroesInRange(200);

            if (rCountOut < rCountIn)
                return;

            if (rCountOut >= rCount.Value && rCount.Value > 0)
                R.Cast();

            if (comboR.Enabled)
            {
                var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (t.IsValidTarget() && (!Player.IsUnderEnemyTurret() || Program.Combo))
                {
                    if (t != null && Player.Distance(t.Position) > Player.Distance(t.Position))
                        R.Cast();
                }
            }
        }
        
        private void LogicW()
        {
            if (autoW4.Enabled)
            {
                var saveAlly = GameObjects.AllyHeroes.FirstOrDefault(ally => ally.HasBuff("rocketgrab2") && !ally.IsMe);
                if (saveAlly != null)
                {
                    var blitz = saveAlly.GetBuff("rocketgrab2").Caster;
                    if (Player.Distance(blitz.Position) <= W.Range + 550 && W.IsReady())
                    {

                        CastW(blitz.Position);
                    }
                }
            }

            foreach (var ally in GameObjects.AllyHeroes.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(ally) < W.Range + 400))
            {
                if (autoW7.Enabled && !ally.IsMe)
                {
                    if (ally.Distance(Player) <= W.Range)
                    {
                        if (ally.IsStunned || ally.IsSlowed)
                        {
                            W.Cast(ally.Position);
                        }
                    }
                }

                int nearEnemys = ally.CountEnemyHeroesInRange(900);

                if (nearEnemys >= wCount.Value && wCount.Value > 0)
                    CastW(W.GetPrediction(ally).CastPosition);

                if (autoW.Enabled && Player.Distance(ally) < W.Range + 100)
                {
                    double dmg = OktwCommon.GetIncomingDamage(ally);
                    if (dmg == 0)
                        continue;

                    int sensitivity = 20;

                    double HpPercentage = (dmg * 100) / ally.Health;
                    double shieldValue = 20 + (Player.Level * 20) + (0.4 * Player.FlatMagicDamageMod);

                    nearEnemys = (nearEnemys == 0) ? 1 : nearEnemys;

                    if (dmg > shieldValue && autoW3.Enabled)
                        W.Cast(W.GetPrediction(ally).CastPosition);
                    else if (dmg > 100 + Player.Level * sensitivity)
                        W.Cast(W.GetPrediction(ally).CastPosition);
                    else if (ally.Health - dmg < nearEnemys * ally.Level * sensitivity)
                        W.Cast(W.GetPrediction(ally).CastPosition);
                    else if (HpPercentage >= Wdmg.Value)
                        W.Cast(W.GetPrediction(ally).CastPosition);
                }
            }
        }
        
        private void CastW(Vector3 pos)
        {
            if (Player.Distance(pos) < W.Range)
                W.Cast(pos);
            else
                W.Cast(Player.Position.Extend(pos, W.Range));
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (qRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (Q.IsReady())
                        Program.DrawCircle(Player.Position, (float)maxGrab.Value, Color.Cyan, 1, 1);
                }
                else
                    Program.DrawCircle(Player.Position, (float)maxGrab.Value, Color.Cyan, 1, 1);
            }

            if (wRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (E.IsReady())
                        Program.DrawCircle(Player.Position, W.Range, Color.Cyan, 1, 1);
                }
                else
                    Program.DrawCircle(Player.Position, W.Range, Color.Cyan, 1, 1);
            }

            if (eRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (E.IsReady())
                        Program.DrawCircle(Player.Position, E.Range, Color.Orange, 1, 1);
                }
                else
                    Program.DrawCircle(Player.Position, E.Range, Color.Orange, 1, 1);
            }

            if (rRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (R.IsReady())
                        Program.DrawCircle(Player.Position, R.Range, Color.Gray, 1, 1);
                }
                else
                    Program.DrawCircle(Player.Position, R.Range, Color.Gray, 1, 1);
            }
        }
    }
}