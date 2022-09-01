﻿using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using EnsoulSharp.SDK.Utility;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace MightyAio.Champions
{
    internal class Ezreal
    {
        private static AIHeroClient Player => ObjectManager.Player;

        private static Menu Menu,itembuy,autolevel,Items,emotes;
        private static double recallFinishTime;
        private static double recallstart;
        private static string sendername;
        private static bool Chatsent;
        private static bool spellfarmkey => Menu["laneclear"].GetValue<MenuKeyBind>("SpellFarm").Active;
        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("Ezreal", "Ezreal", true);

            // Combo
            var comboMenu = new Menu("Combo", "Combo")
            {
                new MenuBool("UseQ", "Use Q"),
                new MenuBool("UseW", "Use W"),
                new MenuBool("UseEQ", "Use EQ", false),
                new MenuKeyBind("UseR", "simi R key", Keys.H, KeyBindType.Press)
            };
            Menu.Add(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "Harass")
            {
                new MenuBool("UseQ", "Use Q"),
                new MenuBool("UseW", "Use W", false),
                new MenuBool("LastHitQ", "Use Q for Last hit", false),
                new MenuSlider("minmana", "minimum mana % for Harass", 60)
            };
            Menu.Add(harassMenu);
            // lane clear
            var laneclear = new Menu("laneclear", "Lane Clear")
            {    
                new MenuKeyBind("SpellFarm","Spell Farm Key",Keys.M,KeyBindType.Toggle),
                new MenuBool("UseW", "Use W"),
                new MenuBool("UseQ", "Use Q", false),
                new MenuBool("UseQlasthit", "Use q for lasthitonly", false),
                new MenuSlider("minmana", "minimum mana % for lane clear", 50)
            };
            Menu.Add(laneclear);
            // BaseUlt
            var baseutl = new Menu("BaseUlt", "Base Ult")
            {
                new MenuBool("BaseUlt", "Base Ult"),
                new MenuBool("BaseBm", "Base Bm"),
                new MenuBool("BasePing", "game ping")
            };
            Menu.Add(baseutl);
            // lasthit
            var lasthit = new Menu("lasthit", "Last Hit")
            {
                new MenuBool("UseQ", "Use Q")
            };
            Menu.Add(lasthit);
            // Feel
            var Feel = new Menu("Feel", "Feel")
            {
                new MenuBool("UseEtofeel", "Use E"),
                new MenuKeyBind("feelkey", "Feel Key", Keys.Z, KeyBindType.Press)
            };
            Menu.Add(Feel);
            // auto item buy
            itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Doran's Blade", "LongSword", "Corrupting Potion", "none"})
            };
            // auto Level up
            autolevel = new Menu("autolevel", "Auto Level Up")
            {
                new MenuBool("autolevel", "Auto Level")
            };
            // use emotes
            emotes = new Menu("emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            // Item
             Items = new Menu("Items", "Items")
            {
                new MenuBool("GunbladeCombo", "use Gunblade When Combo"),
                new MenuBool("GunbladeKS", " use Gunblade to Kill steal"),
                new MenuBool("BOTRKCombo", " use BOTRK in combo"),
                new MenuBool("BOTRKKS", " use BOTRK to Kill steal"),
                new MenuBool("Use9", " Use Spellbinder at max Stacks in Combo")
            };
            // kill steal
            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("UseQ", "Use Q"),
                new MenuBool("UseW", "Use W"),
                new MenuBool("UseEQ", "Use EQ", false)
            };
            // killsteal.Add(new MenuBool("UseR", "Use R"));
            Menu.Add(killsteal);

            // Misc
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuBool("AutoQ", "Auto Q", false),
                new MenuSlider("Mana", "Mana for Auto Q", 90),
                new MenuSlider("setskin", "set skin", 10, 0, 55),
                new MenuBool("UseE", "Use E anti gapcloser"),
                new MenuBool("Tear", "Stack Tear"),
                new MenuSlider("minmana", "minimum mana % for stacking tear", 90)
            };
            miscMenu.Add(Items);
            miscMenu.Add(autolevel);
            miscMenu.Add(itembuy);
            miscMenu.Add(emotes);
            Menu.Add(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawW", "Draw W"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("PermaShow", "Perma Show"),
                new MenuBool("DrawSpell", "Draw Farm Spell Status"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            Menu.Add(drawMenu);

            Menu.Attach();
        }

        #endregion Menu

        #region Spells

        private static Font Berlinfont;
        private static bool spellbinder => Player.HasBuff("3907Counter");
        private static bool Spellbinderready => spellbinder && Player.GetBuffCount("3907Counter") == 100;
        private static int mykills = 0 + Player.ChampionsKilled;
        private static Spell Q, W2, W, E, EQ, R;
        private static int[] SpellLevels;

        #endregion Spells

        #region GameLoad

        public Ezreal()
        {
            SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};

            Q = new Spell(SpellSlot.Q, 1150f);
            W2 = new Spell(SpellSlot.Q, 1150f);
            W2.SetSkillshot(0.3f, 60f, 1200f, true, SpellType.Line);
            Q.SetSkillshot(0.3f, 60f, 2000f, false, SpellType.Line);
            W = new Spell(SpellSlot.W, 1150f);
            W.SetSkillshot(0.3f, 60f, 1200f, false, SpellType.Line);
            E = new Spell(SpellSlot.E, 475f) {Delay = 0.65f};
            EQ = new Spell(SpellSlot.Q, 1625f);
            EQ.SetSkillshot(0.90f, 60f, 1350f, true, SpellType.Line);
            R = new Spell(SpellSlot.R, 20000f);
            R.SetSkillshot(1.10f, 160f, 2000f, false, SpellType.Line);
            CreateMenu();
            Berlinfont = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Berlin San FB Demi",
                    Height = 23,
                    Weight = FontWeight.DemiBold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });

            Game.OnUpdate += GameOnOnUpdate;
            AntiGapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Drawing.OnDraw += DrawingOnOnDraw;
            Teleport.OnTeleport += OnTeleportEvent;
        }
        

        private static void OnTeleportEvent(AIBaseClient Sender, Teleport.TeleportEventArgs args)
        {
            if (args != null && Sender.IsValid && Sender.IsEnemy && Sender is AIHeroClient)
            {
                var damage = R.GetDamage(args.Source) * 0.95;

                if (args.Status == Teleport.TeleportStatus.Start && !args.Source.IsMinion() &&
                    args.Source.Health + args.Source.HPRegenRate * 8 + args.Source.AllShield < damage)
                {
                    if (args.Source.HasBuff("willrevive") || args.Source.HasBuff("bansheesveil") ||
                        args.Source.HasBuff("itemmagekillerveil")) return;
                    Chatsent = false;
                    sendername = Sender.CharacterName;
                    recallFinishTime = args.Duration;
                    recallstart = Variables.GameTimeTickCount;
                }

                if (args.Status == Teleport.TeleportStatus.Abort)
                {
                    Chatsent = true;
                    sendername = null;
                    recallFinishTime = 0;
                    recallstart = 0;
                }

                if (args.Status != Teleport.TeleportStatus.Start)
                {
                    Chatsent = true;
                    sendername = null;
                    recallFinishTime = 0;
                    recallstart = 0;
                }
            }
        }

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled;
            var drawE = Menu["Drawing"].GetValue<MenuBool>("DrawE").Enabled;
            var drawW = Menu["Drawing"].GetValue<MenuBool>("DrawW").Enabled;
            var drawKill = Menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities").Enabled;
            var PermaShow = Menu["Drawing"].GetValue<MenuBool>("PermaShow").Enabled;
            var drawS = Menu["Drawing"].GetValue<MenuBool>("DrawSpell").Enabled;
            var p = Player.Position;

            if (drawQ && (Q.IsReady() || PermaShow)) Drawing.DrawCircleIndicator(p, Q.Range, Color.DarkCyan);
            if (drawW && (W.IsReady() || PermaShow)) Drawing.DrawCircleIndicator(p, W.Range, Color.DarkCyan);

            if (drawE && (E.IsReady() || PermaShow)) Drawing.DrawCircleIndicator(p, E.Range, Color.Red);

            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
                if (enemyVisible.IsValidTarget())
                {
                    var qdmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Qdamage());
                    var wdmg = W.GetDamage(enemyVisible);
                    var aa = string.Format("Q Left:" + Math.Floor(enemyVisible.Health / qdmg));
                    if (drawKill)
                    {
                        if (qdmg > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else if (qdmg + wdmg > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q+W):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else
                            DrawText(Berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                }

            if (drawS)
            {
                if (spellfarmkey)
                    DrawText(Berlinfont, "Spell Farm On",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
                if (!spellfarmkey)
                    DrawText(Berlinfont, "Spell Farm Off",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
            }
        }

        private static float Qdamage()
        {
            var totalqdamage = 0;

            switch (Q.Level)
            {
                case 1:
                    totalqdamage = (int) (20 + Player.TotalAttackDamage * 1.2 + Player.TotalMagicalDamage * 0.15);
                    break;
                case 2:
                    totalqdamage = (int) (45 + Player.TotalAttackDamage * 1.2 + Player.TotalMagicalDamage * 0.15);
                    break;
                case 3:
                    totalqdamage = (int) (70 + Player.TotalAttackDamage * 1.2 + Player.TotalMagicalDamage * 0.15);
                    break;
                case 4:
                    totalqdamage = (int) (95 + Player.TotalAttackDamage * 1.2 + Player.TotalMagicalDamage * 0.15);
                    break;
                case 5:
                    totalqdamage = (int) (120 + Player.TotalAttackDamage * 1.2 + Player.TotalMagicalDamage * 0.15);
                    break;
            }

            return totalqdamage;
        }

        #endregion GameLoad

        #region Update

        private static void GameOnOnUpdate(EventArgs args)
        {
            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = itembuy.GetValue<MenuList>("selectitem").SelectedValue;

            if (item != "none" && time < 1)
                switch (item)
                {
                    case "Doran's Blade":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Blade))
                                Player.BuyItem(ItemId.Dorans_Blade);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                    case "LongSword":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Long_Sword)) Player.BuyItem(ItemId.Long_Sword);
                            if (gold >= 150 && !Player.HasItem(ItemId.Refillable_Potion))
                                Player.BuyItem(ItemId.Refillable_Potion);
                        }

                        break;
                    }
                    case "Corrupting Potion":
                    {
                        if (time < 1 && Player.InShop())
                            if (gold >= 500 && !Player.HasItem(ItemId.Corrupting_Potion))
                                Player.BuyItem(ItemId.Corrupting_Potion);

                        break;
                    }
                }

            if (Player.ChampionsKilled > mykills && emotes.GetValue<MenuBool>("Kill").Enabled)
            {
                mykills = Player.ChampionsKilled;
                Emote();
            }

            if (Menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled &&
                Player.SkinId != Menu["Misc"].GetValue<MenuSlider>("setskin").Value)
                Player.SetSkin(Menu["Misc"].GetValue<MenuSlider>("setskin").Value);

            if (!Player.CanCast) return;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Harass:
                    Harass();
                    break;

                case OrbwalkerMode.Combo:
                    Combo();
                    break;

                case OrbwalkerMode.LaneClear:
                    if (!spellfarmkey) return;
                    LaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            if (Menu["Feel"].GetValue<MenuKeyBind>("feelkey").Active) Feel();
            if (Menu["Combo"].GetValue<MenuKeyBind>("UseR").Active && R.IsReady()) Rsimi();

            Killsteal();
            Stacktear();
            Baseutl();
            if (Menu["Misc"].GetValue<MenuBool>("AutoQ").Enabled) AutoQ();
            if (autolevel.GetValue<MenuBool>("autolevel").Enabled) Levelup();
        }

        private static void AutoQ()
        {
            if (Player.IsRecalling() || Player.IsUnderEnemyTurret()) return;
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            var mymana = Player.ManaPercent;
            var minmana = Menu["Misc"].GetValue<MenuSlider>("Mana").Value;
            if (target == null || !(mymana > minmana)) return;
            var qdmg = Player.CalculatePhysicalDamage(target, Qdamage());
            if (qdmg > target.Health && Q.IsReady() && Q.IsInRange(target))
            {
              
                CastQ(target);
            }
            else if (Q.IsInRange(target) && Q.IsReady())
            {
                CastQ(target);
            }
        }

        private static void Baseutl()
        {
            var BU = Menu["BaseUlt"].GetValue<MenuBool>("BaseUlt").Enabled;
            var BUping = Menu["BaseUlt"].GetValue<MenuBool>("BasePing").Enabled;
            if (recallFinishTime == 0 || !R.IsReady() || !BU) return;

            var dis = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
            var time = recallstart - Variables.GameTimeTickCount + recallFinishTime;
            var objSpawnPoint = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
            if (objSpawnPoint == null) return;
            var timeToFountain = Player.Position.Distance(objSpawnPoint.Position) / R.Speed * 1000 + R.Delay * 1000;
            if (timeToFountain > recallFinishTime) return;
            if (time < timeToFountain)
            {
                if (BUping) Game.ShowPing(PingCategory.OnMyWay, objSpawnPoint.Position);
                DelayAction.Add((int) time, Ischat);

                R.Cast(objSpawnPoint.Position);
            }
        }

        private static void Ischat()
        {
            var BUBM = Menu["BaseUlt"].GetValue<MenuBool>("BaseBm").Enabled;
            if (!Chatsent && BUBM)
            {
                var random = new Random();
                var text = new List<string>
                {
                    "XD"
                };
                var num = random.Next(text.Count);
                Game.Say(text[num], true);
                Chatsent = true;
            }
        }

        private static void Emote()
        {
            var b = emotes.GetValue<MenuList>("selectitem").SelectedValue;
            switch (b)
            {
                case "Mastery":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Mastery);
                    break;

                case "Center":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Center);
                    break;

                case "South":
                    Game.SendSummonerEmote(SummonerEmoteSlot.South);
                    break;

                case "West":
                    Game.SendSummonerEmote(SummonerEmoteSlot.West);
                    break;

                case "East":
                    Game.SendSummonerEmote(SummonerEmoteSlot.East);
                    break;

                case "North":
                    Game.SendSummonerEmote(SummonerEmoteSlot.North);
                    break;
            }
        }

        private static void LastHit()
        {
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, Q.Range);
            var allJgl = GameObjects.GetJungles(ObjectManager.Player.Position, Q.Range);
            var useq = Menu["lasthit"].GetValue<MenuBool>("UseQ").Enabled;

            foreach (var minion in allMinions.Where(X => Q.IsInRange(X) && X.IsValidTarget()))
            {
                var totalQDamage = Player.CalculateDamage(minion, DamageType.Physical, Qdamage());

                if (Q.IsReady() &&
                    minion.Health < totalQDamage && useq)
                    Q.Cast(minion);
            }

            foreach (var jgl in allJgl.Where(X => Q.IsInRange(X) && X.IsValidTarget()))
            {
                var totalQDamage = Player.CalculateDamage(jgl, DamageType.Physical, Qdamage());

                if (Q.IsReady() && totalQDamage > jgl.Health && useq) Q.Cast(jgl);
            }
        }

        private static void Feel()
        {
            var UseEtofeel = Menu["Feel"].GetValue<MenuBool>("UseEtofeel").Enabled;
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (E.IsReady() && UseEtofeel) E.Cast(Game.CursorPos);
        }

        private static void LaneClear()
        {
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, Q.Range);
            var allJgl = GameObjects.GetJungles(ObjectManager.Player.Position, Q.Range);
            var turret = GameObjects.EnemyTurrets;
            var inb = GameObjects.EnemyInhibitors;
            var nexus = GameObjects.EnemyNexus;
            var useq = Menu["laneclear"].GetValue<MenuBool>("UseQ").Enabled;
            var useql = Menu["laneclear"].GetValue<MenuBool>("UseQlasthit").Enabled;
            var settedmana = Menu["laneclear"].GetValue<MenuSlider>("minmana").Value;
            var playermana = (int) Player.ManaPercent;

            if (playermana < settedmana) return;
            foreach (var minion in allMinions.Where(X => Q.IsInRange(X) && X.IsValidTarget()))
            {
                var totalQDamage = Player.CalculateDamage(minion, DamageType.Physical, Qdamage());

                if (Q.IsReady() &&
                    minion.Health < totalQDamage && useql)
                    Q.Cast(minion);
                else if (Q.IsReady() && useq) Q.Cast(minion);
            }

            foreach (var jgl in allJgl.Where(X => Q.IsInRange(X) && X.IsValidTarget())
            ) // in case of Ezreal both W and Q have the same range no need for Change
            {
                if (Q.IsReady()) Q.Cast(jgl);

                if (W.IsReady() && jgl.DistanceToPlayer() < Player.GetRealAutoAttackRange() &&
                    jgl.MaxHealth > 3800) W.Cast(jgl);
            }

            foreach (var t in turret.Where(X =>
                W.IsInRange(X) && X.IsValidTarget() && Player.Distance(X) < Player.GetRealAutoAttackRange()))
            {
                if (W.IsReady()) W.Cast(t);
                if (t.HasBuff("ezrealwattach")) Orbwalker.Attack(t);
            }

            foreach (var inbrs in inb.Where(X =>
                W.IsInRange(X) && X.IsValidTarget() && Player.Distance(X) < Player.GetRealAutoAttackRange()))
                if (W.IsReady())
                {
                    W.Cast(inbrs.Position);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, inbrs);
                }

            if (W.IsReady() && nexus.IsValidTarget() && W.IsInRange(nexus))
            {
                W.Cast(nexus.Position);
                Player.IssueOrder(GameObjectOrder.AttackUnit, nexus);
            }
        }

        private static void Rsimi()
        {
            var rtarget = TargetSelector.GetTarget(4000,DamageType.Magical);
            var Rdmg = R.GetDamage(rtarget);
            if (rtarget.IsValidTarget())
                if (R.IsReady() && rtarget.Health < Rdmg)
                {
                    var rPred = R.GetPrediction(rtarget);
                    if (rPred.Hitchance >= HitChance.High) R.Cast(rPred.CastPosition);
                }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(EQ.Range,DamageType.Mixed);
            var QA = Menu["Combo"].GetValue<MenuBool>("UseQ").Enabled;
            var WA = Menu["Combo"].GetValue<MenuBool>("UseW").Enabled;
            var EQA = Menu["Combo"].GetValue<MenuBool>("UseEQ").Enabled;
            var usegun = Items.GetValue<MenuBool>("GunbladeCombo").Enabled;
            var useBOTRK = Items.GetValue<MenuBool>("BOTRKCombo").Enabled;
            var Usespellbinder = Items.GetValue<MenuBool>("Use9").Enabled;

            if (!target.IsValidTarget()) return;
            if (Usespellbinder && Q.IsInRange(target) && Spellbinderready) Player.UseItem(3907);
            var qdmg = Player.CalculateDamage(target, DamageType.Physical, Qdamage());
            if (W.IsInRange(target) && WA && W.IsReady() && Q.IsReady())
            {
                var tarPered = W.GetPrediction(target);
                var tarsPered = W2.GetPrediction(target);
                if (tarPered.CastPosition.Distance(Player.Position) < 350)
                    W.Cast(tarPered.CastPosition);
                else if (W.Range - 80 > tarsPered.CastPosition.Distance(Player.Position))
                    if (tarsPered.Hitchance >= HitChance.High)
                    {
                        
                            if (tarsPered.Hitchance >= HitChance.High) W.Cast(tarsPered.CastPosition);
                            CastQ(target);
                        

                        
                    }
            } else 
            if (Q.IsInRange(target) && QA && Q.IsReady())
            {
                CastQ(target);
               
            } else if (!Q.IsInRange(target) && EQA && Q.IsReady() && E.IsReady() && qdmg > target.Health)
            {
                E.Cast(target.Position);
                if (Q.Range > target.Distance(Player))
                    CastQ(target);
            }

            
        }

        private static void Killsteal()
        {
            var target = TargetSelector.GetTarget(EQ.Range,DamageType.Physical);

            if (target == null) return;
            var QA = Menu["KillSteal"].GetValue<MenuBool>("UseQ").Enabled;
            var EQA = Menu["KillSteal"].GetValue<MenuBool>("UseEQ").Enabled;
            var qdmg = Player.CalculatePhysicalDamage(target, Qdamage());
            var ap = (int) (Player.TotalMagicalDamage * 0.3);
            var gunbladedmg = Player.CalculateMagicDamage(target, 170 + 4.588 * Player.Level + ap);
            var BOTRKdmg = Player.CalculateMagicDamage(target, 100);
            var ksgun = Items.GetValue<MenuBool>("GunbladeKS").Enabled;
            var BOTRKks = Items.GetValue<MenuBool>("BOTRKKS").Enabled;
            
            if (Q.IsInRange(target) && QA && Q.IsReady() && qdmg > target.Health)
            {
                CastQ(target);
            }

            if (!Q.IsInRange(target) && EQA && Q.IsReady() && E.IsReady() && qdmg > target.Health)
            {
                E.Cast(target.Position);
                if (Q.Range - 50 > target.DistanceToPlayer())
                    CastQ(target);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            var QA = Menu["Harass"].GetValue<MenuBool>("UseQ").Enabled;
            var WA = Menu["Harass"].GetValue<MenuBool>("UseW").Enabled;
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, Q.Range);
            var useq = Menu["Harass"].GetValue<MenuBool>("LastHitQ").Enabled;
            var settedmana = Menu["Harass"].GetValue<MenuSlider>("minmana").Value;
            var playermana = (int) Player.ManaPercent;

            foreach (var minion in allMinions.Where(X => Q.IsInRange(X) && X.IsValidTarget()))
            {
                var totalQDamage = Player.CalculateDamage(minion, DamageType.Physical, Qdamage());

                if (Q.IsReady() &&
                    minion.Health < totalQDamage && useq)
                    Q.Cast(minion);
            }

            if (playermana < settedmana) return;

            if (!target.IsValidTarget()) return;
            if (W.IsInRange(target) && WA && W.IsReady() && Q.IsReady())
            {
                var tarPered = W.GetPrediction(target);
                var tarsPered = W2.GetPrediction(target);
                if (tarPered.CastPosition.Distance(Player.Position) < 350)
                    W.Cast(tarPered.CastPosition);
                else if (W.Range - 50 > tarsPered.CastPosition.Distance(Player.Position))
                    if (tarsPered.Hitchance >= HitChance.High)
                    {
                        
                        if (tarsPered.Hitchance >= HitChance.High) W.Cast(tarsPered.CastPosition);
                        CastQ(target);
                        

                        
                    }
            } else 
            if (Q.IsInRange(target) && QA && Q.IsReady())
            {
                CastQ(target);

            }
        }
        #endregion Update

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (!Menu["Misc"].GetValue<MenuBool>("UseE").Enabled) return;

            if (!E.IsReady() || sender == null || !sender.IsValidTarget(E.Range)) return;
            if (sender.IsMelee)
                if (sender.IsValidTarget(sender.AttackRange + sender.BoundingRadius + 100))
                    E.Cast(Player.PreviousPosition.Extend(sender.PreviousPosition, -E.Range));

            if (sender.IsDashing())
                if (Args.EndPosition.DistanceToPlayer() <= 250 ||
                    sender.PreviousPosition.DistanceToPlayer() <= 300)
                    E.Cast(Player.PreviousPosition.Extend(sender.PreviousPosition, -E.Range));

            if (!sender.IsCastingImporantSpell()) return;
            if (sender.PreviousPosition.DistanceToPlayer() <= 300)
                E.Cast(Player.PreviousPosition.Extend(sender.PreviousPosition, -E.Range));
        }

        private static void Stacktear()
        {
            var target = TargetSelector.GetTarget(1600,DamageType.Physical);
            var mobs = GameObjects.GetMinions(Player.Position,Q.Range).FirstOrDefault(x => x.IsEnemy);
            var mousepos = Game.CursorPos;
            var manap = Menu["Misc"].GetValue<MenuSlider>("minmana").Value;
            var stack = Menu["Misc"].GetValue<MenuBool>("Tear").Enabled;
            if (!stack || Player.IsMoving || target != null || mobs != null || Player.IsRecalling()) return;

            if (Q.IsReady() && manap < Player.ManaPercent &&
                (Player.HasItem(ItemId.Tear_of_the_Goddess) || Player.HasItem((int) ItemId.Manamune) ||
                 Player.HasItem(ItemId.Archangels_Staff))) Q.Cast(mousepos);
        }

        private static void CastQ(AIHeroClient target)
        {
            if (!Q.IsReady()) return;
            var targQ = Q.GetPrediction(target,false,-1,new [] {CollisionObjects.YasuoWall});
            var litteshits = GameObjects.GetMinions(Player.Position, Q.Range);
            var jglcmaps = GameObjects.GetJungles(Player.Position, Q.Range);
            if ((from litteshit in litteshits.Where(x => x.IsValid && x.IsEnemy) let Line = new Geometry.Rectangle(Player.Position,
                Player.Position.Extend(targQ.CastPosition, Q.Range), Q.Width + litteshit.BoundingRadius) where Line.IsInside(litteshit) select litteshit).Any())
            {
                return;
            }
            if ((from jgl in jglcmaps.Where(x => x.IsValid) let Line = new Geometry.Rectangle(Player.Position,
                Player.Position.Extend(targQ.CastPosition, Q.Range), Q.Width + jgl.BoundingRadius) where Line.IsInside(jgl) select jgl).Any())
            {
                return;
            }
          
            //^ CUZ THE DEFUALT COILLISON HAS PROBLEMS 
            if (targQ.CastPosition.Distance(Player.Position)  < 250  )
                Q.Cast(targQ.CastPosition);
            else if (Q.Range > targQ.CastPosition.Distance(Player.Position) && targQ.Hitchance >= HitChance.High)
                Q.Cast(targQ.CastPosition);
            
        }
        private static void Levelup()
        {
            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

            if (qLevel + wLevel + eLevel + rLevel < Player.Level && Player.Level <= 18)
            {
                var level = new[] {0, 0, 0, 0};
                for (var i = 0; i < Player.Level; i++) level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

                if (qLevel < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);

                if (wLevel < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);

                if (eLevel < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);

                if (rLevel < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }
    }
}