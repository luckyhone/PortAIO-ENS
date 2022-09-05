using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using SharpDX.Direct3D9;
using SPrediction;

namespace Olympus_Karthus
{
    internal class Karthus
    {
        #region Properties

        private static float lastQTime = 0f;
        private static Vector3 QPosition;
        private static Menu myMenu;
        private static AIHeroClient objPlayer = ObjectManager.Player;
        private static Spell Q, W, E, R;
        private static Font Font;

        #endregion Properties

        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q, 950f);
            Q.SetSkillshot(1f, 100f, float.MaxValue, false, SpellType.Circle);

            W = new Spell(SpellSlot.W, 1000f);
            W.SetSkillshot(0.25f, 5f, float.MaxValue, false, SpellType.Line);

            E = new Spell(SpellSlot.E, 425f);
            E.SetSkillshot(1f, 520f, float.MaxValue, false, SpellType.Circle);

            R = new Spell(SpellSlot.R, float.MaxValue);
            R.SetSkillshot(3f, float.MaxValue, float.MaxValue, false, SpellType.Circle);

            Font = new Font(Drawing.Direct3DDevice9, new FontDescription { FaceName = "Tahoma", Height = 20, OutputPrecision = FontPrecision.Default, Quality = FontQuality.Antialiased });

            #region Menu Init

            myMenu = new Menu("Olympus.Karthus", "Olympus.Karthus", true);

            var autoUltMenu = new Menu("autoUltMenu", "Auto Ult")
            {
                MenuSettings.AutoR.UseR,
                MenuSettings.AutoR.UseInTeamFight,
                MenuSettings.AutoR.UseIfKillable,
                MenuSettings.AutoR.KillableMinHits,
            };

            autoUltMenu.Add(new MenuSeparator("BlackListSeparator", "BlackList (not implemented yet)"));

            foreach (var target in GameObjects.EnemyHeroes)
            {
                autoUltMenu.Add(new MenuBool(target.CharacterName, target.CharacterName));
            }

            myMenu.Add(autoUltMenu);

            var comboMenu = new Menu("comboMenu", "Combo")
            {
                MenuSettings.Combo.UseQ,
                MenuSettings.Combo.UseW,
                MenuSettings.Combo.UseE,
                new MenuSeparator("QSettings", "Q Settings"),
                MenuSettings.Combo.QHitChance,
                new MenuSeparator("PredictionSeparator", "Prediction"),
                MenuSettings.Combo.Prediction,
                new MenuSeparator("AASettings", "AutoAttack Settings"),
                MenuSettings.Combo.AA,
                MenuSettings.Combo.AALackOfMana,
            };
            myMenu.Add(comboMenu);

            var harassMenu = new Menu("harassMenu", "Harass")
            {
                MenuSettings.Harass.UseQ,
                new MenuSeparator("AASettings", "AutoAttack Settings"),
                MenuSettings.Harass.AA,
                new MenuSeparator("ManaManager", "Mana Manager"),
                MenuSettings.Harass.MinMana,
            };
            myMenu.Add(harassMenu);

            var laneClearMenu = new Menu("laneClearMenu", "Lane Clear")
            {
                MenuSettings.LaneClear.UseQ,
                MenuSettings.LaneClear.UseE,
                new MenuSeparator("QSettings", "Q Settings"),
                MenuSettings.LaneClear.QMode,
                new MenuSeparator("ESettings", "E Settings"),
                MenuSettings.LaneClear.EMinHits,
                new MenuSeparator("ManaManager", "Mana Manager"),
                MenuSettings.LaneClear.QMinMana,
                MenuSettings.LaneClear.EMinMana,
            };
            myMenu.Add(laneClearMenu);

            var jungleClearMenu = new Menu("jungleClearMenu", "Jungle Clear")
            {
                MenuSettings.JungleClear.UseQ,
                MenuSettings.JungleClear.UseW,
                MenuSettings.JungleClear.UseE,
                new MenuSeparator("WSettings", "W Settings"),
                MenuSettings.JungleClear.WMode,
                new MenuSeparator("ESettings", "E Settings"),
                MenuSettings.JungleClear.EMode,
                new MenuSeparator("ManaManager", "Mana Manager"),
                MenuSettings.JungleClear.QMinMana,
                MenuSettings.JungleClear.WMinMana,
                MenuSettings.JungleClear.EMinMana,
            };
            myMenu.Add(jungleClearMenu);

