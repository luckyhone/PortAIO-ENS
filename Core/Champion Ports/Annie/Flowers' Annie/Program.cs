 using EnsoulSharp;
 using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using EnsoulSharp.SDK.Rendering;
 using LeagueSharpCommon;
 using SharpDX;
 using SPrediction;
 using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
 using Keys = EnsoulSharp.SDK.MenuUI.Keys;
 using Menu = EnsoulSharp.SDK.MenuUI.Menu;
 using Render = EnsoulSharp.SDK.Render;

 namespace Flowers__Annie
{
    using System;
    using System.Linq;

    class Program
    {
        internal static AIHeroClient Me;
        internal static Menu Menu;
        internal static Spell Q, W, E, R;
        internal static SpellSlot Flash, Ignite;
        internal static float ClickTime;
        internal static HpBarDraw DrawHpBar = new HpBarDraw();

        public static void Game_OnGameLoad()
        {
            // Judge ChampionName , if not Annie return , not injected.
            if (ObjectManager.Player.CharacterName != "Annie")
                return;

            // this is hero!
            Me = ObjectManager.Player;

            // set Annie Spells
            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 580f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 625f);
            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.50f, 200f, float.MaxValue, false, SpellType.Cone);
            R.SetSkillshot(0.20f, 100f, float.MaxValue, false, SpellType.Circle);

            // set R HitChance
            R.MinHitChance = HitChance.High;

            // set SummonerSpells
            Ignite = Me.GetSpellSlot("SummonerDot");
            Flash = Me.GetSpellSlot("SummonerFlash");

            // Make a Menu
            Menu = new Menu("NightMoon","Flowers' Annie", true);

            var QSetting = Menu.Add(new Menu("QSetting","Q Setting"));
            QSetting.Add(new MenuSeparator("ComboQQQQQ", "       Combo Setting"));
            QSetting.Add(new MenuBool("ComboQ", "--- Always Use !!!!!", true));
            QSetting.Add(new MenuSeparator("HarassQQQQQ", "       Harass Setting"));
            QSetting.Add(new MenuBool("HarassQ", "Use Q To Harass", true).SetValue(true));
            QSetting.Add(new MenuBool("HarassQOnlyStun", "--- Only Have Stun(Use Q)", true).SetValue(true));
            QSetting.Add(new MenuBool("HarassAutoLastHitQ", "--- Auto Q To LastHit", true).SetValue(true));
            QSetting.Add(new MenuSeparator("ClearQQQQQ", "       Clear Setting"));
            QSetting.Add(new MenuBool("LaneClearQ", "Use Q To LaneCLear", true).SetValue(true));
            QSetting.Add(new MenuBool("LaneClearQLastHit", "--- Only Use Q in LastHit", true).SetValue(true));
            QSetting.Add(new MenuBool("JungleClearQ", "Use Q To JungleClear", true).SetValue(true));
            QSetting.Add(new MenuSeparator("LastHitQQQQQ", "       LastHit Setting"));
            QSetting.Add(new MenuBool("LastHitQ", "Use Q To LastHit", true).SetValue(true));

            var WSetting = Menu.Add(new Menu("WSetting","W Setting"));
            WSetting.Add(new MenuSeparator("ComboWWWWWW", "       Combo Setting"));
            WSetting.Add(new MenuBool("ComboW", "--- Always Use !!!!!", true));
            WSetting.Add(new MenuSeparator("HarassWWWWW", "       Harass Setting"));
            WSetting.Add(new MenuBool("HarassW", "Use W To Harass", true).SetValue(true));
            WSetting.Add(new MenuBool("HarassWDebuff", "--- Only Target Has Debuff", true).SetValue(true));
            WSetting.Add(new MenuSeparator("ClearWWWWW", "       Clear Setting"));
            WSetting.Add(new MenuBool("LaneClearW", "Use W To LaneCLear", true).SetValue(true));
            WSetting.Add(new MenuSlider("LaneClearWCount", "--- Min LaneClear minion Counts >= ", 3, 1, 5));
            WSetting.Add(new MenuBool("JungleClearW", "Use W To JungleClear", true).SetValue(true));

