using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SebbyLib;
using SharpDX;
using SPrediction;
using HitChance = EnsoulSharp.SDK.HitChance;
using PredictionOutput = SebbyLib.PredictionOutput;

namespace Jinx_Genesis
{
    class Program
    {
        private static string ChampionName = "Jinx";
        
        public static Menu Config;

        private static AIHeroClient Player { get { return ObjectManager.Player; } }

        private static Spell Q, W, E, R;
        private static float QMANA, WMANA, EMANA ,RMANA;
        private static bool FishBoneActive= false, Combo = false, Farm = false;
        private static AIHeroClient blitz = null;
        private static float WCastTime = Game.Time;

        private static string[] Spells =
        {
            "katarinar","drain","consume","absolutezero", "staticfield","reapthewhirlwind","jinxw","jinxr","shenstandunited","threshe","threshrpenta","threshq","meditate","caitlynpiltoverpeacemaker", "volibearqattack",
            "cassiopeiapetrifyinggaze","ezrealtrueshotbarrage","galioidolofdurand","luxmalicecannon", "missfortunebullettime","infiniteduress","alzaharnethergrasp","lucianq","velkozr","rocketgrabmissile"
        };

        private static List<AIHeroClient> Enemies = new List<AIHeroClient>();

        public static void Game_OnGameLoad()
        {
            if (Player.CharacterName != ChampionName) return;

            LoadMenu();
            Q = new Spell(SpellSlot.Q, Player.AttackRange);
            W = new Spell(SpellSlot.W, 1490f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 2500f);

            W.SetSkillshot(0.6f, 75f, 3300f, true, SpellType.Line);
            E.SetSkillshot(1.2f, 1f, 1750f, false, SpellType.Circle);
            R.SetSkillshot(0.7f, 140f, 1500f, false, SpellType.Line);

            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {
                if (hero.IsEnemy)
                {
                    Enemies.Add(hero);
                }
                else if(hero.CharacterName.Equals("Blitzcrank"))
                {
                    blitz = hero;
                }
            }
            
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalker.OnBeforeAttack += BeforeAttack;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.Print("<font color=\"#00BFFF\">GENESIS </font>Jinx<font color=\"#000000\"> by Sebby </font> - <font color=\"#FFFFFF\">Loaded</font>");
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (Player.ManaPercent < Config.GetValue<MenuSlider>("EmanaCombo").Value)
                return;

            if (E.IsReady())
            {
                var t = sender;
                if (t.IsValidTarget(E.Range) && eGapCloser.GetValue<MenuBool>("EGCchampion" + t.CharacterName).Enabled)
                {
                    if(eGapCloser.GetValue<MenuList>("EmodeGC").Index == 0)
                        E.Cast(args.EndPosition);
                    else
                        E.Cast(Player.Position);
                }
            }
        }

        private static void BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (!FishBoneActive)
                return;

            if (Q.IsReady() && args.Target is AIHeroClient && Config.GetValue<MenuList>("Qchange").Index == 1)
            {
                var t = (AIHeroClient)args.Target;
                if ( t.IsValidTarget())
                {
                    FishBoneToMiniGun(t);
                }
            }

            if (!Combo && args.Target is AIMinionClient)
            {
                var t = (AIMinionClient)args.Target;
                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && Player.ManaPercent > Config.GetValue<MenuSlider>("QmanaLC").Value && CountMinionsInRange(250, t.Position) >= Config.GetValue<MenuSlider>("Qlaneclear").Value)
                {
                    
                }
                else if (GetRealDistance(t) < GetRealPowPowRange(t))
                {
                    args.Process = false;
                    if (Q.IsReady())
                        Q.Cast();
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsMinion())
                    return;

                if (sender.IsMe)
                {
                    if (args.SData.Name == "JinxWMissile")
                        WCastTime = Game.Time;
                }

