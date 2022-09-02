using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using Flowers_Karthus.Common;
using PortAIO;
using ShadowTracker;
using SharpDX;
using SPrediction;

namespace Flowers_Karthus
{
    public static class Program
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static int SkinID;
        public static int LastPingT;
        public static int LastShowNoit;
        public static int CastSpellFarmTime;
        public static Menu Menu;
        public static AIHeroClient Me;
        public static Vector3 PingLocation;
        public static HpBarDraw HpbarDraw = new HpBarDraw();

        public static void Loads()
        {
            Game_OnGameLoad();
        }

        static void Game_OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName.ToLower() != "karthus")
            {
                return;
            }

            var enemyTracker = new EnemyTracker();

            Me = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 880f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 550f);
            R = new Spell(SpellSlot.R, 20000f);

            Q.SetSkillshot(0.95f, 145f, float.MaxValue, false, SpellType.Circle);
            W.SetSkillshot(0.5f, 50f, float.MaxValue, false, SpellType.Circle);
            E.SetSkillshot(1.0f, 505f, float.MaxValue, false, SpellType.Circle);
            R.SetSkillshot(3.0f, float.MaxValue, float.MaxValue, false, SpellType.Circle);

            SkinID = Me.SkinId;

            Menu = new Menu("Flowers' Karthus", "Flowers' Karthus", true);
            
            var ComboMenu = Menu.Add(new Menu("Combo", "Combo"));
            {
                ComboMenu.Add(new MenuBool("ComboQ", "Use Q", true).SetValue(true));
                ComboMenu.Add(new MenuBool("ComboW", "Use W", true).SetValue(true));
                ComboMenu.Add(new MenuBool("ComboE", "Use E", true).SetValue(true));
                ComboMenu.Add(new MenuSlider("ComboEMana", "Use E|When Player ManaPercent >= x%", 30));
                ComboMenu.Add(new MenuBool("ComboDisable", "Disable Auto Attack?", true).SetValue(true));
            }

            var HarassMenu = Menu.Add(new Menu("Harass", "Harass"));
            {
                HarassMenu.Add(new MenuBool("HaraassQ", "Use Q", true).SetValue(true));
                HarassMenu.Add(new MenuBool("HaraassQLH", "Use Q| Last Hit", true).SetValue(true));
                HarassMenu.Add(new MenuBool("HaraassE", "Use E| Last Hit", true).SetValue(false));
                HarassMenu.Add(new MenuKeyBind("AutoHarass", "Auto Harass?", Keys.T, KeyBindType.Toggle)).AddPermashow();
                HarassMenu.Add(new MenuSlider("HaraassMana", "When Player ManaPercent >= x%", 60));
            }

            var LaneClearMenu = Menu.Add(new Menu("LaneClear", "LaneClear"));
            {
                LaneClearMenu.Add(new MenuBool("LaneClearQ", "Use Q", true).SetValue(true));
                LaneClearMenu.Add(new MenuBool("LaneClearE", "Use E", true).SetValue(true));
                LaneClearMenu.Add(new MenuSlider("LaneClearECount", "Use E| Min Hit Minions Count >= x", 3, 1, 5));
                LaneClearMenu.Add(new MenuSlider("LaneClearMana", "When Player ManaPercent >= x%", 60));
            }

            var JungleClearMenu = Menu.Add(new Menu("JungleClear", "JungleClear"));
            {
                JungleClearMenu.Add(new MenuBool("JungleClearQ", "Use Q", true).SetValue(true));
                JungleClearMenu.Add(new MenuBool("JungleClearE", "Use E", true).SetValue(true));
                JungleClearMenu.Add(new MenuSlider("JungleClearMana", "When Player ManaPercent >= x%", 30));
            }

            var LastHitMenu = Menu.Add(new Menu("LastHit", "LastHit"));
            {
                LastHitMenu.Add(new MenuBool("LastHitQ", "Use Q", true).SetValue(true));
                LastHitMenu.Add(new MenuSlider("LastHitMana", "When Player ManaPercent >= x%", 60));
            }

            var KillStealMenu = Menu.Add(new Menu("KillSteal", "KillSteal"));
            {
                KillStealMenu.Add(new MenuBool("KillStealQ", "Use Q", true).SetValue(true));
            }

            var UltMenu = Menu.Add(new Menu("Ult Settings", "Ult Settings"));
            {
                UltMenu.Add(new MenuBool("TeamFightR", "Use R TeamFight", true).SetValue(true));
                UltMenu.Add(new MenuBool("KillStealR", "Use R KillSteal", true).SetValue(true));
                UltMenu.Add(new MenuSlider("KillStealRCount", "Use R KillSteal Count >=", 2, 1, 5));
                foreach (var target in GameObjects.EnemyHeroes)
                {
                    UltMenu.Add(new MenuBool("KillStealR" + target.CharacterName.ToLower(), "Kill: " + target.CharacterName, true).SetValue(true));
                }
            }

            var MiscMenu = Menu.Add(new Menu("Misc", "Misc"));
            {
                MiscMenu.Add(new MenuBool("AutoZombie", "Auto Zombie Mode?", true).SetValue(true));
                MiscMenu.Add(new MenuBool("PingKill", "Auto Ping Kill Target", true).SetValue(true));
                MiscMenu.Add(new MenuBool("NormalPingKill", "Normal Ping?", true).SetValue(true));
                MiscMenu.Add(new MenuBool("NotificationKill", "Notification Kill Target", true).SetValue(true));
            }

            var SkinMenu = Menu.Add(new Menu("SkinChance", "SkinChance"));
            {
                SkinMenu.Add(new MenuBool("EnableSkin", "Enabled", true).SetValue(false)).ValueChanged += EnbaleSkin;
                SkinMenu.Add(new MenuList("SelectSkin", "Select Skin: ", new[] { "Classic Karthus", "Phantom Karthus", "Statue of Karthus", "Grim Reaper Karthus", "Pentakill Karthus", "Fnatic Karthus" }));
            }

            var PredMenu = Menu.Add(new Menu("Prediction", "Prediction"));
            {
                PredMenu.Add(new MenuList("SelectPred", "Select Prediction: ", new[] { "Common Prediction", "OKTW Prediction", "SDK Prediction", "SPrediction(Need F5 Reload)", "xcsoft AIO Prediction" }, 1));
                PredMenu.Add(new MenuSeparator("AboutCommonPred", "Common Prediction -> LeagueSharp.Commmon Prediction"));
                PredMenu.Add(new MenuSeparator("AboutOKTWPred", "OKTW Prediction -> Sebby' Prediction"));
                PredMenu.Add(new MenuSeparator("AboutSDKPred", "SDK Prediction -> LeagueSharp.SDKEx Prediction"));
                PredMenu.Add(new MenuSeparator("AboutSPred", "SPrediction -> Shine' Prediction"));
                PredMenu.Add(new MenuSeparator("AboutxcsoftAIOPred", "xcsoft AIO Prediction -> xcsoft ALL In One Prediction"));
            }

            var DrawMenu = Menu.Add(new Menu("Drawings", "Drawings"));
            {
                DrawMenu.Add(new MenuBool("DrawQ", "Draw Q Range", true).SetValue(false));
                DrawMenu.Add(new MenuBool("DrawW", "Draw W Range", true).SetValue(false));
                DrawMenu.Add(new MenuBool("DrawE", "Draw E Range", true).SetValue(false));
                DrawMenu.Add(new MenuBool("DrawDamage", "Draw ComboDamage", true).SetValue(true));
                DrawMenu.Add(new MenuBool("DrawKillSteal", "Draw KillSteal target", true).SetValue(true));
            }

            Menu.Add(new MenuSeparator("Credit", "Credit: NightMoon"));

            Menu.Attach();

            Game.Print("Flowers' Karthus Load Succeed! Credit: NightMoon");
            
            if (Menu["SelectPred"].GetValue<MenuList>().Index == 3)
            {
                SPrediction.Prediction.Initialize(PredMenu);
            }

            AIBaseClient.OnDoCast += OnProcessSpellCast;
            Game.OnUpdate += OnUpdate;
            
            Drawing.OnDraw += OnDraw;

            Drawing.OnEndScene += OnEndScene;
        }

        private static void OnEndScene(EventArgs args)
        {
            if (!Me.IsDead && !ObjectManager.Player.InShop() && !MenuGUI.IsChatOpen)
            {
                if (Menu["DrawDamage"].GetValue<MenuBool>().Enabled)
                {
                    foreach (
                        var x in ObjectManager.Get<AIHeroClient>()
                            .Where(e => e.IsValidTarget() && !e.IsDead && !e.IsZombie()))
                    {
                        HpBarDraw.Unit = x;
                        HpBarDraw.DrawDmg(ComboDamage(x), new ColorBGRA(255, 204, 0, 170));
                    }
                }
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (sender.IsMe && Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                var castedSlot = ObjectManager.Player.GetSpellSlot(Args.SData.Name);

                if (castedSlot == SpellSlot.Q || castedSlot == SpellSlot.E)
                {
                    CastSpellFarmTime = Environment.TickCount;
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(h => R.IsReady() && h.IsValidTarget() && GetRDamage(h) > h.Health + h.MagicalShield))
            {
                if (Menu["PingKill"].GetValue<MenuBool>().Enabled)
                {
                    Ping(enemy.Position);
                }

                /*if (Menu["NotificationKill"].GetValue<MenuBool>().Enabled && Environment.TickCount - LastShowNoit > 10000)
                {
                    LeagueSharpCommon.Notifications.Notifications.AddNotification(new LeagueSharpCommon.Notifications.Notification("R Kill: " + enemy.CharacterName + "!", 3000, true).SetTextColor(System.Drawing.Color.FromArgb(255, 0, 0)));
                    LastShowNoit = Environment.TickCount;
                }*/
            }

            if (Me.IsRecalling())
            {
                return;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                try{
                    ComboLogic();
                }catch(Exception) {}
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass || Menu["AutoHarass"].GetValue<MenuKeyBind>().Active)
            {
                try{
                    HarassLogic();
                }catch(Exception) {}
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                try{
                    LaneClearLogic();
                }catch(Exception) {}
                JungleClearLogic();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {
                try{
                    LastHitLogic();
                }catch(Exception) {}
            }
            try{
                AutoZombie();
            }catch(Exception){}
            try{
                TeamFightUlt();
            }catch(Exception){}
            try{
                KillStealLogic();
            }catch(Exception){}
            

            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
            {
                Orbwalker.AttackEnabled =  (true);
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.None && !Me.IsZombie() && E.Instance.ToggleState == (SpellToggleState) 2 &&
                !Me.InFountain())
            {
                E.Cast();
            }
        }
        
        private static void ComboLogic()
        {
            Orbwalker.AttackEnabled = (!Menu["ComboDisable"].GetValue<MenuBool>().Enabled);

            if (Me.CountEnemyHeroesInRange(E.Range) == 0 && E.Instance.ToggleState == (SpellToggleState) 2)
            {
                E.Cast();
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target.IsValidTarget(Q.Range) && !target.IsZombie())
            {
                if (Menu["ComboQ"].GetValue<MenuBool>().Enabled && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.CastTo(target, true);
                }

                if (Menu["ComboE"].GetValue<MenuBool>().Enabled && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (Me.ManaPercent >= Menu["ComboEMana"].GetValue<MenuSlider>().Value)
                    {
                        if (E.Instance.ToggleState == (SpellToggleState) 2)
                        {
                            E.Cast();
                        }
                    }
                    else if (E.Instance.ToggleState == (SpellToggleState) 2)
                    {
                        E.Cast();
                    }
                }

                if (Menu["ComboW"].GetValue<MenuBool>().Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.CastTo(target);
                }
            }
        }
        
        private static void HarassLogic()
        {
            if (Me.ManaPercent >= Menu["HaraassMana"].GetValue<MenuSlider>().Value)
            {
                if (Menu["HaraassQLH"].GetValue<MenuBool>().Enabled)
                {
                    var minions = MinionManager.GetMinions(Q.Range)
                        .Where(x => SebbyLib.HealthPrediction.GetHealthPrediction(x, 950) * 0.9 < GetQDamage(x) &&
                                    SebbyLib.HealthPrediction.GetHealthPrediction(x, 950) * 0.9 > 0);

                    var aiBaseClients = minions as AIBaseClient[] ?? minions.ToArray();
                    if (aiBaseClients.Any())
                    {
                        var min = aiBaseClients.FirstOrDefault();

                        if (Environment.TickCount - CastSpellFarmTime > 1500)
                        {
                            Q.Cast(min, true);
                        }
                    }
                }

                if (Menu["HaraassQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
                {
                    var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                    if (target.IsValidTarget())
                    {
                        if (target.IsValidTarget(Q.Range))
                        {
                            Q.CastTo(target, true);
                        }
                    }
                }

                if (Menu["HaraassE"].GetValue<MenuBool>().Enabled && E.IsReady())
                {
                    var minions =
                        ObjectManager.Get<AIMinionClient>()
                            .Where(
                                x =>
                                    x.IsEnemy && x.Name != "gangplankbarrel" && x.Name != "WardCorpse" &&
                                    x.Name != "jarvanivstandard" && x.Distance(Me) < E.Range &&
                                    x.Health < GetEDamage(x)).ToList();

                    if (minions.Count(x => x.Distance(Me) < E.Range) > 0)
                    {
                        if (Environment.TickCount - CastSpellFarmTime > 1500)
                        {
                            if (E.Instance.ToggleState != (SpellToggleState) 2)
                            {
                                E.Cast();
                            }
                        }
                    }
                    else
                    {
                        if (E.Instance.ToggleState == (SpellToggleState) 2)
                        {
                            E.Cast();
                        }
                    }
                }
            }
        }
        
        private static void LaneClearLogic()
        {
            if (Me.ManaPercent >= Menu["LaneClearMana"].GetValue<MenuSlider>().Value || Me.IsZombie())
            {
                var minions = MinionManager.GetMinions(Me.Position, 500);

                if (!minions.Any())
                {
                    return;
                }

                var minion = minions.Find(x => x.Health > Me.GetAutoAttackDamage(x));

                if (Menu["LaneClearQ"].GetValue<MenuBool>().Enabled && Q.IsReady() && minion != null && minion.IsValidTarget(Q.Range))
                {
                    Q.Cast(minion, true);
                }

                if (Menu["LaneClearE"].GetValue<MenuBool>().Enabled && E.IsReady() &&
                    minions.Count(x => x.Distance(Me) <= E.Range) >=
                    Menu["LaneClearECount"].GetValue<MenuSlider>().Value && E.Instance.ToggleState != (SpellToggleState) 2)
                {
                    E.Cast();
                }
                else if (minions.Count(x => x.Distance(Me) <= E.Range) == 0 && E.Instance.ToggleState == (SpellToggleState) 2)
                {
                    E.Cast();
                }
            }
            else if (E.Instance.ToggleState == (SpellToggleState) 2)
            {
                E.Cast();
            }
        }
        
        private static void JungleClearLogic()
        {
            if (Me.ManaPercent >= Menu["JungleClearMana"].GetValue<MenuSlider>().Value || Me.IsZombie())
            {
                var mobs = MinionManager.GetMinions(Me.Position, Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);

                if (!mobs.Any())
                {
                    return;
                }

                var mob = mobs.FirstOrDefault();

                if (Menu["JungleClearQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
                {
                    Q.CastTo(mob, true);
                }

                if (Menu["JungleClearE"].GetValue<MenuBool>().Enabled && E.IsReady() && E.Instance.ToggleState != (SpellToggleState) 2 &&
                    mob.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
                else if (mobs.Count(x => x.Distance(Me) <= E.Range) == 0 && E.Instance.ToggleState == (SpellToggleState) 2)
                {
                    E.Cast();
                }
            }
            else if (E.Instance.ToggleState == (SpellToggleState) 2)
            {
                E.Cast();
            }
        }

        private static void LastHitLogic()
        {
            if (Me.ManaPercent >= Menu["LastHitMana"].GetValue<MenuSlider>().Value || Me.IsZombie())
            {
                if (Menu["LastHitQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
                {
                    var minions = MinionManager.GetMinions(Q.Range)
                        .Where(x => SebbyLib.HealthPrediction.GetHealthPrediction(x, 950) * 0.9 < GetQDamage(x) &&
                                    SebbyLib.HealthPrediction.GetHealthPrediction(x, 950) * 0.9 > 0);

                    if (minions.Any())
                    {
                        var min = minions.FirstOrDefault();

                        Q.Cast(min, true);
                    }
                }
            }
        }
        
        private static void AutoZombie()
        {
            if (Menu["AutoZombie"].GetValue<MenuBool>().Enabled && Me.IsZombie())
            {
                if (E.Instance.ToggleState != (SpellToggleState) 2)
                {
                    E.Cast();
                }

                if (Me.CountEnemyHeroesInRange(Q.Range) > 0)
                {
                    ComboLogic();
                }
                else
                {
                    LaneClearLogic();
                    JungleClearLogic();
                }
            }
        }

        private static void TeamFightUlt()
        {
            var CanCastR = false;

            if (Menu["TeamFightR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes)
                {
                    if (!target.IsValidTarget())
                    {
                        continue;
                    }

                    if (target.IsDead)
                    {
                        continue;
                    }

                    if (target.IsZombie())
                    {
                        continue;
                    }

                    if (target.HasBuff("KindredRNoDeathBuff"))
                    {
                        continue;
                    }

                    if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
                    {
                        continue;
                    }

                    if (target.HasBuff("JudicatorIntervention"))
                    {
                        continue;
                    }

                    if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
                    {
                        continue;
                    }

                    if (target.HasBuff("FioraW"))
                    {
                        continue;
                    }

                    if (target.CountAllyHeroesInRange(850) > 1 && target.CountEnemyHeroesInRange(850) <= 2 &&
                        target.Health + target.MagicalShield < GetRDamage(target) * 2 && Me.IsZombie())
                    {
                        CanCastR = true;
                    }

                    //if (Me.CountEnemiesInRange(850) > 0 && target.Health < GetRDamage(target) + GetQDamage(target) * 3)
                    //{
                    //    CanCastR = true;
                    //}

                    if (Me.CountEnemyHeroesInRange(1000) >= 3 && Me.CountAllyHeroesInRange(850) <= 3)
                    {
                        CanCastR = true;
                    }
                }

                if (Me.IsZombie())
                {
                    var passivetime = Me.GetBuff("KarthusDeathDefiedBuff").EndTime;

                    if (passivetime > 3 && passivetime < 4)
                    {
                        if (CanCastR)
                        {
                            R.Cast();
                        }
                    }
                }
                else if (!Me.IsZombie())
                {
                    if (CanCastR && Me.CountEnemyHeroesInRange(800) == 0)
                    {
                        R.Cast();
                    }
                }
            }
            else
            {
                CanCastR = false;
            }
        }

        private static void KillStealLogic()
        {
            if (GameObjects.EnemyHeroes == null)
            {
                return;
            }

            KillStealRLogic();

            foreach (var target in GameObjects.EnemyHeroes)
            {
                if (target.IsDead || target.IsZombie())
                {
                    continue;
                }

                if (Menu["KillStealQ"].GetValue<MenuBool>().Enabled && Q.IsReady() && target.Health + target.MagicalShield < GetQDamage(target))
                {
                    Q.CastTo(target, true);
                }
            }
        }

        private static void KillStealRLogic()
        {
            if (Menu["KillStealR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                var targets = new List<AIHeroClient>();

                foreach (var ult in EnemyTracker.enemyInfo)
                {
                    if (ult.target.IsDead)
                    {
                        continue;
                    }

                    if (ult.target.IsZombie())
                    {
                        continue;
                    }

                    if (ult.target.HasBuff("KindredRNoDeathBuff"))
                    {
                        continue;
                    }

                    if (ult.target.HasBuff("UndyingRage") && ult.target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
                    {
                        continue;
                    }

                    if (ult.target.HasBuff("JudicatorIntervention"))
                    {
                        continue;
                    }

                    if (ult.target.HasBuff("ChronoShift") && ult.target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
                    {
                        continue;
                    }

                    if (ult.target.HasBuff("FioraW"))
                    {
                        continue;
                    }

                    if (!Menu["KillStealR" + ult.target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                    {
                        continue;
                    }

                    if (ult.target.IsVisible &&
                        R.GetDamage(ult.target) >
                        ult.target.Health + ult.target.MagicalShield + ult.target.HPRegenRate*2)
                    {
                        targets.Add(ult.target);
                    }

                    if (!ult.target.IsVisible && Environment.TickCount > ult.LastSeen + 5000 &&
                        R.GetDamage(ult.target) > EnemyTracker.GetTargetHealth(ult, R.Delay))
                    {
                        targets.Add(ult.target);
                    }

                    if (!ult.target.IsVisible && Environment.TickCount < ult.LastSeen + 5000 && targets.Contains(ult.target))
                    {
                        targets.Remove(ult.target);
                    }
                }

                if (targets.Count >= Menu["KillStealRCount"].GetValue<MenuSlider>().Value)
                {
                    if (!Me.IsZombie() && Me.CountEnemyHeroesInRange(800) > 0)
                    {
                        return;
                    }

                    R.Cast();
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Me.IsDead && !ObjectManager.Player.InShop() && !MenuGUI.IsChatOpen)
            {
                if (Menu["DrawQ"].GetValue<MenuBool>().Enabled && Q.Level > 0)
                {
                    LeagueSharpCommon.Render.Circle.DrawCircle(Me.Position, Q.Range, System.Drawing.Color.FromArgb(253, 164, 17), 1);
                }

                if (Menu["DrawW"].GetValue<MenuBool>().Enabled && W.Level > 0)
                {
                    LeagueSharpCommon.Render.Circle.DrawCircle(Me.Position, W.Range, System.Drawing.Color.FromArgb(9, 253, 242), 1);
                }

                if (Menu["DrawE"].GetValue<MenuBool>().Enabled && E.Level > 0)
                {
                    LeagueSharpCommon.Render.Circle.DrawCircle(Me.Position, E.Range, System.Drawing.Color.FromArgb(143, 16, 146), 1);
                }


                try
                {
                    if (Menu["DrawKillSteal"].GetValue<MenuBool>().Enabled)
                    {
                        Drawing.DrawText(Drawing.Width - 150, Drawing.Height - 500, System.Drawing.Color.Yellow,
                            "Ult Kill Target: ");

                        var targets = new List<AIHeroClient>();
                        foreach (var ult in EnemyTracker.enemyInfo)
                        {
                            if (ult.target.IsDead)
                            {
                                continue;
                            }

                            if (ult.target.IsZombie())
                            {
                                continue;
                            }

                            if (ult.target.HasBuff("KindredRNoDeathBuff"))
                            {
                                continue;
                            }

                            if (ult.target.HasBuff("UndyingRage") &&
                                ult.target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
                            {
                                continue;
                            }

                            if (ult.target.HasBuff("JudicatorIntervention"))
                            {
                                continue;
                            }

                            if (ult.target.HasBuff("ChronoShift") &&
                                ult.target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
                            {
                                continue;
                            }

                            if (ult.target.HasBuff("FioraW"))
                            {
                                continue;
                            }

                            if (!Menu["KillStealR" + ult.target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                            {
                                continue;
                            }

                            if (ult.target.IsVisible &&
                                R.GetDamage(ult.target) >
                                ult.target.Health + ult.target.MagicalShield + ult.target.HPRegenRate * 2)
                            {
                                targets.Add(ult.target);
                            }

                            if (!ult.target.IsVisible && Environment.TickCount > ult.LastSeen + 5000 &&
                                R.GetDamage(ult.target) > EnemyTracker.GetTargetHealth(ult, R.Delay))
                            {
                                targets.Add(ult.target);
                            }

                            if (!ult.target.IsVisible && Environment.TickCount < ult.LastSeen + 5000 &&
                                targets.Contains(ult.target))
                            {
                                targets.Remove(ult.target);
                            }

                        }

                        if (targets.Count > 0)
                        {
                            for (var i = 0; i <= targets.Count; i++)
                            {
                                Drawing.DrawText(Drawing.Width - 150, Drawing.Height - 470 + i * 30,
                                    System.Drawing.Color.Red, "   " + targets.ElementAt(i).CharacterName);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static void EnbaleSkin(MenuBool menuitem, EventArgs args)
        {
            if (!menuitem.Enabled)
            {
                ObjectManager.Player.SetSkin(Menu["SkinChance"]["SelectSkin"].GetValue<MenuList>().Index);
            }
        }
        
        private static void Ping(Vector3 position)
        {
            if (Environment.TickCount - LastPingT < 30 * 1000)
            {
                return;
            }

            LastPingT = Environment.TickCount;
            PingLocation =  position;
            SimplePing();

            DelayAction.Add(150, SimplePing);
            DelayAction.Add(300, SimplePing);
            DelayAction.Add(400, SimplePing);
            DelayAction.Add(800, SimplePing);
        }

        private static void SimplePing()
        {
            Game.ShowPing(
                Menu["NormalPingKill"].GetValue<MenuBool>().Enabled ? PingCategory.Normal : PingCategory.Fallback,
                PingLocation, true);
        }

        private static float ComboDamage(AIHeroClient target)
        {
            var damage = GetQDamage(target) + GetWDamage(target) + GetEDamage(target) + GetRDamage(target);

            return (float)damage;
        }

        private static double GetQDamage(AIBaseClient target)
        {
            return !target.IsValidTarget() ? 0d : Q.GetDamage(target);
        }

        private static double GetWDamage(AIBaseClient target)
        {
            return !target.IsValidTarget() ? 0d : W.GetDamage(target);
        }

        private static double GetEDamage(AIBaseClient target)
        {
            return !target.IsValidTarget() ? 0d : E.GetDamage(target);
        }

        private static double GetRDamage(AIBaseClient target)
        {
            return !target.IsValidTarget() ? 0d : R.GetDamage(target);
        }
    }
}