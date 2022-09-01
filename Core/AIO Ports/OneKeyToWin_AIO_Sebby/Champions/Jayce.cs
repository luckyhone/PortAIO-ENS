using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SebbyLib;
using SharpDX;
using SPrediction;
using xSalice_Reworked.Base;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Jayce : Base
    {
        private Spell Qext, QextCol;
        private float  QMANA2 = 0, WMANA2 = 0, EMANA2 = 0;
        private float Qcd, Wcd, Ecd, Q2cd, W2cd, E2cd;
        private float Qcdt, Wcdt, Ecdt, Q2cdt, W2cdt, E2cdt;
        private Vector3 EcastPos;
        private int Etick = 0;

        public Jayce()
        {
            #region SET SKILLS
            Q = new Spell(SpellSlot.Q, 1030);
            Qext = new Spell(SpellSlot.Q, 1650);
            QextCol = new Spell(SpellSlot.Q, 1650);
            Q1 = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            W1 = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 650);
            E1 = new Spell(SpellSlot.E, 240);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 70, 1450, true, SpellType.Line);
            Qext.SetSkillshot(0.30f, 80, 2000, false, SpellType.Line);
            QextCol.SetSkillshot(0.30f, 100, 1600, true, SpellType.Line);
            Q1.SetTargetted(0.25f, float.MaxValue);
            E.SetSkillshot(0.1f, 120, float.MaxValue, false, SpellType.Circle);
            E1.SetTargetted(0.25f, float.MaxValue);
            #endregion
            
            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("showcd", "Show cooldown", true).SetValue(true));
            drawMenu.Add(new MenuBool("noti", "Show notification & line", true).SetValue(false));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw when skill rdy", true).SetValue(true));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));

            var qConfig = Local.Add(new Menu("qConfig", "Q Config"));
            qConfig.Add(new MenuBool("autoQ", "Auto Q range", true).SetValue(true));
            qConfig.Add(new MenuBool("autoQm", "Auto Q melee", true).SetValue(true));
            qConfig.Add(new MenuBool("QEforce", "force E + Q", true).SetValue(false));
            qConfig.Add(new MenuBool("QEsplash", "Q + E splash minion damage", true).SetValue(true));
            qConfig.Add(new MenuSlider("QEsplashAdjust", "Q + E splash minion radius", 150,50, 250));
            qConfig.Add(new MenuKeyBind("useQE", "Semi-manual Q + E near mouse key", Keys.T, KeyBindType.Press)); //32 == space
            
            var wConfig = Local.Add(new Menu("wConfig", "W Config"));
            wConfig.Add(new MenuBool("autoW", "Auto W range", true).SetValue(true));
            wConfig.Add(new MenuBool("autoWm", "Auto W melee", true).SetValue(true));
            wConfig.Add(new MenuBool("autoWmove", "Disable move if W range active", true).SetValue(true));
            
            var eConfig = Local.Add(new Menu("eConfig", "E Config"));
            eConfig.Add(new MenuBool("autoE", "Auto E range (Q + E)", true).SetValue(true));
            eConfig.Add(new MenuBool("autoEm", "Auto E melee", true).SetValue(true));
            eConfig.Add(new MenuBool("autoEks", "E melee ks only", true).SetValue(false));
            eConfig.Add(new MenuBool("gapE", "Gapcloser R + E", true).SetValue(true));
            eConfig.Add(new MenuBool("intE", "Interrupt spells R + Q + E", true).SetValue(true));

            var rConfig = Local.Add(new Menu("rConfig", "R Config"));
            rConfig.Add(new MenuBool("autoR", "Auto R range", true).SetValue(true));
            rConfig.Add(new MenuBool("autoRm", "Auto R melee", true).SetValue(true));

            HarassMenu.Add(new MenuSlider("harassMana", "Harass Mana", 80, 0, 100));
            Local.Add(new MenuKeyBind("flee", "FLEE MODE", Keys.T, KeyBindType.Press));
            
            FarmMenu.Add(new MenuBool("farmQ", "Lane clear Q + E range", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmW", "Lane clear W range && mele", true).SetValue(true));

            FarmMenu.Add(new MenuBool("jungleQ", "Jungle clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleW", "Jungle clear W", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleE", "Jungle clear E", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleR", "Jungle clear R", true).SetValue(true));

            FarmMenu.Add(new MenuBool("jungleQm", "Jungle clear Q melee", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleWm", "Jungle clear W melee", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleEm", "Jungle clear E melee", true).SetValue(true));

            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalker.OnBeforeAttack += BeforeAttack;
            GameEvent.OnGameTick += OnUpdate;
        }

        private void OnUpdate(EventArgs args)
        {
            if (Range && E.IsReady() && Utils.TickCount - Etick < 250 + Game.Ping)
            {
                E.Cast(EcastPos);
            }

            if (Local["flee"].GetValue<MenuKeyBind>().Active)
            {
                FleeMode();
            }

            if (Range)
            {
                
                if (Local["wConfig"]["autoWmove"].GetValue<MenuBool>().Enabled && Orbwalker.GetTarget() != null && Player.HasBuff("jaycehyperchargevfx"))
                    Orbwalker.MoveEnabled = false;
                else
                    Orbwalker.MoveEnabled = true;

                if (Program.LagFree(1) && Q.IsReady() && Local["qConfig"]["autoQ"].GetValue<MenuBool>().Enabled)
                    LogicQ();

                if (Program.LagFree(2) && W.IsReady() && Local["wConfig"]["autoW"].GetValue<MenuBool>().Enabled)
                    LogicW();
            }
            else
            {
                Orbwalker.MoveEnabled = true;
                if (Program.LagFree(1) && E1.IsReady() && Local["eConfig"]["autoEm"].GetValue<MenuBool>().Enabled)
                    LogicE2();

                if (Program.LagFree(2) && Q1.IsReady() && Local["qConfig"]["autoQm"].GetValue<MenuBool>().Enabled)
                    LogicQ2();
                if (Program.LagFree(3) && W1.IsReady() && Local["wConfig"]["autoWm"].GetValue<MenuBool>().Enabled)
                    LogicW2();
            }

            if (Program.LagFree(4))
            {
                SetValue();
                if(R.IsReady())
                    LogicR();
            }

            Jungle();
            LaneClearLogic();
        }
        
        private void FleeMode()
        {
            if (Range)
            {
                if (E.IsReady())
                    E.Cast(Player.Position.Extend(Game.CursorPos, 150));
                else if (R.IsReady())
                    R.Cast();
            }
            else
            {
                if (Q1.IsReady())
                {
                    var mobs = Cache.GetMinions(Player.ServerPosition, Q1.Range);
                    

                    if (mobs.Count > 0)
                    {
                        AIBaseClient best;
                        best = mobs[0];

                        foreach (var mob in mobs.Where(mob => mob.IsValidTarget(Q1.Range)))
                        {
                            if (mob.Distance(Game.CursorPos) < best.Distance(Game.CursorPos) )
                                best = mob;
                        }
                        if(best.Distance(Game.CursorPos) + 200 < Player.Distance(Game.CursorPos))
                            Q1.Cast(best);
                    }
                    else if (R.IsReady())
                        R.Cast();
                }
                else if (R.IsReady())
                    R.Cast();
            }
            //EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }
        
        private void LogicQ()
        {
            var Qtype = Q;
            if (CanUseQE())
            {
                Qtype = Qext;

                if (Local["qConfig"]["useQE"].GetValue<MenuKeyBind>().Active)
                {
                    var mouseTarget = HeroManager.Enemies.Where(enemy =>
                        enemy.IsValidTarget(Qtype.Range)).OrderBy(enemy => enemy.Distance(Game.CursorPos)).FirstOrDefault();

                    if (mouseTarget != null)
                    {
                        CastQ(mouseTarget);
                        return;
                    }
                }
            }

            var t = TargetSelector.GetTarget(Qtype.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                var qDmg = OktwCommon.GetKsDamage(t, Qtype);

                if (CanUseQE())
                {
                    qDmg = qDmg * 1.4f;
                }

                if (qDmg > t.Health)
                    CastQ(t);
                else if (Program.Combo && Player.Mana > EMANA + QMANA)
                    CastQ(t);
                else if (Program.Harass && Player.ManaPercent > HarassMenu["harassMana"].GetValue<MenuSlider>().Value && OktwCommon.CanHarras())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Qtype.Range) && HarassMenu["Harass" + enemy.CharacterName].GetValue<MenuBool>().Enabled))
                    {
                        CastQ(t);
                    }
                }
                else if ((Program.Combo || Program.Harass) && Player.Mana > RMANA + QMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Qtype.Range) && !OktwCommon.CanMove(enemy)))
                        CastQ(t);
                }
            }
        }

        private void LogicW()
        {
            if (Program.Combo && R.IsReady() && Range && Orbwalker.GetTarget().IsValidTarget() && Orbwalker.GetTarget() is AIHeroClient)
            {
                W.Cast();
            }
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E1.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                var qDmg = OktwCommon.GetKsDamage(t, E1);
                if (qDmg > t.Health)
                    E1.Cast(t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA)
                    E1.Cast(t);
            }
        }

        private void LogicQ2()
        {
            var t = TargetSelector.GetTarget(Q1.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                if (OktwCommon.GetKsDamage(t, Q1) > t.Health)
                    Q1.Cast(t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Q1.Cast(t);
            }
        }

        private void LogicW2()
        {
            if (Player.CountEnemyHeroesInRange(300) > 0 && Player.Mana > 80)
                W.Cast();
        }

        private void LogicE2()
        {
            var t = TargetSelector.GetTarget(E1.Range, DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (OktwCommon.GetKsDamage(t, E1) > t.Health)
                    E1.Cast(t);
                else if (Program.Combo && !Local["eConfig"]["autoEks"].GetValue<MenuBool>().Enabled && !Player.HasBuff("jaycehyperchargevfx"))
                    E1.Cast(t);
            }
        }
        
        private void LogicR()
        {
            if (Range && Local["rConfig"]["autoRm"].GetValue<MenuBool>().Enabled)
            {
                var t = TargetSelector.GetTarget(Q1.Range + 200, DamageType.Physical);
                if (Program.Combo && Qcd > 0.5  && t.IsValidTarget() && ((!W.IsReady() && !t.IsMelee ) || (!W.IsReady() && !Player.HasBuff("jaycehyperchargevfx") && t.IsMelee)))
                {
                    if (Q2cd < 0.5 && t.CountEnemyHeroesInRange(800) < 3)
                        R.Cast();
                    else if (Player.CountEnemyHeroesInRange(300) > 0 && E2cd < 0.5)
                        R.Cast();
                }
            }
            else if (Program.Combo && Local["rConfig"]["autoR"].GetValue<MenuBool>().Enabled)
            {

                var t = TargetSelector.GetTarget(1400, DamageType.Physical);
                if(t.IsValidTarget()&& !t.IsValidTarget(Q1.Range + 200) && Q.GetDamage(t) * 1.4 > t.Health && Qcd < 0.5 && Ecd < 0.5)
                {
                    R.Cast();
                }

                if (!Q.IsReady() && (!E.IsReady() || Local["eConfig"]["autoEks"].GetValue<MenuBool>().Enabled))
                {
                    R.Cast();
                }   
            }
        }
        private void LaneClearLogic()
        {
            if (!Program.LaneClear)
                return;

            if (Range && Q.IsReady() && E.IsReady() && FarmSpells && FarmMenu["farmQ"].GetValue<MenuBool>().Enabled)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var farmPosition = QextCol.GetCircularFarmLocation(minionList, 150);

                if (farmPosition.MinionsHit >= FarmMinions)
                    Q.Cast(farmPosition.Position);
            }

            if (W.IsReady() && FarmSpells && FarmMenu["farmW"].GetValue<MenuBool>().Enabled)
            {
                if (Range)
                {
                    Program.debug("csa");
                    var mobs = Cache.GetMinions(Player.ServerPosition, 550);
                    if (mobs.Count >= FarmMinions)
                    {
                        W.Cast();
                    }
                }
                else
                {
                    var mobs = Cache.GetMinions(Player.ServerPosition, 300);
                    if (mobs.Count >= FarmMinions)
                    {
                        W.Cast();
                    }
                }
            }
        }
        
        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + WMANA)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 700, MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Range)
                    {
                        if (Q.IsReady() && FarmMenu["jungleQ"].GetValue<MenuBool>().Enabled)
                        {
                            Q.Cast(mob.ServerPosition);
                            return;
                        }
                        if (W.IsReady() && FarmMenu["jungleE"].GetValue<MenuBool>().Enabled)
                        {
                            if(Player.InAutoAttackRange(mob))
                                W.Cast();
                            return;
                        }
                        if (FarmMenu["jungleR"].GetValue<MenuBool>().Enabled)
                            R.Cast();
                    }
                    else
                    {
                        if (Q1.IsReady() && FarmMenu["jungleQm"].GetValue<MenuBool>().Enabled && mob.IsValidTarget(Q1.Range))
                        {
                            Q1.Cast(mob);
                            return;
                        }

                        if (W1.IsReady() && FarmMenu["jungleWm"].GetValue<MenuBool>().Enabled )
                        {
                            if(mob.IsValidTarget(300))
                                W.Cast();
                            return;
                        }
                        if (E1.IsReady() && FarmMenu["jungleEm"].GetValue<MenuBool>().Enabled && mob.IsValidTarget(E1.Range))
                        {
                            if( mob.IsValidTarget(E1.Range))
                                E1.Cast(mob);
                            return;
                        }
                        if (FarmMenu["jungleR"].GetValue<MenuBool>().Enabled)
                            R.Cast();
                    }
                }
            }
        }

        private void CastQ(AIBaseClient t)
        {
            if (!CanUseQE())
            {
                Program.CastSpell(Q, t);
                return; 
            }

            bool cast = true;

            if (Local["qConfig"]["QEsplash"].GetValue<MenuBool>().Enabled)
            {
                var poutput = QextCol.GetPrediction(t);
 
                foreach (var minion in poutput.CollisionObjects.Where(minion => minion.IsEnemy && minion.Distance(poutput.CastPosition) > Local["qConfig"]["QEsplashAdjust"].GetValue<MenuSlider>().Value))
                {
                    cast = false;
                    break;
                }
            }
            else
                cast = false;

            if (cast)
                Program.CastSpell(Qext, t);
            else
                Program.CastSpell(QextCol, t);

        }

        private void BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (W.IsReady() && Local["wConfig"]["autoW"].GetValue<MenuBool>().Enabled && Range && args.Target is AIHeroClient)
            {
                if(Program.Combo)
                    W.Cast();
                else if (args.Target.Position.Distance(Player.Position)< 500)
                    W.Cast();
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name.ToLower() == "jayceshockblast" )
            {
                if (Range && E.IsReady() && Local["eConfig"]["autoE"].GetValue<MenuBool>().Enabled)
                {
                    EcastPos = Player.ServerPosition.Extend(args.End, 130 + (Game.Ping /2));
                    Etick = Utils.TickCount;
                    E.Cast(EcastPos);

                }
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q)
            {
                if (W.IsReady() && !Range && Player.Mana > 80)
                    W.Cast();
                if (E.IsReady() && Range && Local["qConfig"]["QEforce"].GetValue<MenuBool>().Enabled)
                    E.Cast(Player.ServerPosition.Extend(args.EndPosition, 120));
            }
        }

        private void Interrupter2_OnInterruptableTarget(AIHeroClient t, Interrupter.InterruptSpellArgs args)
        {
            if (!Local["eConfig"]["intE"].GetValue<MenuBool>().Enabled || E2cd > 0.1)
                return;

            if (Range && !R.IsReady())
                return;

            if (t.IsValidTarget(300))
            {
                if (Range)
                {
                    R.Cast();
                }
                else 
                    E.Cast(t);

            }
            else if (Q2cd < 0.2 && t.IsValidTarget(Q1.Range))
            {
                if (Range)
                {
                    R.Cast();
                }
                else
                {
                    Q.Cast(t);
                    if(t.IsValidTarget(E1.Range))
                        E.Cast(t);
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (!Local["eConfig"]["gapE"].GetValue<MenuBool>().Enabled || E2cd > 0.1)
                return;

            if(Range && !R.IsReady())
                return;

            var t = sender;

            if (t.IsValidTarget(400))
            {
                if (Range)
                {
                    R.Cast();
                }
                else
                    E.Cast(t);
            }
        }

        private bool Range { get { return Q.Instance.Name.ToLower() == "jayceshockblast"; } }
        
        private bool CanUseQE()
        {
            if(E.IsReady() && Player.Mana > QMANA + EMANA && Local["eConfig"]["autoE"].GetValue<MenuBool>().Enabled)
                return true;
            else
                return false;
        }

        private float SetPlus(float valus)
        {
            if (valus < 0)
                return 0;
            else
                return valus;
        }

        private void SetValue()
        {
            if (Range)
            {
                Qcdt = Q.Instance.CooldownExpires;
                Wcdt = W.Instance.CooldownExpires;
                Ecd = E.Instance.CooldownExpires;

                QMANA = Q.Instance.ManaCost;
                WMANA = W.Instance.ManaCost;
                EMANA = E.Instance.ManaCost;
            }
            else
            {
                Q2cdt = Q.Instance.CooldownExpires;
                W2cdt = W.Instance.CooldownExpires;
                E2cdt = E.Instance.CooldownExpires;

                QMANA2 = Q.Instance.ManaCost;
                WMANA2 = W.Instance.ManaCost;
                EMANA2 = E.Instance.ManaCost;
            }

            Qcd = SetPlus(Qcdt - Game.Time);
            Wcd = SetPlus(Wcdt - Game.Time);
            Ecd = SetPlus(Ecdt - Game.Time);
            Q2cd = SetPlus(Q2cdt - Game.Time);
            W2cd = SetPlus(W2cdt - Game.Time);
            E2cd = SetPlus(E2cdt - Game.Time);
        }
        
        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }
        
        private float GetComboDMG(AIBaseClient t)
        {
            float comboDMG = 0;

            if (Qcd < 1 && Ecd < 1)
                comboDMG = Q.GetDamage(t) * 1.4f;
            else if (Qcd < 1)
                comboDMG = Q.GetDamage(t);

            if (Q2cd < 1)
                comboDMG = Q.GetDamage(t, 1);

            if (Wcd < 1)
                comboDMG += (float)Player.GetAutoAttackDamage(t) * 3;

            if (W2cd < 1)
                comboDMG += W.GetDamage(t) * 2;

            if (E2cd < 1)
                comboDMG += E.GetDamage(t) * 3;
            return comboDMG;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Local["draw"]["showcd"].GetValue<MenuBool>().Enabled)
            {
                string msg = "";
                if (Range)
                {
                    msg = "Q" + (int)Q2cd + "   W " + (int)W2cd + "   E " + (int)E2cd;
                    Drawing.DrawText(Drawing.Width * 0.5f-50,Drawing.Height * 0.3f, System.Drawing.Color.Orange, msg);
                }
                else
                {
                    msg = "Q " + (int)Qcd + "   W " + (int)Wcd + "   E " + (int)Ecd;
                    Drawing.DrawText(Drawing.Width * 0.5f - 50, Drawing.Height * 0.3f, System.Drawing.Color.Aqua, msg);
                }
            }
            if (Local["draw"]["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (Q.IsReady())
                    {
                        if (Range)
                        {
                            if (CanUseQE())
                                LeagueSharpCommon.Utility.DrawCircle(Player.Position, Qext.Range, System.Drawing.Color.Cyan, 1, 1);
                            else
                                LeagueSharpCommon.Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                        }
                        else
                            LeagueSharpCommon.Utility.DrawCircle(Player.Position, Q1.Range, System.Drawing.Color.Orange, 1, 1);
                    }
                }
                else
                {
                    if (Range)
                    {
                        if (CanUseQE())
                            LeagueSharpCommon.Utility.DrawCircle(Player.Position, Qext.Range, System.Drawing.Color.Cyan, 1, 1);
                        else
                            LeagueSharpCommon.Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                    }
                    else
                        LeagueSharpCommon.Utility.DrawCircle(Player.Position, Q1.Range, System.Drawing.Color.Cyan, 1, 1);
                }
            }

            if (Local["draw"]["noti"].GetValue<MenuBool>().Enabled)
            {
                var t = TargetSelector.GetTarget(1600, DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var damageCombo = GetComboDMG(t);
                    if (damageCombo > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Combo deal  " + damageCombo + " to " + t.CharacterName);
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Yellow);
                    }

                }
            }
        }
    }
}