            var lastHitMenu = new Menu("lastHitMenu", "Last Hit")
            {
                MenuSettings.LastHit.UseQ,
                new MenuSeparator("ManaManager", "Mana Manager"),
                MenuSettings.LastHit.MinMana,
            };
            myMenu.Add(lastHitMenu);

            var miscMenu = new Menu("miscMenu", "Misc")
            {
                MenuSettings.Misc.AutoDisableE,
                MenuSettings.Misc.AutoZombie,
            };
            myMenu.Add(miscMenu);

            var drawingMenu = new Menu("drawingMenu", "Drawings")
            {
                MenuSettings.Drawing.Disable,
                MenuSettings.Drawing.DamageIndicator,
                MenuSettings.Drawing.UltNotification,
                MenuSettings.Drawing.QWidth,
                new MenuSeparator("range", "Range"),
                MenuSettings.Drawing.QRange,
                MenuSettings.Drawing.WRange,
                MenuSettings.Drawing.ERange,
            };
            myMenu.Add(drawingMenu);

            var creditsMenu = new Menu("creditsMenu", "Credits")
            {
                new MenuSeparator("Hellsing", "Hellsing"),
                new MenuSeparator("berbb", "berbb"),
                new MenuSeparator("NightMoon", "NightMoon"),
                new MenuSeparator("Sayuto", "Sayuto"),
            };
            myMenu.Add(creditsMenu);

            myMenu.Add(new MenuSeparator("KeysSeparator", "Keys"));
            myMenu.Add(MenuSettings.Keys.harassToggle.SetValue(true)).Permashow();
            myMenu.Add(MenuSettings.Keys.farmToggle.SetValue(true)).Permashow();
            myMenu.Add(MenuSettings.Keys.aaFarmToggle.SetValue(false)).Permashow();

            myMenu.Attach();

            #endregion Menu Init

            GameEvent.OnGameTick += OnUpdate;

            /* Drawings */
            Drawing.OnDraw += Drawings.OnDraw;
            Drawing.OnEndScene += Drawings.OnEndScene;
            Drawing.OnPreReset += args => { Font.OnLostDevice(); };
            Drawing.OnPostReset += args => { Font.OnResetDevice(); };

            /* Orbwalker */
            Orbwalker.OnBeforeAttack += Events.OnAction;

            /* AIBaseClient */
            AIBaseClient.OnProcessSpellCast += Events.OnProcessSpellCast;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (objPlayer.IsDead || objPlayer.IsRecalling() || MenuGUI.IsChatOpen)
                return;

            if (MenuSettings.AutoR.UseR.Enabled && R.IsReady())
            {
                Misc.AutoRWhenTeamFight();
                Misc.AutoRWhenKillable();
            }

            if (MenuSettings.Misc.AutoDisableE.Enabled)
                Misc.AutoTurnOffE();
            if (MenuSettings.Misc.AutoZombie.Enabled)
                Misc.AutoZombie();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    OrbwalkerModes.Combo();
                    break;

                case OrbwalkerMode.Harass:
                    OrbwalkerModes.Harass();
                    break;

                case OrbwalkerMode.LaneClear:
                    OrbwalkerModes.LaneClear();
                    OrbwalkerModes.JungleClear();
                    break;

