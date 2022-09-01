using System;
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
    
    internal class Jinx
    {
        private static AIHeroClient Player => ObjectManager.Player;
        private static float Rmin => Menu["Combo"].GetValue<MenuSlider>("Rmin").Value;
        private static float Rmax => Menu["Combo"].GetValue<MenuSlider>("RMax").Value;
        private static bool Close => Menu["Combo"].GetValue<MenuBool>("WR").Enabled;
        private static Menu Menu, AutoLevel, Emotes;
        private static double recallFinishTime;
        private static double recallstart;
        private static string sendername;
        private static bool Chatsent;
     

        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("Jinx", "Jinx", true);

            var comboMenu = new Menu("Combo", "Combo")
            {
                new MenuBool("UseQ", "Use Q"),
                new MenuBool("UseW", "Use W"),
                new MenuBool("WR", "Use W In AA Range",false),
                new MenuBool("UseE", "Use E"),
                new MenuBool("UseR1", "Use R"),
                new MenuKeyBind("UseR", "simi R key", Keys.T, KeyBindType.Press),
                new MenuBool("UseRAoe", "Use R for Aoe"),
                new MenuSlider("MiniRAoe", "MiniRAoe", 3, 1, 5),
                new MenuSlider("Rmin", "R mini Range", 800, 700, 4000),
                new MenuSlider("RMax", "R Max Range", 2000, 2000, 10000),
            };
            Menu.Add(comboMenu);

            var harassMenu = new Menu("Harass", "Harass")
            {
                new MenuBool("UseQ", "Use Q"),
                new MenuBool("UseW", "Use W"),
                new MenuSlider("minmana", "minimum mana % for Harass", 40)
            };
            Menu.Add(harassMenu);

            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuKeyBind("Key","Spell Farm Key",Keys.M,KeyBindType.Toggle),
                new MenuSlider("MC","minimum Minion count when Spell farm is Enabled",2,1,5),
                new MenuBool("UseQ", "Use Q"),
                new MenuBool("UseQT", "don't use rocket under tower", false),
                new MenuSlider("minmana", "minimum mana % for lane clear", 40)
            };
            Menu.Add(laneclear);
            var Lasthit = new Menu("LastHit", "Last Hit")
            {
                new MenuBool("UseQ", "UseQ")
            };
            Menu.Add(Lasthit);

            var baseutl = new Menu("BaseUlt", "Base Ult")
            {
                new MenuBool("BaseUlt", "Base Ult"),
                new MenuBool("BaseBm", "Base Bm"),
                new MenuBool("BasePing", "game ping")
            };
            Menu.Add(baseutl);

            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item", new[] {"Doran's Blade", "LongSword", "none"})
            };
            Menu.Add(itembuy);

            AutoLevel = new Menu("autolevel", "Auto Level Up")
            {
                new MenuBool("autolevel", "Auto Level")
            };


            Emotes = new Menu("emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };

            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("UseW", "Use W")
            };
            Menu.Add(killsteal);

            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuSliderButton("UseSkin", "Use Skin Changer | set Skin",33),
                new MenuSliderButton("AutoW", "Auto W | When Mana %", 80,40,100,false),
                new MenuBool("playsoundonR", "play sound on R usage", false),
                new MenuBool("UseE", "Use E anti gapcloser"),
                AutoLevel,
                Emotes
            };
            Menu.Add(miscMenu);

            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawW", "Draw W",false),
                new MenuBool("DrawE", "Draw E",false),
                new MenuBool("PermaShow", "Perma Show"),
                new MenuBool("DrawSpell", "Draw Farm Spell Status"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            Menu.Add(drawMenu);

            Menu.Attach();
        }
        #endregion Menu

        #region Spells
        private static bool SpellFarm => Menu["laneclear"].GetValue<MenuKeyBind>("Key").Active;
        private static int count => Menu["laneclear"].GetValue<MenuSlider>("MC").Value;
        private static Font Berlinfont;

        private static int mykills = 0 + Player.ChampionsKilled;

        private static Spell Q, W, E, R;

        private static bool HasMinigun => Player.Buffs.Any(x => x.Name.ToLowerInvariant() == "jinxqicon");

        private static int GetMinigunStacks => Player.Buffs.Any(x => x.Name.ToLowerInvariant() == "jinxqramp")
            ? Player.Buffs.Find(x => x.Name.ToLowerInvariant() == "jinxqramp").Count
            : 0;

        private static bool HasRocketLauncher => Player.Buffs.Any(x => x.Name.ToLowerInvariant() == "jinxq");


        private static bool FirecanonStackedUp => Player.Buffs.Any(x =>
            FirecanonItem && x.Name.ToLowerInvariant() == "itemstatikshankcharge" && x.Count == 100);

        private static bool FirecanonItem => Player.InventoryItems.Any(x => x.Id == ItemId.Rapid_Firecannon);
        private static int[] RMinimalDamage { get; } = {0, 25, 35, 45};
        private static float RBonusAdDamageMod { get; } = 0.15f;
        private static float[] RMissingHealthBonusDamage { get; } = {0, 0.25f, 0.3f, 0.35f};
        private static int[] SpellLevels;

        #endregion Spells

        #region GameLoad

        public Jinx()
        {
            SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
            Q = new Spell(SpellSlot.Q, 525f);
            W = new Spell(SpellSlot.W, 1500f);
            W.SetSkillshot(0.60f, 80f, 3300f, true, SpellType.Line);
            E = new Spell(SpellSlot.E, 900f);
            E.SetSkillshot(0.95f, 120f, 1100f, false, SpellType.Circle);
            R = new Spell(SpellSlot.R, 30000f);
            R.SetSkillshot(0.60f, 140f, 1500f, false, SpellType.Line);

            CreateMenu();
            Berlinfont = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Berlin San FB Demi",
                    Height = 24,
                    Weight = FontWeight.Bold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });

            Game.OnUpdate += GameOnOnUpdate;
            AntiGapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Drawing.OnDraw += DrawingOnOnDraw;
            Teleport.OnTeleport += OnTeleportEvent;
            Interrupter.OnInterrupterSpell += OnInterruptible;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }


        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            var usesound = Menu["Misc"].GetValue<MenuBool>("playsoundonR").Enabled;

            if (sender.IsMe && args.SData.Name == "JinxR" && usesound)
            {
              
            }
        }

        private static float SecondGunRange()
        {
            var a = Q.Range + 65;
            if (Q.Level > 0) a = Q.Range + 65 + new[] {75, 100, 125, 150, 175}[Q.Level - 1];
            var additionalRange = FirecanonStackedUp ? Math.Min(Q.Range * 0.35f, 150) : 0;

            return a + additionalRange;
        }

        private static float MiniGunRange()
        {
            var a = Q.Range + 65;

            var additionalRange = FirecanonStackedUp ? Math.Min(Q.Range * 0.35f, 150) : 0;

            return a + additionalRange;
        }

        private static bool IsMovingTowards(AIBaseClient unit, Vector3 position)
        {
            return unit.IsMoving && unit.Path.Last().Distance(position) <= Math.Pow(50, 50);
        }

        private static void OnTeleportEvent(AIBaseClient Sender, Teleport.TeleportEventArgs args)
        {
            if (args != null && Sender.IsValid && Sender.IsEnemy && Sender is AIHeroClient && args.Duration >= 4000)
            {
                double missingHP = 0;
                switch (R.Level)
                {
                    case 0:
                        missingHP = 0;
                        break;

                    case 1:
                        missingHP = 0.25;
                        break;

                    case 2:
                        missingHP = 0.3;
                        break;

                    case 3:
                        missingHP = 0.35;
                        break;
                }

                var rd = rdamagetobase();
                var totald = rd + missingHP * (args.Source.MaxHealth - args.Source.Health);
                var damage = Player.CalculatePhysicalDamage(args.Source, totald) * 0.8;
                if (args.Status == Teleport.TeleportStatus.Start &&
                    args.Source.Health + args.Source.HPRegenRate * 8 + args.Source.AllShield <
                    damage)
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

        private static bool Isinautoattackrange(AIHeroClient target, float range)
        {
            return Player.Distance(target) < range;
        }

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static bool Colliding(AIHeroClient target, Vector3 targetpos)
        {
            var RPred = R.GetPrediction(target);
            var Line = new Geometry.Rectangle(Player.PreviousPosition,
                Player.PreviousPosition.Extend(targetpos, R.Range), R.Width + target.BoundingRadius);
            return Line.IsInside(RPred.UnitPosition.ToVector2());
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

            if (drawQ || PermaShow)
            {
                if (HasRocketLauncher)
                    Drawing.DrawCircleIndicator(p, SecondGunRange(), Color.DarkCyan);
                if (HasMinigun)
                    Drawing.DrawCircleIndicator(p, MiniGunRange(), Color.DarkCyan);
            }

            if (drawW && (W.IsReady() || PermaShow)) Drawing.DrawCircleIndicator(p, W.Range, Color.DarkCyan);

            if (drawE && (E.IsReady() || PermaShow)) Drawing.DrawCircleIndicator(p, E.Range, Color.Red);

            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget()))
                if (enemyVisible.IsValidTarget())
                {
                    var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) +
                                  Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) *
                                  Player.Crit;
                    var aa = string.Format("AA Left: " + (int) (enemyVisible.Health / autodmg));
                    if (drawKill)
                        DrawText(Berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                            (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                }
            if (drawS)
            {
                if (SpellFarm)
                    DrawText(Berlinfont, "Spell Farm On",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
                if (!SpellFarm)
                    DrawText(Berlinfont, "Spell Farm Off",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
            }
        }

        #endregion GameLoad

        #region Update

        private static void GameOnOnUpdate(EventArgs args)
        {
            var useonkill = Emotes.GetValue<MenuBool>("Kill").Enabled;
            if (ObjectManager.Player.ChampionsKilled > mykills && useonkill)
            {
                mykills = ObjectManager.Player.ChampionsKilled;
                Emote();
            }

            if (Math.Abs(W.Delay) > 0.4f)
                W.Delay = Math.Max(0.4f, (600 - Player.PercentAttackSpeedMod / 2.5f * 200) / 1000f);
            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = Menu["autoitem"].GetValue<MenuList>("selectitem").SelectedValue;

            if (item != "none" && time < 1 && Game.MapId == GameMapId.SummonersRift)
            {
                if (item == "Doran's Blade")
                    if (time < 1 && Player.InShop())
                    {
                        if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Blade)) Player.BuyItem(ItemId.Dorans_Blade);
                        if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion)) Player.BuyItem(ItemId.Health_Potion);
                    }

                if (item == "LongSword")
                    if (time < 1 && Player.InShop())
                    {
                        if (gold >= 500 && !Player.HasItem(ItemId.Long_Sword)) Player.BuyItem(ItemId.Long_Sword);
                        if (gold >= 150 && !Player.HasItem(ItemId.Refillable_Potion))
                            Player.BuyItem(ItemId.Refillable_Potion);
                    }
            }

            var getskin = Menu["Misc"].GetValue<MenuSliderButton>("UseSkin").Value;
            var skin = Menu["Misc"].GetValue<MenuSliderButton>("UseSkin").Enabled;
            if (skin && Player.SkinId != getskin) Player.SetSkin(getskin);
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
                    LaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            if (Menu["Combo"].GetValue<MenuKeyBind>("UseR").Active && R.IsReady()) Rsimi();
            Killsteal();
            if (AutoLevel.GetValue<MenuBool>("autolevel").Enabled) Levelup();
            AutoW();
            Baseult();
        }

        private static void AutoW()
        {
            if (Player.IsRecalling() || Player.IsUnderEnemyTurret()) return;
            var auto = Menu["Misc"].GetValue<MenuSliderButton>("AutoW").Enabled;
            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);
            var mymana = Player.ManaPercent;
            var minmana = Menu["Misc"].GetValue<MenuSliderButton>("AutoW").Value;
            if (auto && target != null && mymana > minmana)
            {
                var Wdmg = Player.GetSpellDamage(target, SpellSlot.W);
                if (Wdmg > target.Health && W.IsReady() && W.IsInRange(target) && W.Collision)
                {
                    if ((target.IsWindingUp || !target.IsMoving) && target.Distance(Player.Position) < 250)
                    {
                        W.Cast(target);
                        return;
                    }
                    var targW = W.GetPrediction(target);
                    if (targW.CastPosition.Distance(Player.Position) < 250)
                        W.Cast(targW.CastPosition);
                    else if (W.Range - 50 > targW.UnitPosition.Distance(Player.Position) &&
                             targW.Hitchance >= HitChance.High)
                        W.Cast(targW.UnitPosition);
                }
                else if (W.IsReady() && W.IsInRange(target) && W.Collision)
                {
                    if ((target.IsWindingUp || !target.IsMoving) && target.Distance(Player.Position) < 250)
                    {
                        W.Cast(target);
                        return;
                    }
                    var targW = W.GetPrediction(target);
                    if (targW.UnitPosition.Distance(Player.Position) < 250)
                        W.Cast(targW.CastPosition);
                    else if (W.Range - 50 > targW.UnitPosition.Distance(Player.Position) &&
                             targW.Hitchance >= HitChance.High)
                        W.Cast(targW.UnitPosition);
                }
            }
        }

        private static float rdamage(AIHeroClient target, Vector3? customPosition = null)
        {
            var distance = Player.Distance(customPosition ?? target.Position) > 1500
                ? 1499
                : Player.Distance(customPosition ?? target.Position);
            distance = distance < 100 ? 100 : distance;

            var baseDamage = RMinimalDamage[R.Level] * (distance / 150);
            var bonusAd = RBonusAdDamageMod * (distance / 150);
            var percentDamage = (target.MaxHealth - target.Health) * RMissingHealthBonusDamage[R.Level];

            var finalDamage = Player.CalculateDamage(target, DamageType.Physical,
                baseDamage + percentDamage + Player.FlatPhysicalDamageMod * bonusAd);

            return (float) finalDamage;
            
        }

        private static double rdamagetobase()
        {
            var damage = 0;
            var totalAD = (Player.TotalAttackDamage - Player.BaseAttackDamage) * 1.5;
            switch (R.Level)
            {
                case 0:
                    damage = 0;
                    break;

                case 1:
                    damage = 250;
                    break;

                case 2:
                    damage = 350;
                    break;

                case 3:
                    damage = 450;
                    break;
            }

            var maxDMG = damage + totalAD;
            return maxDMG;
        }

        #region Combo and other speels

        private static void Baseult()
        {
            var BU = Menu["BaseUlt"].GetValue<MenuBool>("BaseUlt").Enabled;
            var BUping = Menu["BaseUlt"].GetValue<MenuBool>("BasePing").Enabled;

            if (recallFinishTime == 0 || !R.IsReady() || !BU) return;
            var objSpawnPoints = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);

            const float accelerationrate = 0.3f;
            if (objSpawnPoints != null)
            {
                var distance = Player.Position.Distance(objSpawnPoints.Position);
                var acceldifference = distance - 1350f;
                if (acceldifference > 150f)
                    acceldifference = 150f;

                var difference = distance - 1500f;

                var missilespeed = (1350f * R.Speed + acceldifference * (R.Speed + accelerationrate * acceldifference) +
                                    difference * 2200f) / distance;
                var timeToFountain = Player.Position.Distance(objSpawnPoints.Position) / missilespeed * 1000 +
                                     R.Delay * 1000;

                var time = recallstart - Variables.GameTimeTickCount + recallFinishTime;
                var objSpawnPoint = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
                var target = TargetSelector.GetTarget(2000,DamageType.Physical);
                if (target != null && target.IsValid)
                {
                    var a = Colliding(target, objSpawnPoints.Position);
                    if (a)
                    {
                        Game.ShowPing(PingCategory.Danger, Player.Position);
                        return;
                    }
                }

                if (objSpawnPoint == null) return;
                if (timeToFountain > recallFinishTime) return;
                if (!(time < timeToFountain)) return;
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
            var b = Emotes.GetValue<MenuList>("selectitem").SelectedValue;
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

        private static void LaneClear()
        {
            var useq = Menu["laneclear"].GetValue<MenuBool>("UseQ").Enabled;
            var useqT = Menu["laneclear"].GetValue<MenuBool>("UseQT").Enabled;
            var settedmana = Menu["laneclear"].GetValue<MenuSlider>("minmana").Value;
            var playermana = (int) Player.ManaPercent;
            if (HasRocketLauncher && Player.IsUnderEnemyTurret() && useqT) Q.Cast();
            if (playermana < settedmana) return;

            if (!useq || Player.IsUnderEnemyTurret())
                return;

            var laneMinions = GameObjects.GetMinions(ObjectManager.Player.Position, SecondGunRange() + 100).ToList();

            if (!laneMinions.Any())
                return;
            if (SpellFarm && laneMinions.Count() >= count)
            {
                if (HasMinigun) Q.Cast();
                return;
            }
            var rocketsLanuncherMinions = laneMinions.Where(x =>
                x.IsValidTarget(SecondGunRange()) &&
                (laneMinions.Count(k =>
                     k.Distance(x) <= 150 &&
                     HealthPrediction.GetPrediction(k, 350) < Player.GetAutoAttackDamage(k) * 1.1f) > 2 ||
                 Player.Distance(x) > MiniGunRange() &&
                 HealthPrediction.GetPrediction(x, 350) < Player.GetAutoAttackDamage(x) * 1.1f)).ToList();

            if (HasMinigun)
            {
                if (!rocketsLanuncherMinions.Any())
                    return;

                foreach (var objAiMinion in rocketsLanuncherMinions.OrderBy(x => x.Health)) Q.Cast();
            }
            else if (HasRocketLauncher && !rocketsLanuncherMinions.Any())
            {
                Q.Cast();
            }

            var jungleMinions = GameObjects.GetJungles(ObjectManager.Player.Position, Player.GetRealAutoAttackRange())
                .ToList();

            if (!jungleMinions.Any())
                return;

            if (!useq)
                return;

            if (HasMinigun && jungleMinions.Count > 1 &&
                jungleMinions.Count(x => Orbwalker.GetTarget()?.Distance(x) < 150) > 1)
                Q.Cast();
            else if (HasRocketLauncher) Q.Cast();
        }

        private static void LastHit()
        {
            var useq = Menu["LastHit"].GetValue<MenuBool>("UseQ").Enabled;

            if (!useq || Player.IsUnderEnemyTurret())
                return;

            var laneMinions = GameObjects.GetMinions(ObjectManager.Player.Position, SecondGunRange() + 100).ToList();

            if (!laneMinions.Any())
                return;

            var rocketsLanuncherMinions = laneMinions.Where(x =>
                x.IsValidTarget(SecondGunRange()) &&
                (laneMinions.Count(k =>
                     k.Distance(x) <= 150 &&
                     HealthPrediction.GetPrediction(k, 350) < Player.GetAutoAttackDamage(k) * 1.1f) > 2 ||
                 Player.Distance(x) > MiniGunRange() &&
                 HealthPrediction.GetPrediction(x, 350) < Player.GetAutoAttackDamage(x) * 1.1f)).ToList();

            if (HasMinigun)
            {
                if (!rocketsLanuncherMinions.Any())
                    return;

                foreach (var objAiMinion in rocketsLanuncherMinions.OrderBy(x => x.Health)) Q.Cast();
            }
            else if (HasRocketLauncher && !rocketsLanuncherMinions.Any())
            {
                Q.Cast();
            }

            var jungleMinions = GameObjects.GetJungles(ObjectManager.Player.Position, Player.GetRealAutoAttackRange())
                .ToList();

            if (!jungleMinions.Any())
                return;

            if (!useq)
                return;

            if (HasMinigun && jungleMinions.Count > 1 &&
                jungleMinions.Count(x => Orbwalker.GetTarget()?.Distance(x) < 150) > 1)
                Q.Cast();
            else if (HasRocketLauncher) Q.Cast();
        }

        private static void Rsimi()
        {
            var rtarget = TargetSelector.GetTarget(R.Range,DamageType.Physical);
            if (rtarget == null) return;
            if (rtarget.IsValidTarget() && !rtarget.HasBuffOfType(BuffType.Invulnerability) &&
                !rtarget.HasBuffOfType(BuffType.SpellShield))
                if (R.IsReady())
                {
                    var rPred = R.GetPrediction(rtarget, true, -1, new [] {CollisionObjects.Heroes});
                    if (rPred.Hitchance >= HitChance.VeryHigh) R.Cast(rPred.CastPosition);
                }
        }

        private static void Combo()
        {
            var useQ = Menu["Combo"].GetValue<MenuBool>("UseQ").Enabled;
            var useW = Menu["Combo"].GetValue<MenuBool>("UseW").Enabled;
            var useE = Menu["Combo"].GetValue<MenuBool>("UseE").Enabled;
            var UseR1 = Menu["Combo"].GetValue<MenuBool>("UseR1").Enabled;
            var useR = Menu["Combo"].GetValue<MenuBool>("UseRAoe").Enabled;
            var useRAoenum = Menu["Combo"].GetValue<MenuSlider>("MiniRAoe").Value;
            if (Q.IsReady() && useQ)
            {
                var target = TargetSelector.GetTarget(SecondGunRange(),DamageType.Physical);

                if (target != null)
                {
                    if (Isinautoattackrange(target, MiniGunRange()) && HasRocketLauncher &&
                        target.Health + target.AllShield > Player.GetAutoAttackDamage(target) * 2.2f)
                    {
                        Q.Cast();
                        return;
                    }

                    if (!Isinautoattackrange(target, MiniGunRange()) &&
                        Isinautoattackrange(target, SecondGunRange()) && !HasRocketLauncher)
                    {
                        Q.Cast();
                        return;
                    }

                    if (HasMinigun && GetMinigunStacks >= 2 &&
                        target.Health + target.AllShield < Player.GetAutoAttackDamage(target) * 2.2f &&
                        target.Health + target.AllShield > Player.GetAutoAttackDamage(target) * 2f)
                    {
                        Q.Cast();
                        return;
                    }
                }
                
            }

            var t = TargetSelector.GetTarget(Rmax,DamageType.Physical);

            if (t != null && !t.IsDead && UseR1 && !t.HasBuffOfType(BuffType.Invulnerability) &&
                Player.Distance(t) > Rmin )
            {
                if (t.Health + t.AllShield < rdamage(t))
                {
                    if ((t.IsWindingUp || !t.IsMoving))
                    {
                        R.Cast(t);
                        return;
                    }
                    var rPrediction = R.GetPrediction(t);

                    if (rPrediction.Hitchance >= HitChance.VeryHigh) R.Cast(rPrediction.CastPosition);
                }
                  
               
            }

            if (W.IsReady() && useW &&
                !Player.IsUnderEnemyTurret() &&
                Player.Mana - (50 + 10 * (W.Level - 1)) > (R.IsReady() ? 100 : 0))
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                

                if (target != null)
                {
                    if (target.IsValidTarget(W.Range) &&
                        !target.HasBuffOfType(BuffType.Invulnerability) &&
                        !target.HasBuffOfType(BuffType.SpellShield))
                    {
                        if ((target.IsWindingUp || !target.IsMoving) && target.Distance(Player.Position) < 250)
                        {
                            W.Cast(target);
                            return;
                        }
                        var wPrediction = W.GetPrediction(target);
                        if (wPrediction.CastPosition.Distance(Player.Position) < 250 && Close)
                            W.Cast(wPrediction.CastPosition); 
                        
                        else if (wPrediction.Hitchance >= HitChance.High && target.Distance(Player) > MiniGunRange())
                        {
                            W.Cast(wPrediction.UnitPosition);
                            return;
                        }
                    }
                }
            
            }

            if (E.IsReady() && Player.Mana - 50 > (R.IsReady() ? 100:0) && useE)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);

                if (target != null)
                {
                    if (target.HasBuffOfType(BuffType.Stun)
                        || target.HasBuffOfType(BuffType.Asleep) ||
                        target.HasBuffOfType(BuffType.Snare) ||
                        target.HasBuffOfType(BuffType.Asleep) ||
                        target.HasBuffOfType(BuffType.Knockup) ||
                        target.HasBuffOfType(BuffType.Suppression) ||
                        target.HasBuffOfType(BuffType.Taunt))
                    {
                        E.Cast(target.Position);
                        return;
                    }

                    var ePrediction = E.GetPrediction(target);

                    if (ePrediction.Hitchance >= HitChance.High &&
                        ePrediction.CastPosition.Distance(target) > 150 || ePrediction.Hitchance >= HitChance.High &&
                        ePrediction.CastPosition.Distance(target) > 150 && IsMovingTowards(target, Player.Position))
                    {
                        E.Cast(ePrediction.CastPosition);
                        return;
                    }
                }
            }

            if (!R.IsReady() || Player.IsUnderEnemyTurret() || !useR)
                return;
            var targets = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            R.CastIfWillHit(targets, useRAoenum);
        }

        private static void Killsteal()
        {
            var UseW = Menu["KillSteal"].GetValue<MenuBool>("UseW").Enabled;
            if (W.IsReady() && UseW && Player.Mana - 90 > (R.IsReady() ? 130 : 30) &&
                !Player.Position.IsUnderEnemyTurret()
            )
                
                if (GameObjects.EnemyHeroes.Any(
                    x => x.IsValidTarget(W.Range) && !x.HasBuffOfType(BuffType.SpellShield) &&
                         !x.HasBuffOfType(BuffType.Invulnerability)))
                    foreach (
                        var wPrediction in
                        from enemy in GameObjects.EnemyHeroes.Where(
                            x => x.IsValidTarget(W.Range) && !x.HasBuffOfType(BuffType.SpellShield) &&
                                 !x.HasBuffOfType(BuffType.Invulnerability))
                        let health = enemy.Health
                        let wDamage = Player.GetSpellDamage(enemy, SpellSlot.W)
                        let wPrediction = W.GetPrediction(enemy)
                        where health <= wDamage && wPrediction.Hitchance >= HitChance.High
                        select wPrediction)
                        W.Cast(wPrediction.UnitPosition);
            if (!E.IsReady() || !(Player.Mana - 50 > 100))
                return;

            foreach (
                var enemy in GameObjects.EnemyHeroes.Where(
                    x =>
                        x.IsValidTarget(E.Range) &&
                        x.Buffs.Any(
                            m =>
                                m.Name.Equals("zhonyasringshield", StringComparison.CurrentCultureIgnoreCase) ||
                                m.Name.Equals("bardrstasis", StringComparison.CurrentCultureIgnoreCase))))
                if (enemy.Buffs.Any(m =>
                    m.Name.Equals("zhonyasringshield", StringComparison.CurrentCultureIgnoreCase) ||
                    m.Name.Equals("bardrstasis", StringComparison.CurrentCultureIgnoreCase)))
                {
                    var buffTime = enemy.Buffs.FirstOrDefault(m =>
                        m.Name.Equals("zhonyasringshield", StringComparison.CurrentCultureIgnoreCase) ||
                        m.Name.Equals("bardrstasis", StringComparison.CurrentCultureIgnoreCase));

                    if (buffTime != null && buffTime.EndTime - Variables.GameTimeTickCount < 1 &&
                        buffTime.EndTime - Variables.GameTimeTickCount > .3f &&
                        enemy.IsValidTarget(E.Range)) E.Cast(enemy.Position);
                }
                else if (enemy.IsValidTarget(E.Range))
                {
                    E.CastIfHitchanceMinimum(enemy, HitChance.Dash);
                }
        }

        private static void Harass()
        {
            var targets = GameObjects.EnemyHeroes;
            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical) ;
            bool useW = Menu["Harass"].GetValue<MenuBool>("UseW").Enabled;
            bool useQ = Menu["Harass"].GetValue<MenuBool>("UseQ").Enabled;
            var mana = Menu["Harass"].GetValue<MenuSlider>("minmana").Value;
            if (Player.ManaPercent < mana)
                return;
            if (useW && target != null)
            {
                if ((target.IsWindingUp || !target.IsMoving) && target.Distance(Player.Position) < 250)
                {
                    W.Cast(target);
                    return;
                }
                var targW = W.GetPrediction(target);
                if (targW.CastPosition.Distance(Player.Position) < 250)
                    W.Cast(targW.CastPosition);
                else if (W.Range - 50 > targW.UnitPosition.Distance(Player.Position) &&
                         targW.Hitchance >= HitChance.High)
                    W.Cast(targW.UnitPosition);
            }

            if (HasMinigun && useQ)
                foreach (var source in targets.Where(x => x.IsValidTarget(SecondGunRange())))
                {
                    Q.Cast();
                    return;
                }
            else if (targets.Any(x =>
                x.IsValidTarget(SecondGunRange()) &&
                Isinautoattackrange(x, Player.GetRealAutoAttackRange())) && HasRocketLauncher) Q.Cast();
        }

        #endregion Combo and other speels

        #endregion Update

        #region Events

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (!Menu["Misc"].GetValue<MenuBool>("UseE").Enabled) return;
            
            if (!E.IsReady() || sender == null || !sender.IsEnemy || !sender.IsValidTarget(E.Range)) return;
            var pred = E.GetPrediction(sender);
            if (pred != null && pred.Hitchance >= HitChance.Dash) E.Cast(pred.CastPosition);
        }

        private static void OnInterruptible(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!E.IsReady() || args.Sender.Position.Distance(Player) > 350 || !sender.IsEnemy || !sender.IsValidTarget(E.Range) ||
                Math.Abs(args.EndTime) > 1) return;
            var pred = E.GetPrediction(sender);
            if (pred != null && pred.Hitchance >= HitChance.High) E.Cast(pred.CastPosition);
        }

        #endregion Events

        private static void Levelup()
        {
            if (Math.Abs(Player.PercentCooldownMod) >= 0.8) return; // if it's urf Don't auto level 
            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

            if (qLevel + wLevel + eLevel + rLevel >= Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++) level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

            if (qLevel < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);

            if (wLevel < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);

            if (eLevel < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rLevel < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
        }
    }
}