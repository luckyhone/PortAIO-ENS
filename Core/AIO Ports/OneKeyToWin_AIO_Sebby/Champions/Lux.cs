using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using PortAIO;
using SebbyLib;
using SharpDX;
using SPrediction;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Lux : Base
    {
        private Vector3 Epos = Vector3.Zero;
        private float DragonDmg = 0;
        private double DragonTime = 0;

        public Lux()
        {
            Q = new Spell(SpellSlot.Q, 1175);
            Q1 = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1075);
            R = new Spell(SpellSlot.R, 3000);

            Q1.SetSkillshot(0.25f, 80f, 1200f, true, SpellType.Line);
            Q.SetSkillshot(0.25f, 80f, 1200f, false, SpellType.Line);
            W.SetSkillshot(0.25f, 110f, 1200f, false, SpellType.Line);
            E.SetSkillshot(0.3f, 250f, 1050f, false, SpellType.Circle);
            R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SpellType.Line);

            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("noti", "Show notification", true).SetValue(true));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            drawMenu.Add(new MenuBool("wRange", "W range", true).SetValue(false));
            drawMenu.Add(new MenuBool("eRange", "E range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRange", "R range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRangeMini", "R range minimap", true).SetValue(true));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw when skill rdy", true).SetValue(true));

            var qConfig = Local.Add(new Menu("qConfig", "Q Config"));
            qConfig.Add(new MenuBool("autoQ", "Auto Q", true).SetValue(true)); 
            qConfig.Add(new MenuBool("harassQ", "Harass Q", true).SetValue(true));
            
            var qGapCloser = qConfig.Add(new Menu("qGapCloser", "Q Gap Closer"));
            qGapCloser.Add(new MenuBool("gapQ", "Auto Q Gap Closer", true).SetValue(true));
            var useGapOn = qGapCloser.Add(new Menu("useGapOn", "Use on:"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                useGapOn.Add(new MenuBool("Qgap" + enemy.CharacterName, enemy.CharacterName).SetValue(true));
            var useOn = qConfig.Add(new Menu("useOn", "Use on:"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                useOn.Add(new MenuBool("Qon" + enemy.CharacterName, enemy.CharacterName).SetValue(true));

            var eConfig = Local.Add(new Menu("eConfig", "E Config"));
            eConfig.Add(new MenuBool("autoE", "Auto E", true).SetValue(true));
            eConfig.Add(new MenuBool("harassE", "Harass E", true).SetValue(false));
            eConfig.Add(new MenuBool("autoEcc", "Auto E only CC enemy", true).SetValue(false));
            eConfig.Add(new MenuBool("autoEslow", "Auto E slow logic detonate", true).SetValue(true));
            eConfig.Add(new MenuBool("autoEdet", "Only detonate if target in E ", true).SetValue(false));

            var wConfig = Local.Add(new Menu("wConfig", "W Config"));
            var wShield = wConfig.Add(new Menu("wShield", "W Shield Config"));
            wShield.Add(new MenuSlider("Wdmg", "W dmg % hp",10, 100, 0));
            var shieldAlly = wShield.Add(new Menu("wShield", "Shield Ally"));
            foreach (var ally in GameObjects.AllyHeroes)
            {
                var allimenu = shieldAlly.Add(new Menu(ally.CharacterName, ally.CharacterName));
                allimenu.Add(new Menu(ally.CharacterName,ally.CharacterName).Add(new MenuBool("damage" + ally.CharacterName, "Damage incoming", true)));
                allimenu.Add(new Menu(ally.CharacterName,ally.CharacterName).Add(new MenuBool("HardCC" + ally.CharacterName, "Hard CC")));
                allimenu.Add(new Menu(ally.CharacterName,ally.CharacterName).Add(new MenuBool("Poison" + ally.CharacterName, "Poison")));
            }

            var rConfig = Local.Add(new Menu("rConfig", "R Config"));
            rConfig.Add(new MenuBool("autoR", "Auto R", true).SetValue(true));
            rConfig.Add(new MenuBool("passiveR", "Include R passive damage", true).SetValue(false));
            rConfig.Add(new MenuBool("Rcc", "R fast KS combo", true).SetValue(true));
            rConfig.Add(new MenuSlider("RaoeCount", "R x enemies in combo [0 == off]", 3, 0,5));
            rConfig.Add(new MenuSlider("hitchanceR", "Hit Chance R", 2, 0,3));
            rConfig.Add(new MenuKeyBind("useR", "Semi-manual cast R key",Keys.T, KeyBindType.Press)); //32 == space  

            var rJungleStealer = rConfig.Add(new Menu("rJungleStealer", "R Jungle stealer"));
            rJungleStealer.Add(new MenuBool("Rjungle", "R Jungle stealer", true).SetValue(true));
            rJungleStealer.Add(new MenuBool("Rdragon", "Dragon", true).SetValue(true));
            rJungleStealer.Add(new MenuBool("Rbaron", "Baron", true).SetValue(true));
            rJungleStealer.Add(new MenuBool("Rred", "Red", true).SetValue(true));
            rJungleStealer.Add(new MenuBool("Rblue", "Blue", true).SetValue(true));
            rJungleStealer.Add(new MenuBool("Rally", "Ally stealer", true).SetValue(false));
            
            FarmMenu.Add(new MenuBool("farmE", "Lane clear E", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleQ", "Jungle clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleE", "Jungle clear E", true).SetValue(true));
            
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (Q.IsReady() && sender.IsValidTarget(Q.Range) && Local["qGapCloser"].GetValue<MenuBool>("gapQ").Enabled && Local.GetValue<MenuBool>("Qgap" + sender.CharacterName).Enabled)
                Q.Cast(sender);
        }

        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "LuxLightStrikeKugel")
            {
                Epos = args.End;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (R.IsReady() )
            {
                if (Local["rJungleStealer"].GetValue<MenuBool>("Rjungle").Enabled)
                {
                    KsJungle();
                }
                
                
                if (Local.GetValue<MenuKeyBind>("useR").Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                    if (t.IsValidTarget())
                        R.Cast(t, true, true);
                }
            }
            else
                DragonTime = 0; 


            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if ((Program.LagFree(4) || Program.LagFree(1) || Program.LagFree(3)) && W.IsReady() && !Player.IsRecalling())
                //LogicW();
            if (Program.LagFree(1) && Q.IsReady() && Local.GetValue<MenuBool>("autoQ").Enabled)
                LogicQ();
            if (Program.LagFree(2) && E.IsReady() && Local.GetValue<MenuBool>("autoE").Enabled)
                LogicE();
           if (Program.LagFree(3) && R.IsReady())
                LogicR();
        }

        private void LogicW()
        {
            foreach (var ally in GameObjects.AllyHeroes.Where(ally => ally.IsValid && !ally.IsDead  && Local["shieldAlly"][ally.CharacterName].GetValue<MenuBool>("damage" + ally.CharacterName).Enabled && Player.Position.Distance(ally.Position) < W.Range))
            {
                double dmg = OktwCommon.GetIncomingDamage(ally);


                int nearEnemys = ally.CountEnemyHeroesInRange(800);

                if (dmg == 0 && nearEnemys == 0)
                    continue;

                int sensitivity = 20;
                
                double HpPercentage = (dmg * 100) / ally.Health;
                double shieldValue = 65 + W.Level * 25 + 0.35 * Player.FlatMagicDamageMod;

                if (Local["shieldAlly"][ally.CharacterName].GetValue<MenuBool>("HardCC" + ally.CharacterName).Enabled && nearEnemys > 0 && HardCC(ally))
                {
                    W.CastOnUnit(ally);
                }
                else if (Local["shieldAlly"][ally.CharacterName].GetValue<MenuBool>("Poison" + ally.CharacterName).Enabled && ally.HasBuffOfType(BuffType.Poison))
                {
                    W.Cast(W.GetPrediction(ally).CastPosition);
                }

                nearEnemys = (nearEnemys == 0) ? 1 : nearEnemys;

                if (dmg > shieldValue)
                    W.Cast(W.GetPrediction(ally).CastPosition);
                else if (dmg > 100 + Player.Level * sensitivity)
                    W.Cast(W.GetPrediction(ally).CastPosition);
                else if (ally.Health - dmg < nearEnemys * ally.Level * sensitivity)
                    W.Cast(W.GetPrediction(ally).CastPosition);
                else if (HpPercentage >= Local["wShield"].GetValue<MenuSlider>("Wdmg").Value)
                    W.Cast(W.GetPrediction(ally).CastPosition);
            }
        }
        
        private void LogicQ()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && E.GetDamage(enemy) + Q.GetDamage(enemy) + BonusDmg(enemy) > enemy.Health))
            {
                CastQ(enemy);
                return;
            }

            var t = Orbwalker.GetTarget() as AIHeroClient;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (t.IsValidTarget() && Local["qConfig"]["useOn"].GetValue<MenuBool>("Qon" + t.CharacterName).Enabled)
            {
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    CastQ(t);
                else if (Program.Harass  && Local["qConfig"].GetValue<MenuBool>("harassQ").Enabled  && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    CastQ(t);
                else if(OktwCommon.GetKsDamage(t,Q) > t.Health)
                    CastQ(t);

                foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                    CastQ(enemy);
            }
        }
        
        private void CastQ(AIBaseClient t)
        {
            var poutput = Q1.GetPrediction(t);
            var col = poutput.CollisionObjects.Count(ColObj => ColObj.IsEnemy && ColObj.IsMinion() && !ColObj.IsDead); 
     
            if ( col < 4)
                Program.CastSpell(Q, t);
        }
        
        private void LogicE()
        {
            try
            {
                if (Player.HasBuff("LuxLightStrikeKugel") && !Program.None)
                {
                    int eBig = Epos.CountEnemyHeroesInRange(350);
                    if (Local["eConfig"].GetValue<MenuBool>("autoEslow").Enabled)
                    {
                        int detonate = eBig - Epos.CountEnemyHeroesInRange(160);

                        if (detonate > 0 || eBig > 1)
                            E.Cast();
                    }
                    else if (Local["eConfig"].GetValue<MenuBool>("autoEdet").Enabled)
                    {
                        if (eBig > 0)
                            E.Cast();
                    }
                    else
                    {
                        E.Cast();
                    }
                }
                else
                {
                    var t = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                    if (t.IsValidTarget())
                    {
                        if (!Local["eConfig"].GetValue<MenuBool>("autoEcc").Enabled)
                        {
                            if (Program.Combo && Player.Mana > RMANA + EMANA)
                                Program.CastSpell(E, t);
                            else if (Program.Harass && OktwCommon.CanHarras() &&
                                     Config.GetValue<MenuBool>("Harass" + t.CharacterName).Enabled &&
                                     Local["eConfig"].GetValue<MenuBool>("harassE").Enabled &&
                                     Player.Mana > RMANA + EMANA + EMANA + RMANA)
                                Program.CastSpell(E, t);
                            else if (OktwCommon.GetKsDamage(t, E) > t.Health)
                                Program.CastSpell(E, t);
                        }

                        foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy =>
                                     enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                            E.Cast(enemy, true);
                    }
                    else if (FarmSpells && FarmMenu.GetValue<MenuBool>("farmE").Enabled && Player.Mana > RMANA + WMANA)
                    {
                        var minionList = Cache.GetMinions(Player.Position, E.Range);
                        var farmPosition = E.GetCircularFarmLocation(minionList, E.Width);

                        if (farmPosition.MinionsHit >= FarmMinions)
                            E.Cast(farmPosition.Position);
                    }
                }
            }catch(Exception){}
        }
        
        private void LogicR()
        {
            if (Local["rConfig"].GetValue<MenuBool>("autoR").Enabled )
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(target => target.IsValidTarget(R.Range) && target.CountAllyHeroesInRange(600) < 2 && OktwCommon.ValidUlt(target)))
                {
                    float Rdmg = OktwCommon.GetKsDamage(target, R);

                    if (Items.HasItem(target,3155))
                    {
                        Rdmg = Rdmg - 250;
                    }

                    if (Items.HasItem(target,3156))
                    {
                        Rdmg = Rdmg - 400;
                    }

                    if (Local["rConfig"].GetValue<MenuBool>("passiveR").Enabled)
                    {
                        if (target.HasBuff("luxilluminatingfraulein"))
                            Rdmg += (float)Player.CalculateDamage(target, DamageType.Magical, 10 + (8 * Player.Level) + 0.2 * Player.FlatMagicDamageMod);
                        
                        if (Player.HasBuff("itemmagicshankcharge") && Player.GetBuff("itemmagicshankcharge").Count == 100)
                            Rdmg += (float)Player.CalculateDamage(target, DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
                    }
                    if (Rdmg > target.Health)
                    {
                        castR(target);
                        Program.debug("R normal");
                    }
                    else if (!OktwCommon.CanMove(target) && Local["rConfig"].GetValue<MenuBool>("Rcc").Enabled && target.IsValidTarget(E.Range))
                    {
                        float dmgCombo = Rdmg;

                        if (E.IsReady())
                        {
                            var eDmg = E.GetDamage(target);
                            
                            if (eDmg > target.Health)
                                return;
                            else
                                dmgCombo += eDmg;
                        }

                        if (target.IsValidTarget(800))
                            dmgCombo += BonusDmg(target);

                        if (dmgCombo > target.Health)
                        {
                            R.CastIfWillHit(target, 2);
                            R.Cast(target);
                        }

                    }
                    else if (Program.Combo && Local["rConfig"].GetValue<MenuSlider>("RaoeCount").Value > 0)
                    {
                        R.CastIfWillHit(target, Local["rConfig"].GetValue<MenuSlider>("RaoeCount").Value);
                    }
                }
            }
        }
        
        private float BonusDmg(AIHeroClient target)
        {
            float damage = 10 + (Player.Level) * 8 + 0.2f * Player.FlatMagicDamageMod;
            if (Player.HasBuff("lichbane"))
            {
                damage += (Player.BaseAttackDamage * 0.75f) + ((Player.BaseAbilityDamage + Player.FlatMagicDamageMod) * 0.5f);
            }

            return (float)(Player.GetAutoAttackDamage(target) + Player.CalculateDamage(target, DamageType.Magical, damage));
        }
        
        private void castR(AIHeroClient target)
        {
            var inx = Local.GetValue<MenuSlider>("hitchanceR").Value;
            if (inx == 0)
            {
                R.Cast(R.GetPrediction(target).CastPosition);
            }
            else if (inx == 1)
            {
                R.Cast(target);
            }
            else if (inx == 2)
            {
                Program.CastSpell(R, target);
            }
            else if (inx == 3)
            {
                List<Vector2> waypoints = target.Path.ToList().ToVector2();
                if ((Player.Distance(waypoints.Last<Vector2>().To3D()) - Player.Distance(target.Position)) > 400)
                {
                    Program.CastSpell(R, target);
                }
            }
        }
        
        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + RMANA + WMANA)
            {
                var mobs = Cache.GetMinions(Player.Position, 600, MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && FarmMenu.GetValue<MenuBool>("jungleQ").Enabled)
                    {
                        Q.Cast(mob.Position);
                        return;
                    }
                    if (E.IsReady() && FarmMenu.GetValue<MenuBool>("jungleE").Enabled)
                    {
                        E.Cast(mob.Position);
                        return;
                    }
                }
            }
        }
        
        private void KsJungle()
        {
            var mobs = Cache.GetMinions(Player.Position, R.Range, MinionManager.MinionTeam.Neutral);
            foreach (var mob in mobs)
            {
                //debug(mob.BaseSkinName);
                if (((mob.Name == "SRU_Dragon" && Local["rJungleStealer"].GetValue<MenuBool>("Rdragon").Enabled)
                    || (mob.Name == "SRU_Baron" && Local["rJungleStealer"].GetValue<MenuBool>("Rbaron").Enabled)
                    || (mob.Name == "SRU_Red" && Local["rJungleStealer"].GetValue<MenuBool>("Rred").Enabled)
                    || (mob.Name == "SRU_Blue" && Local["rJungleStealer"].GetValue<MenuBool>("Rblue").Enabled))
                    && (mob.CountAllyHeroesInRange(1000) == 0 || Local["rJungleStealer"].GetValue<MenuBool>("Rally").Enabled)
                    && mob.Health < mob.MaxHealth
                    && mob.Distance(Player.Position) > 1000
                    )
                {
                    if (DragonDmg == 0)
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
                        //Program.debug("DS  " + DmgSec);
                        if (DragonDmg - mob.Health > 0)
                        {
                            var timeTravel = R.Delay;
                            var timeR = (mob.Health - R.GetDamage(mob)) / (DmgSec / 3);
                            //Program.debug("timeTravel " + timeTravel + "timeR " + timeR + "d " + R.GetDamage(mob));
                            if (timeTravel > timeR)
                                R.Cast(mob.Position);
                        }
                        else
                            DragonDmg = mob.Health;

                        //Program.debug("" + GetUltTravelTime(ObjectManager.Player, R.Speed, R.Delay, mob.Position));
                    }
                }
            }
        }
        
        private bool HardCC(AIHeroClient target)
        {
            
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned)
            {
                return true;

            }
            else
                return false;
        }
        
        private void SetMana()
        {
            if ((Config.GetValue<MenuBool>("manaDisable").Enabled && Program.Combo) || Player.HealthPercent < 20)
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
        
        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (Local.GetValue<MenuBool>("rRangeMini").Enabled)
            {
                if (Local.GetValue<MenuBool>("onlyRdy").Enabled)
                {
                    if(R.IsReady())
                        DrawCircle(Player.Position, R.Range, Color.Aqua, 1, 20, true);
                }
                else
                    DrawCircle(Player.Position, R.Range, Color.Aqua, 1, 20, true);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            
            if (Local.GetValue<MenuBool>("qRange").Enabled)
            {
                if (Local.GetValue<MenuBool>("onlyRdy").Enabled)
                {
                    if (Q.IsReady())
                        DrawCircle(Player.Position, Q.Range, Color.Cyan, 1, 1);
                }
                else
                    DrawCircle(Player.Position, Q.Range, Color.Cyan, 1, 1);
            }
            if (Local.GetValue<MenuBool>("wRange").Enabled)
            {
                if (Local.GetValue<MenuBool>("onlyRdy").Enabled)
                {
                    if (W.IsReady())
                        DrawCircle(ObjectManager.Player.Position, W.Range, Color.Orange, 1, 1);
                }
                else
                    DrawCircle(ObjectManager.Player.Position, W.Range, Color.Orange, 1, 1);
            }
            if (Local.GetValue<MenuBool>("eRange").Enabled)
            {
                if (Local.GetValue<MenuBool>("onlyRdy").Enabled)
                {
                    if (E.IsReady())
                        DrawCircle(Player.Position, E.Range, Color.Yellow, 1, 1);
                }
                else
                    DrawCircle(Player.Position, E.Range, Color.Yellow, 1, 1);
            }
            if (Local.GetValue<MenuBool>("rRange").Enabled)
            {
                if (Local.GetValue<MenuBool>("onlyRdy").Enabled)
                {
                    if (R.IsReady())
                        DrawCircle(Player.Position, R.Range, Color.Gray, 1, 1);
                }
                else
                    DrawCircle(Player.Position, R.Range, Color.Gray, 1, 1);
            }
            if (R.IsReady() && Local.GetValue<MenuBool>("noti").Enabled)
            {
                var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);

                if ( t.IsValidTarget() && R.GetDamage(t) > t.Health)
                {
                    Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.CharacterName + " have: " + t.Health + "hp");
                    drawLine(t.Position, Player.Position, 5, System.Drawing.Color.Red);
                }
            }
        }
    }
}