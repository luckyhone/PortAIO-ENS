using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using hikiMarksmanRework.Core.Drawings;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;
using SharpDX;
using SPrediction;
using Geometry = LeagueSharpCommon.Geometry.Geometry;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Prediction = EnsoulSharp.SDK.Prediction;
using Render = LeagueSharpCommon.Render;

namespace hikiMarksmanRework.Champions
{
    class Lucian
    {
        public Lucian()
        {
            LucianOnLoad();
        }
        private static readonly Render.Sprite HikiSprite = new Render.Sprite(PortAIO.Properties.Resources.logo, new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
        private static void LucianOnLoad()
        {
            LucianMenu.Config =
                new Menu("hikiMarksman:AIO - Lucian", "hikiMarksman:AIO - Lucian", true);
            {
                LucianSpells.Init();
                LucianMenu.MenuInit();
            }
            LucianMenu.Config.SetFontColor(
                SharpDX.Color.Gold);

            Game.Print("<font color='#ff3232'>hikiMarksman:AIO - Lucian: </font><font color='#00FF00'>loaded! You can rekt everyone with this assembly</font>", ObjectManager.Player.CharacterName);
            Game.Print(string.Format("<font color='#ff3232'>hikiMarksman:AIO - </font><font color='#00FF00'>Assembly Version: </font><font color='#ff3232'><b>{0}</b></font> ", typeof(Program).Assembly.GetName().Version));
            Game.Print("<font color='#ff3232'>If you like this assembly feel free to upvote on Assembly Database</font>");


            HikiSprite.Add(0);
            HikiSprite.OnDraw();
            DelayAction.Add(8000, () => HikiSprite.Remove());

            Game.OnUpdate += LucianOnUpdate;
            Spellbook.OnCastSpell += LucianOnCastSpell;
            AIBaseClient.OnDoCast += LucianOnSpellCast;
            Drawing.OnDraw += LucianOnDraw;
        }
        public static bool UltActive
        {
            get { return ObjectManager.Player.HasBuff("LucianR"); }
        }

        private static void LucianOnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            
            if (UltActive && (args.Slot == SpellSlot.R) && sender.Owner.IsMe)
            {
                args.Process = false;
                Orbwalker.AttackEnabled = (false);
            }
            else
            {
                Orbwalker.AttackEnabled = true;
                args.Process = true;
            }
        }