                case OrbwalkerMode.LastHit:
                    OrbwalkerModes.LastHit();
                    break;
            }
        }

        private class Drawings
        {
            public static void OnDraw(EventArgs args)
            {
                if (MenuSettings.Drawing.Disable.Enabled)
                    return;

                if (MenuSettings.Drawing.QRange.Enabled)
                {
                    Render.Circle.DrawCircle(objPlayer.Position, Q.Range, System.Drawing.Color.Red);
                }
                if (MenuSettings.Drawing.WRange.Enabled && W.IsReady())
                {
                    Render.Circle.DrawCircle(objPlayer.Position, W.Range, System.Drawing.Color.DarkRed);
                }
                if (MenuSettings.Drawing.ERange.Enabled && E.IsReady())
                {
                    Render.Circle.DrawCircle(objPlayer.Position, E.Range, System.Drawing.Color.OrangeRed);
                }
                if (MenuSettings.Drawing.QWidth.Enabled)
                {
                    if (Variables.GameTimeTickCount - lastQTime > 1000)
                        return;

                    Render.Circle.DrawCircle(QPosition, Q.Width, System.Drawing.Color.White);
                }
            }

            public static void OnEndScene(EventArgs args)
            {
                if (MenuSettings.Drawing.Disable.Enabled)
                    return;

                if (MenuSettings.Drawing.DamageIndicator.Enabled)
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(2000) && !x.IsDead && x.IsHPBarRendered))
                    {
                        Vector2 pos = Drawing.WorldToScreen(target.Position);

                        if (!pos.IsOnScreen())
                            return;

                        var damage = Misc.GetComboDamage(target, true, true, true, true);

                        var hpBar = target.HPBarPosition;

                        if (damage > target.Health)
                        {
                            Drawing.DrawText(hpBar.X + 69, hpBar.Y - 45, System.Drawing.Color.White, "KILLABLE");
                        }

                        var damagePercentage = ((target.Health - damage) > 0 ? (target.Health - damage) : 0) / target.MaxHealth;
                        var currentHealthPercentage = target.Health / target.MaxHealth;

                        var startPoint = new Vector2(hpBar.X - 45 + damagePercentage * 104, hpBar.Y - 18);
                        var endPoint = new Vector2(hpBar.X - 45 + currentHealthPercentage * 104, hpBar.Y - 18);

                        Drawing.DrawLine(startPoint, endPoint, 12, System.Drawing.Color.Yellow);
                    }
                }
                if (MenuSettings.Drawing.UltNotification.Enabled)
                {
                    var killable = new Dictionary<AIHeroClient, float>();

                    foreach (var target in GameObjects.EnemyHeroes.Where(x => !x.IsDead))
                    {
                        var damage = R.GetDamage(target);

                        if (damage > target.Health)
                            killable.Add(target, damage);
                    }

                    #region Magic Happen Here

                    if (killable.Count() > 0)
                    {
                        killable = killable.OrderBy(x => x.Value / x.Key.HealthPercent).ToDictionary(x => x.Key, x => x.Value);
                    }

                    var pos = new Vector2(150, 200);
                    Font.DrawText(null, "Killable targets: " + killable.Count, (int)pos.X, (int)pos.Y, R.IsReady() ? new ColorBGRA(170, 255, 47, 255) : new ColorBGRA(255, 69, 0, 255));

                    foreach (var target in killable)
                    {
                        pos += new Vector2(0, 30);
                        var formatString = "{0} - {1}% overkill";
                        int alliesNearby;

                        if (!target.Key.IsHPBarRendered)
                        {
                            formatString += " (no vision)";
                        }
                        else if ((alliesNearby = target.Key.CountAllyHeroesInRange(1000)) > 0)
                        {
                            formatString += string.Format(" ({0} allies nearby)", alliesNearby);
                        }
                        else
                        {
                            formatString += " (free kill)";
                        }
                        Font.DrawText(null, string.Format(formatString, target.Key.CharacterName, Math.Floor(target.Value / target.Key.Health) - 100), (int)pos.X, (int)pos.Y, new ColorBGRA(255, 222, 173, 255));
                    }

                    #endregion Magic Happen Here
                }
            }
        }

        private class Events
        {
            public static void OnAction(object sender, BeforeAttackEventArgs args)
            {
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    if (MenuSettings.Combo.AA.Enabled)
                    {
                        if (MenuSettings.Combo.AALackOfMana.Enabled && objPlayer.Mana <= 100)
                            return;

                        args.Process = false;
                    }
                }
                if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
                {
                    if (MenuSettings.Harass.AA.Enabled)
                        args.Process = false;
                }
                else if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                {
                    if (MenuSettings.Keys.aaFarmToggle.Active)
                        return;

                    if (MenuSettings.Keys.farmToggle.Active && (args.Target.Type == GameObjectType.AIMinionClient || args.Target.Type == GameObjectType.NeutralMinionCampClient))
                    {
                        if (objPlayer.Mana < 200)
                            return;

                        args.Process = false;
                    }
                }
                else if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
                {
                    if (MenuSettings.LastHit.UseQ.Enabled && Q.IsReady())
                    {
                        if (objPlayer.ManaPercent < MenuSettings.LastHit.MinMana.Value)
                            return;

                        var target = (AIBaseClient)args.Target;

                        if (args.Target.Health > objPlayer.GetAutoAttackDamage(target) || MenuSettings.Keys.aaFarmToggle.Active)
                            return;

                        if (MenuSettings.Keys.aaFarmToggle.Active && (args.Target.Type == GameObjectType.AIMinionClient || args.Target.Type == GameObjectType.NeutralMinionCampClient))
                        {
                            if (objPlayer.Mana < 200)
                                return;

                            args.Process = false;
                        }
                    }
                }

            }
            public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
            {
                if (sender.IsEnemy)
                    return;

                var spellSlot = objPlayer.GetSpellSlot(args.SData.Name);

                if (spellSlot == SpellSlot.Q)
                {
                    lastQTime = Variables.GameTimeTickCount;
                    QPosition = args.To;
                }
            }
        }

        private class Misc
        {
            public static void AutoRWhenKillable()
            {
                var canCast = false;

                if (MenuSettings.AutoR.UseIfKillable.Enabled)
                {
                    var killable = new Dictionary<AIHeroClient, float>();

                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        if (target.IsValidTarget() == false)
                            return;
                        if (target.IsDead)
                            return;
                        if (target.HasBuffOfType(BuffType.Invulnerability))
                            return;
                        if (target.HasBuffOfType(BuffType.SpellImmunity))
                            return;

                        var damage = R.GetDamage(target);

                        if (damage > target.Health)
                            killable.Add(target, damage);

                        canCast = true;
                    }

                    if (objPlayer.IsHPBarRendered)
                    {
                        if (killable.Count() < MenuSettings.AutoR.KillableMinHits.Value)
                            return;

                        if (canCast && objPlayer.CountEnemyHeroesInRange(800) == 0)
                            R.Cast();
                    }
                    else if (!objPlayer.IsHPBarRendered)
                    {
                        if (killable.Count() < MenuSettings.AutoR.KillableMinHits.Value)
                            return;

                        var pasiveTime = objPlayer.GetBuff("KarthusDeathDefiedBuff").EndTime;

                        if (canCast && pasiveTime > 3 && pasiveTime < 4)
                            R.Cast();
                    }
                }
            }
            public static void AutoRWhenTeamFight()
            {
                var canCast = false;

                if (MenuSettings.AutoR.UseInTeamFight.Enabled)
                {
                    foreach (var target in GameObjects.EnemyHeroes)
                    {
                        if (target.IsValidTarget() == false)
                            return;
                        if (target.IsDead)
                            return;
                        if (target.HasBuffOfType(BuffType.Invulnerability))
                            return;
                        if (target.HasBuffOfType(BuffType.SpellImmunity))
                            return;

                        if (target.CountAllyHeroesInRange(850) > 1 && target.CountEnemyHeroesInRange(850) <= 2)
                        {
                            if (target.Health + target.MagicalShield > R.GetDamage(target) * 2 && objPlayer.IsHPBarRendered)
                                return;

                            canCast = true;
                        }
                        if (objPlayer.CountEnemyHeroesInRange(1000) >= 3 && objPlayer.CountAllyHeroesInRange(850) <= 3)
                        {
                            canCast = true;
                        }
                    }

                    if (objPlayer.IsHPBarRendered)
                    {
                        if (canCast && objPlayer.CountEnemyHeroesInRange(800) == 0)
                            R.Cast();
                    }
                    else if (!objPlayer.IsHPBarRendered)
                    {
                        var pasiveTime = objPlayer.GetBuff("KarthusDeathDefiedBuff").EndTime;

                        if (canCast && pasiveTime > 3 && pasiveTime < 4)
                            R.Cast();
                    }
                }
                else
                    canCast = false;
            }
            public static void AutoTurnOffE()
            {
                var minions = GameObjects.GetMinions(E.Range + 50);
                var mobs = GameObjects.GetJungles(E.Range + 50);

                if (objPlayer.HasBuff("KarthusDefile"))
                {
                    if (objPlayer.CountEnemyHeroesInRange(E.Range + 50) == 0 && (minions.Count() == 0 && mobs.Count() == 0))
                    {
                        E.Cast();
                    }
                }
            }
            public static void AutoZombie()
            {
                if (!objPlayer.IsHPBarRendered)
                {
                    if (E.ToggleState != (SpellToggleState)2)
                        E.Cast();

                    if (objPlayer.CountEnemyHeroesInRange(Q.Range) > 0)
                    {
                        OrbwalkerModes.Combo();
                    }
                    else
                    {
                        OrbwalkerModes.LaneClear();
                        OrbwalkerModes.JungleClear();
                    }
                }
            }
            public static float GetComboDamage(AIBaseClient target, bool q = false, bool w = false, bool e = false, bool r = false, bool summonerignite = false)
            {
                float damage = 0;

                if (target == null || target.IsDead)
                    return 0;
                if (target.HasBuffOfType(BuffType.Invulnerability))
                    return 0;

                if (q && Q.IsReady())
                    damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.Q);
                if (e && E.ToggleState == (SpellToggleState)2)
                    damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.E);
                if (r && R.IsReady())
                    damage += (float)Damage.GetSpellDamage(objPlayer, target, SpellSlot.R);

                if (objPlayer.GetBuffCount("itemmagicshankcharge") == 100) // oktw sebby
                    damage += (float)objPlayer.CalculateMagicDamage(target, 100 + 0.1 * objPlayer.TotalMagicalDamage);

                if (target.HasBuff("ManaBarrier") && target.HasBuff("BlitzcrankManaBarrierCO"))
                    damage += target.Mana / 2f;
                if (target.HasBuff("GarenW"))
                    damage = damage * 0.7f;

                return damage;
            }
        }

        private class OrbwalkerModes
        {
            public static bool predictionMode = true;

            public static void Combo()
            {
                var target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);

                if (target == null)
                    return;

                if (MenuSettings.Combo.UseQ.Enabled && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (Variables.GameTimeTickCount - lastQTime < 300)
                        return;

                    switch (MenuSettings.Combo.Prediction.SelectedValue)
                    {
                        case "SPrediction":
                            predictionMode = true;
                            break;

                        case "Common Prediction":
                            predictionMode = false;
                            break;
                    }

                    if (predictionMode)
                    {
                        var predictionOutput = Q.GetSPrediction(target);

                        switch (MenuSettings.Combo.QHitChance.SelectedValue)
                        {
                            case "Low":
                                if (predictionOutput.HitChance >= HitChance.Low)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;

                            case "Medium":
                                if (predictionOutput.HitChance >= HitChance.Medium)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;

                            case "High":
                                if (predictionOutput.HitChance >= HitChance.High)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;

                            case "VeryHigh":
                                if (predictionOutput.HitChance >= HitChance.VeryHigh)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;

                            case "Immobile":
                                if (predictionOutput.HitChance >= HitChance.Immobile)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;
                        }
                    }
                    else
                    {
                        var predictionOutput = Q.GetPrediction(target);

                        switch (MenuSettings.Combo.QHitChance.SelectedValue)
                        {
                            case "Low":
                                if (predictionOutput.Hitchance >= HitChance.Low)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;

                            case "Medium":
                                if (predictionOutput.Hitchance >= HitChance.Medium)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;

                            case "High":
                                if (predictionOutput.Hitchance >= HitChance.High)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;

                            case "VeryHigh":
                                if (predictionOutput.Hitchance >= HitChance.VeryHigh)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;

                            case "Immobile":
                                if (predictionOutput.Hitchance >= HitChance.Immobile)
                                {
                                    Q.Cast(predictionOutput.CastPosition);
                                }
                                break;
                        }
                    }
                }
                if (MenuSettings.Combo.UseW.Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    var getSPrediction = W.GetPrediction(target);

                    if (getSPrediction.Hitchance >= HitChance.Medium && objPlayer.Mana >= 350)
                    {
                        W.Cast(getSPrediction.CastPosition);
                    }
                }
                if (MenuSettings.Combo.UseE.Enabled && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (objPlayer.HasBuff("KarthusDefile") && GameObjects.EnemyHeroes.Count(x => x.DistanceToPlayer() > E.Range) == GameObjects.EnemyHeroes.Count())
                    {
                        E.Cast();
                    }
                    if (target.DistanceToPlayer() < E.Range - 60 && !objPlayer.HasBuff("KarthusDefile"))
                    {
                        E.Cast();
                    }
                }
            }
            public static void Harass()
            {
                if (!MenuSettings.Keys.harassToggle.Active)
                    return;

                if (objPlayer.ManaPercent < MenuSettings.Harass.MinMana.Value)
                    return;

                if (MenuSettings.Harass.UseQ.Enabled && Q.IsReady())
                {
                    if (Variables.GameTimeTickCount - lastQTime < 1000)
                        return;

                    var target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);

                    if (target == null)
                        return;

                    var getPrediction = Q.GetPrediction(target);

                    if (getPrediction.Hitchance >= HitChance.Medium)
                        Q.Cast(getPrediction.CastPosition);
                }
            }
            public static void JungleClear()
            {
                if (!MenuSettings.Keys.farmToggle.Active)
                    return;

                var mobs = GameObjects.GetJungles(Q.Range).OrderByDescending(x => x.MaxHealth);

                if (mobs.Count() == 0)
                    return;

                if (MenuSettings.JungleClear.UseQ.Enabled && Q.IsReady())
                {
                    if (objPlayer.ManaPercent < MenuSettings.JungleClear.QMinMana.Value)
                        return;

                    var QMobs = mobs.Where(x => x.IsValidTarget(Q.Range));

                    if ((QMobs.FirstOrDefault().Health < objPlayer.GetAutoAttackDamage(QMobs.FirstOrDefault())) && !MenuSettings.Keys.aaFarmToggle.Active)
                        return;
                    if (Variables.GameTimeTickCount - lastQTime < 1000)
                        return;

                    var getPrediction = Q.GetPrediction(QMobs.FirstOrDefault());

                    if (getPrediction.Hitchance >= HitChance.Medium)
                        Q.Cast(getPrediction.CastPosition);
                }
                if (MenuSettings.JungleClear.UseE.Enabled && E.IsReady() && objPlayer.IsHPBarRendered)
                {
                    var EMobs = mobs.Where(x => x.IsValidTarget(E.Range));

                    if (EMobs.Count() == 0)
                        return;

                    var enoughtMana = objPlayer.ManaPercent > MenuSettings.JungleClear.EMinMana.Value;

                    int jungleType = (int)EMobs.FirstOrDefault().GetJungleType(); // 9 - legendary, 5 - big, 3 - small

                    switch (MenuSettings.JungleClear.EMode.SelectedValue)
                    {
                        case "All Monsters":
                            if (jungleType == 9 || jungleType == 5 || jungleType == 3)
                            {
                                if (enoughtMana && !objPlayer.HasBuff("KarthusDefile"))
                                {
                                    E.Cast();
                                }
                                else if (!enoughtMana && objPlayer.HasBuff("KarthusDefile"))
                                {
                                    E.Cast();
                                }
                            }
                            break;

                        case "Big Monsters":
                            if (jungleType == 9 || jungleType == 5)
                            {
                                if (enoughtMana && !objPlayer.HasBuff("KarthusDefile"))
                                {
                                    E.Cast();
                                }
                                else if (!enoughtMana || objPlayer.HasBuff("KarthusDefile"))
                                {
                                    E.Cast();
                                }
                            }
                            break;
                    }
                }
                if (MenuSettings.JungleClear.UseW.Enabled && W.IsReady())
                {
                    var WMobs = mobs.Where(x => x.IsValidTarget(W.Range) && !x.IsDead);

                    if (WMobs.Count() == 0)
                        return;
                    if (objPlayer.ManaPercent < MenuSettings.JungleClear.WMinMana.Value)
                        return;

                    int jungleType = (int)WMobs.FirstOrDefault().GetJungleType(); // 9 - legendary, 5 - big, 3 - small

                    switch (MenuSettings.JungleClear.WMode.SelectedValue)
                    {
                        case "All Monsters":
                            if (jungleType == 5 || jungleType == 3)
                            {
                                var getPrediction = W.GetPrediction(WMobs.FirstOrDefault());

                                if (getPrediction.Hitchance >= HitChance.Medium)
                                    W.Cast(getPrediction.CastPosition.Extend(objPlayer.Position, -100));
                            }
                            break;

                        case "Big Monsters":
                            if (jungleType == 5)
                            {
                                var getPrediction = W.GetPrediction(WMobs.FirstOrDefault());

                                if (getPrediction.Hitchance >= HitChance.Medium)
                                    W.Cast(getPrediction.CastPosition.Extend(objPlayer.Position, 150));
                            }
                            break;
                    }
                }
            }
            public static void LaneClear()
            {
                if (!MenuSettings.Keys.farmToggle.Active)
                    return;

                var minions = GameObjects.GetMinions(Q.Range)
                    .OrderBy(m => m.Health / m.MaxHealth * 100)
                    .ToList();

                if (minions.Count() == 0)
                    return;

                if (MenuSettings.LaneClear.UseQ.Enabled && Q.IsReady())
                {
                    var min = minions.Where(x => x.IsValidTarget(Q.Range) && !x.IsDead);

                    if (objPlayer.ManaPercent < MenuSettings.LaneClear.QMinMana.Value)
                        return;

                    var getCircularFarmLocation = Q.GetCircularFarmLocation(minions);

                    if (Variables.GameTimeTickCount - lastQTime < 1000)
                        return;

                    switch (MenuSettings.LaneClear.QMode.SelectedValue)
                    {
                        case "Always":
                            if (getCircularFarmLocation.MinionsHit >= 1)
                            {
                                Q.Cast(getCircularFarmLocation.Position);
                            }
                            break;

                        case "Out of Range":
                            if (getCircularFarmLocation.Position.DistanceToPlayer() > objPlayer.AttackRange)
                            {
                                if (objPlayer.Distance(getCircularFarmLocation.Position) >= Q.Range)
                                    return;

                                if (getCircularFarmLocation.MinionsHit >= 1)
                                {
                                    Q.Cast(getCircularFarmLocation.Position);
                                }
                            }
                            break;
                    }
                }
                if (MenuSettings.LaneClear.UseE.Enabled && E.IsReady() && objPlayer.IsHPBarRendered)
                {
                    var enoughtMana = objPlayer.ManaPercent > MenuSettings.LaneClear.EMinMana.Value;

                    var min = minions.Where(x => x.IsValidTarget(E.Range) && !x.IsDead);

                    if (enoughtMana && min.Count() >= MenuSettings.LaneClear.EMinHits.Value && !objPlayer.HasBuff("KarthusDefile"))
                    {
                        E.Cast();
                    }
                    else if ((!enoughtMana || min.Count() <= 2) && objPlayer.HasBuff("KarthusDefile"))
                    {
                        E.Cast();
                    }
                }
            }
            public static void LastHit()
            {
                if (!MenuSettings.Keys.farmToggle.Active)
                    return;

                if (objPlayer.ManaPercent < MenuSettings.LastHit.MinMana.Value)
                    return;

                if (MenuSettings.LastHit.UseQ.Enabled && Q.IsReady())
                {
                    if (Variables.GameTimeTickCount - lastQTime < 1000)
                        return;

                    var minion = GameObjects.GetMinions(Q.Range)
                        .Where(x => x.IsValidTarget(Q.Range) && HealthPrediction.GetPrediction(x, 950) * 0.9 < Q.GetDamage(x) && HealthPrediction.GetPrediction(x, 950) * 0.9 > 0)
                        .OrderBy(x => x.IsMelee)
                        .FirstOrDefault();

                    if (minion != null)
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }
    }

    internal class MenuSettings
    {
        public class AutoR
        {
            public static MenuSlider KillableMinHits = new MenuSlider("KillableMinHits", "Min Killable Hits", 2, 1, 4);
            public static MenuBool UseIfKillable = new MenuBool("UseIfKillable", "Use When Killable");
            public static MenuBool UseInTeamFight = new MenuBool("UseInTeamFight", "Use When Team Fight", false);
            public static MenuBool UseR = new MenuBool("UseR", "Use R");
        }

        public class Combo
        {
            public static MenuBool AA = new MenuBool("AA", "Disable AutoAttack");
            public static MenuBool AALackOfMana = new MenuBool("AALackOfMana", "If No Mana (100) allow Use AA");
            public static MenuList Prediction = new MenuList("Prediction", "Select Prediction:", new[] { "SPrediction (don't work for now)", "Common Prediction" }, 1);
            public static MenuList QHitChance = new MenuList("QHitChance", "Hitchance:", new[] { "Low", "Medium", "High", "VeryHigh", "Immobile" }, 3);
            public static MenuBool UseE = new MenuBool("UseE", "Use E");
            public static MenuBool UseQ = new MenuBool("UseQ", "Use Q");
            public static MenuBool UseW = new MenuBool("UseW", "Use W");
        }

        public class Drawing
        {
            public static MenuBool Disable = new MenuBool("Disable", "Disable", false);
            public static MenuBool DamageIndicator = new MenuBool("DamageIndicator", "Damage Indicator");
            public static MenuBool UltNotification = new MenuBool("UltNotification", "Ult Killable Notification");
            public static MenuBool QWidth = new MenuBool("QWidth", "Q Width");
            public static MenuBool QRange = new MenuBool("QRange", "Q Range");
            public static MenuBool WRange = new MenuBool("WRange", "W Range", false);
            public static MenuBool ERange = new MenuBool("ERange", "E Range");
        }

        public class Harass
        {
            public static MenuBool AA = new MenuBool("AA", "Disable AutoAttack");
            public static MenuSlider MinMana = new MenuSlider("minMana", "Min Mana Percent", 50, 0, 100);
            public static MenuBool UseQ = new MenuBool("UseQ", "Use Q");
        }

        public class JungleClear
        {
            public static MenuSlider EMinMana = new MenuSlider("EMinMana", "Min Mana Percent For E", 65, 0, 100);
            public static MenuList EMode = new MenuList("EMode", "Select Mode:", new[] { "All Monsters", "Big Monsters" }, 0);
            public static MenuSlider QMinMana = new MenuSlider("QMinMana", "Min Mana Percent For Q", 10, 0, 100);
            public static MenuBool UseE = new MenuBool("UseE", "Use E");
            public static MenuBool UseQ = new MenuBool("UseQ", "Use Q");
            public static MenuBool UseW = new MenuBool("UseW", "Use W", false);
            public static MenuSlider WMinMana = new MenuSlider("WMinMana", "Min Mana Percent For W", 50, 0, 100);
            public static MenuList WMode = new MenuList("WMode", "Select Mode:", new[] { "All Monsters", "Big Monsters" }, 1);
        }

        public class Keys
        {
            public static MenuKeyBind aaFarmToggle = new MenuKeyBind("aaFarmToggle", "AutoAttack Farm Key", EnsoulSharp.SDK.MenuUI.Keys.N, KeyBindType.Toggle);
            public static MenuKeyBind farmToggle = new MenuKeyBind("farmToggle", "Spell Farm Key", EnsoulSharp.SDK.MenuUI.Keys.J, KeyBindType.Toggle);
            public static MenuKeyBind harassToggle = new MenuKeyBind("harassToggle", "Harass Key", EnsoulSharp.SDK.MenuUI.Keys.H, KeyBindType.Toggle);
        }

        public class LaneClear
        {
            public static MenuSlider EMinHits = new MenuSlider("EMinHits", "Min Minions Hit", 3, 1, 6);
            public static MenuSlider EMinMana = new MenuSlider("EMinMana", "Min Mana Percent For E", 70, 0, 100);
            public static MenuSlider QMinMana = new MenuSlider("QMinMana", "Min Mana Percent For Q", 50, 0, 100);
            public static MenuList QMode = new MenuList("QMode", "Select Mode:", new[] { "Always", "Out of Range" });
            public static MenuBool UseE = new MenuBool("UseE", "Use E");
            public static MenuBool UseQ = new MenuBool("UseQ", "Use Q");
        }

        public class LastHit
        {
            public static MenuSlider MinMana = new MenuSlider("MinMana", "Min Mana Percent Q", 20, 0, 100);
            public static MenuBool UseQ = new MenuBool("UseQ", "Use Q");
        }

        public class Misc
        {
            public static MenuBool AutoDisableE = new MenuBool("AutoDisableE", "Auto Disable E");
            public static MenuBool AutoZombie = new MenuBool("AutoZombie", "Auto Zombie");
        }
    }

    internal class Program
    {
        public static void Loads()
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Karthus")
                return;

            Karthus.OnLoad();
        }
    }
}