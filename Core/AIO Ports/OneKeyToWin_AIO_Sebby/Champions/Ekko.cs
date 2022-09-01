using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SharpDX;
using SebbyLib;
using SPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Prediction = SebbyLib.Prediction;
using Render = LeagueSharpCommon.Render;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Ekko : Base
    {
        private float  Wtime = 0, Wtime2 = 0;
        private static GameObject RMissile, WMissile2, WMissile;
        public static Core.MissileReturn missileManager;

        public Ekko()
        {
            Q = new Spell(SpellSlot.Q, 750); 
            Q1 = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 1620);
            E = new Spell(SpellSlot.E, 330f);
            R = new Spell(SpellSlot.R, 280f);

            Q.SetSkillshot(0.25f, 60f, 1650f, false, SpellType.Line);
            Q1.SetSkillshot(0.5f, 150f, 1000f, false, SpellType.Circle);
            W.SetSkillshot(2.5f, 200f, float.MaxValue, false, SpellType.Circle);
            R.SetSkillshot(0.4f, 280f, float.MaxValue, false, SpellType.Circle);

            missileManager = new Core.MissileReturn("ekkoqmis", "ekkoqreturn", Q);

            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            drawMenu.Add(new MenuBool("wRange", "W range", true).SetValue(false));
            drawMenu.Add(new MenuBool("eRange", "E range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRange", "R range", true).SetValue(false));
            drawMenu.Add(new MenuBool("Qhelp", "Show Q,W helper", true).SetValue(true));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw only ready spells", true).SetValue(true));

            var wConfig = Local.Add(new Menu("wConfig", "W Config"));
            wConfig.Add(new MenuBool("autoW", "Auto W", true).SetValue(true));
            wConfig.Add(new MenuBool("Waoe", "Cast if 2 targets", true).SetValue(false));

            var rConfig = Local.Add(new Menu("rConfig", "R Config"));
            rConfig.Add(new MenuBool("autoR", "Auto R", true).SetValue(true));
            rConfig.Add(new MenuSlider("rCount", "Auto R if enemies in range", 3, 0, 5));                     
            
            FarmMenu.Add(new MenuBool("farmQ", "Lane clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmW", "Farm W", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleQ", "Jungle clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleW", "Jungle clear W", true).SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            
            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if (Program.LagFree(1) && Q.IsReady() )
                LogicQ();
            if (Program.LagFree(2) && W.IsReady() && Local["wConfig"]["autoW"].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                LogicW();
            if (Program.LagFree(3) && E.IsReady() )
                LogicE();
            if ( R.IsReady() )
                LogicR();
        }

        private void LogicR()
        {
            if (Local["rConfig"]["autoR"].GetValue<MenuBool>().Enabled)
            {
                if (Program.LagFree(4) && Program.Combo && RMissile != null && RMissile.IsValid)
                {
                    if (RMissile.Position.CountEnemyHeroesInRange(R.Range) >= Local["rConfig"]["rCount"].GetValue<MenuSlider>().Value && Local["rConfig"]["rCount"].GetValue<MenuSlider>().Value > 0)
                        R.Cast();

                    foreach (var t in GameObjects.EnemyHeroes.Where(t => t.IsValidTarget() && RMissile.Position.Distance(Prediction.GetPrediction(t, R.Delay).CastPosition) < R.Range && RMissile.Position.Distance(t.ServerPosition) < R.Range))
                    {
                        var comboDMG = OktwCommon.GetKsDamage(t, R);

                        if (Q.IsReady())
                            comboDMG += Q.GetDamage(t);

                        if (E.IsReady())
                            comboDMG += E.GetDamage(t);

                        if (W.IsReady())
                            comboDMG += W.GetDamage(t);

                        if (t.Health < comboDMG && OktwCommon.ValidUlt(t))
                            R.Cast();

                        Program.debug("ks");

                    }
                }

                double dmg = OktwCommon.GetIncomingDamage(Player);

                if (dmg > 0)
                {
                    if (Player.Health - dmg < Player.Level * 10)
                    {
                        R.Cast();
                    }
                } 
            }
        }

        private void LogicE()
        {
            if (Program.Combo && WMissile != null && WMissile.IsValid)
            {
                if (WMissile.Position.CountEnemyHeroesInRange(200) > 0 && WMissile.Position.Distance(Player.ServerPosition) < 100)
                {
                    E.Cast(Player.Position.Extend(WMissile.Position, E.Range));
                }
            }

            var t = TargetSelector.GetTarget(800, DamageType.Magical);

            if (E.IsReady() && Player.Mana > RMANA + EMANA
                 && Player.CountEnemyHeroesInRange(260) > 0
                 && Player.Position.Extend(Game.CursorPos, E.Range).CountEnemyHeroesInRange(500) < 3
                 && t.Position.Distance(Game.CursorPos) > t.Position.Distance(Player.Position))
            {
                E.Cast(Player.Position.Extend(Game.CursorPos, E.Range));
            }
            else if (Program.Combo && Player.Health > Player.MaxHealth * 0.4
                && Player.Mana > RMANA + EMANA
                && !Player.IsUnderEnemyTurret()
                && Player.Position.Extend(Game.CursorPos, E.Range).CountEnemyHeroesInRange(700) < 3)
            {
                if (t.IsValidTarget() && Player.Mana > QMANA + EMANA + WMANA && t.Position.Distance(Game.CursorPos) + 300 < t.Position.Distance(Player.Position))
                {
                    E.Cast(Player.Position.Extend(Game.CursorPos, E.Range));
                }
            }
            else if (t.IsValidTarget() && Program.Combo  && E.GetDamage(t) + W.GetDamage(t) > t.Health)
            {
                E.Cast(Player.Position.Extend(t.Position, E.Range));
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > QMANA + RMANA )
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 500 , MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && FarmMenu["jungleW"].GetValue<MenuBool>().Enabled)
                    {
                        W.Cast(mob.Position);
                        return;
                    }
                    if (Q.IsReady() && FarmMenu["jungleQ"].GetValue<MenuBool>().Enabled)
                    {
                        Q.Cast(mob.Position);
                        return;
                    }  
                }
            }
        }
        private void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid  )
            {
                if (obj.Name == "Ekko" && obj.IsAlly)
                    RMissile = obj;
                if (obj.Name == "Ekko_Base_W_Indicator.troy")
                {
                    WMissile = obj;
                    Wtime = Game.Time;
                }
                if (obj.Name == "Ekko_Base_W_Cas.troy")
                {
                    WMissile2 = obj;
                    Wtime2 = Game.Time;
                }
            }     
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var t1 = TargetSelector.GetTarget(Q1.Range, DamageType.Magical);
            if (t.IsValidTarget())
            {
                missileManager.Target = t;
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Harass && Config["harass" + t.CharacterName].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + WMANA + QMANA + QMANA && OktwCommon.CanHarras())
                        Program.CastSpell(Q, t);
                else if (OktwCommon.GetKsDamage(t, Q) * 2 > t.Health)
                    Program.CastSpell(Q, t);
                if (Player.Mana > RMANA + QMANA + WMANA )
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy,true,true);
                }

            }
            else if (t1.IsValidTarget())
            {
                missileManager.Target = t1;
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q1, t1);
                else if (Program.Harass && Config["harass" + t1.CharacterName].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + WMANA + QMANA + QMANA && OktwCommon.CanHarras())
                    Program.CastSpell(Q1, t1);
                else if (OktwCommon.GetKsDamage(t1, Q1) * 2 > t1.Health)
                    Program.CastSpell(Q1, t1);
                if (Player.Mana > RMANA + QMANA + WMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q1.Range) && !OktwCommon.CanMove(enemy)))
                        Q1.Cast(enemy, true, true);
                }
            }
            else if (FarmSpells && FarmMenu["farmQ"].GetValue<MenuBool>().Enabled)
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q1.Range);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, 100);
                if (Qfarm.MinionsHit >= FarmMinions)
                    Q.Cast(Qfarm.Position);
            }
            
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (t.IsValidTarget() )
            {
                
                if (Local["wConfig"]["Waoe"].GetValue<MenuBool>().Enabled)
                {
                    W.CastIfWillHit(t, 2);
                    if (t.CountEnemyHeroesInRange(250) > 1)
                    {
                        Program.CastSpell(W, t);
                    }
                }
                    
                if (Program.Combo  && W.GetPrediction(t).CastPosition.Distance(t.Position) > 200)
                    Program.CastSpell(W, t);
            }
            if (!Program.None)
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                    W.Cast(enemy, true, true);
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

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        public static void drawText2(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if ( Local["draw"]["Qhelp"].GetValue<MenuBool>().Enabled)
            {
                if (WMissile != null && WMissile.IsValid)
                {
                    Render.Circle.DrawCircle(WMissile.Position, 300, System.Drawing.Color.Yellow, 1);
                    drawText2("W:  " + String.Format("{0:0.0}", Wtime + 3 - Game.Time), WMissile.Position, System.Drawing.Color.White);

                }
                if (WMissile2 != null && WMissile2.IsValid)
                {
                    Render.Circle.DrawCircle(WMissile2.Position, 300, System.Drawing.Color.Red, 1);
                    drawText2("W:  " + String.Format("{0:0.0}", Wtime2 + 1 - Game.Time), WMissile2.Position, System.Drawing.Color.Red);

                }
            }

            if (Local["draw"]["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (Q.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                }
                else
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }
            if (Local["draw"]["wRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (W.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
                }
                else
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
                
            }
            if (Local["draw"]["eRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (E.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, 800, System.Drawing.Color.Yellow, 1);
                }
                else
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 800, System.Drawing.Color.Yellow, 1);
            }
            if (Local["draw"]["rRange"].GetValue<MenuBool>().Enabled)
            {
                if (RMissile != null && RMissile.IsValid)
                {
                    if (Local["draw"]["rRange"].GetValue<MenuBool>().Enabled)
                    {
                        if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                        {
                            if (R.IsReady())
                                Render.Circle.DrawCircle(RMissile.Position, R.Width, System.Drawing.Color.YellowGreen, 1);
                        }
                        else
                            Render.Circle.DrawCircle(RMissile.Position, R.Width, System.Drawing.Color.YellowGreen, 1);

                        drawLine(RMissile.Position, Player.Position, 10, System.Drawing.Color.YellowGreen);
                    }
                }
            }
        }
    }
}
