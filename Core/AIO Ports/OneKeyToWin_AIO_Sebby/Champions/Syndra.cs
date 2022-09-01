using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using SebbyLib;
using SharpDX;
using SPrediction;
using HitChance = EnsoulSharp.SDK.HitChance;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Syndra : Base
    {
        private Spell EQ, Eany;
        private static List<AIMinionClient> BallsList = new List<AIMinionClient>();
        private bool EQcastNow = false;

        public Syndra()
        {
            Q = new Spell(SpellSlot.Q, 790);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 700);
            EQ = new Spell(SpellSlot.Q, Q.Range + 500);
            Eany = new Spell(SpellSlot.Q, Q.Range + 500);
            R = new Spell(SpellSlot.R, 675);

            Q.SetSkillshot(0.6f, 125f, float.MaxValue, false, SpellType.Circle);
            W.SetSkillshot(0.25f, 140f, 1600f, false, SpellType.Circle);
            E.SetSkillshot(0.25f, 100, 2500f, false, SpellType.Line);
            EQ.SetSkillshot(0.6f, 100f, 2500f, false, SpellType.Line);
            Eany.SetSkillshot(0.30f, 50f, 2500f, false, SpellType.Line);

            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            drawMenu.Add(new MenuBool("wRange", "W range", true).SetValue(false));
            drawMenu.Add(new MenuBool("eRange", "E range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRange", "R range", true).SetValue(false));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw when skill rdy", true).SetValue(true));

            var qConfigMenu = Local.Add(new Menu("qConfig", "Q Config"));
            qConfigMenu.Add(new MenuBool("autoQ", "Auto Q", true).SetValue(true));
            qConfigMenu.Add(new MenuBool("harassQ", "Harass Q", true).SetValue(true));
            qConfigMenu.Add(new MenuSlider("QHarassMana", "Harass Mana",30));
            
            var wconfigMenu = Local.Add(new Menu("wconfig", "W Config"));
            wconfigMenu.Add(new MenuBool("autoW", "Auto W", true).SetValue(true));
            wconfigMenu.Add(new MenuBool("harassW", "Harass W", true).SetValue(true));

            var econfigMenu = Local.Add(new Menu("econfig", "E Config"));
            econfigMenu.Add(new MenuBool("autoE", "Auto Q + E combo, ks", true).SetValue(true));
            econfigMenu.Add(new MenuBool("harassE", "Harass Q + E", true).SetValue(false));
            econfigMenu.Add(new MenuBool("EInterrupter", "Auto Q + E Interrupter", true).SetValue(true));
            econfigMenu.Add(new MenuKeyBind("useQE", "Semi-manual Q + E near mouse key", Keys.T, KeyBindType.Press)); //32 == space
            var autoQEGap = econfigMenu.Add(new Menu("autoQEGap", "Auto Q + E Gapcloser"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                autoQEGap.Add(new MenuBool("Egapcloser" + enemy.CharacterName, enemy.CharacterName, true).SetValue(true));
            var useQEon = econfigMenu.Add(new Menu("useQEon", "Use Q + E on"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                useQEon.Add(new MenuBool("Eon" + enemy.CharacterName, enemy.CharacterName, true).SetValue(true));

            var rconfigMenu = Local.Add(new Menu("rconfig", "R Config"));
            rconfigMenu.Add(new MenuBool("autoR", "Auto R KS", true).SetValue(true));
            rconfigMenu.Add(new MenuBool("Rcombo", "Extra combo dmg calculation", true).SetValue(true));
            var useOn = rconfigMenu.Add(new Menu("useOn", "Use on"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                useOn.Add(new MenuList("Rmode" + enemy.CharacterName, enemy.CharacterName,new[] { "KS ", "Always ", "Never " }, 0));
            
            FarmMenu.Add(new MenuBool("farmQout", "Last hit Q minion out range AA", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmQ", "Lane clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmW", "Lane clear W", true).SetValue(true));

            FarmMenu.Add(new MenuBool("jungleQ", "Jungle clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleW", "Jungle clear W", true).SetValue(true));
            
            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == SpellSlot.Q && EQcastNow && E.IsReady())
            {
                var customeDelay = Q.Delay - (E.Delay + ((Player.Distance(args.End)) / E.Speed));
                DelayAction.Add((int)(customeDelay * 1000), () => E.Cast(args.End));
            }
        }

        private void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (E.IsReady() && Local["EInterrupter"].GetValue<MenuBool>().Enabled)
            {
                if(sender.IsValidTarget(E.Range))
                {
                    E.Cast(sender.Position);
                }
                else if (Q.IsReady() && sender.IsValidTarget(EQ.Range))
                {
                    TryBallE(sender);
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (E.IsReady() && Local["Egapcloser" + sender.CharacterName].GetValue<MenuBool>().Enabled)
            {
                if (Q.IsReady())
                {
                    EQcastNow = true;
                    Q.Cast(sender);
                }
                else if(sender.IsValidTarget(E.Range))
                {
                    E.Cast(sender);
                }
            }
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly && sender.Type == GameObjectType.AIMinionClient && sender.Name == "Seed")
            {
                var ball = sender as AIMinionClient;
                BallsList.Add(ball);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!E.IsReady())
                EQcastNow = false;

            if (Program.LagFree(0))
            { 
                SetMana();
                BallCleaner();
                Jungle();
            }

            if (Program.LagFree(1) && E.IsReady() && Local["autoE"].GetValue<MenuBool>().Enabled)
                try{
                    LogicE();
                }catch(Exception){}

            if (Program.LagFree(2) && Q.IsReady() && Local["autoQ"].GetValue<MenuBool>().Enabled)
                try{
                    LogicQ();
                }catch(Exception){}

            if (Program.LagFree(3) && W.IsReady() && Local["autoW"].GetValue<MenuBool>().Enabled)
                try{
                    LogicW();
                }catch(Exception){}

            if (Program.LagFree(4) && R.IsReady() && Local["autoR"].GetValue<MenuBool>().Enabled)
                try{
                    LogicR();
                }catch(Exception){}
        }
        
        private void TryBallE(AIHeroClient t)
        {
            if (Q.IsReady())
            {
                CastQE(t);
            }
            if(!EQcastNow)
            {
                var ePred = Eany.GetPrediction(t);
                if (ePred.Hitchance >= HitChance.High)
                {
                    var playerToCP = Player.Distance(ePred.CastPosition);
                    foreach (var ball in BallsList.Where(ball => Player.Distance(ball.Position) < E.Range))
                    {
                        var ballFinalPos = Player.ServerPosition.Extend(ball.Position, playerToCP);
                        if (BallsList.Count == 0)
                        {
                            return;
                        }
                        if (ballFinalPos.Distance(ePred.CastPosition) < 50)
                            E.Cast(ball.Position);
                    }
                }
            }
        }
        
        private void LogicE()
        {
            if(Local["useQE"].GetValue<MenuKeyBind>().Active)
            {
                var mouseTarget = GameObjects.EnemyHeroes.Where(enemy => 
                    enemy.IsValidTarget(Eany.Range)).OrderBy(enemy => enemy.Distance(Game.CursorPos)).FirstOrDefault();

                if (mouseTarget != null)
                {
                    TryBallE(mouseTarget);
                    return;
                }
            }

            var t = TargetSelector.GetTarget(Eany.Range, DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (OktwCommon.GetKsDamage(t, E) + Q.GetDamage(t)> t.Health)
                    TryBallE(t);
                if (Program.Combo && Player.Mana > RMANA + EMANA + QMANA && Local["useQEon"]["Eon" + t.CharacterName].GetValue<MenuBool>().Enabled)
                    TryBallE(t);
                if (Program.Harass && Player.Mana > RMANA + EMANA + QMANA + WMANA && Local["harassE"].GetValue<MenuBool>().Enabled && Config["Harass" + t.CharacterName].GetValue<MenuBool>().Enabled)
                    TryBallE(t);
            }
        }
        
        private void LogicR()
        {
            R.Range = R.Level == 3 ? 750 : 675;

            bool Rcombo = Local["Rcombo"].GetValue<MenuBool>().Enabled;

            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(R.Range) && OktwCommon.ValidUlt(enemy)))
            {
                int Rmode = Local["useOn"]["Rmode" + enemy.CharacterName].GetValue<MenuList>().Index;

                if (Rmode == 2)
                    continue;
                else if (Rmode == 1)
                    R.Cast(enemy);

                var comboDMG = OktwCommon.GetKsDamage(enemy, R) ;
                comboDMG += R.GetDamage(enemy, 1) * (R.Instance.Ammo - 3);
                if (Rcombo)
                {
                    if (Q.IsReady() && enemy.IsValidTarget(600))
                        comboDMG += Q.GetDamage(enemy);

                    if (E.IsReady())
                        comboDMG += E.GetDamage(enemy);

                    if (W.IsReady())
                        comboDMG += W.GetDamage(enemy);
                }

                if (enemy.Health < comboDMG)
                {
                    R.Cast(enemy);
                }
            }
        }
        
        private void LogicW()
        {
            if (W.Instance.ToggleState == (SpellToggleState) 1)
            {
                var t = TargetSelector.GetTarget(W.Range - 150, DamageType.Magical);
                if (t.IsValidTarget())
                {
                    if (Program.Combo && Player.Mana > RMANA + QMANA + WMANA)
                        CatchW(t);
                    else if (Program.Harass && Local["harassW"].GetValue<MenuBool>().Enabled && Config["Harass" + t.CharacterName].GetValue<MenuBool>().Enabled 
                        && Player.ManaPercent > Local["QHarassMana"].GetValue<MenuSlider>().Value && OktwCommon.CanHarras())
                    {
                        CatchW(t);
                    }
                    else if (OktwCommon.GetKsDamage(t, W) > t.Health)
                        CatchW(t);
                    else if (Player.Mana > RMANA + WMANA)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                            CatchW(t);
                    }
                }
                else if (Program.LaneClear && !Q.IsReady() && FarmSpells && FarmMenu["farmW"].GetValue<MenuBool>().Enabled)
                {
                    var allMinions = Cache.GetMinions(Player.ServerPosition, W.Range);
                    var farmPos = W.GetCircularFarmLocation(allMinions, W.Width);

                    if (farmPos.MinionsHit >= FarmMinions)
                        CatchW(allMinions.FirstOrDefault());
                }
            }
            else
            {
                var t = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (t.IsValidTarget())
                {
                    Program.CastSpell(W, t);
                }
                else if (FarmSpells && FarmMenu["farmW"].GetValue<MenuBool>().Enabled)
                {
                    var allMinions = Cache.GetMinions(Player.ServerPosition, W.Range);
                    var farmPos = W.GetCircularFarmLocation(allMinions, W.Width);

                    if (farmPos.MinionsHit > 1)
                        W.Cast(farmPos.Position);
                }
            }
        }
        
        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Player.Mana > RMANA + QMANA + EMANA && !E.IsReady())
                    Program.CastSpell(Q, t);
                else if (Program.Harass && Local["harassQ"].GetValue<MenuBool>().Enabled && Config["Harass" + t.CharacterName].GetValue<MenuBool>().Enabled && Player.ManaPercent > Local["QHarassMana"].GetValue<MenuSlider>().Value && OktwCommon.CanHarras())
                    Program.CastSpell(Q, t);
                else if (OktwCommon.GetKsDamage(t, Q) > t.Health)
                    Program.CastSpell(Q, t);
                else if (Player.Mana > RMANA + QMANA)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Program.CastSpell(Q, t);
                }
            }

            if (ObjectManager.Player.Spellbook.IsAutoAttack)
                return;

            if (!Program.None && !Program.Combo )
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, Q.Range);

                if (FarmMenu["farmQout"].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + QMANA + EMANA + WMANA)
                {
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range) && (!ObjectManager.Player.InAutoAttackRange(minion) || (!minion.IsUnderEnemyTurret() && minion.IsUnderEnemyTurret()))))
                    {
                        var hpPred = SebbyLib.HealthPrediction.GetHealthPrediction(minion, 600);
                        if (hpPred < Q.GetDamage(minion)  && hpPred > minion.Health - hpPred * 2)
                        {
                            Q.Cast(minion);
                            return;
                        }
                    }
                }
                if (FarmSpells && FarmMenu["farmQ"].GetValue<MenuBool>().Enabled)
                {
                    var farmPos = Q.GetCircularFarmLocation(allMinions, Q.Width);
                    if (farmPos.MinionsHit >= FarmMinions)
                        Q.Cast(farmPos.Position);
                }
            }
        }
        
        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + QMANA)
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
                    else if (W.IsReady() && FarmMenu["jungleW"].GetValue<MenuBool>().Enabled && Environment.TickCount - Q.LastCastAttemptTime > 900)
                    {
                        W.Cast(mob.ServerPosition);
                        return;
                    }
                }
            }
        }
        
        private void CastQE(AIBaseClient target)
        {
            SebbyLib.SkillshotType CoreType2 = SebbyLib.SkillshotType.SkillshotLine;

            var predInput2 = new SebbyLib.PredictionInput
            {
                Aoe = false,
                Collision = EQ.Collision,
                Speed = EQ.Speed,
                Delay = EQ.Delay,
                Range = EQ.Range,
                From = Player.ServerPosition,
                Radius = EQ.Width,
                Unit = target,
                Type = CoreType2
            };

            var poutput2 = SebbyLib.Prediction.GetPrediction(predInput2);

            if (OktwCommon.CollisionYasuo(Player.ServerPosition, poutput2.CastPosition))
                return;

            Vector3 castQpos = poutput2.CastPosition;

            if (Player.Distance(castQpos) > Q.Range)
                castQpos = Player.Position.Extend(castQpos, Q.Range);

            if (Config["EHitChance"].GetValue<MenuList>().Index == 0)
            {
                if (poutput2.Hitchance >= SebbyLib.HitChance.VeryHigh)
                {
                    EQcastNow = true;
                    Q.Cast(castQpos);
                }

            }
            else if (Config["EHitChance"].GetValue<MenuList>().Index == 1)
            {
                if (poutput2.Hitchance >= SebbyLib.HitChance.High)
                {
                    EQcastNow = true;
                    Q.Cast(castQpos);
                }

            }
            else if (Config["EHitChance"].GetValue<MenuList>().Index == 2)
            {
                if (poutput2.Hitchance >= SebbyLib.HitChance.Medium)
                {
                    EQcastNow = true;
                    Q.Cast(castQpos);
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Local["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (Q.IsReady())
                        CircleRender.Draw(ObjectManager.Player.Position, Q.Range, Color.Cyan, 1);
                }
                else
                    CircleRender.Draw(ObjectManager.Player.Position, Q.Range, Color.Cyan, 1);
            }
            if (Local["wRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (W.IsReady())
                        CircleRender.Draw(ObjectManager.Player.Position, W.Range, Color.Orange, 1);
                }
                else
                    CircleRender.Draw(ObjectManager.Player.Position, W.Range, Color.Orange, 1);
            }
            if (Local["eRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (E.IsReady())
                        CircleRender.Draw(ObjectManager.Player.Position, EQ.Range, Color.Yellow, 1);
                }
                else
                    CircleRender.Draw(ObjectManager.Player.Position, EQ.Range, Color.Yellow, 1);
            }
            if (Local["rRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (R.IsReady())
                        CircleRender.Draw(ObjectManager.Player.Position, R.Range, Color.Gray, 1);
                }
                else
                    CircleRender.Draw(ObjectManager.Player.Position, R.Range, Color.Gray, 1);
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

        private void BallCleaner()
        {
            if (BallsList.Count > 0)
            {
                BallsList.RemoveAll(ball => !ball.IsValid || ball.Mana == 19);
            }
        }

        private void CatchW(AIBaseClient t, bool onlyMinin = false)
        {

            if (Environment.TickCount - W.LastCastAttemptTime < 150)
                return;

            var catchRange = 925;
            AIBaseClient obj = null;
            if (BallsList.Count > 0 && !onlyMinin)
            {
                obj = BallsList.Find(ball => ball.Distance(Player) < catchRange);
            }
            if (obj == null)
            {
                obj = MinionManager.GetMinions(Player.ServerPosition, catchRange, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly, MinionManager.MinionOrderTypes.MaxHealth).FirstOrDefault();
            }

            if (obj != null)
            {
                foreach (var minion in MinionManager.GetMinions(Player.ServerPosition, catchRange, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly, MinionManager.MinionOrderTypes.MaxHealth))
                {
                    if (t.Distance(minion) < t.Distance(obj))
                        obj = minion;
                }
                
                W.Cast(obj.Position);
            }
        }
    }
}