using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using LeagueSharpCommon.Geometry;
using SPrediction;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = EnsoulSharp.SDK.Render;

namespace Flowers_ADC_Series.Pluging
{
    using ADCCOMMON;
    using System;
    using Color = System.Drawing.Color;
    

    internal class Ashe : Logic
    {
        public Ashe()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1255f);
            E = new Spell(SpellSlot.E, 5000f);
            R = new Spell(SpellSlot.R, 2000f);

            W.SetSkillshot(0.25f, 60f, 2000f, true, SpellType.Cone);
            E.SetSkillshot(0.25f, 300f, 1400f, false, SpellType.Line);
            R.SetSkillshot(0.25f, 130f, 1600f, true, SpellType.Line);

            var comboMenu = Menu.Add(new Menu("Combo", "Combo"));
            {
                comboMenu.Add(new MenuBool("ComboQ", "Use Q", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboSaveMana", "Save Mana To Cast Q", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboW", "Use W", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboE", "Use E", true).SetValue(true));
                comboMenu.Add(new MenuBool("ComboR", "Use R", true).SetValue(true));
            }

            var harassMenu = Menu.Add(new Menu("Harass", "Harass"));
            {
                harassMenu.Add(new MenuBool("HarassW", "Use W", true).SetValue(true));
                harassMenu.Add(
                    new MenuSlider("HarassWMana", "When Player ManaPercent >= x%", 60));
            }

            var clearMenu = Menu.Add(new Menu("Clear", "Clear"));
            {
                var laneClearMenu = clearMenu.Add(new Menu("LaneClear", "LaneClear"));
                {
                    laneClearMenu.Add(new MenuBool("LaneClearW", "Use W", true).SetValue(true));
                    laneClearMenu.Add(
                        new MenuSlider("LaneClearWCount", "If W CanHit Counts >= ", 3, 1, 5));
                    laneClearMenu.Add(
                        new MenuSlider("LaneClearWMana", "If Player ManaPercent >= %", 60));
                }

                var jungleClearMenu = clearMenu.Add(new Menu("JungleClear", "JungleClear"));
                {
                    jungleClearMenu.Add(new MenuBool("JungleClearQ", "Use Q", true).SetValue(true));
                    jungleClearMenu.Add(new MenuBool("JungleClearW", "Use W", true).SetValue(true));
                    jungleClearMenu.Add(
                        new MenuSlider("JungleClearMana", "When Player ManaPercent >= x%",30));
                }

                clearMenu.Add(new MenuSeparator("asdqweqwe", " "));
                ManaManager.AddSpellFarm(clearMenu);
            }

            var killStealMenu = Menu.Add(new Menu("KillSteal", "KillSteal"));
            {
                killStealMenu.Add(new MenuBool("KillStealW", "KillSteal W", true).SetValue(true));
                killStealMenu.Add(new MenuBool("KillStealR", "KillSteal R", true).SetValue(true));
                foreach (var target in GameObjects.EnemyHeroes)
                {
                    killStealMenu.Add(new MenuBool("KillStealR" + target.CharacterName.ToLower(), 
                        "Kill: " + target.CharacterName, true).SetValue(true));
                }
            }

            var miscMenu = Menu.Add(new Menu("Misc", "Misc"));
            {
                var rMenu = miscMenu.Add(new Menu("R Settings", "R Settings"));
                {
                    rMenu.Add(new MenuBool("AutoR", "Auto R?", true).SetValue(true));
                    rMenu.Add(new MenuBool("Interrupt", "Interrupt Danger Spells", true).SetValue(true));
                    rMenu.Add(
                        new MenuKeyBind("SemiR", "Semi-manual R Key", Keys.T, KeyBindType.Press));
                }

                var antiGapcloserMenu = miscMenu.Add(new Menu("Anti Gapcloser", "Anti Gapcloser"));
                {
                    antiGapcloserMenu.Add(new MenuBool("AntiGapCloser", "Enabled", true).SetValue(true));
                    antiGapcloserMenu.Add(
                        new MenuSlider("AntiGapCloserHp", "AntiGapCloser |When Player HealthPercent <= x%").SetValue(
                            30));
                    antiGapcloserMenu.Add(new MenuSeparator("AntiGapCloserRList", "AntiGapCloser R List:"));
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        antiGapcloserMenu.Add(new MenuBool("AntiGapCloserR" + target.CharacterName.ToLower(),
                            "GapCloser: " + target.CharacterName, true).SetValue(true));
                    }
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

                var humainzerMenu = utilityMenu.Add(new Menu("Humanier", "Humanizer"));
                {
                    HumanizerManager.AddToMenu(humainzerMenu);
                }

                var itemsMenu = utilityMenu.Add(new Menu("Items", "Items"));
                {
                    ItemsManager.AddToMenu(itemsMenu);
                }
            }

            var drawMenu = Menu.Add(new Menu("Drawings", "Drawings"));
            {
                drawMenu.Add(new MenuBool("DrawW", "Draw W Range", true).SetValue(false));
                drawMenu.Add(new MenuBool("DrawR", "Draw R Range", true).SetValue(false));
                drawMenu.Add(new MenuBool("DrawRMin", "Draw R Range(MinMap)", true).SetValue(false));
                ManaManager.AddDrawFarm(drawMenu);
                //DamageIndicator.AddToMenu(drawMenu);
            }

            AntiGapcloser.OnGapcloser += OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
            AIBaseClient.OnProcessSpellCast += OnSpellCast;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
        }


        private void OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (!sender.IsEnemy || !R.IsReady() || Menu["Misc"]["Anti Gapcloser"]["AntiGapCloser"].GetValue<MenuBool>().Enabled || 
                Me.HealthPercent > Menu["Misc"]["Anti Gapcloser"]["AntiGapCloserHp"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            if (Menu["Misc"]["Anti Gapcloser"]["AntiGapCloserR" + sender.CharacterName.ToLower()].GetValue<MenuBool>().Enabled && Args.EndPosition.DistanceToPlayer() <= 300)
            {
                SpellManager.PredCast(R, sender);
            }
        }

        private void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs Args)
        {
            if (Menu["Misc"]["R Settings"]["Interrupt"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                if (!sender.IsEnemy || Args.DangerLevel < Interrupter.DangerLevel.High || !sender.IsValidTarget(R.Range))
                {
                    return;
                }

                SpellManager.PredCast(R, sender);
            }
        }

        private void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (!sender.IsMe || !Orbwalker.IsAutoAttack(Args.SData.Name))
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (Menu["Combo"]["ComboQ"].GetValue<MenuBool>().Enabled)
                    {
                        var target = (AIHeroClient)Args.Target;

                        if (target != null && !target.IsDead && !target.IsZombie())
                        {
                            if (Me.HasBuff("asheqcastready"))
                            {
                                Q.Cast();
                                Orbwalker.ResetAutoAttackTimer();
                            }
                        }
                    }
                    break;
                case OrbwalkerMode.LaneClear:
                    if (ManaManager.HasEnoughMana(Menu["Clear"]["JungleClear"]["JungleClearMana"].GetValue<MenuSlider>().Value) && ManaManager.SpellFarm)
                    {
                        if (Menu["Clear"]["JungleClear"]["JungleClearQ"].GetValue<MenuBool>().Enabled && Args.Target is AIMinionClient)
                        {
                            var mobs = MinionManager.GetMinions(Me.Position, Me.GetRealAutoAttackRange(),
                                MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);

                            if (mobs.Any())
                            {
                                foreach (var mob in mobs)
                                {
                                    if (!mob.IsValidTarget(Me.GetRealAutoAttackRange()) ||
                                        !(mob.Health > Me.GetAutoAttackDamage(mob) * 2))
                                    {
                                        continue;
                                    }

                                    if (Me.HasBuff("asheqcastready"))
                                    {
                                        Q.Cast();
                                        Orbwalker.ResetAutoAttackTimer();
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (Me.IsDead || Me.IsRecalling())
            {
                return;
            }

            if (Menu["Misc"]["R Settings"]["SemiR"].GetValue<MenuKeyBind>().Active)
            {
                OneKeyR();
            }

            AutoRLogic();
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
                    JungleClear();
                    break;
            }
        }

        private void AutoRLogic()
        {
            if (Menu["Misc"]["R Settings"]["AutoR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && x.Check(R.Range)))
                {
                    if (!(target.DistanceToPlayer() > Me.GetRealAutoAttackRange()) ||
                        !(target.DistanceToPlayer() <= 700) ||
                        !(target.Health > Me.GetAutoAttackDamage(target)) ||
                        !(target.Health < R.GetDamage(target) + Me.GetAutoAttackDamage(target)*3) ||
                        target.HasBuffOfType(BuffType.SpellShield))
                    {
                        continue;
                    }

                    SpellManager.PredCast(R, target);
                    return;
                }
            }
        }

        private void KillSteal()
        {
            if (Menu.GetBool("KillStealW") && W.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.Check(W.Range)))
                {
                    if (!target.IsValidTarget(W.Range) || !(target.Health < W.GetDamage(target)))
                        continue;

                    if (target.DistanceToPlayer() <= Me.GetRealAutoAttackRange() && 
                        Me.HasBuff("AsheQAttack"))
                    {
                        continue;
                    }

                    SpellManager.PredCast(W, target);
                    return;
                }
            }

            if (!Menu.GetBool("KillStealR") || !R.IsReady())
            {
                return;
            }

            foreach (
                var target in
                HeroManager.Enemies.Where(
                    x =>
                        x.Check(2000) &&
                        Menu.GetBool("KillStealR" + x.CharacterName.ToLower())))
            {
                if (!(target.DistanceToPlayer() > 800) || !(target.Health < R.GetDamage(target)) ||
                    target.HasBuffOfType(BuffType.SpellShield))
                {
                    continue;
                }

                SpellManager.PredCast(R, target);
                return;
            }
        }

        private void Combo()
        {
            if (Menu.GetBool("ComboR") && R.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.Check(1200)))
                {
                    if (target.IsValidTarget(600) && Me.CountEnemyHeroesInRange(600) >= 3 && target.CountAllyHeroesInRange(200) <= 2)
                    {
                        SpellManager.PredCast(R, target);
                    }

                    if (Me.CountEnemyHeroesInRange(800) == 1 &&
                        target.DistanceToPlayer() > Me.GetRealAutoAttackRange() &&
                        target.DistanceToPlayer() <= 700 &&
                        target.Health > Me.GetAutoAttackDamage(target) &&
                        target.Health < R.GetDamage(target) + Me.GetAutoAttackDamage(target)*3 &&
                        !target.HasBuffOfType(BuffType.SpellShield))
                    {
                        SpellManager.PredCast(R, target);
                    }

                    if (target.DistanceToPlayer() <= 1000 &&
                        (!target.CanMove || target.HasBuffOfType(BuffType.Stun) ||
                        R.GetPrediction(target).Hitchance == HitChance.Immobile))
                    {
                        SpellManager.PredCast(R, target);
                    }
                }
            }

            if (Menu.GetBool("ComboW") && W.IsReady() && !Me.HasBuff("AsheQAttack"))
            {
                if ((Menu.GetBool("ComboSaveMana") &&
                     Me.Mana > (R.IsReady() ? R.Instance.ManaCost : 0) + W.Instance.ManaCost + Q.Instance.ManaCost) ||
                    !Menu.GetBool("ComboSaveMana"))
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);

                    if (target.Check(W.Range))
                    {
                        SpellManager.PredCast(W, target);
                    }
                }
            }

            if (Menu.GetBool("ComboE") && E.IsReady())
            {
                var target = TargetSelector.GetTarget(1000, DamageType.Physical);

                if (target.Check(1000))
                {
                    var EPred = E.GetPrediction(target);

                    if ((NavMesh.GetCollisionFlags(EPred.CastPosition) == CollisionFlags.Grass ||
                         NavMesh.IsWallOfType(target.ServerPosition,CollisionFlags.Grass, 20)) && !target.IsVisible)
                    {
                        E.Cast(EPred.CastPosition);
                    }
                }
            }
        }

