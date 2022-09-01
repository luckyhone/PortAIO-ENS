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

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Ahri : Base
    {
        private static GameObject QMissile = null, EMissile = null;
        public AIHeroClient Qtarget = null;
        public Core.MissileReturn missileManager;

        private readonly MenuBool noti = new MenuBool("noti", "Show notification & line");
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);
        private readonly MenuBool Qhelp = new MenuBool("Qhelp", "Show Q helper", false);

        private readonly MenuBool autoQ2 = new MenuBool("autoQ", "Auto Q");
        private readonly MenuBool harassQ = new MenuBool("harassQ", "Harass Q");
        private readonly MenuBool aimQ = new MenuBool("aimQ", "Auto aim Q missile");

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W");
        private readonly MenuBool harassW = new MenuBool("harassW", "Harass W");

        private readonly MenuBool autoE = new MenuBool("autoE", "Auto E");
        private readonly MenuBool harassE = new MenuBool("harassE", "Harass E");
        private readonly List<MenuBool> Eon = new List<MenuBool>();
        private readonly List<MenuBool> Egapcloser = new List<MenuBool>();

        private readonly MenuBool autoR = new MenuBool("autoR", "R KS");
        private readonly MenuBool autoR2 = new MenuBool("autoR2", "auto R fight logic + aim Q");

        private readonly MenuBool farmQ = new MenuBool("farmQ", "Lane clear Q");
        private readonly MenuBool farmW = new MenuBool("farmW", "Lane clear W");
        private readonly MenuBool jungleQ = new MenuBool("jungleQ", "Jungle clear Q");
        private readonly MenuBool jungleW = new MenuBool("jungleW", "Jungle clear W");

        public Ahri()
        {
            Q = new Spell(SpellSlot.Q, 870);
            W = new Spell(SpellSlot.W, 580);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 90, 1550, false, SpellType.Line);
            E.SetSkillshot(0.25f, 60, 1550, true, SpellType.Line);

            missileManager = new Core.MissileReturn("AhriOrbMissile", "AhriOrbReturn", Q);

            Local.Add(new Menu("draw", "Draw")
            {
                noti,
                onlyRdy,
                qRange,
                wRange,
                eRange,
                rRange,
                Qhelp
            });

            Local.Add(new Menu("qConfig", "Q Config")
            {
                autoQ2,
                harassQ,
                aimQ
            });

            Local.Add(new Menu("wConfig", "W Config")
            {
                autoW,
                harassW
            });

            var EonList = new Menu("eonList", "Use E on");
            var EgapcloserList = new Menu("EgapcloserList", "Gapcloser");

            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                var Eons = new MenuBool("Eon" + enemy.CharacterName, enemy.CharacterName);
                var Egapclosers = new MenuBool("Egapcloser" + enemy.CharacterName, enemy.CharacterName);

                EonList.Add(Eons);
                Eon.Add(Eons);
                
                EgapcloserList.Add(Egapclosers);
                Egapcloser.Add(Egapclosers);
                
            }

            Local.Add(new Menu("eConfig", "E Config")
            {
                autoE,
                harassE,
                EonList,
                EgapcloserList
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                autoR2
            });

            FarmMenu.Add(new Menu("farm", "Farm")
            {
                farmQ,
                farmW,
                jungleQ,
                jungleW
            });

            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += SpellMissile_OnCreateOld;
            GameObject.OnDelete += Obj_SpellMissile_OnDelete;
        }

        private void Obj_SpellMissile_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.IsEnemy || sender.Type != GameObjectType.MissileClient || !sender.IsValid)
                return;

            MissileClient missile = (MissileClient) sender;

            if (missile.SData.Name != null)
            {
                if (missile.SData.Name == "AhriOrbReturn")
                    QMissile = null;
                if (missile.SData.Name == "AhriSeduceMissile")
                    EMissile = null;
            }
        }

        private void SpellMissile_OnCreateOld(GameObject sender, EventArgs args)
        {
            if (sender.IsEnemy || sender.Type != GameObjectType.MissileClient || !sender.IsValid)
                return;

            MissileClient missile = (MissileClient) sender;

            if (missile.SData.Name != null)
            {
                if (missile.SData.Name == "AhriOrbMissile" || missile.SData.Name == "AhriOrbReturn")
                {
                    QMissile = sender;
                }

                if (missile.SData.Name == "AhriSeduceMissile")
                {
                    EMissile = sender;
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (E.IsReady() && sender.IsValidTarget(E.Range) &&
                Egapcloser.Any(e => e.Enabled && e.Name == "Egapcloser" + sender.CharacterName))
            {
                E.Cast(sender);
            }
        }

        private void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (E.IsReady() && Player.Distance(sender.Position) < E.Range)
            {
                E.Cast(sender);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if (E.IsReady() && autoE.Enabled)
                LogicE();
            if (Program.LagFree(2) && W.IsReady() && autoW.Enabled)
                LogicW();
            if (Program.LagFree(3) && Q.IsReady() && autoQ2.Enabled)
                LogicQ();
            if (Program.LagFree(4) && R.IsReady() && Program.Combo)
                LogicR();
        }

        private void LogicR()
        {
            var dashPosition = Player.Position.Extend(Game.CursorPos, 450);

            if (Player.Distance(Game.CursorPos) < 450)
                dashPosition = Game.CursorPos;

            if (dashPosition.CountEnemyHeroesInRange(800) > 2)
                return;

            if (autoR2.Enabled)
            {
                if (Player.HasBuff("AhriTumble"))
                {
                    var BuffTime = OktwCommon.GetPassiveTime(Player, "AhriTumble");
                    if (BuffTime < 3)
                    {
                        R.Cast(dashPosition);
                    }

                    var posPred = missileManager.CalculateReturnPos();
                    if (posPred != Vector3.Zero)
                    {

                        if (missileManager.Missile.SData.Name == "AhriOrbReturn" && Player.Distance(posPred) > 200)
                        {
                            R.Cast(posPred);
                            Program.debug("AIMMMM");
                        }
                    }
                }
            }

            if (autoR.Enabled)
            {
                var t = TargetSelector.GetTarget(450 + R.Range, DamageType.Magical);
                if (t.IsValidTarget())
                {
                    var comboDmg = R.GetDamage(t) * 3;
                    if (Q.IsReady())
                    {
                        comboDmg += Q.GetDamage(t) * 2;
                    }

                    if (W.IsReady())
                    {
                        comboDmg += W.GetDamage(t) + W.GetDamage(t, 1);
                    }

                    if (t.CountAllyHeroesInRange(600) < 2 && comboDmg > t.Health &&
                        t.Position.Distance(Game.CursorPos) < t.Position.Distance(Player.Position) &&
                        dashPosition.Distance(t.Position) < 500)
                    {
                        R.Cast(dashPosition);
                    }

                    foreach (var target in GameObjects.EnemyHeroes.Where(target =>
                                 target.IsMelee && target.IsValidTarget(300)))
                    {
                        R.Cast(dashPosition);
                    }
                }
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Player.Mana > RMANA + WMANA)
                    W.Cast();
                else if (Program.Harass && Player.Mana > RMANA + QMANA + WMANA && 
                         harassW.Enabled &&
                         HarassMenu.GetValue<MenuBool>("harass" + t.CharacterName).Enabled)
                    W.Cast();
                else if (W.GetDamage(t) + W.GetDamage(t, 1) + Q.GetDamage(t) * 2 >
                         t.Health - OktwCommon.GetIncomingDamage(t))
                    W.Cast();
            }
            else if (FarmSpells && QMissile == null && farmW.Enabled)
            {
                var minionList = Cache.GetMinions(Player.Position, W.Range);
                if (minionList.Count >= FarmMinions && minionList.Any(minion => minion.Health < W.GetDamage(minion)))
                    W.Cast();
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (t.IsValidTarget())
            {
                missileManager.Target = t;
                if (EMissile == null || !EMissile.IsValid)
                {

                    if (Program.Combo && Player.Mana > RMANA + QMANA)
                        Program.CastSpell(Q, t);
                    else if (Program.Harass && Player.Mana > RMANA + WMANA + QMANA + QMANA &&
                             harassQ.Enabled &&
                             HarassMenu.GetValue<MenuBool>("harass" + t.CharacterName).Enabled &&
                             OktwCommon.CanHarras())
                        Program.CastSpell(Q, t);
                    else if (Q.GetDamage(t) * 2 + OktwCommon.GetEchoLudenDamage(t) >
                             t.Health - OktwCommon.GetIncomingDamage(t))
                        Q.Cast(t, true);
                }

                if (!Program.None && Player.Mana > RMANA + WMANA)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy =>
                                 enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true);
                }
            }
            else if (FarmSpells && farmQ.Enabled)
            {
                var minionList = Cache.GetMinions(Player.Position, Q.Range);
                if (minionList != null)
                {
                    var farmPosition = Q.GetLineFarmLocation(minionList, Q.Width);
                    if (farmPosition.MinionsHit >= FarmMinions)
                        Q.Cast(farmPosition.Position);
                }
            }
        }

        private void LogicE()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy =>
                         enemy.IsValidTarget(E.Range) &&
                         OktwCommon.GetKsDamage(enemy, E) + Q.GetDamage(enemy) + W.GetDamage(enemy) > enemy.Health))
                Program.CastSpell(E, enemy);

            var t = Orbwalker.GetTarget() as AIHeroClient;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Player.Mana > RMANA + EMANA &&
                    Eon.Any(e => e.Enabled && e.Name == "Eon" + t.CharacterName))
                    Program.CastSpell(E, t);
                else if (Program.Harass && Player.Mana > RMANA + EMANA + WMANA + EMANA && harassE.Enabled &&
                         HarassMenu.GetValue<MenuBool>("harass" + t.CharacterName).Enabled)
                    Program.CastSpell(E, t);
                if (!Program.None && Player.Mana > RMANA + EMANA)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy =>
                                 enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy) &&
                                 Eon.Any(e => e.Enabled && e.Name == "Eon" + t.CharacterName)))
                        E.Cast(enemy);
                }
            }
        }
        
        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > QMANA + RMANA)
            {
                var mobs = Cache.GetMinions(Player.Position, 600, MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && jungleW.Enabled)
                    {
                        W.Cast();
                        return;
                    }

                    if (Q.IsReady() && jungleQ.Enabled)
                    {
                        Q.Cast(mob.Position);
                        return;
                    }
                }
            }
        }

        private void SetMana()
        {
            if (manaDisable.Enabled && Program.Combo || Player.HealthPercent < 20)
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

        public new static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (qRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (Q.IsReady())
                        CircleRender.Draw(Player.Position, Q.Range, Color.Cyan, 1);
                }
                else
                    CircleRender.Draw(Player.Position, Q.Range, Color.Cyan, 1);
            }

            if (wRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (W.IsReady())
                        CircleRender.Draw(Player.Position, W.Range, Color.Orange, 1);
                }
                else
                    CircleRender.Draw(Player.Position, W.Range, Color.Orange, 1);
            }

            if (eRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (E.IsReady())
                        CircleRender.Draw(Player.Position, E.Range, Color.Yellow, 1);
                }
                else
                    CircleRender.Draw(Player.Position, E.Range, Color.Yellow, 1);
            }

            if (noti.Enabled)
            {

                var t = TargetSelector.GetTarget(1500, DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var comboDmg = 0f;
                    if (R.IsReady())
                    {
                        comboDmg += R.GetDamage(t) * 3;
                    }

                    if (Q.IsReady())
                    {
                        comboDmg += Q.GetDamage(t) * 2;
                    }

                    if (W.IsReady())
                    {
                        comboDmg += W.GetDamage(t) + W.GetDamage(t, 1);
                    }

                    if (comboDmg > t.Health)
                    {

                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red,
                            "COMBO KILL " + t.CharacterName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Yellow);
                    }
                }
            }
        }
    }
}