        private static void ECast(AIHeroClient enemy)
        {
            var range = ObjectManager.Player.GetRealAutoAttackRange(enemy);
            var path = Geometry.CircleCircleIntersection(Geometry.To2D(ObjectManager.Player.ServerPosition),
                Prediction.GetPrediction(enemy, 0.25f).UnitPosition.ToVector2(), LucianSpells.E.Range, range);
            
            if (path.Count() > 0)
            {
                var epos = path.MinOrDefault(x => x.Distance(Game.CursorPos));
                if (epos.ToVector3().IsUnderEnemyTurret() || Vector3Extensions.IsWall(epos.ToVector3()))
                {
                    return;
                }

                if (epos.ToVector3().CountEnemyHeroesInRange(LucianSpells.E.Range - 100) > 0)
                {
                    return;
                }
                LucianSpells.E.Cast(epos);
            }
            if (path.Count() == 0)
            {
                var epos = ObjectManager.Player.ServerPosition.Extend(enemy.ServerPosition, -LucianSpells.E.Range);
                if (epos.IsUnderEnemyTurret() || Vector3Extensions.IsWall(epos))
                {
                    return;
                }
                {
                    return;
                }

                // no intersection or target to close
                LucianSpells.E.Cast(ObjectManager.Player.ServerPosition.Extend(enemy.ServerPosition, -LucianSpells.E.Range));
            }
        }
        private static void LucianOnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Orbwalker.IsAutoAttack(args.SData.Name) && args.Target is AIHeroClient && args.Target.IsValid)
            {
                if (LucianSpells.Q.IsReady() && Helper.LEnabled("lucian.q.combo") &&
                    ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.Q.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    LucianSpells.Q.CastOnUnit(((AIHeroClient)args.Target));
                }
                if (LucianSpells.W.IsReady() && Helper.LEnabled("lucian.w.combo") &&
                    ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.W.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff")
                    && LucianSpells.W.GetPrediction(((AIHeroClient)args.Target)).Hitchance >= HitChance.Medium)
                {
                    LucianSpells.W.Cast(((AIHeroClient)args.Target).Position);
                }
                if (LucianSpells.E.IsReady() && Helper.LEnabled("lucian.e.combo") &&
                    ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.Q2.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    ECast(((AIHeroClient)args.Target));
                }
            }
            else if (sender.IsMe && Orbwalker.IsAutoAttack(args.SData.Name) && args.Target is AIMinionClient && args.Target.IsValid && ((AIMinionClient)args.Target).Team == GameObjectTeam.Neutral
                && ObjectManager.Player.ManaPercent > Helper.LSlider("lucian.clear.mana"))
            {
                if (LucianSpells.Q.IsReady() && Helper.LEnabled("lucian.q.combo") &&
                    ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.Q.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    LucianSpells.Q.CastOnUnit(((AIMinionClient)args.Target));
                }
                if (LucianSpells.W.IsReady() && Helper.LEnabled("lucian.w.combo") &&
                    ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.W.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    LucianSpells.W.Cast(((AIMinionClient)args.Target).Position);
                }
                if (LucianSpells.E.IsReady() && Helper.LEnabled("lucian.e.combo") &&
                   ((AIMinionClient)args.Target).IsValidTarget(1000) &&
                    Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    LucianSpells.E.Cast(Game.CursorPos);
                }

            }
        }
        

        private static void LucianOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    break;
            }
            if (LucianMenu.Config["lucian.semi.manual.ult"].GetValue<MenuKeyBind>().Active)
            {
                SemiManual();
            }
        }

        private static void SemiManual()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(LucianSpells.Q.Range) && 
                LucianSpells.R.GetPrediction(x).CollisionObjects.Count == 0))
            {
                LucianSpells.R.Cast(enemy);
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Helper.LSlider("lucian.harass.mana"))
            {
                return;
            }
            if (LucianSpells.Q.IsReady() || LucianSpells.Q2.IsReady() && Helper.LEnabled("lucian.q.harass") && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                HarassQCast();
            }
            if (LucianSpells.W.IsReady() && Helper.LEnabled("lucian.w.harass") && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(LucianSpells.W.Range) && LucianSpells.W.GetPrediction(x).Hitchance >= HitChance.Medium))
                {
                    LucianSpells.W.Cast(enemy);
                }
            }
        }

        private static void HarassQCast()
        {
            switch (LucianMenu.Config["Harass Settings"]["lucian.q.type"].GetValue<MenuList>().Index)
            {
                case 0:
                        var minions = ObjectManager.Get<AIMinionClient>().Where(o => o.IsValidTarget(LucianSpells.Q.Range));
                        var target = ObjectManager.Get<AIHeroClient>().Where(x => x.IsValidTarget(LucianSpells.Q2.Range)).FirstOrDefault(x => LucianMenu.Config["Harass Settings"]["Q Whitelist (Extended)"]["lucian.white" + x.CharacterName].GetValue<MenuBool>().Enabled);
                        if (target != null && minions != null)
                        {
                            if (target.Distance(ObjectManager.Player.Position) > LucianSpells.Q.Range && target.CountEnemyHeroesInRange(LucianSpells.Q2.Range) > 0)
                            {
                                foreach (var minion in minions)
                                {
                                    if (LucianSpells.Q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, LucianSpells.Q2.Range), 0, HitChance.VeryHigh))
                                    {
                                        LucianSpells.Q2.CastOnUnit(minion);
                                    }
                                }
                            }
                        }
                        break;
                case 1:
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(LucianSpells.Q.Range)))
                    {
                        LucianSpells.Q.CastOnUnit(enemy);
                    }
                    break;
            }
        }

        private static void Clear()
        {
            if (ObjectManager.Player.ManaPercent < Helper.LSlider("lucian.clear.mana"))
            {
                return;
            }
            if (LucianSpells.Q.IsReady() && Helper.LEnabled("lucian.q.clear") && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                

                foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.ServerPosition, LucianSpells.Q.Range, MinionManager.MinionTypes.All,
                             MinionManager.MinionTeam.NotAlly))
                {
                    var prediction = Prediction.GetPrediction(minion, LucianSpells.Q.Delay,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRadius);

                    var collision = LucianSpells.Q.GetCollision(ObjectManager.Player.Position.ToVector2(),
                        new List<Vector2> { prediction.UnitPosition.ToVector2() });

                    foreach (var cs in collision)
                    {
                        if (collision.Count >= Helper.LSlider("lucian.q.minion.hit.count"))
                        {
                            if (collision.Last().Distance(ObjectManager.Player) -
                                collision[0].Distance(ObjectManager.Player) <= 600
                                && collision[0].Distance(ObjectManager.Player) <= 500)
                            {
                                LucianSpells.Q.Cast(cs);
                            }
                        }
                    }

                }
            }
            if (LucianSpells.W.IsReady() && Helper.LEnabled("lucian.w.clear") && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                if (LucianSpells.W.GetCircularFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, LucianSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).MinionsHit >= Helper.LSlider("lucian.w.minion.hit.count"))
                {
                    LucianSpells.W.Cast(LucianSpells.W.GetCircularFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, LucianSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).Position);
                }
            }
        }

        private static void LucianOnDraw(EventArgs args)
        {
            LucianDrawing.Init();
        }
    }
}