        private void Harass()
        {
            if (ManaManager.HasEnoughMana(Menu.GetSlider("HarassWMana")))
            {
                if (Menu.GetBool("HarassW") && W.IsReady() && !Me.HasBuff("AsheQAttack"))
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);

                    if (target.Check(W.Range))
                    {
                        SpellManager.PredCast(W, target);
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
            if (ManaManager.HasEnoughMana(Menu["Clear"]["LaneClear"]["LaneClearWMana"].GetValue<MenuSlider>().Value) && ManaManager.SpellFarm)
            {
                if (Menu["Clear"]["LaneClear"]["LaneClearW"].GetValue<MenuBool>().Enabled && W.IsReady())
                {
                    var minions = MinionManager.GetMinions(Me.Position, W.Range);

                    if (minions.Any())
                    {
                        var wFarm = FarmPrediction.GetBestCircularFarmLocation(minions.Select(x => x.Position.To2D()).ToList(),
                            W.Width, W.Range);

                        if (wFarm.MinionsHit >= Menu["Clear"]["LaneClear"]["LaneClearWCount"].GetValue<MenuSlider>().Value)
                        {
                            W.Cast(wFarm.Position);
                        }
                    }
                }
            }
        }

        private void JungleClear()
        {
            if (ManaManager.HasEnoughMana(Menu["Clear"]["JungleClear"]["JungleClearMana"].GetValue<MenuSlider>().Value))
            {
                if (Menu["Clear"]["JungleClear"]["JungleClearW"].GetValue<MenuBool>().Enabled && !Me.HasBuff("AsheQAttack"))
                {
                    var mobs = MinionManager.GetMinions(Me.Position, W.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral,
                        MinionManager.MinionOrderTypes.MaxHealth);

                    if (mobs.Any())
                    {
                        var mob = mobs.FirstOrDefault();

                        if (mob != null)
                        {
                            W.Cast(mob.Position);
                        }
                    }
                }
            }
        }

        private void OneKeyR()
        {
            Orbwalker.Move(Game.CursorPos);

            if (R.IsReady())
            {
                var select = TargetSelector.SelectedTarget;
                var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);

                if (select != null && !target.HasBuffOfType(BuffType.SpellShield) && target.IsValidTarget(R.Range))
                {
                    SpellManager.PredCast(R, target);
                }
                else if (select == null && target != null && !target.HasBuffOfType(BuffType.SpellShield) &&
                    target.IsValidTarget(R.Range))
                {
                    SpellManager.PredCast(R, target);
                }
            }
        }

        private void OnDraw(EventArgs Args)
        {
            if (!Me.IsDead && !Me.InShop() && !MenuGUI.IsChatOpen  )
            {
                if (Menu.GetBool("DrawW") && W.IsReady())
                {
                    Render.Circle.DrawCircle(Me.Position, W.Range, Color.FromArgb(9, 253, 242), 1);
                }

                if (Menu.GetBool("DrawR") && R.IsReady())
                {
                    Render.Circle.DrawCircle(Me.Position, R.Range, Color.FromArgb(19, 130, 234), 1);
                }
            }
        }

        private void OnEndScene(EventArgs Args)
        {
            if (!Me.IsDead && !Me.InShop() && !MenuGUI.IsChatOpen  )
            {
#pragma warning disable 618
                if (Menu.GetBool("DrawRMin") && R.IsReady())
                {
                    LeagueSharpCommon.Utility.DrawCircle(Me.Position, R.Range, Color.FromArgb(14, 194, 255), 1, 30, true);
                }
#pragma warning restore 618
            }
        }
    }
}