            var ESetting = Menu.Add(new Menu("ESetting","E Setting"));
            ESetting.Add(new MenuSeparator("ComboEEEEE", "       Combo Setting"));
            ESetting.Add(new MenuBool("SmartEInCombo", "Smart E in Combo Mode", true).SetValue(true));
            ESetting.Add(new MenuSeparator("ClearEEEEE", "       Clear Setting"));
            ESetting.Add(new MenuBool("JungleClearE", "If Jungle Attack Me", true).SetValue(true));

            var RSetting = Menu.Add(new Menu("RSetting","R Setting"));
            RSetting.Add(new MenuSeparator("ComboRRRRR", "       Combo Setting"));
            RSetting.Add(new MenuSlider("ComboRCountsEnemiesinRange", "--- Min Hit Enemies Counts >= ", 2, 1, 5));
            RSetting.Add(new MenuBool("ComboRCanKill", "--- Or Can Kill Enemy", true).SetValue(true));
            RSetting.Add(new MenuSeparator("FlashRRRRR", "       FlashR Setting"));
            RSetting.Add(new MenuKeyBind("EnableFlashR", "FlashR Key!", Keys.T, KeyBindType.Press));
            RSetting.Add(new MenuSlider("FlashRCountsEnemiesinRange", "--- Min Hit Enemies Counts >= ", 3, 1, 5));
            RSetting.Add(new MenuSlider("FlashRCountsAlliesinRange", "--- And Min Allies Counts >= (0 = off)", 2, 0, 5));
            RSetting.Add(new MenuBool("FlashRCanKillEnemy", "--- Or Can Kill (Only In 1v1) (TEST)", true).SetValue(true));
            RSetting.Add(new MenuSeparator("BearRRRRR", "       Bear Setting"));
            RSetting.Add(new MenuBool("BearAutoFollow", "--- Auto Follow Enemy Or MySelf", true).SetValue(true));

            var KillSteal = Menu.Add(new Menu("KillSteal","KillSteal"));
            KillSteal.Add(new MenuBool("KillStealEnable", "Enable", true).SetValue(true));
            KillSteal.Add(new MenuBool("KillStealQ", "Use Q", true).SetValue(true));
            KillSteal.Add(new MenuBool("KillStealW", "Use W", true).SetValue(true));
            KillSteal.Add(new MenuBool("KillStealR", "Use R", true).SetValue(false));
            KillSteal.Add(new MenuBool("KillStealIgnite", "Use Ignite", true).SetValue(true));

            var ManaControl = Menu.Add(new Menu("ManaControl","Mana Control"));
            ManaControl.Add(new MenuSlider("HarassMana", "Min Harass Mana >= %",60));
            ManaControl.Add(new MenuSlider("LaneClearMana", "Min LaneClear Mana >= %",40));
            ManaControl.Add(new MenuSlider("JungleClearMana", "Min JungleClear Mana >= %",30));
            ManaControl.Add(new MenuSlider("LastHitMana", "Min LastHit Mana >= %",20));
            ManaControl.Add(new MenuSlider("AutoStackMana", "Min Auto Stack Passive Mana >= %",50));

            var StackPassive = Menu.Add(new Menu("StackPassive","Stack Passive"));
            StackPassive.Add(new MenuBool("AutoStackEnable", "Enable", true).SetValue(true));
            StackPassive.Add(new MenuBool("AutoStackW", "--- Use W", true).SetValue(true));
            StackPassive.Add(new MenuBool("AutoStackE", "--- Use E", true).SetValue(true));

            var Misc = Menu.Add(new Menu("Misc","Misc"));
            Misc.Add(new MenuSeparator("IntAntt", "         GapCloser & Interrupt"));
            Misc.Add(new MenuBool("IntAnttEnable", "Enable", true).SetValue(true));
            Misc.Add(new MenuBool("IntAnttQ", "--- Use Q", true).SetValue(true));
            Misc.Add(new MenuBool("IntAnttW", "--- Use W", true).SetValue(true));
            Misc.Add(new MenuBool("IntAnttE", "--- Use E", true).SetValue(true));
            Misc.Add(new MenuSeparator("StunSet", "         Stun Setting"));
            Misc.Add(new MenuBool("ClearStun", "--- In ClearMode, Have Passive Disable Use Spells", true).SetValue(true));
            // Menu.SubMenu("Misc").AddItem(new MenuItem("LastHitStun", "--- In LastHitMode, Have Passive Disable Use Spells", true).SetValue(true));
            Misc.Add(new MenuSeparator("SupportMode", "         Support Mode"));
            Misc.Add(new MenuBool("SupportEnable", "Enable!", true).SetValue(false));