                if (!E.IsReady() || !sender.IsEnemy || !Config.GetValue<MenuBool>("Espell").Enabled ||
                    Player.ManaPercent < Config.GetValue<MenuSlider>("EmanaCombo").Value || !
                        ((AIHeroClient) sender).IsValid || !sender.IsValidTarget(E.Range))
                    return;

                var foundSpell = Spells.Find(x => args.SData.Name.ToLower() == x);
                if (foundSpell != null)
                {
                    E.Cast(sender.Position);
                }
            }catch(Exception){}
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            SetValues();

            if (Q.IsReady())
                Qlogic();
            if (W.IsReady())
                Wlogic();
            if (E.IsReady())
                Elogic();
            if (R.IsReady())
                Rlogic();
        }
        
        private static void Rlogic()
        {
            R.Range = Config.GetValue<MenuSlider>("RcustomeMax").Value;

            if (semiR.GetValue<MenuKeyBind>("useR").Active)
            {
                var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if(semiR.GetValue<MenuList>("semiMode").Index == 0)
                    {
                        R.Cast(t);
                    }
                    else
                    {
                        R.CastIfWillHit(t, 2);
                        R.Cast(t, true, true);
                    }
                }   
            }

            if (Config.GetValue<MenuBool>("Rks").Enabled)
            {
                bool cast = false;
                

                if (overkillProtection.GetValue<MenuBool>("RoverAA").Enabled && (!Orbwalker.CanAttack() || Player.Spellbook.IsAutoAttack))
                    return;

                foreach (var target in Enemies.Where(target => target.IsValidTarget(R.Range) && OktwCommon.ValidUlt(target) ))
                {
                    
                    float predictedHealth = target.Health + target.HPRegenRate * 2;

                    var Rdmg = R.GetDamage(target, 1);
                    if(Player.Distance(target.Position) < 1500)
                    {

                        Rdmg = Rdmg * (Player.Distance(target.Position) / 1500);
                       
                    }

                    if (Rdmg > predictedHealth)
                    {
                        cast = true;
                        EnsoulSharp.SDK.PredictionOutput output = R.GetPrediction(target);
                        Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
                        direction.Normalize();

                        foreach (var enemy in Enemies.Where(enemy => enemy.IsValidTarget()))
                        {
                            if (enemy.NetworkId == target.NetworkId || !cast)
                                continue;
                            EnsoulSharp.SDK.PredictionOutput prediction = R.GetPrediction(enemy);
                            Vector3 predictedPosition = prediction.CastPosition;
                            Vector3 v = output.CastPosition - Player.Position;
                            Vector3 w = predictedPosition - Player.Position;
                            double c1 = Vector3.Dot(w, v);
                            double c2 = Vector3.Dot(v, v);
                            double b = c1 / c2;
                            Vector3 pb = Player.Position + ((float)b * v);
                            float length = Vector3.Distance(predictedPosition, pb);
                            if (length < (R.Width + 150 + enemy.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.Position))
                                cast = false;
                        }

                        if (cast)
                        {
                            if (overkillProtection.GetValue<MenuBool>("RoverW").Enabled && target.IsValidTarget(W.Range) && W.GetDamage(target) > target.Health && W.Instance.Cooldown - (W.Instance.CooldownExpires - Game.Time) < 1.1)
                                return;

                            if (target.CountEnemyHeroesInRange(400) > Config.GetValue<MenuSlider>("Raoe").Value)
                                CastSpell(R, target);

                            if (RValidRange(target) && target.CountAllyHeroesInRange(overkillProtection.GetValue<MenuSlider>("Rover").Value) == 0)
                                CastSpell(R, target);
                        }
                    }
                }
            }
        }
        
        private static bool RValidRange(AIBaseClient t)
        {
            var range = GetRealDistance(t);

            if (Config.GetValue<MenuList>("Rmode").Index == 0)
            {
                if (range > GetRealPowPowRange(t))
                    return true;
                else
                    return false;

            }
            else if (Config.GetValue<MenuList>("Rmode").Index == 1)
            {
                if (range > Q.Range)
                    return true;
                else
                    return false;
            }
            else if (Config.GetValue<MenuList>("Rmode").Index == 2)
            {
                if (range > Config.GetValue<MenuSlider>("Rcustome").Value && !Player.InAutoAttackRange(t))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        
        private static bool WValidRange(AIBaseClient t)
        {
            var range = GetRealDistance(t);

            if (Config.GetValue<MenuList>("Wmode").Index == 0)
            {
                if (range > GetRealPowPowRange(t) && Player.CountEnemyHeroesInRange(GetRealPowPowRange(t)) == 0)
                    return true;
                else
                    return false;

            }
            else if (Config.GetValue<MenuList>("Wmode").Index == 1)
            {
                if (range > Q.Range + 50 && Player.CountEnemyHeroesInRange(Q.Range + 50) == 0)
                    return true;
                else
                    return false;
            }
            else if (Config.GetValue<MenuList>("Wmode").Index == 2)
            {
                if(range > Config.GetValue<MenuSlider>("Wcustome").Value && Player.CountEnemyHeroesInRange(Config.GetValue<MenuSlider>("Wcustome").Value) == 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        
        private static void Elogic()
        {
            if (Player.ManaPercent < Config.GetValue<MenuSlider>("EmanaCombo").Value)
                return;

            if (blitz != null && blitz.Distance(Player.Position) < E.Range)
            {
                foreach (var enemy in Enemies.Where(enemy => enemy.IsValidTarget(2000) && enemy.HasBuff("RocketGrab")))
                {
                    E.Cast(blitz.Position.Extend(enemy.Position, 30));
                    return;
                }
            }

            foreach (var enemy in Enemies.Where(enemy => enemy.IsValidTarget(E.Range) ))
            {

                E.CastIfWillHit(enemy, Config.GetValue<MenuSlider>("Eaoe").Value);

                if(Config.GetValue<MenuBool>("Ecc").Enabled)
                {
                    if (!OktwCommon.CanMove(enemy))
                        E.Cast(enemy.Position);
                    E.CastIfHitchanceEquals(enemy, HitChance.Immobile);
                }

                if(enemy.MoveSpeed < 250 && Config.GetValue<MenuBool>("Eslow").Enabled)
                    E.Cast(enemy);
                if (Config.GetValue<MenuBool>("Edash").Enabled)
                    E.CastIfHitchanceEquals(enemy, HitChance.Dash);
            }
            

            if (Config.GetValue<MenuBool>("Etel").Enabled)
            {
                foreach (var Object in ObjectManager.Get<AIBaseClient>().Where(Obj => Obj.IsEnemy && Obj.Distance(Player.Position) < E.Range && (Obj.HasBuff("teleport_target") || Obj.HasBuff("Pantheon_GrandSkyfall_Jump"))))
                {
                    E.Cast(Object.Position);
                }
            }

            if (Combo && Player.IsMoving && Config.GetValue<MenuBool>("Ecombo").Enabled)
            {
                var t = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (t.IsValidTarget(E.Range) && E.GetPrediction(t).CastPosition.Distance(t.Position) > 200)
                {
                    if (Player.Position.Distance(t.Position) > Player.Position.Distance(t.Position))
                    {
                        if (t.Position.Distance(Player.Position) < t.Position.Distance(Player.Position))
                            CastSpell(E, t);
                    }
                    else
                    {
                        if (t.Position.Distance(Player.Position) > t.Position.Distance(Player.Position))
                            CastSpell(E, t);
                    }
                }
            }
        }
        private static void Wlogic()
        {
            var t = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (t.IsValidTarget() && WValidRange(t))
            {
                if (Config.GetValue<MenuBool>("Wks").Enabled && GetKsDamage(t, W) > t.Health && OktwCommon.ValidUlt(t))
                {
                    CastSpell(W, t);
                }

                if (Combo && Config.GetValue<MenuBool>("Wcombo").Enabled && Player.ManaPercent > Config.GetValue<MenuSlider>("WmanaCombo").Value)
                {
                    CastSpell(W, t);
                }
                else if (Farm && Orbwalker.CanAttack() && !Player.Spellbook.IsAutoAttack && Config.GetValue<MenuBool>("Wharass").Enabled && Player.ManaPercent > Config.GetValue<MenuSlider>("WmanaHarass").Value)
                {
                    if (Config.GetValue<MenuList>("Wts").Index == 0)
                    {
                        if (harassWEnemy.GetValue<MenuBool>("haras" + t.CharacterName).Enabled)
                            CastSpell(W, t);
                    }
                    else
                    {
                        foreach (var enemy in Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && WValidRange(t) && Config.GetValue<MenuBool>("haras" + enemy.CharacterName).Enabled))
                            CastSpell(W, enemy);
                    }
                }
                
            }
        }
        
        private static void Qlogic()
        {
            if (FishBoneActive)
            {
                var orbT = Orbwalker.GetTarget();
                if(orbT == null) return;
                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && Player.ManaPercent > Config.GetValue<MenuSlider>("QmanaLC").Value && ((AIMinionClient) orbT).IsValid)
                {
                    
                }
                else if (Config.GetValue<MenuList>("Qchange").Index == 0 && ((AIHeroClient) orbT).IsValid)
                {
                    var t = (AIHeroClient)Orbwalker.GetTarget();
                    FishBoneToMiniGun(t);
                }  
                else
                {
                    if (!Combo && Orbwalker.ActiveMode != OrbwalkerMode.None)
                        Q.Cast();
                }
            }
            else
            {
                var t = TargetSelector.GetTarget(Q.Range + 40, DamageType.Physical);
                if(t == null) return;
                if (t.IsValidTarget())
                {
                    if ((!Player.InAutoAttackRange(t) || t.CountEnemyHeroesInRange(250) >= Config.GetValue<MenuSlider>("Qaoe").Value))
                    {
                        if (Combo && Config.GetValue<MenuBool>("Qcombo").Enabled && (Player.ManaPercent > Config.GetValue<MenuSlider>("QmanaCombo").Value || Player.GetAutoAttackDamage(t) * Config.GetValue<MenuSlider>("QmanaIgnore").Value > t.Health))
                        {
                            Q.Cast();
                        }
                        if (Farm && Orbwalker.CanAttack() && !Player.Spellbook.IsAutoAttack && harassQEnemy.GetValue<MenuBool>("harasQ" + t.CharacterName).Enabled && Config.GetValue<MenuBool>("Qharass").Enabled && (Player.ManaPercent > Config.GetValue<MenuSlider>("QmanaHarass").Value || Player.GetAutoAttackDamage(t) * Config.GetValue<MenuSlider>("QmanaIgnore").Value > t.Health))
                        {
                            Q.Cast();
                        }
                    }
                }
                else
                {
                    if (Combo && Player.ManaPercent > Config.GetValue<MenuSlider>("QmanaCombo").Value)
                    {
                        Q.Cast();
                    }
                    else if (Farm && !Player.Spellbook.IsAutoAttack && Config.GetValue<MenuBool>("farmQout").Enabled && Orbwalker.CanAttack())
                    {
                        foreach (var minion in MinionManager.GetMinions(Q.Range + 30).Where(
                        minion => !Player.InAutoAttackRange(minion) && minion.Health < Player.GetAutoAttackDamage(minion) * 1.2 && GetRealPowPowRange(minion) < GetRealDistance(minion) && Q.Range < GetRealDistance(minion)))
                        {
                            Orbwalker.ForceTarget = minion;
                            Q.Cast();
                            return;
                        }
                    }
                    if(Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && Player.ManaPercent > Config.GetValue<MenuSlider>("QmanaLC").Value)
                    {
                        var orbT = Orbwalker.GetTarget();
                        if(orbT == null) return;
                        if (((AIMinionClient) orbT).IsValid && CountMinionsInRange(250, orbT.Position) >= Config.GetValue<MenuSlider>("Qlaneclear").Value)
                        {
                            Q.Cast();
                        }
                    }
                }
            }
        }

        private static int CountMinionsInRange(float range, Vector3 pos)
        {
            var minions = MinionManager.GetMinions(pos, range);
            int count = 0;
            foreach (var minion in minions)
            {
                count++;
            }
            return count;
        }
        
        public static float GetKsDamage(AIBaseClient t, Spell QWER)
        {
            var totalDmg = QWER.GetDamage(t);

            if (Player.HasBuff("summonerexhaust"))
                totalDmg = totalDmg * 0.6f;

            if (t.HasBuff("ferocioushowl"))
                totalDmg = totalDmg * 0.7f;

            if (t is AIHeroClient)
            {
                var champion = (AIHeroClient)t;
                if (champion.CharacterName == "Blitzcrank" && !champion.HasBuff("BlitzcrankManaBarrierCD") && !champion.HasBuff("ManaBarrier"))
                {
                    totalDmg -= champion.Mana / 2f;
                }
            }

            var extraHP = t.Health - SebbyLib.HealthPrediction.GetHealthPrediction(t, 500);

            totalDmg += extraHP;
            totalDmg -= t.HPRegenRate;
            totalDmg -= t.PercentLifeStealMod * 0.005f * t.FlatPhysicalDamageMod;

            return totalDmg;
        }
        
        private static void CastSpell(Spell QWER, AIBaseClient target)
        {
            if (Config.GetValue<MenuList>("PredictionMODE").Index == 0)
            {
                if (QWER.Slot == SpellSlot.W)
                {
                    if (Config.GetValue<MenuList>("Wpred").Index == 0)
                        QWER.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    else
                        QWER.Cast(target);
                }
                if (QWER.Slot == SpellSlot.R)
                {
                    if (Config.GetValue<MenuList>("Rpred").Index == 0)
                        QWER.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    else
                        QWER.Cast(target);
                }
                if (QWER.Slot == SpellSlot.E)
                {
                    if (Config.GetValue<MenuList>("Epred").Index == 0)
                        QWER.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    else
                        QWER.Cast(target);
                }
            }
            else
            {
                SebbyLib.SkillshotType CoreType2 = SebbyLib.SkillshotType.SkillshotLine;
                bool aoe2 = false;

                if (QWER.Type == (SpellType) SkillshotType.SkillshotCircle)
                {
                    CoreType2 = SebbyLib.SkillshotType.SkillshotCircle;
                    aoe2 = true;
                }

                var predInput2 = new SebbyLib.PredictionInput
                {
                    Aoe = aoe2,
                    Collision = QWER.Collision,
                    Speed = QWER.Speed,
                    Delay = QWER.Delay,
                    Range = QWER.Range,
                    From = Player.Position,
                    Radius = QWER.Width,
                    Unit = target,
                    Type = CoreType2
                };

                var poutput2 = SebbyLib.Prediction.GetPrediction(predInput2);

                if (QWER.Slot == SpellSlot.W)
                {
                    if (Config.GetValue<MenuList>("Wpred").Index == 0)
                    {
                        if (poutput2.Hitchance >= SebbyLib.HitChance.VeryHigh)
                            QWER.Cast(poutput2.CastPosition);
                    }
                    else
                    {
                        if (poutput2.Hitchance >= SebbyLib.HitChance.High)
                            QWER.Cast(poutput2.CastPosition);
                    }
                }
                if (QWER.Slot == SpellSlot.R)
                {
                    if (Config.GetValue<MenuList>("Rpred").Index == 0)
                    {
                        if (poutput2.Hitchance >= SebbyLib.HitChance.VeryHigh)
                            QWER.Cast(poutput2.CastPosition);
                    }
                    else
                    {
                        if (poutput2.Hitchance >= SebbyLib.HitChance.High)
                            QWER.Cast(poutput2.CastPosition);
                    }
                }
                if (QWER.Slot == SpellSlot.E)
                {
                    if (Config.GetValue<MenuList>("Epred").Index == 0)
                    {
                        if (poutput2.Hitchance >= SebbyLib.HitChance.VeryHigh)
                            QWER.Cast(poutput2.CastPosition);
                    }
                    else
                    {
                        if (poutput2.Hitchance >= SebbyLib.HitChance.High)
                            QWER.Cast(poutput2.CastPosition);
                    }
                }
            }
        }
        
        private static void FishBoneToMiniGun(AIBaseClient t)
        {
            var realDistance = GetRealDistance(t);

            if(realDistance < GetRealPowPowRange(t) && t.CountEnemyHeroesInRange(250) < Config.GetValue<MenuSlider>("Qaoe").Value)
            {
                if (Player.ManaPercent < Config.GetValue<MenuSlider>("QmanaCombo").Value || Player.GetAutoAttackDamage(t) * Config.GetValue<MenuSlider>("QmanaIgnore").Value < t.Health)
                    Q.Cast();

            }
        }

        private static float GetRealDistance(AIBaseClient target) { return Player.Position.Distance(target.Position) + Player.BoundingRadius + target.BoundingRadius; }

        private static float GetRealPowPowRange(GameObject target) { return 650f + Player.BoundingRadius + target.BoundingRadius; }
        
        private static void SetValues()
        {
            if (Config.GetValue<MenuList>("Wmode").Index == 2)
                Config.GetValue<MenuSlider>("Wcustome").Visible = true;
            else
                Config.GetValue<MenuSlider>("Wcustome").Visible = false;

            if (Config.GetValue<MenuList>("Rmode").Index == 2)
                Config.GetValue<MenuSlider>("Rcustome").Visible = true;
            else
                Config.GetValue<MenuSlider>("Rcustome").Visible = false;


            if (Player.HasBuff("JinxQ"))
                FishBoneActive = true;
            else
                FishBoneActive = false;

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                Combo = true;
            else
                Combo = false;

            if (
                (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && Config.GetValue<MenuBool>("LaneClearHarass").Enabled) ||
                (Orbwalker.ActiveMode == OrbwalkerMode.LastHit && Config.GetValue<MenuBool>("LaneClearHarass").Enabled) || 
                (Orbwalker.ActiveMode == OrbwalkerMode.Harass && Config.GetValue<MenuBool>("MixedHarass").Enabled)
            )
                Farm = true;
            else
                Farm = false;

            Q.Range = 685f + Player.BoundingRadius + 25f * Player.Spellbook.GetSpell(SpellSlot.Q).Level;

            QMANA = 10f;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            RMANA = R.Instance.ManaCost;
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.GetValue<MenuBool>("qRange").Enabled)
            {
                if (!FishBoneActive)
                    CircleRender.Draw(Player.Position, 590f + Player.BoundingRadius, Color.Gray, 1);
                else
                    CircleRender.Draw(Player.Position, Q.Range - 40, Color.Gray, 1);
            }
            if (Config.GetValue<MenuBool>("wRange").Enabled)
            {
                if (Config.GetValue<MenuBool>("onlyRdy").Enabled)
                {
                    if (W.IsReady())
                        CircleRender.Draw(Player.Position, W.Range, Color.Gray, 1);
                }
                else
                    CircleRender.Draw(Player.Position, W.Range, Color.Gray, 1);
            }
            if (Config.GetValue<MenuBool>("eRange").Enabled)
            {
                if (Config.GetValue<MenuBool>("onlyRdy").Enabled)
                {
                    if (E.IsReady())
                        CircleRender.Draw(Player.Position, E.Range, Color.Gray, 1);
                }
                else
                    CircleRender.Draw(Player.Position, E.Range, Color.Gray, 1);
            }
        }

        private static Menu harassQEnemy, harassWEnemy, eGapCloser, castOnEnemy, semiR, overkillProtection;

        private static void LoadMenu()
        {
            Config = new Menu(ChampionName + " GENESIS", ChampionName + " GENESIS", true);
            Config.Attach();

            var draw = Config.Add(new Menu("draw", "Draw"));

            draw.Add(new MenuBool("qRange", "Q range").SetValue(false));
            draw.Add(new MenuBool("wRange", "W range").SetValue(false));
            draw.Add(new MenuBool("eRange", "E range").SetValue(false));
            draw.Add(new MenuBool("rRange", "R range").SetValue(false));
            draw.Add(new MenuBool("onlyRdy", "Draw only ready spells").SetValue(true));

            var qConfig = Config.Add(new Menu("qConfig", "Q Config"));
            qConfig.Add(new MenuBool("Qcombo", "Combo Q").SetValue(true));
            qConfig.Add(new MenuBool("Qharass", "Harass Q").SetValue(true));
            qConfig.Add(new MenuBool("farmQout", "Farm Q out range AA minion").SetValue(true));
            qConfig.Add(new MenuSlider("Qlaneclear", "Lane clear x minions",4, 2,10));
            qConfig.Add(new MenuList("Qchange", "Q change mode FishBone -> MiniGun",new[] { "Real Time", "Before AA"}, 1));
            qConfig.Add(new MenuSlider("Qaoe", "Force FishBone if can hit x target",3, 0,5));
            qConfig.Add(new MenuSlider("QmanaIgnore", "Ignore mana if can kill in x AA",4, 0,10));
            harassQEnemy = qConfig.Add(new Menu("harassQEnemy", "Harass Q enemy:"));
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy))
                harassQEnemy.Add(new MenuBool("harasQ" + enemy.CharacterName, enemy.CharacterName).SetValue(true));
            
            var wConfig = Config.Add(new Menu("wConfig", "W Config"));
            wConfig.Add(new MenuBool("Wcombo", "Combo W").SetValue(true));
            wConfig.Add(new MenuBool("Wharass", "W harass").SetValue(true));
            wConfig.Add(new MenuBool("Wks", "W KS").SetValue(true));
            wConfig.Add(new MenuList("Wts", "Harass mode",new[] { "Target selector", "All in range" }, 0));
            wConfig.Add(new MenuList("Wmode", "W mode",new[] { "Out range MiniGun", "Out range FishBone", "Custome range" }, 0));
            wConfig.Add(new MenuSlider("Wcustome", "Custome minimum range",600, 0,1500));
            harassWEnemy = wConfig.Add(new Menu("harassWEnemy", "Harass W enemy:"));
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy))
                harassWEnemy.Add(new MenuBool("haras" + enemy.CharacterName, enemy.CharacterName).SetValue(true));

            var eConfig = Config.Add(new Menu("eConfig", "E Config"));
            eConfig.Add(new MenuBool("Ecombo", "Combo E").SetValue(true));
            eConfig.Add(new MenuBool("Etel", "E on enemy teleport").SetValue(true));
            eConfig.Add(new MenuBool("Ecc", "E on CC").SetValue(true));
            eConfig.Add(new MenuBool("Eslow", "E on slow").SetValue(true));
            eConfig.Add(new MenuBool("Edash", "E on dash").SetValue(true));
            eConfig.Add(new MenuBool("Espell", "E on special spell detection").SetValue(true));
            eConfig.Add(new MenuSlider("Eaoe", "E if can catch x enemies",3, 0,5));
            eGapCloser = eConfig.Add(new Menu("eGapCloser", "E Gap Closer"));
            eGapCloser.Add(new MenuList("EmodeGC", "Gap Closer position mode",new[] { "Dash end position", "Jinx position"}, 0));
            castOnEnemy = eGapCloser.Add(new Menu("castOnEnemy", "Cast on enemy:"));
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy))
                castOnEnemy.Add(new MenuBool("EGCchampion" + enemy.CharacterName, enemy.CharacterName).SetValue(true));

            var rConfig = Config.Add(new Menu("rConfig", "R Config"));
            rConfig.Add(new MenuBool("Rks", "R KS").SetValue(true));
            semiR = rConfig.Add(new Menu("semiR", "Semi-manual cast R"));
            semiR.Add(new MenuKeyBind("useR", "Semi-manual cast R key",Keys.T, KeyBindType.Press)); //32 == space
            semiR.Add(new MenuList("semiMode", "Semi-manual cast mode",new[] { "Low hp target", "AOE"}, 0));
            rConfig.Add(new MenuList("Rmode", "R mode",new[] { "Out range MiniGun ", "Out range FishBone ", "Custome range " }, 0));
            rConfig.Add(new MenuSlider("Rcustome", "Custome minimum range",1000, 0,1600));
            rConfig.Add(new MenuSlider("RcustomeMax", "Max range",3000, 0,10000));
            rConfig.Add(new MenuSlider("Raoe", "R if can hit x target and can kill",2, 0,5));
            overkillProtection = rConfig.Add(new Menu("overkillProtection", "OverKill protection"));
            overkillProtection.Add(new MenuSlider("Rover", "Don't R if allies near target in x range ",500, 0,1000));
            overkillProtection.Add(new MenuBool("RoverAA", "Don't R if Jinx winding up").SetValue(true));
            overkillProtection.Add(new MenuBool("RoverW", "Don't R if can W KS").SetValue(true));

            //Config.SubMenu("MISC").SubMenu("Use harass mode").AddItem(new MenuItem("LaneClearmode", "LaneClear").SetValue(true));
            //Config.SubMenu("MISC").SubMenu("Use harass mode").AddItem(new MenuItem("Mixedmode", "Mixed").SetValue(true));
            //Config.SubMenu("MISC").SubMenu("Use harass mode").AddItem(new MenuItem("LastHitmode", "LastHit").SetValue(true));

            //Config.SubMenu("Mana Manager").AddItem(new MenuItem("ManaKs", "always safe mana to KS R or W").SetValue(true));
            var manaManager = Config.Add(new Menu("manaManager", "Mana Manager"));
            manaManager.Add(new MenuSlider("QmanaCombo", "Q combo mana",20, 0,100));
            manaManager.Add(new MenuSlider("QmanaHarass", "Q harass mana",40, 0,100));
            manaManager.Add(new MenuSlider("QmanaLC", "Q lane clear mana",80, 0,100));
            manaManager.Add(new MenuSlider("WmanaCombo", "W combo mana",20, 0,100));
            manaManager.Add(new MenuSlider("WmanaHarass", "W harass mana",40, 0,100));
            manaManager.Add(new MenuSlider("EmanaCombo", "E mana",20, 0,100));

            var predictionConfig = Config.Add(new Menu("predictionConfig", "Prediction Config"));
            predictionConfig.Add(new MenuList("PredictionMODE", "Prediction MODE",new[] { "Common prediction", "OKTW© PREDICTION"}, 1));
            predictionConfig.Add(new MenuList("Wpred", "W Hit Chance",new[] {"VeryHigh W", "High W"}, 0));
            predictionConfig.Add(new MenuList("Epred", "E Hit Chance",new[] { "VeryHigh E", "High E" }, 0));
            predictionConfig.Add(new MenuList("Rpred", "R Hit Chance",new[] { "VeryHigh R", "High R" }, 0));

            var harassKeyConfig = Config.Add(new Menu("harassKeyConfig", "Harass key config"));
            harassKeyConfig.Add(new MenuBool("LaneClearHarass", "LaneClear Harass").SetValue(true));
            harassKeyConfig.Add(new MenuBool("LastHitHarass", "LastHit Harass").SetValue(true));
            harassKeyConfig.Add(new MenuBool("MixedHarass", "Mixed Harass").SetValue(true));

            //Config.Item("Qchange").GetValue<StringList>().SelectedIndex == 1
            //Config.Item("haras" + enemy.ChampionName).GetValue<bool>()
            //Config.Item("QmanaCombo").GetValue<Slider>().Value
        }
    }
}