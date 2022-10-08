using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using LeagueSharpCommon;
using PortAIO;
using SPrediction;
using Dash = EnsoulSharp.SDK.Dash;
using Geometry = EnsoulSharp.SDK.Geometry;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;
using Render = EnsoulSharp.SDK.Render;


namespace Flowers_ADC_Series.Pluging
{
    using ADCCOMMON;
    using Color = System.Drawing.Color;
    internal class Lucian : Logic
    {
        private int CastSpellTime;

        public Lucian()
        {
            Q = new Spell(SpellSlot.Q, 650f);
            QExtend = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, 1200f);

            Q.SetTargetted(0.25f, float.MaxValue);
            QExtend.SetSkillshot(0.35f, 25f, float.MaxValue, false, SpellType.Line);
            W.SetSkillshot(0.3f, 80f, 1600, true, SpellType.Line);
            E.SetSkillshot(0.25f, 1f, float.MaxValue, false, SpellType.Line);
            R.SetSkillshot(0.2f, 110f, 2800, true, SpellType.Line);

            var comboMenu = Menu.Add(new Menu("Combo", "Combo"));
            {
                comboMenu.Add(new MenuBool("ComboQ", "Use Q", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboQExtended", "Use Q Extended", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboW", "Use W", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboE", "Use E", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboELogic", "Use E|First E Logic?", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboR", "Use R", true).SetValue(true));
            }

            var harassMenu = Menu.Add(new Menu("Harass", "Harass"));
            {
                harassMenu.Add(new MenuBool("HarassQ", "Use Q", true).SetValue(true));
                harassMenu.Add(new MenuBool("HarassQExtended", "Use Q Extended", true).SetValue(true));
                harassMenu.Add(new MenuBool("HarassW", "Use W", true).SetValue(false));
                harassMenu.Add(
                    new MenuSlider("HarassMana", "When Player ManaPercent >= x%", 60));
            }

            var clearMenu = Menu.Add(new Menu("Clear", "Clear"));
            {
                var laneClearMenu = clearMenu.Add(new Menu("LaneClear", "LaneClear"));
                {
                    laneClearMenu.Add(new MenuBool("LaneClearQ", "Use Q", true).SetValue(true));
                    laneClearMenu.Add(new MenuBool("LaneClearW", "Use W", true).SetValue(true));
                    laneClearMenu.Add(
                        new MenuSlider("LaneClearMana", "When Player ManaPercent >= x%", 60));
                }

                var jungleClearMenu = clearMenu.Add(new Menu("JungleClear", "JungleClear"));
                {
                    jungleClearMenu.Add(new MenuBool("JungleClearQ", "Use Q", true).SetValue(true));
                    jungleClearMenu.Add(new MenuBool("JungleClearW", "Use W", true).SetValue(true));
                    jungleClearMenu.Add(new MenuBool("JungleClearE", "Use E", true).SetValue(true));
                    jungleClearMenu.Add(
                        new MenuSlider("JungleClearMana", "When Player ManaPercent >= x%", 30));
                }

                clearMenu.Add(new MenuSeparator("asdqweqwe", " - "));
                ManaManager.AddSpellFarm(clearMenu);
            }

            var killStealMenu = Menu.Add(new Menu("KillSteal", "KillSteal"));
            {
                killStealMenu.Add(new MenuBool("KillStealQ", "Use Q", true).SetValue(true));
                killStealMenu.Add(new MenuBool("KillStealW", "Use W", true).SetValue(true));
            }

            var miscMenu = Menu.Add(new Menu("Misc", "Misc"));
            {
                var eMenu = miscMenu.Add(new Menu("E Settings", "E Settings"));
                {
                    eMenu.Add(new MenuBool("Anti", "Anti Gapcloser E", true).SetValue(true));
                    eMenu.Add(new MenuBool("ShortELogic", "Smart Short E Logic", true).SetValue(true));
                    eMenu.Add(new MenuBool("underE", "Dont E to Enemy Turret", true).SetValue(true));
                    eMenu.Add(new MenuBool("ECheck", "Check Wall/ Building", true).SetValue(true));
                    eMenu.Add(new MenuBool("SafeCheck", "Safe Check", true).SetValue(true));
                }

                var rMenu = miscMenu.Add(new Menu("R Settings", "R Settings"));
                {
                    rMenu.Add(new MenuBool("RMove", "Auto Move|If R Is Casting?", true).SetValue(true));
                    rMenu.Add(new MenuBool("RYoumuu", "Auto Youmuu|If R Is Casting?", true).SetValue(true));
                }
            }

            var utilityMenu = Menu.Add(new Menu("Utility", "Utility"));
            {
                var skinMenu = utilityMenu.Add(new Menu("Skin Change", "Skin Change"));
                {
                    SkinManager.AddToMenu(skinMenu);
                }

                var autoLevelMenu = utilityMenu.Add(new Menu("Auto Levels", "Auto Levels"));
                {
                    LevelsManager.AddToMenu(autoLevelMenu);
                }

                var itemsMenu = utilityMenu.Add(new Menu("Items", "Items"));
                {
                    ItemsManager.AddToMenu(itemsMenu);
                }
            }

            var drawMenu = Menu.Add(new Menu("Drawings", "Drawings"));
            {
                drawMenu.Add(new MenuBool("DrawQ", "Draw Q Range", true).SetValue(false));
                drawMenu.Add(new MenuBool("DrawQEx", "Draw QEx Range", true).SetValue(false));
                drawMenu.Add(new MenuBool("DrawW", "Draw W Range", true).SetValue(false));
                drawMenu.Add(new MenuBool("DrawE", "Draw E Range", true).SetValue(false));
                ManaManager.AddDrawFarm(drawMenu);
                //DamageIndicator.AddToMenu(drawMenu);
            }

            Game.OnUpdate += OnUpdate;
            AIBaseClient.OnDoCast += OnProcessSpellCast;
            AIBaseClient.OnDoCast += OnSpellCast;
            AntiGapcloser.OnGapcloser += OnEnemyGapcloser;
            Drawing.OnDraw += OnDraw;
        }

        private void OnUpdate(EventArgs args)
        {
            if (Me.IsDead || Me.IsRecalling())
            {
                return;
            }

            if (Menu["Misc"]["R Settings"]["RMove"].GetValue<MenuBool>().Enabled && Me.HasBuff("LucianR"))
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                return;
            }

            KillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    FarmHarass();
                    LaneClear();
                    break;
            }
        }

