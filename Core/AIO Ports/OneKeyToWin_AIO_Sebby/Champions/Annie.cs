using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SebbyLib;
using SPrediction;
using HealthPrediction = SebbyLib.HealthPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Annie : Base
    {
        private SpellSlot flash;
        public AIBaseClient Tibbers;
        public float TibbersTimer = 0;
        private bool HaveStun = false;
        private Spell FR;

        public Annie()
        {
            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 550f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 625f);
            FR = new Spell(SpellSlot.R, 1000f );

            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.3f, 80f, float.MaxValue, false, SpellType.Line);
            R.SetSkillshot(0.25f, 180f, float.MaxValue, false, SpellType.Circle);
            FR.SetSkillshot(0.25f, 180f, float.MaxValue, false, SpellType.Circle);

            flash = Player.GetSpellSlot("summonerflash");

            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            drawMenu.Add(new MenuBool("wRange", "W range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRange", "R range", true).SetValue(false));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw only ready spells", true).SetValue(true));

            var qConfig = Local.Add(new Menu("qConfig", "W Config"));
            qConfig.Add(new MenuBool("autoQ", "Auto Q", true).SetValue(true));
            qConfig.Add(new MenuBool("harassQ", "Harass Q", true).SetValue(true));

            var wConfig = Local.Add(new Menu("wConfig", "W Config"));
            wConfig.Add(new MenuBool("autoW", "Auto W", true).SetValue(true));
            wConfig.Add(new MenuBool("harassW", "Harass W", true).SetValue(true));

            var eConfig = Local.Add(new Menu("eConfig", "E Config"));
            eConfig.Add(new MenuBool("autoE", "Auto E stack stun", true).SetValue(true));

            var rConfig = Local.Add(new Menu("rConfig", "R Config"));
            var ultiManager = rConfig.Add(new Menu("ultiManager", "Ultimate Manager"));
            foreach (var enemy in GameObjects.EnemyHeroes)
                ultiManager.Add(new MenuList("UM" + enemy.CharacterName, enemy.CharacterName, new[] { "Normal", "Always", "Never", "Always Stun"}, 0));
            rConfig.Add(new MenuBool("autoRks", "Auto R KS", true).SetValue(true));
            rConfig.Add(new MenuBool("autoRcombo", "Auto R Combo if stun is ready", true).SetValue(true));
            rConfig.Add(new MenuSlider("rCount", "Auto R x enemies", 3, 2, 5));
            rConfig.Add(new MenuBool("tibers", "Tibbers Auto Pilot", true).SetValue(true));
            
            if (flash != SpellSlot.Unknown)
            {
                rConfig.Add(new MenuSlider("rCountFlash", "Auto flash + R stun x enemies",4, 2, 5));
            }

            //var farmMenu = FarmMenu.Add(new Menu("farmMenu", "Farm"));
            FarmMenu.Add(new MenuBool("farmQ", "Farm Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmW", "Lane clear W", true).SetValue(false));
            
            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.IsAlly && obj is AIMinionClient && obj.Name.ToLower() == "tibbers")
            {
                Tibbers = obj as AIBaseClient ;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.HasBuff("Recall"))
                return;

            HaveStun = Player.HasBuff("pyromania_particle");

            SetMana();

            if (R.IsReady() && (Program.LagFree(1) || Program.LagFree(3)) && !HaveTibers)
            {
                var realRange = R.Range;

                if (flash.IsReady())
                    realRange = FR.Range;

                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(realRange) && OktwCommon.ValidUlt(enemy)))
                {
                    if (enemy.IsValidTarget(R.Range))
                    {
                        int Rmode = Config["rConfig"]["ultiManager"]["UM" + enemy.CharacterName].GetValue<MenuList>().Index;

                        if (Rmode == 2)
                            continue;

                        var poutput = R.GetPrediction(enemy, true);
                        var aoeCount = poutput.AoeTargetsHitCount;

                        if (Rmode == 1)
                            R.Cast(poutput.CastPosition);

                        if (Rmode == 3 && HaveStun)
                            R.Cast(poutput.CastPosition);

                        if (aoeCount >= Config["rConfig"]["rCount"].GetValue<MenuSlider>().Value && Config["rConfig"]["rCount"].GetValue<MenuSlider>().Value > 0)
                            R.Cast(poutput.CastPosition);
                        else if (Program.Combo && HaveStun && Config["rConfig"]["autoRcombo"].GetValue<MenuBool>().Enabled)
                            R.Cast(poutput.CastPosition);
                        else if (Config["rConfig"]["autoRks"].GetValue<MenuBool>().Enabled)
                        {
                            var comboDmg = OktwCommon.GetKsDamage(enemy, R);

                            if (W.IsReady() && RMANA + WMANA < Player.Mana)
                                comboDmg += W.GetDamage(enemy);

                            if (Q.IsReady() && RMANA + WMANA + QMANA < Player.Mana)
                                comboDmg += Q.GetDamage(enemy);

                            if (enemy.Health < comboDmg)
                                R.Cast(poutput.CastPosition);
                        }
                    }
                    else if(HaveStun && flash.IsReady())
                    {
                        var poutputFlas = FR.GetPrediction(enemy, true);
                        var aoeCountFlash = poutputFlas.AoeTargetsHitCount;
                        if (HaveStun && aoeCountFlash >= Config["rConfig"]["rCountFlash"].GetValue<MenuSlider>().Value && Config["rConfig"]["rCountFlash"].GetValue<MenuSlider>().Value > 0)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(flash, poutputFlas.CastPosition);
                            R.Cast(poutputFlas.CastPosition);
                        }
                    }
                }
            }

            var t = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (t.IsValidTarget() && Program.LagFree(2))
            {
                if (Q.IsReady() && Config["qConfig"]["autoQ"].GetValue<MenuBool>().Enabled)
                {
                    if (Program.Combo && RMANA + WMANA < Player.Mana)
                        Q.Cast(t);
                    else if (Program.Harass && RMANA + WMANA + QMANA < Player.Mana && Config["qConfig"]["harassQ"].GetValue<MenuBool>().Enabled && HarassMenu["harass" + t.CharacterName].GetValue<MenuBool>().Enabled)
                        Q.Cast(t);
                    else
                    {
                        var qDmg = OktwCommon.GetKsDamage(t, Q);
                        var wDmg = W.GetDamage(t);
                        if (qDmg > t.Health)
                            Q.Cast(t);
                        else if (qDmg + wDmg > t.Health && Player.Mana > QMANA + WMANA)
                            Q.Cast(t);
                    }
                }
                if (W.IsReady() && Config["wConfig"]["autoW"].GetValue<MenuBool>().Enabled && t.IsValidTarget(W.Range))
                {
                    var poutput = W.GetPrediction(t, true);
                    var aoeCount = poutput.AoeTargetsHitCount;

                    if (Program.Combo && RMANA + WMANA < Player.Mana)
                        W.Cast(poutput.CastPosition);
                    else if (Program.Harass && RMANA + WMANA + QMANA < Player.Mana && Config["wConfig"]["harassW"].GetValue<MenuBool>().Enabled)
                        W.Cast(poutput.CastPosition);
                    else
                    {
                        var wDmg = OktwCommon.GetKsDamage(t, W);
                        var qDmg = Q.GetDamage(t);
                        if (wDmg > t.Health)
                            W.Cast(poutput.CastPosition);
                        else if (qDmg + wDmg > t.Health && Player.Mana > QMANA + WMANA)
                            W.Cast(poutput.CastPosition);
                    }
                }
            }
            else if(Q.IsReady() || W.IsReady())
            {
                if (FarmMenu["farmQ"].GetValue<MenuBool>().Enabled)
                {
                    if (Config["supportMode"].GetValue<MenuBool>().Enabled)
                    {
                        if (Program.LaneClear && Player.Mana > RMANA + QMANA)
                            farm();
                    }
                    else
                    {
                        if ((!HaveStun || Program.LaneClear) && Program.Harass)
                            farm();
                    }
                }
            }

            if (Program.LagFree(3))
            {
                if (!HaveStun)
                {
                    if (E.IsReady() && !Program.LaneClear && Config["eConfig"]["autoE"].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + EMANA + QMANA + WMANA)
                        E.Cast();
                    else if (W.IsReady() && Player.InFountain())
                        W.Cast(Player.Position);
                }
                if (R.IsReady())
                {
                    if (Config["rConfig"]["tibers"].GetValue<MenuBool>().Enabled && HaveTibers && Tibbers != null && Tibbers.IsValid)
                    {
                        var enemy = HeroManager.Enemies.Where(x => x.IsValidTarget() && Tibbers.Distance(x.Position) < 1000 && !x.IsUnderEnemyTurret()).OrderBy(x => x.Distance(Tibbers)).FirstOrDefault();
                        if(enemy != null)
                        {

                            if (Tibbers.Distance(enemy.Position) > 200)
                                ObjectManager.Player.IssueOrder(GameObjectOrder.PetMove, enemy);
                            else
                                ObjectManager.Player.IssueOrder(GameObjectOrder.PetAttack, enemy);
                        }
                        else
                        {
                            var annieTarget = Orbwalker.GetTarget() as AIBaseClient;
                            if (annieTarget != null)
                            {
                                if (Tibbers.Distance(annieTarget.Position) > 200)
                                    ObjectManager.Player.IssueOrder(GameObjectOrder.PetMove, annieTarget);
                                else
                                    ObjectManager.Player.IssueOrder(GameObjectOrder.PetAttack, annieTarget);
                            }
                            else if (Tibbers.IsUnderEnemyTurret())
                            {
                                ObjectManager.Player.IssueOrder(GameObjectOrder.PetMove, Player);
                            }
                        }
                    }
                    else
                    {
                        Tibbers = null;
                    }
                }
            }
        }
        
        private void farm()
        {
            if(Program.LaneClear)
            { 
                var mobs = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady())
                        W.Cast(mob);
                    else if (Q.IsReady())
                        Q.Cast(mob);
                }
            }

            var minionsList = Cache.GetMinions(Player.ServerPosition, Q.Range);
            if (Q.IsReady())
            {
                var minion = minionsList.FirstOrDefault(x => HealthPrediction.LaneClearHealthPrediction(x, 250, 50) < Q.GetDamage(x) && x.Health > Player.GetAutoAttackDamage(x));
                Q.Cast(minion);
            }
            else if (FarmSpells && W.IsReady() && FarmMenu["farmW"].GetValue<MenuBool>().Enabled)
            {
                var farmLocation = W.GetCircularFarmLocation(minionsList, W.Width);
                if (farmLocation.MinionsHit >= FarmMinions)
                    W.Cast(farmLocation.Position);
            }
        }

        private bool HaveTibers
        {
            get { return Player.HasBuff("infernalguardiantimer"); }
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

            if (!R.IsReady() || HaveTibers)
                RMANA = 0;
            else 
                RMANA = R.Instance.ManaCost;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config["draw"]["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (Config["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (Q.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }
            if (Config["draw"]["wRange"].GetValue<MenuBool>().Enabled)
            {
                if (Config["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (W.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }

            if (Config["draw"]["rRange"].GetValue<MenuBool>().Enabled)
            {
                if (Config["draw"]["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (R.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range + R.Width / 2, System.Drawing.Color.Gray, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range + R.Width / 2, System.Drawing.Color.Gray, 1);
            }
        }
    }
}