            var Drawing = Menu.Add(new Menu("Drawing","Drawing"));
            Drawing.Add(new MenuBool("DrawQ", "Draw Q Range", true).SetValue(false));
            Drawing.Add(new MenuBool("DrawW", "Draw W Range", true).SetValue(false));
            Drawing.Add(new MenuBool("DrawR", "Draw R Range", true).SetValue(false));
            Drawing.Add(new MenuBool("DrawRF", "Draw R + Flash Range", true).SetValue(false));
            Drawing.Add(new MenuBool("DrawDamage", "Draw Damage In HpBar", true).SetValue(false));

            // Finish
            Menu.Attach();

            // Chat
            Game.Print("<font color='#2848c9'>Flowers Annie</font> --> <font color='#b756c5'>Version : 1.0.0.3</font> <font size='30'><font color='#d949d4'>Good Luck!</font></font>");

            // Events
            Orbwalker.OnBeforeAttack += BeforeAttack;
            AntiGapcloser.OnGapcloser += OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
            AIBaseClient.OnDoCast += OnProcessSpellCast;
            Game.OnUpdate += OnUpdate;
            EnsoulSharp.Drawing.OnDraw += OnDraw;
            EnsoulSharp.Drawing.OnEndScene += OnEndScene;
        }

        private static void OnEndScene(EventArgs args)
        {
            if(Me.IsDead) return;
            if (Menu.GetBool("DrawDamage"))
            {
                foreach (var e in ObjectManager.Get<AIHeroClient>().Where(e => e.IsValidTarget() && !e.IsDead && !e.IsZombie()))
                {
                    HpBarDraw.Unit = e;
                    HpBarDraw.DrawDmg(GetDamage(e, true, true, true, true), new SharpDX.ColorBGRA(255, 204, 0, 170));
                }
            }
        }


        private static void BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (HarassMode && Menu.GetBool("SupportEnable") && (args.Target.Type == GameObjectType.AIMinionClient))
            {
                args.Process = false;
            }
        }

        /// <summary>
        /// use qwe to gapcloser
        /// </summary>
        /// <param name="gapcloser"></param>
        private static void OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (sender.IsEnemy && Menu.GetBool("IntAnttEnable"))
            {
                if (!HaveStun && BuffCounts == 3 && E.IsReady() && Menu.GetBool("IntAnttE"))
                {
                    E.Cast();
                }

                if (HaveStun)
                {
                    if (Q.IsReady() && Menu.GetBool("IntAnttQ") && sender.IsValidTarget(300))
                    {
                        Q.Cast(sender, true);
                    }
                    else if (W.IsReady() && Menu.GetBool("IntAnttW") && sender.IsValidTarget(250))
                    {
                        W.Cast(sender, true);
                    }
                }
            }
        }

        /// <summary>
        ///  use QWE to interrupt spells
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (args.DangerLevel >= Interrupter.DangerLevel.Medium && Menu.GetBool("IntAnttEnable"))
            {
                if (!HaveStun && BuffCounts == 3 && E.IsReady() && Menu.GetBool("IntAnttE"))
                {
                    E.Cast();
                }

                if (HaveStun)
                {
                    if (Q.IsReady() && Menu.GetBool("IntAnttQ") && sender.IsValidTarget(Q.Range))
                    {
                        Q.Cast(sender, true);
                    }
                    else if (W.IsReady() && Menu.GetBool("IntAnttW") && sender.IsValidTarget(W.Range))
                    {
                        W.Cast(sender, true);
                    }
                }
            }
        }

        /// <summary>
        ///  use e in combo & clear
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender == null || !args.Target.IsMe)
                    return;

                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo &&
                    Menu["ESetting"]["SmartEInCombo"].GetValue<MenuBool>().Enabled && E.IsReady())
                {
                    if (sender.IsEnemy && sender is AIHeroClient)
                    {
                        E.Cast();
                    }
                }

                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear &&
                    Menu["ESetting"]["JungleClearE"].GetValue<MenuBool>().Enabled && E.IsReady())
                {
                    if (sender.IsMinion() && sender is AIMinionClient && Me.HealthPercent < 70)
                    {
                        E.Cast();
                    }
                }
            }catch(Exception) {}
        }

        /// <summary>
        /// you can said this is event loop ....zzz
        /// </summary>
        /// <param name="args"></param>
        private static void OnUpdate(EventArgs args)
        {
            if (Menu.GetBool("BearAutoFollow"))
            {
                AutoBearLogic();
            }

            // if hero dead or reacll not to inject others void
            if (Me.IsDead || Me.IsRecalling())
                return;

            switch (Orbwalker.ActiveMode) // judge you press what key in orbwalker
            {
                case OrbwalkerMode.Combo:// if you press combo key , load combo logic
                    ComboLogic(); 
                    break;
                case OrbwalkerMode.Harass:// if you press harass key , load harass logic
                    HarassLogic(); 
                    break;
                case OrbwalkerMode.LaneClear: // if you press clear key , load clear logic
                    LaneClearLogic();
                    JungleClearLogic();
                    break;
                case OrbwalkerMode.LastHit: // if you press lasthit key , load lasthit logic
                    LastHitLogic();
                    break;
            }

            if (Menu.GetKey("EnableFlashR"))
            {
                FlashRLogic();
            }

            if (Menu.GetBool("KillStealEnable"))
            {
                KillStealLogic();
            }

            if (Menu.GetBool("AutoStackEnable"))
            {
                AutoStackPassiveLogic();
            }
        }

        /// <summary>
        /// inject combo logic
        /// </summary>
        private static void ComboLogic()
        {
            var e = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (e != null)
            {
                if (R.IsReady() && !HaveBear && e.IsValidTarget(R.Range))
                {
                    var RPred = R.GetPrediction(e, true);

                    if (RPred.AoeTargetsHitCount >= Menu.GetSlider("ComboRCountsEnemiesinRange") && HaveStun)
                    {
                        R.Cast(RPred.CastPosition);
                    }

                    if (Menu.GetBool("ComboRCanKill") && CanR(e))
                    {
                        if (BuffCounts == 3 && E.IsReady())
                        {
                            E.Cast();
                        }

                        R.Cast(e, true, true);
                    }
                }

                if (Q.IsReady() && e.IsValidTarget(Q.Range) && (R.IsReady() && HaveBear) || !R.IsReady())
                {
                    Q.Cast(e, true);
                }

                if (W.IsReady() && e.IsValidTarget(W.Range) && (R.IsReady() && HaveBear) || !R.IsReady())
                {
                    W.Cast(e, true, true);
                }

                if (Ignite.IsReady() && e.IsValidTarget(600) && e.Health < GetDamage(e, false, false, false, true))
                {
                    Me.Spellbook.CastSpell(Ignite, e);
                }
            }
        }

        private static bool CanR(AIHeroClient e)
        {
            if (HaveBear)
            {
                return false;
            }

            if (Q.IsReady() && W.IsReady() && (e.Health < GetDamage(e, true, true, true, true)) && (Me.Mana > Q.Mana + W.Mana + R.Mana) && e.IsValidTarget(W.Range) && Ignite.IsReady() && BuffCounts >= 3)
            {
                return true;
            }

            if (Q.IsReady() && W.IsReady() && (e.Health < GetDamage(e, true, true, true)) && (Me.Mana > Q.Mana + W.Mana + R.Mana) && e.IsValidTarget(W.Range) && BuffCounts >= 3)
            {
                return true;
            }

            if (Q.IsReady() && (e.Health < GetDamage(e, true, false, true)) && (Me.Mana > Q.Mana + R.Mana) && e.IsValidTarget(Q.Range))
            {
                return true;
            }

            if (W.IsReady() && (e.Health < GetDamage(e, false, true, true)) && (Me.Mana > W.Mana + R.Mana) && e.IsValidTarget(W.Range))
            {
                return true;
            }

            if (!Q.IsReady() && !W.IsReady() && (e.Health < GetDamage(e, false, false, true)) && (Me.Mana > R.Mana) && e.IsValidTarget(R.Range))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  inject q harass or q lasthit or smart w logic
        /// </summary>
        private static void HarassLogic()
        {
            if (Menu.GetSlider("HarassMana") < Me.ManaPercent)
            {
                if (Menu.GetBool("HarassAutoLastHitQ"))
                {
                    LastHitLogic();
                }

                if (Menu.GetBool("HarassQ") && Q.IsReady())
                {
                    var e = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                    if (Menu.GetBool("HarassQOnlyStun") && e != null)
                    {
                        if (HaveStun && e.IsValidTarget(Q.Range))
                        {
                            Q.Cast(e, true);
                        }
                    }
                    else if (!Menu.GetBool("HarassQOnlyStun") && e != null)
                    {
                        if (e.IsValidTarget(Q.Range))
                        {
                            Q.Cast(e, true);
                        }
                    }
                }

                if (Menu.GetBool("HarassW") && W.IsReady())
                {
                    var e = TargetSelector.GetTarget(W.Range, DamageType.Magical);

                    if (Menu.GetBool("HarassWDebuff"))
                    {
                        if (e.IsValidTarget(W.Range))
                        {
                            if (e.HasBuffOfType(BuffType.Charm) || e.HasBuffOfType(BuffType.Fear) || e.HasBuffOfType(BuffType.Stun) || e.HasBuffOfType(BuffType.Slow) || e.HasBuffOfType(BuffType.Suppression))
                            {
                                W.Cast(e, true, true);
                            }
                        }
                    }
                    else if (!Menu.GetBool("HarassWDebuff"))
                    {
                        if (e.IsValidTarget(W.Range))
                        {
                            W.Cast(e, true, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  qw laneclear
        /// </summary>
        private static void LaneClearLogic()
        {
            if (Menu.GetSlider("LaneClearMana") < Me.ManaPercent && !(Menu.GetBool("ClearStun") && HaveStun))
            {
                var Minions = MinionManager.GetMinions(Q.Range);

                if (Minions.Count() > 0)
                {
                    foreach (var min in Minions)
                    {
                        if (Q.IsReady() && Menu.GetBool("LaneClearQ") && Menu.GetBool("LaneClearQLastHit") && min.IsValidTarget(Q.Range))
                        {
                            if (min.Health < Q.GetDamage(min) && min.Health > Me.GetAutoAttackDamage(min))
                            {
                                Q.Cast(min, true);
                            }
                            else if (Q.IsReady() && Menu.GetBool("LaneClearQ") && !Menu.GetBool("LaneClearQLastHit") && min.IsValidTarget(Q.Range))
                            {
                                Q.Cast(min, true);
                            }
                        }
                    }

                    if (W.IsReady() && Menu.GetBool("LaneClearW"))
                    {
                        var WFarmLocation = W.GetCircularFarmLocation(Minions, W.Width);

                        if (WFarmLocation.MinionsHit >= Menu.GetSlider("LaneClearWCount"))
                        {
                            W.Cast(WFarmLocation.Position);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  qwe jungle
        /// </summary>
        private static void JungleClearLogic()
        {
            if (Menu.GetSlider("JungleClearMana") < Me.ManaPercent)
            {
                var mobs = MinionManager.GetMinions(Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);

                if (mobs.Count() > 0)
                {
                    foreach (var mob in mobs)
                    {
                        if (Q.IsReady() && mob.Health < Q.GetDamage(mob) && W.IsReady() && mob.IsValidTarget(Q.Range))
                        {
                            Q.Cast(mob, true);
                        }
                        else if (Q.IsReady() && Menu.GetBool("JungleClearQ") && mob.IsValidTarget(Q.Range))
                        {
                            Q.Cast(mob, true);
                        }
                        else if (W.IsReady() && Menu.GetBool("JungleClearW") && mob.IsValidTarget(W.Range))
                        {
                            W.Cast(mob, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// use q to last hit
        /// </summary>
        private static void LastHitLogic()
        {
            if ((Menu.GetBool("LastHitQ") && Menu.GetSlider("LastHitMana") < Me.ManaPercent) || (Menu.GetBool("HarassAutoLastHitQ") && Menu.GetSlider("HarassMana") < Me.ManaPercent))
            {
                if (Q.IsReady() && !HaveStun)
                {
                    var Minions = MinionManager.GetMinions(Q.Range);

                    foreach (var min in Minions.Where(m => !m.IsDead && m.Health < Q.GetDamage(m)))
                    {
                        if (min != null)
                        {
                            if (min.Health > Me.GetAutoAttackDamage(min))
                            {
                                Q.Cast(min, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// auto r to follow enemies or me
        /// </summary>
        private static void AutoBearLogic()
        {
            if (HaveBear)
            {
                var e = TargetSelector.GetTarget(2000, DamageType.Magical);

                if (e != null && e.IsValidTarget(2000) && !e.IsZombie())
                {
                    if (Game.Time > ClickTime + 1.5)
                    {
                        R.Cast(e);
                        ClickTime = Game.Time;
                    }
                }
                else if (e == null && Game.Time > ClickTime + 1.5)
                {
                    R.Cast(Me.ServerPosition);
                    ClickTime = Game.Time;
                }
            }
        }

        /// <summary>
        ///  inject flashr logic
        /// </summary>
        private static void FlashRLogic()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            foreach (var e in HeroManager.Enemies.Where(em => em.IsValidTarget(R.Range + 425) && !em.IsZombie()))
            {
                if (e != null)
                {
                    Game.Print(e.CharacterName);
                    if (BuffCounts == 3 && E.IsReady() && !HaveStun)
                    {
                        E.Cast();
                    }

                    var RPred = R.GetPrediction(e, true);
                    var RHitCount = R.GetPrediction(e, true).AoeTargetsHitCount;
                    if (RPred.Hitchance >= HitChance.High && HaveStun)
                    {
                        if (Me.CountAllyHeroesInRange(1000) >= Menu.GetSlider("FlashRCountsAlliesinRange") && Me.CountEnemyHeroesInRange(R.Range + 425) >= Menu.GetSlider("FlashRCountsEnemiesinRange") && RHitCount >= Menu.GetSlider("FlashRCountsEnemiesinRange"))
                        {
                            Me.Spellbook.CastSpell(Flash, R.GetPrediction(e, true).CastPosition);
                            if (!Flash.IsReady())
                            {
                                R.Cast(R.GetPrediction(e, true).CastPosition);
                            }
                        }
                        else if (Menu.GetBool("FlashRCanKillEnemy") && Me.CountEnemyHeroesInRange(R.Range + 425) == 1 && Q.IsReady() && Ignite.IsReady() && e.Health < GetDamage(e, true, false, true, true))
                        {
                            Me.Spellbook.CastSpell(Flash, R.GetPrediction(e, true).CastPosition);
                            if (!Flash.IsReady())
                            {
                                R.Cast(R.GetPrediction(e, true).CastPosition); Me.Spellbook.CastSpell(Ignite, e); Q.Cast(e, true);
                            }
                        }
                        else if (Menu.GetBool("FlashRCanKillEnemy") && Me.CountEnemyHeroesInRange(R.Range + 425) == 1 && Q.IsReady() && e.Health < GetDamage(e, true, false, true))
                        {
                            Me.Spellbook.CastSpell(Flash, R.GetPrediction(e, true).CastPosition);
                            if (!Flash.IsReady())
                            {
                                R.Cast(R.GetPrediction(e, true).CastPosition);
                                Q.Cast(e, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  use Q W R Ignite to killsteal
        /// </summary>
        private static void KillStealLogic()
        {
            foreach (var e in HeroManager.Enemies.Where(e => !e.IsZombie() && !e.IsDead && e.IsValidTarget()))
            {
                if (e != null)
                {
                    if (Q.IsReady() && Menu.GetBool("KillStealQ") && e.Health + e.MagicalShield < Q.GetDamage(e) && e.IsValidTarget(Q.Range))
                    {
                        Q.Cast(e, true);
                        return;
                    }

                    if (W.IsReady() && Menu.GetBool("KillStealW") && e.Health + e.MagicalShield < W.GetDamage(e) && e.IsValidTarget(W.Range))
                    {
                        W.Cast(e, true);
                        return;
                    }

                    if (R.IsReady() && Menu.GetBool("KillStealR") && e.Health + e.MagicalShield < R.GetDamage(e) && e.IsValidTarget(R.Range))
                    {
                        R.Cast(e, true);
                        return;
                    }

                    if (Ignite.IsReady() && Menu.GetBool("KillStealIgnite") && e.Health < Me.GetSummonerSpellDamage(e, SummonerSpell.Ignite) && e.IsValidTarget(600))// ignite range is 600f
                    {
                        Me.Spellbook.CastSpell(Ignite, e);
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///  we to static passive
        /// </summary>
        private static void AutoStackPassiveLogic()
        {
            if (Menu.GetBool("AutoStackEnable") && NoneMode && !Menu.GetKey("EnableFlashR"))
            {
                if (Me.InFountain() && !HaveStun)
                {
                    if (E.IsReady() && Menu.GetBool("AutoStackE"))
                        E.Cast();
                    else if (W.IsReady() && Menu.GetBool("AutoStackW"))
                        W.Cast(Game.CursorPos);
                }
                else if (!HaveStun && Menu.GetSlider("AutoStackMana") < Me.ManaPercent && !Me.IsRecalling() && !Me.InFountain())
                {
                    if (E.IsReady() && Menu.GetBool("AutoStackE"))
                    {
                        E.Cast();
                    }
                    else if (W.IsReady() && Menu.GetBool("AutoStackW"))
                    {
                        var countenemies = Me.CountEnemyHeroesInRange(1000);
                        var minions = MinionManager.GetMinions(1000);

                        if (countenemies == 0 && minions == null && W.IsReady())
                        {
                            W.Cast(Game.CursorPos);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Drawing Spell Range Events
        /// </summary>
        /// <param name="args"></param>
        private static void OnDraw(EventArgs args)
        {
            // if heros is dead, not draw circle
            if (Me.IsDead)
                return;

            // Draw Q Range
            if (Q.IsReady() && Menu.GetBool("DrawQ"))
                CircleRender.Draw(Me.Position, Q.Range, Color.AliceBlue);

            // Draw W Range
            if (W.IsReady() && Menu.GetBool("DrawW"))
                CircleRender.Draw(Me.Position, W.Range, Color.DarkMagenta);

            // Draw R Range
            if (R.IsReady() && Menu.GetBool("DrawR"))
                CircleRender.Draw(Me.Position, R.Range, Color.Indigo);

            // Flash Range is 425!!! 
            if (R.IsReady() && Flash.IsReady() && Menu.GetBool("DrawRF"))
                CircleRender.Draw(Me.Position, R.Range + 425, Color.Red);
        }

        /// <summary>
        /// if bear life , the hero has this buff
        /// </summary>
        private static bool HaveBear
        {
            get { return Me.HasBuff("InfernalGuardianTimer"); }
        }

        /// <summary>
        /// if hero have stun , the hero has this buff
        /// </summary>
        private static bool HaveStun
        {
            get { return Me.HasBuff("Energized"); }
        }

        /// <summary>
        /// get annie buff count
        /// </summary>
        private static int BuffCounts
        {
            get
            {
                int count = 0;
                if (Me.HasBuff("Pyromania"))
                {
                    count = Me.GetBuffCount("Pyromania");
                }
                else if (!Me.HasBuff("Pyromania") || HaveStun)
                {
                    count = 0;
                }
                return count;
            }
        }

        private static float GetDamage(AIBaseClient target, bool SpellQ = false, bool SpellW = false, bool SpellR = false, bool CastIgnite = false)
        {
            float damage = 0;

            if (target == null)
                damage = 0;

            if (SpellQ && Q.IsReady())
            {
                damage += Q.GetDamage(target);
            }

            if (SpellW && W.IsReady())
            {
                damage += W.GetDamage(target);
            }

            if (SpellR && R.IsReady())
            {
                damage += R.GetDamage(target);
            }

            if (CastIgnite && Ignite.IsReady())
            {
                damage += (float)Me.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            }

            if (target.HasBuff("Undying Rage"))
                damage = 0;

            if (target.HasBuff("Judicator's Intervention"))
                damage = 0;

            if (target.HasBuff("KindredrNoDeathBuff"))
                damage = 0;

            return damage;
        }

        /// <summary>
        /// if you press Combo key , that said is in Combo mode
        /// </summary>
        private static bool ComboMode
        {
            get { return Orbwalker.ActiveMode == OrbwalkerMode.Combo; }
        }

        /// <summary>
        /// // if you press Harass key , that said is in Harass mode
        /// </summary>
        private static bool HarassMode
        {
            get { return Orbwalker.ActiveMode == OrbwalkerMode.Harass; }
        }

        /// <summary>
        /// if you press Clear key , that said is in Clear mode (LaneClear / JungleClear)
        /// </summary>
        private static bool ClearMode
        {
            get { return Orbwalker.ActiveMode == OrbwalkerMode.LaneClear; }
        }

        /// <summary>
        /// if you press LastHit key , that said is in LastHit mode
        /// </summary>
        private static bool LastHitMode
        {
            get { return Orbwalker.ActiveMode == OrbwalkerMode.LastHit; }
        }

        /// <summary>
        /// if you not press any key , that said is None mode
        /// </summary>
        private static bool NoneMode
        {
            get { return Orbwalker.ActiveMode == OrbwalkerMode.None; }
        }
    }
}