        private void KillSteal()
        {
            if (Menu.GetBool("KillStealW") && W.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.Check(W.Range) && x.Health < W.GetDamage(x)))
                {
                    if (target.Check(W.Range))
                    {
                        SpellManager.PredCast(W, target, true);
                    }
                }
            }

            if (Menu["KillSteal"]["KillStealQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(QExtend.Range) && x.Health < Q.GetDamage(x)))
                {
                    if (target.Check(QExtend.Range))
                    {
                        if (target.IsValidTarget(Q.Range))
                        {
                            Q.Cast(target, true);
                        }
                        else
                        {
                            var pred = QExtend.GetPrediction(target, true);
                            var collisions = MinionManager.GetMinions(Me.ServerPosition, Q.Range, MinionManager.MinionTypes.All,
                                MinionManager.MinionTeam.NotAlly);

                            if (!collisions.Any())
                            {
                                return;
                            }

                            foreach (var minion in collisions)
                            {
                                var poly = new LeagueSharpCommon.Geometry.Geometry.Polygon.Rectangle(Me.ServerPosition,
                                    Me.ServerPosition.Extend(minion.ServerPosition, QExtend.Range), QExtend.Width);

                                if (poly.IsInside(pred.UnitPosition))
                                {
                                    Q.Cast(minion);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Combo()
        {
            if (Menu["Combo"]["ComboELogic"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                var target = TargetSelector.GetTarget(975f, DamageType.Physical);

                if (target.IsValidTarget(975f) && target.DistanceToPlayer() > ObjectManager.Player.GetRealAutoAttackRange(Me))
                {
                    if (Utils.TickCount - CastSpellTime > 400)
                    {
                        Cast_E(target, true);
                    }
                }
            }

            if (Menu["Combo"]["ComboQExtended"].GetValue<MenuBool>().Enabled && Q.IsReady() && !Dash.IsDashing(Me) && !Me.Spellbook.IsAutoAttack)
            {
                var target = TargetSelector.GetTarget(QExtend.Range, DamageType.Physical);

                if (target.Check(QExtend.Range) && target.DistanceToPlayer() > Q.Range &&
                    (!E.IsReady() || (E.IsReady() && target.DistanceToPlayer() > 975f)))
                {
                    var pred = QExtend.GetPrediction(target, true);
                    var collisions = MinionManager.GetMinions(Me.ServerPosition, Q.Range, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.NotAlly);

                    if (!collisions.Any())
                    {
                        return;
                    }

                    foreach (var minion in collisions)
                    {
                        var poly = new LeagueSharpCommon.Geometry.Geometry.Polygon.Rectangle(Me.ServerPosition,
                            Me.ServerPosition.Extend(minion.ServerPosition, QExtend.Range), QExtend.Width);

                        if (poly.IsInside(pred.UnitPosition))
                        {
                            Q.Cast(minion);
                        }
                    }
                }
            }

            if (Menu["Combo"]["ComboR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);

                if (target.Check(R.Range) &&
                    R.GetDamage(target) * (7.5 + 7.5 * Me.AttackSpeedMod) > target.Health &&
                    target.Distance(Me) > ObjectManager.Player.GetCurrentAutoAttackRange() + 300)
                {
                    R.Cast(target);
                }
            }
        }

        private void Harass()
        {
            if (ManaManager.HasEnoughMana(Menu["Harass"]["HarassMana"].GetValue<MenuSlider>().Value))
            {
                if (Menu["Harass"]["HarassQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
                {
                    var target = TargetSelector.GetTarget(QExtend.Range, DamageType.Physical);

                    if (target.Check(QExtend.Range))
                    {
                        if (target.IsValidTarget(Q.Range))
                        {
                            Q.Cast(target);
                        }
                        else if (target.IsValidTarget(QExtend.Range) && Menu["Harass"]["HarassQExtended"].GetValue<MenuBool>().Enabled)
                        {
                            var pred = QExtend.GetPrediction(target, true);
                            var collisions = MinionManager.GetMinions(Me.ServerPosition, Q.Range, MinionManager.MinionTypes.All,
                                MinionManager.MinionTeam.NotAlly);

                            if (!collisions.Any())
                            {
                                return;
                            }

                            foreach (var minion in collisions)
                            {
                                var poly = new LeagueSharpCommon.Geometry.Geometry.Polygon.Rectangle(Me.ServerPosition,
                                    Me.ServerPosition.Extend(minion.ServerPosition, QExtend.Range), QExtend.Width);

                                if (poly.IsInside(pred.UnitPosition))
                                {
                                    Q.Cast(minion);
                                }
                            }
                        }
                    }
                }

                if (Menu["Harass"]["HarassW"].GetValue<MenuBool>().Enabled && W.IsReady())
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);

                    if (target.Check(W.Range))
                    {
                        SpellManager.PredCast(W, target, true);
                    }
                }
            }
        }

        private void FarmHarass()
        {
            if (ManaManager.SpellHarass)
            {
                Harass();
            }
        }

        private void LaneClear()
        {
            if (ManaManager.HasEnoughMana(Menu["Clear"]["LaneClear"]["LaneClearMana"].GetValue<MenuSlider>().Value) && ManaManager.SpellFarm)
            {
                if (Menu["Clear"]["LaneClear"]["LaneClearQ"].GetValue<MenuBool>().Enabled && Utils.TickCount - CastSpellTime >= 400)
                {
                    var allMinions = MinionManager.GetMinions(Me.Position, Q.Range);

                    if (allMinions.Any())
                    {
                        var minion = allMinions.FirstOrDefault();

                        if (minion != null)
                        {
                            var qExminions = MinionManager.GetMinions(Me.Position, 900);

                            if (Helps.CountHits(allMinions, Me.Position.Extend(minion.Position, 900)) >= 2)
                            {
                                Q.CastOnUnit(minion);
                            }
                        }
                    }
                }

                if (Menu["Clear"]["LaneClear"]["LaneClearW"].GetValue<MenuBool>().Enabled && Utils.TickCount - CastSpellTime >= 400)
                {
                    var allMinionE = MinionManager.GetMinions(Me.ServerPosition, W.Range);

                    if (allMinionE.Count > 2)
                    {
                        var pred = W.GetCircularFarmLocation(allMinionE);

                        W.Cast(pred.Position);
                    }
                }
            }
        }

        private void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (Me.GetSpellSlot(Args.SData.Name) == SpellSlot.Q || Me.GetSpellSlot(Args.SData.Name) == SpellSlot.W ||
                Me.GetSpellSlot(Args.SData.Name) == SpellSlot.E)
            {
                CastSpellTime = Utils.TickCount;
            }

            if (Me.GetSpellSlot(Args.SData.Name) == SpellSlot.R && Menu["Misc"]["R Settings"]["RYoumuu"].GetValue<MenuBool>().Enabled)
            {
                if (Items.HasItem(ObjectManager.Player,ItemId.Youmuus_Ghostblade))
                {
                    Items.UseItem(ObjectManager.Player,(int)ItemId.Youmuus_Ghostblade);
                }
            }
        }

        private void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (sender.IsMe && Orbwalker.IsAutoAttack(Args.SData.Name))
            {

                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    var target = Args.Target as AIHeroClient;

                    if (target != null)
                    {
                        if (Menu["Combo"]["ComboE"].GetValue<MenuBool>().Enabled && E.IsReady())
                        {
                            Cast_E(target, false);
                        }
                        else if (Menu["Combo"]["ComboQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
                        {
                            Q.Cast(target, true);
                        }
                        else if (Menu["Combo"]["ComboW"].GetValue<MenuBool>().Enabled && W.IsReady())
                        {
                            W.Cast(target.Position);
                        }
                    }
                }

                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                {
                    if (ManaManager.HasEnoughMana(Menu["Clear"]["JungleClear"]["JungleClearMana"].GetValue<MenuSlider>().Value) && ManaManager.SpellFarm)
                    {
                        var mobs = MinionManager.GetMinions(Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral,
                            MinionManager.MinionOrderTypes.MaxHealth);

                        if (mobs.Any())
                        {
                            var mob = mobs.FirstOrDefault();

                            if (Menu["Clear"]["JungleClear"]["JungleClearE"].GetValue<MenuBool>().Enabled && E.IsReady())
                            {
                                var ex = Me.Position.Extend(Game.CursorPos, 150);

                                E.Cast(ex);
                            }
                            else if (Menu["Clear"]["JungleClear"]["JungleClearQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
                            {
                                Q.Cast(mob, true);
                            }
                            else if (Menu["Clear"]["JungleClear"]["JungleClearW"].GetValue<MenuBool>().Enabled && W.IsReady())
                            {
                                W.Cast(mob, true);
                            }
                        }
                    }
                }
            }
        }

        private void OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (Menu["Misc"]["E Settings"]["Anti"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                if (gapcloser.EndPosition.Distance(Me.Position) <= 200 || sender.Distance(Me) < 250)
                {
                    E.Cast(Me.Position.Extend(sender.Position, -E.Range));
                }
            }
        }

        private void OnDraw(EventArgs Args)
        {
            if (!Me.IsDead && !MenuGUI.IsShopOpen && !MenuGUI.IsChatOpen  )
            {
                if (Menu.GetBool("DrawQ") && Q.IsReady())
                {
                    CircleRender.Draw(Me.Position, Q.Range, Color.Green.ToSharpDxColor(), 1);
                }

                if (Menu.GetBool("DrawQEx") && QExtend.IsReady())
                {
                    CircleRender.Draw(Me.Position, QExtend.Range, Color.Green.ToSharpDxColor(), 1);
                }

                if (Menu.GetBool("DrawW") && W.IsReady())
                {
                    CircleRender.Draw(Me.Position, W.Range, Color.FromArgb(9, 253, 242).ToSharpDxColor(), 1);
                }

                if (Menu.GetBool("DrawE") && E.IsReady())
                {
                    CircleRender.Draw(Me.Position, E.Range, Color.FromArgb(188, 6, 248).ToSharpDxColor(), 1);
                }
            }
        }

        private void Cast_E(AIHeroClient target, bool FirstE)
        {
            if (FirstE)
            {
                var castpos = Me.ServerPosition.Extend(target.ServerPosition, 220);
                var maxepos = Me.ServerPosition.Extend(target.ServerPosition, E.Range);

                if (maxepos.IsUnderEnemyTurret() && Menu["Misc"]["E Settings"]["underE"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if (NavMesh.GetCollisionFlags(maxepos).HasFlag(CollisionFlags.Wall) ||
                      NavMesh.GetCollisionFlags(maxepos).HasFlag(CollisionFlags.Building) &&
                    Menu["Misc"]["E Settings"]["ECheck"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if (maxepos.CountEnemyHeroesInRange(500) >= 3 && maxepos.CountAllyHeroesInRange(400) < 3 && Menu["Misc"]["E Settings"]["SafeCheck"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if (!ObjectManager.Player.InAutoAttackRange(target) &&
                         target.ServerPosition.Distance(castpos) > ObjectManager.Player.GetRealAutoAttackRange(Me) &&
                         target.ServerPosition.Distance(maxepos) <= ObjectManager.Player.GetRealAutoAttackRange(Me))
                {
                    E.Cast(maxepos);
                }
            }
            else
            {
                var castpos = Me.ServerPosition.Extend(Game.CursorPos, 220);
                var maxepos = Me.ServerPosition.Extend(Game.CursorPos, E.Range);

                if ((castpos.IsUnderEnemyTurret() || maxepos.IsUnderEnemyTurret()) && Menu["Misc"]["E Settings"]["underE"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if ((NavMesh.GetCollisionFlags(castpos).HasFlag(CollisionFlags.Wall) ||
                     NavMesh.GetCollisionFlags(castpos).HasFlag(CollisionFlags.Building) &&
                     (NavMesh.GetCollisionFlags(maxepos).HasFlag(CollisionFlags.Wall) ||
                      NavMesh.GetCollisionFlags(maxepos).HasFlag(CollisionFlags.Building))) &&
                    Menu["Misc"]["E Settings"]["ECheck"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if (((castpos.CountEnemyHeroesInRange(500) >= 3 && castpos.CountAllyHeroesInRange(400) < 3) ||
                     (maxepos.CountEnemyHeroesInRange(500) >= 3 && maxepos.CountAllyHeroesInRange(400) < 3)) &&
                    Menu["Misc"]["E Settings"]["SafeCheck"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if (ObjectManager.Player.InAutoAttackRange(target) &&
                    target.ServerPosition.Distance(castpos) <= ObjectManager.Player.GetRealAutoAttackRange(Me))
                {
                    E.Cast(Menu["Misc"]["E Settings"]["ShortELogic"].GetValue<MenuBool>().Enabled ? castpos : maxepos);
                }
                else if (!ObjectManager.Player.InAutoAttackRange(target) && target.ServerPosition.Distance(castpos) <=
                         ObjectManager.Player.GetRealAutoAttackRange(Me))
                {
                    E.Cast(Menu["Misc"]["E Settings"]["ShortELogic"].GetValue<MenuBool>().Enabled ? castpos : maxepos);
                }
                else if (!ObjectManager.Player.InAutoAttackRange(target) &&
                         target.ServerPosition.Distance(castpos) > ObjectManager.Player.GetRealAutoAttackRange(Me) &&
                         target.ServerPosition.Distance(maxepos) <= ObjectManager.Player.GetRealAutoAttackRange(Me))
                {
                    E.Cast(maxepos);
                }
            }
        }
    }
}