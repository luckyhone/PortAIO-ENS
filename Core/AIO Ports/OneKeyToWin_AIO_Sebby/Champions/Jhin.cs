
using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SebbyLib;
using SharpDX;
using SebbyLib;
using ShadowTracker;
using SPrediction;
using EnumerableExtensions = EnsoulSharp.SDK.EnumerableExtensions;
using HealthPrediction = SebbyLib.HealthPrediction;
using Prediction = SebbyLib.Prediction;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Jhin : Base
    {
        private bool Ractive = false;
        private Vector3 rPosLast;
        private AIHeroClient rTargetLast;
        private Vector3 rPosCast;

        private Items.Item
                    FarsightOrb = new Items.Item(3342, 4000f),
                    ScryingOrb = new Items.Item(3363, 3500f);

        private static string[] Spells =
        {
            "katarinar","drain","consume","absolutezero", "staticfield","reapthewhirlwind","jinxw","jinxr","shenstandunited","threshe","threshrpenta","threshq","meditate","caitlynpiltoverpeacemaker", "volibearqattack",
            "cassiopeiapetrifyinggaze","ezrealtrueshotbarrage","galioidolofdurand","luxmalicecannon", "missfortunebullettime","infiniteduress","alzaharnethergrasp","lucianq","velkozr","rocketgrabmissile"
        };

        public Jhin()
        {
            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 2900);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 3500);

            W.SetSkillshot(0.75f, 40, 10000, false, SpellType.Line);
            E.SetSkillshot(1f, 120, 1600, false, SpellType.Circle);
            R.SetSkillshot(0.24f, 80, 5000, false, SpellType.Line);

            var drawMenu = Local.Add(new Menu("draw", "Draw"));
            drawMenu.Add(new MenuBool("qRange", "Q range", true).SetValue(false));
            drawMenu.Add(new MenuBool("wRange", "W range", true).SetValue(false));
            drawMenu.Add(new MenuBool("eRange", "E range", true).SetValue(false));
            drawMenu.Add(new MenuBool("rRange", "R range", true).SetValue(false));
            drawMenu.Add(new MenuBool("onlyRdy", "Draw only ready spells", true).SetValue(true));
            drawMenu.Add(new MenuBool("rRangeMini", "R range minimap", true).SetValue(true));

            var qConfigMenu = Local.Add(new Menu("qConfig", "Q Config"));
            qConfigMenu.Add(new MenuBool("autoQ", "Auto Q", true).SetValue(true));
            qConfigMenu.Add(new MenuBool("harassQ", "Harass Q", true).SetValue(true));
            qConfigMenu.Add(new MenuBool("Qminion", "Q on minion", true).SetValue(true));

            var wConfigMenu = Local.Add(new Menu("wConfig", "W Config"));
            wConfigMenu.Add(new MenuBool("autoW", "Auto W", true).SetValue(true));
            wConfigMenu.Add(new MenuBool("autoWcombo", "Auto W only in combo", true).SetValue(false));
            wConfigMenu.Add(new MenuBool("harassW", "Harass W", true).SetValue(true));
            wConfigMenu.Add(new MenuBool("Wmark", "W marked only (main target)", true).SetValue(true));
            wConfigMenu.Add(new MenuBool("Wmarkall", "W marked (all enemys)", true).SetValue(true));
            wConfigMenu.Add(new MenuBool("Waoe", "W aoe (above 2 enemy)", true).SetValue(true));
            wConfigMenu.Add(new MenuBool("autoWcc", "Auto W CC enemy", true).SetValue(true));
            wConfigMenu.Add(new MenuSlider("MaxRangeW", "Max W range", 2500, 0,2500));

            var eConfig = Local.Add(new Menu("eConfig", "E Config"));
            eConfig.Add(new MenuBool("autoE", "Auto E on hard CC", true).SetValue(true));
            eConfig.Add(new MenuBool("bushE", "Auto E bush", true).SetValue(true));
            eConfig.Add(new MenuBool("Espell", "E on special spell detection", true).SetValue(true));
            eConfig.Add(new MenuList("EmodeCombo", "E combo mode", new[] { "always", "run - cheese", "disable" }, 1));
            eConfig.Add(new MenuSlider("Eaoe", "Auto E x enemies", 3, 0, 5));
            var EGapCloser = eConfig.Add(new Menu("EGapCloser","E Gap Closer"));
            EGapCloser.Add(new MenuList("EmodeGC", "Gap Closer position mode", new[] { "Dash end position", "My hero position" }, 0));
            foreach (var enemy in GameObjects.EnemyHeroes)
                EGapCloser.Add(new MenuBool("EGCchampion" + enemy.CharacterName, enemy.CharacterName, true).SetValue(true));

            var rConfig = Local.Add(new Menu("rConfig", "R Config"));
            rConfig.Add(new MenuBool("autoR", "Enable R", true).SetValue(true));
            rConfig.Add(new MenuBool("Rvisable", "Don't shot if enemy is not visable", true).SetValue(false));
            rConfig.Add(new MenuBool("Rks", "Auto R if can kill in 3 hits", true).SetValue(true));
            rConfig.Add(new MenuKeyBind("useR", "Semi-manual cast R key", Keys.T, KeyBindType.Press)); //32 == space
            rConfig.Add(new MenuSlider("MaxRangeR", "Max R range", 3000,  0,3500));
            rConfig.Add(new MenuSlider("MinRangeR", "Min R range", 1000,  0,3500));
            rConfig.Add(new MenuSlider("Rsafe", "R safe area", 1000,  0,2000));
            rConfig.Add(new MenuBool("trinkiet", "Auto blue trinkiet", true).SetValue(true));

            FarmMenu.Add(new MenuBool("farmQ", "Lane clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmW", "Lane clear W", true).SetValue(true));
            FarmMenu.Add(new MenuBool("farmE", "Lane clear E", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleE", "Jungle clear E", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleQ", "Jungle clear Q", true).SetValue(true));
            FarmMenu.Add(new MenuBool("jungleW", "Jungle clear W", true).SetValue(true));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R)
            {
                if (Local["rConfig"]["trinkiet"].GetValue<MenuBool>().Enabled && !IsCastingR)
                {
                    if (Player.Level < 9)
                        ScryingOrb.Range = 2500;
                    else
                        ScryingOrb.Range = 3500;

                    if (ScryingOrb.IsReady)
                        ScryingOrb.Cast(rPosLast);
                    if (FarsightOrb.IsReady)
                        FarsightOrb.Cast(rPosLast);
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if(sender.IsMe && args.SData.Name.ToLower() == "jhinr")
            {
                rPosCast = args.End;
            }
            if (!E.IsReady() || sender.IsMinion() || !sender.IsEnemy || !Local["eConfig"]["Espell"].GetValue<MenuBool>().Enabled || !sender.IsValid() || !sender.IsValidTarget(E.Range))
                return;

            var foundSpell = EnumerableExtensions.Find(Spells, x => args.SData.Name.ToLower() == x);
            if (foundSpell != null)
            {
                E.Cast(sender.Position);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender,AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (E.IsReady() && Player.Mana > RMANA + WMANA)
            {
                var t = sender;
                if (t.IsValidTarget(W.Range) && Local["eConfig"]["EGapCloser"]["EGCchampion" + t.CharacterName].GetValue<MenuBool>().Enabled)
                {
                    if (Local["eConfig"]["EGapCloser"]["EmodeGC"].GetValue<MenuList>().Index == 0)
                        E.Cast(gapcloser.EndPosition);
                    else
                        E.Cast(Player.ServerPosition);
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

            if (Program.LagFree(1) && R.IsReady() )
                LogicR();

            if (IsCastingR)
            {
                //OktwCommon.blockMove = true;
                //OktwCommon.blockAttack = true;
                Orbwalker.AttackEnabled = false;
                Orbwalker.MoveEnabled = false;
                return;
            }
            else
            {
                //OktwCommon.blockMove = false;
                //OktwCommon.blockAttack = false;
                Orbwalker.AttackEnabled = true;
                Orbwalker.MoveEnabled = true;
            }


            if (Program.LagFree(4) && E.IsReady() && Orbwalker.CanMove())
                LogicE();

            if (Program.LagFree(2) && Q.IsReady() && Local["qConfig"]["autoQ"].GetValue<MenuBool>().Enabled)
                LogicQ();

            if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp && Local["wConfig"]["autoW"].GetValue<MenuBool>().Enabled)
                LogicW();
        }

        private void LogicR()
        {
            if (!IsCastingR)
                R.Range = Local["rConfig"]["MaxRangeR"].GetValue<MenuSlider>().Value;
            else
                R.Range = 3500;

            var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (t.IsValidTarget())
            {
                rPosLast = R.GetPrediction(t).CastPosition;
                if (Local["rConfig"]["useR"].GetValue<MenuKeyBind>().Active && !IsCastingR)
                {
                    R.Cast(rPosLast);
                    rTargetLast = t;
                }
                
                if (!IsCastingR && Local["rConfig"]["Rks"].GetValue<MenuBool>().Enabled && Local["rConfig"]["autoR"].GetValue<MenuBool>().Enabled
                    && GetRdmg(t) * 4 > t.Health && t.CountAllyHeroesInRange(700) == 0 && Player.CountEnemyHeroesInRange(Local["rConfig"]["Rsafe"].GetValue<MenuSlider>().Value) == 0 
                    && Player.Distance(t) > Local["rConfig"]["MinRangeR"].GetValue<MenuSlider>().Value
                    && !Player.IsUnderEnemyTurret() && OktwCommon.ValidUlt(t) && !OktwCommon.IsSpellHeroCollision(t, R))
                {
                    R.Cast(rPosLast);
                    rTargetLast = t;
                }
                if (IsCastingR)
                {
                    if(InCone(t.ServerPosition))
                        R.Cast(t);
                    else
                    {
                        foreach(var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(R.Range) && InCone(t.ServerPosition)).OrderBy(enemy => enemy.Health))
                        {
                            R.Cast(t);
                            rPosLast = R.GetPrediction(enemy).CastPosition;
                            rTargetLast = enemy;
                        }
                    }
                }
            }
            else if (IsCastingR && rTargetLast != null && !rTargetLast.IsDead)
            {
                if(!Local["rConfig"]["Rvisable"].GetValue<MenuBool>().Enabled && InCone(rTargetLast.Position) && InCone(rPosLast))
                    R.Cast(rPosLast);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (t.IsValidTarget())
            {
                var wDmg = GetWdmg(t);
                if (wDmg > t.Health - OktwCommon.GetIncomingDamage(t))
                    Program.CastSpell(W, t);

                if (Local["wConfig"]["autoWcombo"].GetValue<MenuBool>().Enabled && !Program.Combo)
                    return;

                if (Player.CountEnemyHeroesInRange(400) > 1 || Player.CountEnemyHeroesInRange(250) > 0)
                    return;

                if (t.HasBuff("jhinespotteddebuff") || !Local["wConfig"]["Wmark"].GetValue<MenuBool>().Enabled )
                {
                    if (Player.Distance(t) < Local["wConfig"]["MaxRangeW"].GetValue<MenuSlider>().Value)
                    {
                        if (Program.Combo && Player.Mana > RMANA + WMANA)
                            Program.CastSpell(W, t);
                        else if (Program.Harass && Local["wConfig"]["harassW"].GetValue<MenuBool>().Enabled && Config["harass" + t.CharacterName].GetValue<MenuBool>().Enabled
                            && Player.Mana > RMANA + WMANA + QMANA + WMANA && OktwCommon.CanHarras())
                            Program.CastSpell(W, t);
                    }
                }

                if (!Program.None && Player.Mana > RMANA + WMANA)
                {
                    if(Local["wConfig"]["Waoe"].GetValue<MenuBool>().Enabled)
                        W.CastIfWillHit(t, 2);

                    if (Local["wConfig"]["autoWcc"].GetValue<MenuBool>().Enabled)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                            Program.CastSpell(W, enemy);
                    }
                    if (Local["wConfig"]["Wmarkall"].GetValue<MenuBool>().Enabled)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(W.Range) && enemy.HasBuff("jhinespotteddebuff")))
                            Program.CastSpell(W, enemy);
                    }
                }
            }
            if (FarmSpells && FarmMenu["farmW"].GetValue<MenuBool>().Enabled)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, W.Range);
                var farmPosition = W.GetLineFarmLocation(minionList, W.Width);

                if (farmPosition.MinionsHit >= FarmMinions)
                    W.Cast(farmPosition.Position);
            }
        }

        private void LogicE()
        {
            if (Local["eConfig"]["autoE"].GetValue<MenuBool>().Enabled)
            {
                var trapPos = OktwCommon.GetTrapPos(E.Range);
                if (!trapPos.IsZero)
                    E.Cast(trapPos);

                foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                    E.Cast(enemy);
            }

            var t = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (t.IsValidTarget() && Local["eConfig"]["EmodeCombo"].GetValue<MenuList>().Index != 2)
            {
                if (Program.Combo && !Player.IsWindingUp)
                {
                    if (Local["eConfig"]["EmodeCombo"].GetValue<MenuList>().Index == 1)
                    {
                        if (E.GetPrediction(t).CastPosition.Distance(t.Position) > 100)
                        {
                            if (Player.Position.Distance(t.ServerPosition) > Player.Position.Distance(t.Position))
                            {
                                if (t.Position.Distance(Player.ServerPosition) < t.Position.Distance(Player.Position))
                                    Program.CastSpell(E, t);
                            }
                            else
                            {
                                if (t.Position.Distance(Player.ServerPosition) > t.Position.Distance(Player.Position))
                                    Program.CastSpell(E, t);
                            }
                        }
                    }
                    else
                    {
                        Program.CastSpell(E, t);
                    }
                }

                E.CastIfWillHit(t, Local["eConfig"]["Eaoe"].GetValue<MenuSlider>().Value);
            }
            else if (FarmSpells && FarmMenu["farmE"].GetValue<MenuBool>().Enabled)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, E.Range);
                var farmPosition = E.GetCircularFarmLocation(minionList, E.Width);

                if (farmPosition.MinionsHit >= FarmMinions)
                    E.Cast(farmPosition.Position);
            }
        }

        private void LogicQ()
        {
            var torb = Orbwalker.GetTarget();

            if (torb == null || torb.Type != GameObjectType.AIHeroClient)
            {
                if (Local["qConfig"]["Qminion"].GetValue<MenuBool>().Enabled)
                {
                    var t = TargetSelector.GetTarget(Q.Range + 300, DamageType.Physical);
                    if (t.IsValidTarget() )
                    {
                        
                        var minion = Cache.GetMinions(Prediction.GetPrediction(t, 0.1f).CastPosition, 300).Where(minion2 => minion2.IsValidTarget(Q.Range)).OrderBy(x => x.Distance(t)).FirstOrDefault();
                        if (minion.IsValidTarget())
                        {
                            if (t.Health < GetQdmg(t))
                                Q.CastOnUnit(minion);
                            if (Program.Combo && Player.Mana > RMANA + EMANA)
                                Q.CastOnUnit(minion);
                            else if (Program.Harass && Local["qConfig"]["harassQ"].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + EMANA + WMANA + EMANA && Config["Harass" + t.CharacterName].GetValue<MenuBool>().Enabled)
                                Q.CastOnUnit(minion);
                        }
                    }
                }

            }
            else if(!Orbwalker.CanAttack() && !Player.IsWindingUp)
            {
                var t = torb as AIHeroClient;
                if (t.Health < GetQdmg(t) + GetWdmg(t))
                    Q.CastOnUnit(t);
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Q.CastOnUnit(t);
                else if (Program.Harass && Local["qConfig"]["harassQ"].GetValue<MenuBool>().Enabled && Player.Mana > RMANA + QMANA + WMANA + EMANA && Config["Harass" + t.CharacterName].GetValue<MenuBool>().Enabled)
                    Q.CastOnUnit(t);
            }
            if (FarmSpells && Local["qConfig"]["farmQ"].GetValue<MenuBool>().Enabled)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, Q.Range);
                
                if (minionList.Count >= FarmMinions)
                {
                    var minionAttack = minionList.FirstOrDefault(x => Q.GetDamage(x) > HealthPrediction.GetHealthPrediction(x, 300));
                    if(minionAttack.IsValidTarget())
                        Q.CastOnUnit(minionAttack);
                }
                    
            }
        }


        private bool InCone(Vector3 Position)
        {
            var range = R.Range;
            var angle = 70f * (float)Math.PI / 180;
            var end2 = rPosCast.To2D() - Player.Position.To2D();
            var edge1 = end2.Rotated(-angle / 2);
            var edge2 = edge1.Rotated(angle);

            var point = Position.To2D() - Player.Position.To2D();
            if (point.Distance(new Vector2()) < range * range && edge1.CrossProduct(point) > 0 && point.CrossProduct(edge2) > 0)
                return true;

            return false;
        }

        private void Jungle()
        {
            if (Program.LaneClear)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionManager.MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    if (W.IsReady() && FarmMenu["jungleW"].GetValue<MenuBool>().Enabled)
                    {
                        W.Cast(mob.ServerPosition);
                        return;
                    }
                    if (E.IsReady() && FarmMenu["jungleE"].GetValue<MenuBool>().Enabled)
                    {
                        E.Cast(mob.ServerPosition);
                        return;
                    }
                    if (Q.IsReady() && FarmMenu["jungleQ"].GetValue<MenuBool>().Enabled)
                    {
                        Q.CastOnUnit(mob);
                        return;
                    }
                }
            }
        }

        private bool IsCastingR { get { return R.Instance.Name == "JhinRShot"; } }

        private double GetRdmg(AIBaseClient target)
        {
            var damage = ( -25 + 75 * R.Level + 0.2 * Player.FlatPhysicalDamageMod) * (1 + (100 - target.HealthPercent) * 0.02);

            return Player.CalculateDamage(target, DamageType.Physical, damage);
        }

        private double GetWdmg(AIBaseClient target)
        {
            var damage = 55 + W.Level * 35 + 0.7 * Player.FlatPhysicalDamageMod;

            return Player.CalculateDamage(target, DamageType.Physical, damage);
        }

        private double GetQdmg(AIBaseClient target)
        {
            var damage = 35 + Q.Level * 25 + 0.4 * Player.FlatPhysicalDamageMod;

            return Player.CalculateDamage(target, DamageType.Physical, damage);
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
                RMANA = WMANA - Player.PARRegenRate * W.Instance.Cooldown;
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
            if (Local["rRangeMini"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (R.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Aqua, 1);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Aqua, 1);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Local["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (Q.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan);
            }
            if (Local["wRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (W.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange);
            }
            if (Local["eRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (E.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow);
            }
            if (Local["rRange"].GetValue<MenuBool>().Enabled)
            {
                if (Local["onlyRdy"].GetValue<MenuBool>().Enabled)
                {
                    if (R.IsReady())
                        LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray);
                }
                else
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray);
            }
        }
    }
}