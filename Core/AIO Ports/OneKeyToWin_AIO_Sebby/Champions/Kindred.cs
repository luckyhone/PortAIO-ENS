using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SebbyLib;
using SPrediction;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Kindred : Base
    {
        public static Core.OKTWdash Dash;

        public Kindred()
        {
            Q = new Spell(SpellSlot.Q, 340);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 550);

            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            drawMenu.Add(new MenuBool("wRange", "W range", true).SetValue(false));
            drawMenu.Add(new MenuBool("eRange", "E range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRange", "R range", true).SetValue(false));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw only ready spells", true).SetValue(true));

            var qConfig = Local.Add(new Menu("qConfig", "Q Config"));
            qConfig.Add(new MenuBool("autoQ", "Auto Q", true).SetValue(true));
            Dash = new Core.OKTWdash(Q);

            var wConfig = Local.Add(new Menu("wConfig", "W Config"));
            wConfig.Add(new MenuBool("autoW", "Auto W", true).SetValue(true));
            wConfig.Add(new MenuBool("harassW", "Harass W", true).SetValue(true));

            var eConfig = Local.Add(new Menu("eConfig", "E Config"));
            eConfig.Add(new MenuBool("autoE", "Auto E", true).SetValue(true));
            eConfig.Add(new MenuBool("harassE", "Harass E", true).SetValue(true));
            var useon = eConfig.Add(new Menu("useon", "Use on:"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                useon.Add(new MenuBool("Euse" + enemy.CharacterName, enemy.CharacterName, true).SetValue(true));

            var rConfig = Local.Add(new Menu("rConfig", "R Config"));
            rConfig.Add(new MenuBool("autoR", "Auto R", true).SetValue(true));
            rConfig.Add(new MenuSlider("Renemy", "Don't R if x enemies",4, 0, 5));

            FarmMenu.Add(new MenuBool("farmQ", "Lane clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmW", "Lane clear W", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmE", "Lane clear E", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleQ", "Jungle clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleW", "Jungle clear W", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleE", "Jungle clear E", true).SetValue(true));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalker.OnAfterAttack += Orbwalker_AfterAttack;
        }

        private void Orbwalker_AfterAttack(object unit, AfterAttackEventArgs e)
        {
            if (e.Target.Type != GameObjectType.AIHeroClient)
                return;

            if (Program.Combo && Player.Mana > RMANA + QMANA && Q.IsReady() && Local["qConfig"]["autoQ"].GetValue<MenuBool>().Enabled)
            {
                var t = e.Target as AIHeroClient;
                if (t.IsValidTarget())
                {
                    var dashPos = Dash.CastDash();
                    if (!dashPos.IsZero && dashPos.CountEnemyHeroesInRange(500) > 0)
                    {
                        Q.Cast(dashPos);
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if (Program.LagFree(1) && E.IsReady() && Local["eConfig"]["autoE"].GetValue<MenuBool>().Enabled)
                LogicE();

            if (Program.LagFree(2) && W.IsReady() && Local["wConfig"]["autoW"].GetValue<MenuBool>().Enabled )
                LogicW();

            if (Program.LagFree(3) && Q.IsReady() && Local["qConfig"]["autoQ"].GetValue<MenuBool>().Enabled)
                LogicQ();

            if (R.IsReady() && Local["rConfig"]["autoR"].GetValue<MenuBool>().Enabled)
                LogicR();
        }
        
        private void LogicQ()
        {
            if (Program.Combo && Player.Mana > RMANA + QMANA)
            {
                if (Orbwalker.GetTarget() != null)
                    return;
                var dashPos = Dash.CastDash();
                if (!dashPos.IsZero && dashPos.CountEnemyHeroesInRange(500) > 0)
                {
                    Q.Cast(dashPos);
                }
            }
            if (FarmSpells && FarmMenu["farmQ"].GetValue<MenuBool>().Enabled)
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, 400);
                if (allMinionsQ.Count >= FarmMinions)
                    Q.Cast(Game.CursorPos);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(650, DamageType.Physical);
            if (t.IsValidTarget() && !Q.IsReady())
            {
                if (Program.Combo && Player.Mana > RMANA + WMANA)
                    W.Cast();
                else if (Program.Harass && Local["wConfig"]["harassW"].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + EMANA + WMANA + EMANA && HarassMenu["harass" + t.CharacterName].GetValue<MenuBool>().Enabled)
                    W.Cast();
            }
            var tks = TargetSelector.GetTarget(1600, DamageType.Physical);
            if (tks.IsValidTarget())
            {
                if (W.GetDamage(tks) * 3 > tks.Health - OktwCommon.GetIncomingDamage(tks))
                    W.Cast();
            }

            if (FarmSpells && FarmMenu["farmW"].GetValue<MenuBool>().Enabled)
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, 600);
                if (allMinionsQ.Count >= FarmMinions)
                    W.Cast();
            }
        }

        private void LogicE()
        {
            var torb = Orbwalker.GetTarget();
            if (torb == null || torb.Type != GameObjectType.AIHeroClient)
                return;
            else
            {
                var t = torb as AIHeroClient;

                if (t.IsValidTarget(E.Range))
                {
                    if (!Local["eConfig"]["useon"]["Euse" + t.CharacterName].GetValue<MenuBool>().Enabled)
                        return;
                    if (Program.Combo && Player.Mana > RMANA + EMANA)
                        E.CastOnUnit(t);
                    else if (Program.Harass && Local["eConfig"]["harassE"].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + EMANA + WMANA + EMANA && HarassMenu["harass" + t.CharacterName].GetValue<MenuBool>().Enabled)
                        E.CastOnUnit(t);
                }
            }
        }

        private void LogicR()
        {
            var rEnemy = Local["rConfig"]["Renemy"].GetValue<MenuSlider>().Value;

            double dmg = OktwCommon.GetIncomingDamage(Player);

            if (dmg == 0 )
                return;

            if (Player.Health - dmg <  Player.Level * 10 && Player.CountEnemyHeroesInRange(500) < rEnemy)
                R.Cast(Player);
            
        }

        private void Jungle()
        {
            if (Program.LaneClear)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    if (E.IsReady() && FarmMenu["jungleE"].GetValue<MenuBool>().Enabled)
                    {
                        E.Cast(mob);
                        return;
                    }
                    if (Q.IsReady() && FarmMenu["jungleQ"].GetValue<MenuBool>().Enabled)
                    {
                        Q.Cast(Game.CursorPos);
                        return;
                    }
                    if (W.IsReady() && FarmMenu["jungleW"].GetValue<MenuBool>().Enabled)
                    {
                        W.Cast();
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
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
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
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }
            if (Local["draw"]["rRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (R.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }
    }
}