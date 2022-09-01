using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SebbyLib;
using SharpDX;
using Color = System.Drawing.Color;
using HealthPrediction = EnsoulSharp.SDK.HealthPrediction;
using HitChance = EnsoulSharp.SDK.HitChance;
using Prediction = EnsoulSharp.SDK.Prediction;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Ezreal : Base
    {
        Vector3 CursorPosition = Vector3.Zero;
        public double lag = 0;
        public double WCastTime = 0;
        public double QCastTime = 0;
        public float DragonDmg = 0;
        public double DragonTime = 0;
        public bool Esmart = false;
        public double OverKill = 0;
        public double OverFarm = 0;
        public double diag = 0;
        public double diagF = 0;
        public int Muramana = 3042;
        public int Tear = 3070;
        public int Manamune = 3004;
        public double NotTime = 0;

        public static Core.OKTWdash Dash;

        private readonly MenuBool noti = new MenuBool("noti", "Show notification",true);
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells",true);
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W", true);
        private readonly MenuBool wPush = new MenuBool("wPush", "W on towers", true);
        private readonly MenuBool harassW = new MenuBool("harassW", "Harass W", true);

        private readonly MenuKeyBind smartE = new MenuKeyBind("smartE", "SmartCast E key",Keys.E,KeyBindType.Press);
        private readonly MenuKeyBind smartEW = new MenuKeyBind("smartEW", "SmartCast E + W key",Keys.E,KeyBindType.Press);
        private readonly MenuBool EKsCombo = new MenuBool("EKsCombo", "E ks combo");
        private readonly MenuBool EAntiMelee = new MenuBool("EAntiMelee", "E anti-melee");
        private readonly MenuBool autoEgrab = new MenuBool("autoEgrab", "Auto E anti grab");

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R");
        private readonly MenuBool Rcc = new MenuBool("Rcc", "R cc");
        private readonly MenuBool Raoe = new MenuBool("Raoe", "R AOE");
        private readonly MenuBool Rjungle = new MenuBool("Rjungle", "R Jungle stealer");
        private readonly MenuBool Rdragon = new MenuBool("Rdragon", "Dragon");
        private readonly MenuBool Rbaron = new MenuBool("Rbaron", "Baron");
        private readonly MenuBool Rred = new MenuBool("Rred", "Red");
        private readonly MenuBool Rblue = new MenuBool("Rblue", "Blue");
        private readonly MenuBool Rally = new MenuBool("Rally", "Ally stealer");
        private readonly MenuKeyBind useR = new MenuKeyBind("useR", "Semi-manual cast R key",Keys.T,KeyBindType.Press);
        private readonly MenuBool Rturrent = new MenuBool("Rturrent", "Don't R under turret");
        private readonly MenuSlider MaxRangeR = new MenuSlider("MaxRangeR", "Max R range", 3000, 0, 5000);
        private readonly MenuSlider MinRangeR = new MenuSlider("MinRangeR", "Min R range", 900, 0, 5000);

        private readonly MenuSlider HarassMana = new MenuSlider("HarassMana", "Harass Mana", 30, 100, 0);

        private readonly MenuBool farmQ = new MenuBool("farmQ", "LaneClear Q");
        private readonly MenuBool FQ = new MenuBool("FQ", "Farm Q out range");
        private readonly MenuBool LCP = new MenuBool("LCP", "FAST LaneClear");

        private new readonly MenuBool debug = new MenuBool("debug", "Debug", false);

        private readonly MenuBool stack = new MenuBool("stack", "Stack Tear if full mana");

        public Ezreal()
        {
            Q = new Spell(SpellSlot.Q, 1180);
            W = new Spell(SpellSlot.W, 1180);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 3000f);
            
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SpellType.Line);
            W.SetSkillshot(0.25f, 60f, 1700f, false, SpellType.Line);
            R.SetSkillshot(1.1f, 160f, 2000f, false, SpellType.Line);

            Local.Add(new Menu("draw", "Draw")
            {
                noti,
                onlyRdy,
                qRange,
                wRange,
                eRange,
                rRange,
            });

            Local.Add(new Menu("wConfig", "Q Config")
            {
                autoW,
                wPush,
                harassW
            });

            Local.Add(new Menu("eConfig", "E Config")
            {
                smartE,
                smartEW,
                EKsCombo,
                EAntiMelee,
                autoEgrab
            });
            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                Rcc,
                Raoe,
                new Menu("rJungleStealer", "R Jungle stealer")
                {
                    Rjungle,
                    Rdragon,
                    Rbaron,
                    Rred,
                    Rblue,
                    Rally
                },
                useR,
                Rturrent,
                MaxRangeR,
                MinRangeR
            });
            Local.Add(new Menu("extra", "Extra")
            {
                HarassMana,
                //debug,
                stack
            });

            FarmMenu.Add(farmQ);
            FarmMenu.Add(FQ);
            FarmMenu.Add(LCP);

            Dash = new Core.OKTWdash(E);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;
            AIBaseClient.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
        }

        private void Obj_AI_Base_OnBuffAdd(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            if (sender.IsMe && autoEgrab.Enabled)
            {
                if (args.Buff.Name == "ThreshQ" || args.Buff.Name == "rocketgrab2")
                {
                    var dashPos = Dash.CastDash(true);
                    if (!dashPos.IsZero)
                    {
                        E.Cast(dashPos);
                    }
                    else
                    {
                        E.Cast(Game.CursorPos);
                    }
                }
            }
        }

        private void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (wPush.Enabled && args.Target.Type == GameObjectType.AITurretClient)
            {
                if (W.IsReady() && Player.Mana > RMANA + WMANA + EMANA)
                {
                    if (W.Cast(args.Target.Position))
                    {
                        args.Process = false;
                        return;
                    }
                }
            }

            if (W.IsReady() && Player.Mana > RMANA + WMANA + EMANA)
            {
                var target = args.Target as AIHeroClient;
                if (target != null)
                {
                    var prediction = W.GetPrediction(target);

                    if (prediction.Hitchance < HitChance.Medium || target.Distance(Player) - Player.BoundingRadius > Player.AttackRange - 50)
                        return;

                    if (Program.Combo && Player.Mana > RMANA + WMANA + EMANA)
                    {
                        if (W.Cast(prediction.CastPosition))
                        {
                            args.Process = false;
                            return;
                        }
                    }
                    //HarassMenu.GetValue<MenuBool>("harass"+t.CharacterName).Enabled
                    else if (Program.Harass && harassW.Enabled && HarassMenu.GetValue<MenuBool>("harass"+target.CharacterName).Enabled && Player.Mana > Player.MaxMana * 0.8 && Player.ManaPercent > HarassMana.Value && OktwCommon.CanHarras())
                    {
                        if (W.Cast(prediction.CastPosition))
                        {
                            args.Process = false;
                            return;
                        }
                    }
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (qRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if(Q.IsReady())
                        DrawCircle(Player.Position,Q.Range,SharpDX.Color.Cyan,1,1);
                }
                else
                    DrawCircle(Player.Position, Q.Range, SharpDX.Color.Cyan, 1, 1);
            }

            if (wRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (W.IsReady())
                    {
                        DrawCircle(Player.Position, W.Range, SharpDX.Color.Orange, 1, 1);
                    }
                }
                else
                    DrawCircle(Player.Position, W.Range, SharpDX.Color.Orange, 1, 1);
            }

            if (eRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (E.IsReady())
                    {
                        DrawCircle(Player.Position, E.Range, SharpDX.Color.Yellow, 1, 1);
                    }
                }
                else
                    DrawCircle(Player.Position, E.Range, SharpDX.Color.Yellow, 1, 1);
            }

            if (rRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (R.IsReady())
                    {
                        DrawCircle(Player.Position, R.Range, SharpDX.Color.Gray, 1, 1);
                    }
                }
                else
                    DrawCircle(Player.Position, R.Range, SharpDX.Color.Gray, 1, 1);
            }

            if (noti.Enabled)
            {
                var target = TargetSelector.GetTarget(1500, DamageType.Physical);
                if (target.IsValidTarget())
                {
                    var poutput = Q.GetPrediction(target);
                    if((int)poutput.Hitchance == 5)
                        CircleRender.Draw(poutput.CastPosition,50,SharpDX.Color.YellowGreen);
                    if (Q.GetDamage(target) > target.Health)
                    {
                        CircleRender.Draw(target.Position, 200, SharpDX.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q kill: " + target.CharacterName + " have: " + target.Health + "hp");
                    }
                    else if (Q.GetDamage(target) + W.GetDamage(target) > target.Health)
                    {
                        CircleRender.Draw(target.Position, 200, SharpDX.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q + W kill: " + target.CharacterName + " have: " + target.Health + "hp");
                    }
                    else if (Q.GetDamage(target) + W.GetDamage(target) + E.GetDamage(target) > target.Health)
                    {
                        CircleRender.Draw(target.Position, 200, SharpDX.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q + W + E kill: " + target.CharacterName + " have: " + target.Health + "hp");
                    }
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (R.IsReady() && Rjungle.Enabled)
            {
                KsJungle();
            }
            else
                DragonTime = 0;

            if (E.IsReady())
            {
                if (Program.LagFree(0))
                    LogicE();

                if (smartE.Active)
                    Esmart = true;
                if (smartEW.Active && W.IsReady())
                {
                    CursorPosition = Game.CursorPos;
                    W.Cast(CursorPosition);
                }

                if (Esmart && Player.Position.Extend(Game.CursorPos, E.Range).CountEnemyHeroesInRange(500) < 4)
                    E.Cast(Player.Position.Extend(Game.CursorPos, E.Range));
                
                if (!CursorPosition.IsZero)
                    E.Cast(Player.Position.Extend(CursorPosition, E.Range));
            }
            else
            {
                CursorPosition = Vector3.Zero;
                Esmart = false;
            }

            if (Q.IsReady())
                LogicQ();
        }

        private void SetMana()
        {
            if (manaDisable.Enabled && Combo || Player.ManaPercent < 20)
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

        private void KsJungle()
        {
            var mobs = GameObjects.GetJungles(Player.PreviousPosition, float.MaxValue);
            foreach (var mob in mobs)
            {
                if(mob.Health == mob.MaxHealth)
                    continue;
                if (((mob.SkinName.ToLower().Contains("dragon") && Rdragon.Enabled) ||
                     (mob.SkinName == "SRU_Baron" && Rbaron.Enabled) ||
                     (mob.SkinName == "SRU_Red" && Rred.Enabled) ||
                     (mob.SkinName == "SRU_Blue" && Rblue.Enabled))
                    && (mob.CountAllyHeroesInRange(1000) == 0 || Rally.Enabled)
                    && mob.Distance(Player.Position) > 1000)
                {
                    if(DragonDmg == 0)
                        DragonDmg = mob.Health;
                    if (Game.Time - DragonTime > 3)
                    {
                        if (DragonDmg - mob.Health > 0)
                        {
                            DragonDmg = mob.Health;
                        }
                        DragonTime = Game.Time;
                    }
                    else
                    {
                        var DmgSec = (DragonDmg - mob.Health) * (Math.Abs(DragonTime - Game.Time) / 3);
                        if (DragonDmg - mob.Health > 0)
                        {
                            
                            var timeTravel = GetUltTravelTime(Player, R.Speed, R.Delay, mob.Position);
                            var timeR = (mob.Health - R.GetDamage(mob)) / (DmgSec / 3);
                            if (timeTravel > timeR)
                                R.Cast(mob.Position);
                        }
                        else
                            DragonDmg = mob.Health;
                    }
                }
            }
        }
        private float GetUltTravelTime(AIHeroClient source, float speed, float delay, Vector3 targetpos)
        {
            float distance = Vector3.Distance(source.PreviousPosition, targetpos);
            float missilespeed = speed;

            return (distance / missilespeed + delay);
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(1300, DamageType.Physical);

            if (EAntiMelee.Enabled)
            {
                if (GameObjects.EnemyHeroes.Any(target =>
                    target.IsValidTarget(1000) && target.IsMelee &&
                    Player.Distance(Prediction.GetPrediction(target, 0.2f).CastPosition) < 250))
                {
                    var dashPos = Dash.CastDash(true);
                    if (!dashPos.IsZero)
                    {
                        E.Cast(dashPos);
                    }
                }
            }

            if (t.IsValidTarget() && Program.Combo && EKsCombo.Enabled && Player.HealthPercent > 40 &&
                t.Distance(Game.CursorPos) + 300 < t.Position.Distance(Player.Position) &&
                !Player.InAutoAttackRange(t) && !Player.IsUnderEnemyTurret() && Game.Time - OverKill > 0.3)
            {
                var dashPosition = Player.Position.Extend(Game.CursorPos, E.Range);

                if (dashPosition.CountEnemyHeroesInRange(900) < 3)
                {
                    var dmgCombo = 0f;

                    if (t.IsValidTarget(950))
                    {
                        dmgCombo = (float) Player.GetAutoAttackDamage(t) + E.GetDamage(t);
                    }

                    if (Q.IsReady() && Player.Mana > QMANA + EMANA &&
                        Q.WillHit(dashPosition, Q.GetPrediction(t).UnitPosition))
                        dmgCombo = Q.GetDamage(t);
                    if (W.IsReady() && Player.Mana > QMANA + EMANA + WMANA)
                    {
                        dmgCombo += W.GetDamage(t);
                    }

                    if (dmgCombo > t.Health && OktwCommon.ValidUlt(t))
                    {
                        E.Cast(dashPosition);
                        OverKill = Game.Time;
                    }
                }
            }
        }

        private void LogicQ()
        {
            if (Variables.TickCount - W.LastCastAttemptTime < 125)
            {
                return;
            }

            if (Program.LagFree(1))
            {
                if(!Orbwalker.CanMove() )
                    return;

                bool cc = !Program.None && Player.MinionsKilled > RMANA + QMANA + EMANA;
                bool harass = Program.Harass && Player.ManaPercent > HarassMana.Value && OktwCommon.CanHarass();

                if (Program.Combo && Player.Mana > RMANA + QMANA)
                {
                    var t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                    if (t.IsValidTarget() && (!W.IsReady() || !autoW.Enabled || !OktwCommon.CanMove(t)))
                        //CastSpell(Q,t);
                        Q.Cast(t);
                    
                    if(harass && (!W.IsReady() || autoW.Enabled) && HarassMenu.GetValue<MenuBool>("harass"+t.CharacterName).Enabled)
                        Program.CastSpell(Q,t);
                }
            }
            else if (Program.LagFree(2))
            {
                if (Player.Mana > QMANA && Farm)
                {
                    famrQ();
                    lag = Game.Time;
                }
                else if (stack.Enabled && Variables.TickCount - Q.LastCastAttemptTime > 4000 &&
                         !Player.HasBuff("Recall") && Player.Mana > Player.MaxMana * 0.95 && Program.None &&
                         (Items.HasItem(Player, Tear) || Items.HasItem(Player, Manamune)))
                {
                    Q.Cast(Player.Position.Extend(Game.CursorPos, 500));
                }
            }
        }
        private float GetPassiveTime()
        {
            return
                ObjectManager.Player.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == "ezrealrisingspellforce")
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        public void famrQ()
        {
            if (Program.LaneClear)
            {
                var mobs = GameObjects.GetMinions(Player.Position, 800, MinionTypes.All).ToList();
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    Q.Cast(mob.Position);
                }
            }

            if (!Orbwalker.CanMove() || Orbwalker.CanAttack())
            {
                return;
            }

            var minions = GameObjects.GetMinions(Player.Position, Q.Range);
            int orbTarget = 0;

            if (Orbwalker.GetTarget() != null)
                orbTarget = Orbwalker.GetTarget().NetworkId;
            if (FQ.Enabled)
            {
                foreach (var minion in minions.Where(minion =>
                    minion.IsValidTarget() && orbTarget != minion.NetworkId && !Player.InAutoAttackRange(minion)))
                {
                    int delay = (int) ((minion.Distance(Player) / Q.Speed + Q.Delay) * 1000);
                    var hpPred = HealthPrediction.GetPrediction(minion, delay);
                    if (hpPred > 0 && hpPred < Q.GetDamage(minion))
                    {
                        if (Q.Cast(minion) == CastStates.SuccessfullyCasted)
                            return;
                    }
                }
            }

            if (farmQ.Enabled && !Orbwalker.CanAttack() && FarmSpells)
            {
                var PT = Game.Time - GetPassiveTime() > -1.5 || !E.IsReady();

                foreach (var minion in minions.Where(minion => Player.InAutoAttackRange(minion)))
                {
                    int delay = (int) ((minion.Distance(Player) / Q.Speed + Q.Delay) * 1000);
                    var hpPred = HealthPrediction.GetPrediction(minion, delay);
                    if (hpPred < 20)
                        continue;
                    
                    var qDmg = Q.GetDamage(minion);
                    if (hpPred < qDmg && orbTarget != minion.NetworkId)
                    {
                        if (Q.Cast(minion) == CastStates.SuccessfullyCasted)
                            return; 
                    }
                    else if (PT || LCP.Enabled)
                    {
                        if (minion.HealthPercent > 80)
                        {
                            if (Q.Cast(minion) == CastStates.SuccessfullyCasted)
                                return;
                        }
                    }
                }
            }
        }
    }
}