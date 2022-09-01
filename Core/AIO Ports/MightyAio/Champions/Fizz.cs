using System;
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
    internal class Fizz
    {
        private static AIHeroClient Player => ObjectManager.Player;

        public static Menu Menu { get; set; }

        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("Fizz", "Fizz", true);

            // Combo
            var comboMenu = new Menu("Combo", "Combo")
            {
                new MenuBool("UseAllinCombo", "Use RQWE"),
                new MenuBool("UseEEQWCombo", "Use EEQW only uses when you hit R"),
                new MenuBool("UseQWECombo", "Use QWE"),
                new MenuBool("UseQWCombo", "Use QW"),
                new MenuBool("UseQCombo", "Use Q"),
                new MenuBool("UseWCombo", "Use W"),
                new MenuBool("UseECombo", "Use E"),
                new MenuSlider("OnlyUse", "Only Use R When target is below % ", 100)
            };
            var ally = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsEnemy
                select hero;
            foreach (var hero in ally)
                comboMenu.Add(new MenuBool(hero.CharacterName, "Use R on " + hero.CharacterName));
            Menu.Add(comboMenu);
            // Harass
            var harassMenu = new Menu("Harass", "Harass")
            {
                new MenuBool("UseQWECombo", "Use QWE"),
                new MenuBool("UseQWCombo", "Use QW"),
                new MenuBool("UseQCombo", "Use Q"),
                new MenuBool("UseWCombo", "Use W"),
                new MenuBool("UseECombo", "Use E")
            };
            Menu.Add(harassMenu);
            // lane clear
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuBool("UseW", "Use W"),
                new MenuBool("UseQ", "Use Q"),
                new MenuBool("UseE", "Use E"),
                new MenuSlider("minmana", "minimum mana % for lane clear", 40)
            };
            Menu.Add(laneclear);
            // lasthit
            var lasthit = new Menu("lasthit", "Last Hit")
            {
                new MenuBool("UseWlasthit", "Use W"),
                new MenuBool("UseQWlasthit", "Uses abilities to ks jungle buffs")
            };
            Menu.Add(lasthit);
            // Feel
            var Feel = new Menu("Feel", "Feel")
            {
                new MenuBool("UseEtofeel", "Use E"),
                new MenuBool("UseQtofeel", "Use Q uses Q to neareast target to mouse "),
                new MenuKeyBind("feelkey", "Feel Key", Keys.Z, KeyBindType.Press)
            };
            Menu.Add(Feel);
            // auto item buy
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Doran's Ring", "The Dark Seal", "Corrupting Potion", "none"})
            };
            Menu.Add(itembuy);
            // auto Level up
            var autolevel = new Menu("autolevel", "Auto Level Up")
            {
                new MenuBool("autolevelu", "Auto Level")
            };
            Menu.Add(autolevel);
            // use emotes
            var emotes = new Menu("emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill"),
                new MenuBool("R", "Use on R hit")
            };
            Menu.Add(emotes);
            // Item
            var Items = new Menu("Items", "Items")
            {
                new MenuBool("GunbladeCombo", "use Gunblade When Combo"),
                new MenuBool("GunbladeKS", " use Gunblade to Kill steal"),
                new MenuBool("HPKS2", "HEXTECH PROTOBELT in Combo as gabcloser"),
                new MenuBool("HPKS", "HEXTECH PROTOBELT to Kill steal"),
                new MenuBool("Use9", "Use Spell binder at max stacks in combo")
            };
            Menu.Add(Items);
            // kill steal
            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("UseQ", "Use Q"),
                new MenuBool("UseE", "Use E"),
                new MenuBool("UseR", "Use R", false),
                new MenuBool("UseQWE", "Use Q W E"),
                new MenuBool("UseQW", "Use QW"),
                new MenuBool("UseFlash", "Use Flash", false)
            };
            Menu.Add(killsteal);

            // Misc
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseETower", "Dodge tower shots with E"),
                new MenuSlider("setskin", "set skin", 14, 0, 15)
            };
            Menu.Add(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
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
        private static Spell Q, W, E, R, RC, RG;
        private static int[] SpellLevels;

        #endregion Spells

        #region GameLoad

        public Fizz()
        {
            var championName = Player.CharacterName.ToLowerInvariant();

            switch (championName)
            {
                case "fizz":
                    // skill order
                    SpellLevels = new[] {2, 3, 1, 3, 3, 4, 3, 2, 3, 2, 4, 2, 2, 1, 1, 4, 1, 1};
                    break;
            }

            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, Player.GetRealAutoAttackRange());
            E = new Spell(SpellSlot.E, 400);
            E.SetSkillshot(0.25f, 330, float.MaxValue, false, SpellType.Circle);
            R = new Spell(SpellSlot.R, 1300);
            RC = new Spell(SpellSlot.R, 910);
            RG = new Spell(SpellSlot.R, 1300);
            R.SetSkillshot(0.25f, 80, 1300, false, SpellType.Line);
            RC.SetSkillshot(0.25f, 80, 1300, false, SpellType.Line);
            RG.SetSkillshot(0.25f, 80, 1300, false, SpellType.Line);
            RG.SetSkillshot(0.25f, 80, 1300, false, SpellType.Line);

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
            AIBaseClient.OnProcessSpellCast += ObjAiBaseOnOnProcessSpellCast;
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled;
            var drawE = Menu["Drawing"].GetValue<MenuBool>("DrawE").Enabled;
            var drawR = Menu["Drawing"].GetValue<MenuBool>("DrawR").Enabled;
            var drawKill = Menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities").Enabled;
            var p = Player.Position;


            if (drawQ && Q.IsReady()) Drawing.DrawCircleIndicator(p, Q.Range, Color.DarkCyan);

            if (drawE && E.IsReady()) Drawing.DrawCircleIndicator(p, E.Range, Color.Red);

            if (drawR && R.IsReady()) Drawing.DrawCircleIndicator(p, R.Range, Color.DarkCyan);

            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget()))
            {
                var magicdmg = 0;
                if (W.Level == 0)
                    magicdmg = 0;
                else
                    magicdmg = (int) W.GetDamage(enemyVisible);

                if (enemyVisible.IsValidTarget())
                {
                    var aa = string.Format("AA Left:" +
                                           (int) (enemyVisible.Health /
                                                  (Player.CalculatePhysicalDamage(enemyVisible,
                                                      Player.TotalAttackDamage) + magicdmg)));

                    var rc = new[] {0, 255, 325, 425}[R.Level] + 1 * Player.TotalMagicalDamage;
                    var rg = new[] {0, 300, 400, 500}[R.Level] + 1.2 * Player.TotalMagicalDamage;
                    var qdmg = Player.GetSpellDamage(enemyVisible, SpellSlot.Q) +
                               Player.GetSpellDamage(enemyVisible, SpellSlot.W);
                    var wdmg = Player.GetSpellDamage(enemyVisible, SpellSlot.W) +
                               Player.CalculateMagicDamage(enemyVisible, Wsdmg()) + Player.TotalAttackDamage;
                    var edmg = Player.GetSpellDamage(enemyVisible, SpellSlot.E);
                    var rdmg = Player.GetSpellDamage(enemyVisible, SpellSlot.R);
                    var rcdmg = Player.CalculateMagicDamage(enemyVisible, rc);
                    var rgdmg = Player.CalculateMagicDamage(enemyVisible, rg);

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
                        else if (qdmg + wdmg + edmg > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q+W+E):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else if (qdmg + wdmg + edmg + rdmg > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q+W+E+ R GUPPY):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else if (qdmg + wdmg + edmg + rcdmg > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q+W+E+ R CHOMPER):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else if (qdmg + wdmg + edmg + rgdmg > enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q+W+E+ R GIGALODON):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else
                            DrawText(Berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                }
            }
        }

        private static float Wsdmg()
        {
            var wdmsss = 0;
            var magidmg = Player.TotalMagicalDamage / 2;
            if (W.Level == 1)
                wdmsss = 50 + (int) magidmg;
            else if (W.Level == 2)
                wdmsss = 70 + (int) magidmg;
            else if (W.Level == 3)
                wdmsss = 90 + (int) magidmg;
            else if (W.Level == 4)
                wdmsss = 110 + (int) magidmg;
            else if (W.Level == 5) wdmsss = 130 + (int) magidmg;
            return wdmsss;
        }

        private static void ObjAiBaseOnOnProcessSpellCast(AIBaseClient sender,
            AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender is AITurretClient && args.Target.IsMe && E.IsReady() &&
                Menu["Misc"].GetValue<MenuBool>("UseETower").Enabled) E.Cast(Game.CursorPos);

            if (!sender.IsMe) return;

            if (args.SData.Name == "FizzW") Orbwalker.ResetAutoAttackTimer();
        }

        #endregion GameLoad

        #region Update

        private static void GameOnOnUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(R.Range,DamageType.Magical);
            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = Menu["autoitem"].GetValue<MenuList>("selectitem").SelectedValue;

            if (item != "none")
                switch (item)
                {
                    case "Doran's Ring":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Ring)) Player.BuyItem(ItemId.Dorans_Ring);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                    case "The Dark Seal":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Dark_Seal))
                                Player.BuyItem(ItemId.Dark_Seal);
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

            var useonkill = Menu["emotes"].GetValue<MenuBool>("Kill").Enabled;

            if (ObjectManager.Player.ChampionsKilled > mykills && useonkill)
            {
                mykills = ObjectManager.Player.ChampionsKilled;
                Emote();
            }

            var getskin = Menu["Misc"].GetValue<MenuSlider>("setskin").Value;

            Player.SetSkin(getskin);
            if (Player.HasItem(ItemId.Zhonyas_Hourglass) && Player.HealthPercent < 20) Player.UseItem(3157);

            foreach (
                var targets in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget()))
            {
            }

            if (!Player.CanCast) return;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Harass:
                    DoHarass();
                    break;

                case OrbwalkerMode.Combo:
                    DoCombo();
                    break;

                case OrbwalkerMode.LaneClear:
                    DoLaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    DoLastHit();
                    break;
            }

            if (Menu["Feel"].GetValue<MenuKeyBind>("feelkey").Active) Feel();
            Killsteal();
            Levelup();
        }

        public static void Emote()
        {
            var b = Menu["emotes"].GetValue<MenuList>("selectitem").SelectedValue;
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

        public static void CastRSmart(AIHeroClient target)
        {
            var castPosition = R.GetPrediction(target).CastPosition;
            castPosition = Player.Position.Extend(castPosition, R.Range);
            var FizzRCtargetRangeMin = 455;
            var FizzRGtargetRangeMin = 910;
            var hittchance = 0;

            if (target.MoveSpeed >= 500)
                hittchance = 0;
            else if (target.MoveSpeed < 500)
                hittchance = 1;
            else if (target.IsSlowed || target.IsStunned || target.IsAsleep || target.IsCharmed || target.IsFeared ||
                     target.IsTaunted || target.IsGrounded || target.IsSuppressed) hittchance = 2;
            if (R.IsReady() && hittchance == 1)
            {
                var targR = R.GetPrediction(target);
                if (targR.CastPosition.Distance(Player.Position) < 200 )
                    R.Cast(targR.CastPosition);
            }

            if (R.IsReady() && hittchance >= 1)
            {
                var targR = R.GetPrediction(target);
                if (targR.CastPosition.Distance(Player.Position) < FizzRCtargetRangeMin ) R.Cast(targR.CastPosition);
            }

            if (R.IsReady() && hittchance >= 1)
            {
                var targR = R.GetPrediction(target);
                if (targR.CastPosition.Distance(Player.Position) < FizzRGtargetRangeMin  ) R.Cast(targR.CastPosition);
            }

            if (R.IsReady() && hittchance >= 1)
            {
                var targR = R.GetPrediction(target);
                if (targR.CastPosition.Distance(Player.Position) < R.Range - 100 &&
                    targR.Hitchance >= HitChance.High) R.Cast(targR.CastPosition);
            }
        }

        private static void DoLastHit()
        {
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, W.Range + Player.BoundingRadius);
            var allJgl = GameObjects.GetJungles(ObjectManager.Player.Position, Q.Range);
            var usew = Menu["lasthit"].GetValue<MenuBool>("UseWlasthit").Enabled;
            var ks = Menu["lasthit"].GetValue<MenuBool>("UseQWlasthit").Enabled;

            if (!Player.IsMoving) Orbwalker.MoveEnabled = true;

            foreach (var minion in allMinions)
            {
                var totalWDamage = Wsdmg();
                if (W.IsReady() && usew &&
                    minion.IsValidTarget(W.Range + Player.BoundingRadius + minion.BoundingRadius) &&
                    W.Range + Player.BoundingRadius + minion.BoundingRadius > Player.Distance(minion.Position) &&
                    minion.Health < totalWDamage + Player.TotalAttackDamage)
                    if (!minion.IsDead)
                    {
                        Orbwalker.MoveEnabled = false;
                        Player.IssueOrder(GameObjectOrder.MoveTo, minion);
                        if (!minion.IsDead)
                        {
                            W.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                        }
                    }
            }

            foreach (var jgl in allJgl)
            {
                var totalQDamage = Player.GetSpellDamage(jgl, SpellSlot.Q);
                var totalWDamage = Wsdmg();

                if (Q.IsReady() && ks && jgl.IsValidTarget() && Q.IsInRange(jgl) && totalQDamage > jgl.Health)
                    Q.Cast(jgl);
                if (W.IsReady() && ks && jgl.IsValidTarget(W.Range + Player.BoundingRadius + jgl.BoundingRadius) &&
                    W.Range + Player.BoundingRadius + jgl.BoundingRadius > Player.Distance(jgl.Position) &&
                    totalWDamage > jgl.Health) W.CastOnUnit(jgl);
            }
        }

        private static void Feel()
        {
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, Q.Range);
            var jungless = GameObjects.GetJungles(ObjectManager.Player.Position, Q.Range);
            var qto = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            var UseEtofeel = Menu["Feel"].GetValue<MenuBool>("UseEtofeel").Enabled;
            var UseQtofeel = Menu["Feel"].GetValue<MenuBool>("UseQtofeel").Enabled;
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (E.IsReady() && UseEtofeel)
            {
                E.Cast(Game.CursorPos);
                E.Cast(Game.CursorPos);
            }

            foreach (var minion in allMinions)
            {
                if (!minion.IsValidTarget()) return;
                if (minion.Distance(Game.CursorPos) < 150 && UseQtofeel) Q.Cast(minion);
            }

            foreach (var jgl in jungless)
            {
                if (!jgl.IsValidTarget()) return;
                if (jgl.Distance(Game.CursorPos) < 150 && UseQtofeel) Q.Cast(jgl);
            }

            if (!qto.IsValidTarget()) return;
            if (qto.Distance(Game.CursorPos) < 150 && UseQtofeel) Q.Cast(qto);
        }

        private static void DoLaneClear()
        {
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, Q.Range);
            var allJgl = GameObjects.GetJungles(ObjectManager.Player.Position, Q.Range);
            var useq = Menu["laneclear"].GetValue<MenuBool>("UseQ").Enabled;
            var usew = Menu["laneclear"].GetValue<MenuBool>("UseW").Enabled;
            var usee = Menu["laneclear"].GetValue<MenuBool>("UseE").Enabled;
            var settedmana = Menu["laneclear"].GetValue<MenuSlider>("minmana").Value;
            var playermana = (int) Player.ManaPercent;
            if (!Player.IsMoving) Orbwalker.MoveEnabled = true;

            if (playermana < settedmana) return;
            foreach (var minion in allMinions)
            {
                var totalQDamage = Player.GetSpellDamage(minion, SpellSlot.Q);
                var totalWDamage = Wsdmg();
                var laneE = GameObjects.GetMinions(ObjectManager.Player.Position, E.Range + E.Width);
                var Efarmpos = E.GetLineFarmLocation(laneE, E.Width);

                if (Q.IsReady() && minion.IsValidTarget() && Q.IsInRange(minion) &&
                    minion.Health < totalQDamage && useq)
                    Q.Cast(minion);
                if (W.IsReady() && minion.IsValidTarget(W.Range + Player.BoundingRadius + minion.BoundingRadius) &&
                    W.Range + Player.BoundingRadius + minion.BoundingRadius > Player.Distance(minion.Position) &&
                    minion.Health < totalWDamage + Player.TotalAttackDamage && usew)
                    W.CastOnUnit(minion);
                if (minion.IsValidTarget(E.Range) &&
                    Efarmpos.MinionsHit >= 3 && laneE.Count >= 3 &&
                    Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzE" && usee)
                    E.Cast(minion);
            }

            foreach (var jgl in allJgl)
            {
                var laneJ = GameObjects.GetJungles(ObjectManager.Player.Position, E.Range + E.Width);
                var jfarmpos = E.GetLineFarmLocation(laneJ, E.Width);

                if (Q.IsReady() && jgl.IsValidTarget() && Q.IsInRange(jgl)) Q.Cast(jgl);
                if (W.IsReady() && jgl.IsValidTarget(W.Range + Player.BoundingRadius + jgl.BoundingRadius) &&
                    W.Range + Player.BoundingRadius + jgl.BoundingRadius > Player.Distance(jgl.Position))
                    W.CastOnUnit(jgl);
                if (jgl.IsValidTarget(E.Range) &&
                    jfarmpos.MinionsHit >= 3 && laneJ.Count >= 3 &&
                    Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzE")
                    E.Cast(jgl);
            }
        }

        private static void DoCombo()
        {
            var target = TargetSelector.GetTarget(R.Range,DamageType.Magical);
            var allin = Menu["Combo"].GetValue<MenuBool>("UseAllinCombo").Enabled;
            var comboE = Menu["Combo"].GetValue<MenuBool>("UseEEQWCombo").Enabled;
            var QWE = Menu["Combo"].GetValue<MenuBool>("UseQWECombo").Enabled;
            var QW = Menu["Combo"].GetValue<MenuBool>("UseQWCombo").Enabled;
            // var autowauto = Menu["Combo"].GetValue<MenuBool>("autowauto");
            var Qability = Menu["Combo"].GetValue<MenuBool>("UseQCombo").Enabled;
            var Wability = Menu["Combo"].GetValue<MenuBool>("UseWCombo").Enabled;
            var Eability = Menu["Combo"].GetValue<MenuBool>("UseECombo").Enabled;
            var usegun = Menu["Items"].GetValue<MenuBool>("GunbladeCombo").Enabled;
            var useHP = Menu["Items"].GetValue<MenuBool>("HPKS2").Enabled;
            var minmama = Menu["Combo"].GetValue<MenuSlider>("OnlyUse").Value;
            var Usespellbinder = Menu["Items"].GetValue<MenuBool>("Use9").Enabled;
            if (!target.IsValidTarget()) return;
            var Rtargets = Menu["Combo"].GetValue<MenuBool>(target.CharacterName).Enabled;
            if (Player.HasBuff("fizzeicon") && Player.Distance(target) > E.Range) E.Cast(target.Position);
            if (Usespellbinder && Q.IsInRange(target) && Spellbinderready) Player.UseItem(3907);
            // all in combo
            if (R.IsInRange(target) && R.IsReady() && allin && Rtargets && target.HealthPercent < minmama)
            {
                CastRSmart(target);
            }
            else if (target.HasBuff("fizzrcling") && comboE && Q.IsReady() && W.IsReady() && E.IsReady())
            {
                if (Q.IsInRange(target))
                {
                    W.Cast();
                    Q.CastOnUnit(target);
                    if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzE") E.Cast(target);
                }
                else
                {

                    E.Cast(target.Position);
                    E.Cast(target.Position);
                    W.Cast();
                    Q.CastOnUnit(target);
                }
            }
            else
                // q w e
            if (Q.IsInRange(target) && W.IsReady() && E.IsReady() && QWE)
            {
                W.Cast();
                Q.CastOnUnit(target);
                if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzE") E.Cast(target);
            }
            else
                // W q combo
            if (Q.IsInRange(target) && W.IsReady() && QW)
            {
                W.Cast();
                Q.CastOnUnit(target);
            }
            else
                // just q
            if (Q.IsInRange(target) && Q.IsReady() && Qability)
            {
                Q.CastOnUnit(target);
            }
            // just w
            else if (W.IsInRange(target) && W.IsReady() && Wability)
            {
                W.Cast();
            }
            // just E
            else if (E.IsInRange(target) && E.IsReady() && Eability)
            {
                if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzE") E.Cast(target);
            }
        }

        private static void Killsteal()
        {
            var target = TargetSelector.GetTarget(R.Range,DamageType.Magical);
            var useonrhit = Menu["emotes"].GetValue<MenuBool>("Kill").Enabled;

            if (!target.IsValidTarget()) return;
            if (target.HasBuff("fizzrcling") && useonrhit) DelayAction.Add(1000, () => Emote());

            var totalqdmg = Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.W);
            var totalwdmg = Player.GetSpellDamage(target, SpellSlot.W) + Player.CalculateMagicDamage(target, Wsdmg()) +
                            Player.TotalAttackDamage;
            var totalEdmg = Player.GetSpellDamage(target, SpellSlot.E);
            var totalRdmg = Player.GetSpellDamage(target, SpellSlot.R);
            var FizzRGtargetRangeMin = 910;
            var rg = R.Level;
            var level = Player.Level;
            var ap = (int) (Player.TotalMagicalDamage * 0.3);
            var ap2 = (int) (Player.TotalMagicalDamage * 0.25);
            var gunbladedmg = Player.CalculateMagicDamage(target, 170 + 4.588 * level + ap);
            var Hextechdmg = Player.CalculateMagicDamage(target, 70.588 + 4.12 * level + ap2);
            var ksr = Menu["KillSteal"].GetValue<MenuBool>("UseR").Enabled;
            var ksqwE = Menu["KillSteal"].GetValue<MenuBool>("UseQWE").Enabled;
            var ksqw = Menu["KillSteal"].GetValue<MenuBool>("UseQW").Enabled;
            var ksE = Menu["KillSteal"].GetValue<MenuBool>("UseE").Enabled;
            var ksflash = Menu["KillSteal"].GetValue<MenuBool>("UseFlash").Enabled;
            var ksq = Menu["KillSteal"].GetValue<MenuBool>("UseQ").Enabled;
            var ksgun = Menu["Items"].GetValue<MenuBool>("GunbladeKS").Enabled;
            var HPKS = Menu["Items"].GetValue<MenuBool>("HPKS").Enabled;
            
            if (rg == 1)
                rg = 300 + (int) (Player.TotalMagicalDamage * 1.2);
            else if (rg == 2)
                rg = 400 + (int) (Player.TotalMagicalDamage * 1.2);
            else if (rg == 3) rg = 500 + (int) (Player.TotalMagicalDamage * 1.2);

            var rgdmg = Player.CalculateMagicDamage(target, rg);
            if (R.IsReady() && R.IsInRange(target) && target.Health < rgdmg &&
                target.Distance(Player) > FizzRGtargetRangeMin && ksr) CastRSmart(target);
            // just q
            if (Q.IsInRange(target) && Q.IsReady() && totalqdmg > target.Health + target.AllShield && ksq)
            {
                Q.CastOnUnit(target);
            }
            else
                // just E
            if (E.IsInRange(target) && E.IsReady() && totalEdmg > target.Health + target.AllShield && ksE)
            {
                if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzE") E.Cast(target);
            }
            else
                // just E flash
            if (Player.Distance(target) < E.Range * 3 && E.IsReady() && totalEdmg > target.Health + target.AllShield &&
                ksflash)
            {
                {
                    DelayAction.Add(500, () => Player.Spellbook.CastSpell(SpellSlot.Summoner2, target.Position));
                    E.Cast(target.Position);
                    E.Cast(target.Position);
                }
            }
            else
                // W q combo
            if (Q.IsInRange(target) && W.IsReady() && totalqdmg + totalwdmg > target.Health + target.AllShield && ksqw)
            {
                W.Cast();
                Q.CastOnUnit(target);
            }
            else

                // q w e
            if (Q.IsInRange(target) && W.IsReady() && E.IsReady() &&
                totalqdmg + totalwdmg + totalEdmg > target.Health + target.AllShield && ksqwE)
            {
                W.Cast();
                Q.CastOnUnit(target);
                E.Cast(target.Position);
                E.Cast(target.Position);
            }
        }


        private static void DoHarass()
        {
            var QWE = Menu["Harass"].GetValue<MenuBool>("UseQWECombo").Enabled;
            var QW = Menu["Harass"].GetValue<MenuBool>("UseQWCombo").Enabled;
            // var autowauto = Menu["Combo"].GetValue<MenuBool>("autowauto");
            var Qability = Menu["Harass"].GetValue<MenuBool>("UseQCombo").Enabled;
            var Wability = Menu["Harass"].GetValue<MenuBool>("UseWCombo").Enabled;
            var Eability = Menu["Harass"].GetValue<MenuBool>("UseECombo").Enabled;
            var target = TargetSelector.GetTarget(Q.Range + E.Range,DamageType.Magical);

            if (Player.HasBuff("fizzeicon") && Player.Distance(target) > E.Range) E.Cast(target.Position);
            // use w for farm
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, W.Range + Player.BoundingRadius);
            foreach (var minion in allMinions)
            {
                var totalWDamage = Wsdmg();
                if (W.IsReady() && minion.IsValidTarget(W.Range + Player.BoundingRadius + minion.BoundingRadius) &&
                    W.Range + Player.BoundingRadius + minion.BoundingRadius > Player.Distance(minion.Position) &&
                    minion.Health < totalWDamage + Player.TotalAttackDamage)
                    if (!minion.IsDead)
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, minion);
                        if (!minion.IsDead)
                        {
                            W.Cast(minion);
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                        }
                    }
            }

            if (!target.IsValidTarget()) return;
            // q w e
            if (Q.IsInRange(target) && W.IsReady() && E.IsReady() && QWE)
            {
                W.Cast();
                Q.CastOnUnit(target);
                if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzE") E.Cast(target);
            }
            else
                // W q combo
            if (Q.IsInRange(target) && W.IsReady() && QW)
            {
                W.Cast();
                Q.CastOnUnit(target);
            }
            else
                // just q
            if (Q.IsInRange(target) && Q.IsReady() && Qability)
            {
                Q.CastOnUnit(target);
            }
            // just w
            else if (W.IsInRange(target) && W.IsReady() && Wability)
            {
                W.Cast();
            }
            // just E
            else if (E.IsInRange(target) && E.IsReady() && Eability)
            {
                if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzE") E.Cast(target);
            }
        }

        #endregion Update

        private static void Levelup()
        {
            var auto = Menu["autolevel"].GetValue<MenuBool>("autolevelu").Enabled;
            if (!auto) return;

            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

            if (qLevel + wLevel + eLevel + rLevel >= ObjectManager.Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < ObjectManager.Player.Level; i++)
                level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

            if (qLevel < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);

            if (wLevel < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);

            if (eLevel < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);

            if (rLevel < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
        }
    }

    internal static class SpellEx
    {
        public static bool IsEnabledAndReady(this Spell spell)
        {
            return Fizz.Menu[Orbwalker.ActiveMode.ToString()]
                .GetValue<MenuBool>("Use" + spell.Slot + Orbwalker.ActiveMode).Enabled && spell.IsReady();
        }
    }
}