﻿using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using SharpDX;
using SharpDX.Direct3D9;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
namespace MightyAio.Champions
{
    internal class Udyr
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static Menu _menu, _emotes;
        private static AIHeroClient Player => ObjectManager.Player;
        private static int[] _SkillOrder;
        private static bool isInQStance => Player.HasBuff("UdyrTigerStance");
        private static bool isInWStance => Player.HasBuff("UdyrTurtleStance");
        private static bool isInEStance => Player.HasBuff("UdyrBearStance");
        private static bool isInRStance => Player.HasBuff("UdyrPhoenixStance");
        private static bool HavePhoenixAoe => Player.HasBuff("UdyrPhoenixActivation");

        private static int MonkeyAgility
        {
            get
            {
                var data = ObjectManager.Player.Buffs.FirstOrDefault(b => b.Name == "UdyrMonkeyAgilityBuff");
                return data != null ? data.Count : 0;
            }
        }

        private static Font _berlinfont;
        private static int _chatsent;
        private static int _mykills = 0 + Player.ChampionsKilled;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Udyr", "Udyr", true);
            var Qmenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QM", "Mana For using Q in Harass", 30)
            };
            _menu.Add(Qmenu);
            var Wmenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WS", "Use W to block Spells"),
                new MenuSlider("WHC", "Use W in Combo When My health is below", 40),
                new MenuBool("WH", "Use W in Harass"),
                new MenuSlider("WM", "Mana For using W in Harass", 50)
            };
            _menu.Add(Wmenu);
            var Emenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EG", "Use E on Gapcloser"),
                new MenuBool("EI", "Use E on Interpreter"),
                new MenuBool("EH", "Use W in Harass"),
                new MenuSlider("EM", "Mana For using E in Harass", 50),
                new MenuBool("EF", "Use E in Feel"),
                new MenuKeyBind("Feel", "Feel Key", Keys.Z, KeyBindType.Press),
                new MenuBool("FeelB", "Use Bm in Feel mode")
            };
            _menu.Add(Emenu);
            var Rmenu = new Menu("R", "R")
            {
                new MenuBool("RC", "Use R in Combo"),
                new MenuBool("RH", "Use R in Harass"),
                new MenuSlider("RM", "Mana For using R in Harass", 50)
            };
            _menu.Add(Rmenu);

            var LaneClear = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W"),
                new MenuSlider("WH", "Use W When my Health % is Below", 70),
                new MenuBool("R", "Use R"),
                new MenuSlider("RC", "Use R if it can hit", 2, 1, 5),
                new MenuSlider("Mana", "Mana For LaneClear Only", 40),
                new MenuBool("P", "Keep Passive Alive", false)
            };
            _menu.Add(LaneClear);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 3, 0, 55),
                new MenuBool("autolevel", "Auto Level"),
                new MenuList("LevelUp", "LevelUpType",
                    new[] {"QWE", "RWE"})
            };

            // use emotes
            _emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            miscMenu.Add(_emotes);
            _menu.Add(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            _menu.Add(drawMenu);
            _menu.Attach();
        }

        #endregion

        #region load

        public Udyr()
        {
            _q = new Spell(SpellSlot.Q, Player.GetRealAutoAttackRange());
            _w = new Spell(SpellSlot.W, Player.GetRealAutoAttackRange());
            _e = new Spell(SpellSlot.E, Player.GetRealAutoAttackRange());
            _r = new Spell(SpellSlot.R, Player.GetRealAutoAttackRange());
            CreateMenu();
            switch (_menu["Misc"].GetValue<MenuList>("LevelUp").SelectedValue)
            {
                case "QWE":
                    _SkillOrder = new[] {1, 2, 3, 1, 1, 2, 1, 2, 1, 2, 2, 3, 3, 3, 3, 1, 2, 3};
                    break;
                case "RWE":
                    _SkillOrder = new[] {4, 2, 3, 4, 4, 2, 4, 2, 4, 2, 2, 3, 3, 3, 3, 4, 2, 3};
                    break;
            }

            _berlinfont = new Font(
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
            Drawing.OnDraw += DrawingOnOnDraw;
            AntiGapcloser.OnGapcloser += GapcloserOnOnGapcloser;
            Interrupter.OnInterrupterSpell += InterrupterOnOnInterrupterSpell;
            AIBaseClient.OnDoCast += AIBaseClientOnOnProcessSpellCast;
        }

        #endregion

        #region Args

        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && sender.IsValidTarget(800))
                if (_e.IsReady() &&
                    Player.Distance(sender) <= 800 && _menu["E"].GetValue<MenuBool>("EI").Enabled)
                {
                    if (!isInEStance)
                        _e.Cast();
                    else
                        return;
                }
        }

        private void InterrupterOnOnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (sender.IsEnemy && sender.IsValidTarget(800))
                if (_e.IsReady() &&
                    Player.Distance(sender) <= 800)
                {
                    if (!isInEStance && _menu["E"].GetValue<MenuBool>("EI").Enabled)
                        _e.Cast();
                    else
                        return;
                }

            if (sender.IsEnemy && sender.IsCastingImporantSpell() && _menu["W"].GetValue<MenuBool>("WS").Enabled) _w.Cast();
        }

        private void GapcloserOnOnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (sender.IsEnemy && _e.IsReady() && _menu["E"].GetValue<MenuBool>("EG").Enabled &&
                Player.Distance(sender) <= _e.Range && !isInEStance)
                _e.Cast(sender, true);

            else if (_w.IsReady() && !isInEStance && Player.Distance(sender) < 600) _w.Cast();
        }

        #endregion

        #region Drawing

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawKill = _menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities").Enabled;
            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
                if (enemyVisible.IsValidTarget())
                {
                    var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) +
                                  Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) *
                                  Player.Crit;
                    var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                    if (drawKill)
                        DrawText(_berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                            (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, Color.White);
                }
        }

        #endregion

        #region OnUpdate

        private void GameOnOnUpdate(EventArgs args)
        {
            if (Player.ChampionsKilled > _mykills && _emotes.GetValue<MenuBool>("Kill").Enabled)
            {
                _mykills = Player.ChampionsKilled;
                Emote();
            }

            var getskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = _menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled;
            if (skin) Player.SetSkin(getskin);

            if (Player.IsRecalling() || Player.IsDead || !Player.CanCast) return;

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
            }

            if (_menu["Misc"].GetValue<MenuBool>("autolevel").Enabled) Levelup();
            if (_menu["E"].GetValue<MenuKeyBind>("Feel").Active) Feel();
        }

        #endregion

        #region Orbwalker Mode
        private void  maptype()
        {
         
        }
        private void Combo()
        {
            var target = TargetSelector.GetTarget(900,DamageType.Mixed);
            if (target == null) return;
            if (_e.IsReady() && _menu["E"].GetValue<MenuBool>("EC").Enabled)
            {
                if (Player.Distance(target) > _e.Range)
                    _e.Cast();

                else if (Player.Distance(target) <= _e.Range && !target.HasBuff("udyrbearstuncheck")) _e.Cast();
            }

            if (_w.IsReady() && Player.HealthPercent <= _menu["W"].GetValue<MenuSlider>("WHC").Value &&
                _menu["W"].GetValue<MenuBool>("WC").Enabled && Player.Distance(target) <= _w.Range &&
                target.HasBuff("udyrbearstuncheck")) _w.Cast();
            if (_q.IsReady() && _menu["Q"].GetValue<MenuBool>("QC").Enabled && Player.Distance(target) <= _q.Range &&
                target.HasBuff("udyrbearstuncheck")) _q.Cast();
            if (_r.IsReady() && _menu["R"].GetValue<MenuBool>("RC").Enabled && Player.Distance(target) <= _r.Range &&
                target.HasBuff("udyrbearstuncheck")) _r.Cast();
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(900,DamageType.Mixed);
            if (target == null) return;
            if (_e.IsReady() && _menu["E"].GetValue<MenuBool>("EH").Enabled &&
                _menu["E"].GetValue<MenuSlider>("EM").Value > Player.ManaPercent)
            {
                if (Player.Distance(target) > _e.Range)
                    _e.Cast();

                else if (Player.Distance(target) <= _e.Range && !target.HasBuff("udyrbearstuncheck")) _e.Cast();
            }

            if (_q.IsReady() && _menu["Q"].GetValue<MenuSlider>("QM").Value > Player.ManaPercent &&
                _menu["Q"].GetValue<MenuBool>("QH").Enabled && Player.Distance(target) <= _q.Range &&
                target.HasBuff("udyrbearstuncheck")) _q.Cast();
            if (_r.IsReady() && _menu["R"].GetValue<MenuSlider>("RM").Value > Player.ManaPercent &&
                _menu["R"].GetValue<MenuBool>("RH").Enabled && Player.Distance(target) <= _r.Range &&
                target.HasBuff("udyrbearstuncheck")) _r.Cast();
        }

        private void JungleClear()
        {
            var Jgl = GameObjects.GetJungles(Player.Position, Player.GetRealAutoAttackRange())
                .OrderBy(x => x.DistanceToPlayer()).FirstOrDefault();
            if (!Jgl.IsValidTarget() || Jgl == null) return;
            var buffEndTime = PassvieBuffTimer(Player);
            if (_menu["LaneClear"].GetValue<MenuBool>("P").Enabled)
            {
                if (_q.IsReady() && (MonkeyAgility != 3 || buffEndTime > Game.Time + buffEndTime / 2))
                {
                    _q.Cast();
                    return;
                }

                if (_w.IsReady() && !_q.IsReady() && (MonkeyAgility != 3 || buffEndTime > Game.Time + buffEndTime / 2))
                {
                    _w.Cast();
                    return;
                }

                if (_e.IsReady() && !_w.IsReady() && (MonkeyAgility != 3 || buffEndTime > Game.Time + buffEndTime / 2))
                {
                    _e.Cast();
                    return;
                }

                if (_r.IsReady() && !_r.IsReady() && !_w.IsReady() &&
                    (MonkeyAgility != 3 || buffEndTime > Game.Time + buffEndTime / 2))
                {
                    _r.Cast();
                    return;
                }
            }

            if (!isInRStance && _r.IsReady() && _menu["LaneClear"].GetValue<MenuBool>("R").Enabled)
            {
                var allMinionsQ = GameObjects.GetJungles(_q.Range);

                var Rfarm = _r.GetCircularFarmLocation(allMinionsQ, _r.Range);
                if (Rfarm.MinionsHit >= _menu["LaneClear"].GetValue<MenuSlider>("RC").Value)
                    _r.Cast();
            }

            if (_menu["LaneClear"].GetValue<MenuBool>("Q").Enabled && _q.IsReady())
                _q.Cast();

            else if (_menu["LaneClear"].GetValue<MenuBool>("W").Enabled && !isInWStance && _w.IsReady() &&
                     Player.HealthPercent <= _menu["LaneClear"].GetValue<MenuSlider>("WH").Value) _w.Cast();
        }

        private void LaneClear()
        {
            var Minion = GameObjects.GetMinions(Player.Position, Player.GetRealAutoAttackRange())
                .OrderBy(x => x.DistanceToPlayer()).FirstOrDefault();
            if (!Minion.IsValidTarget() || Minion == null ||
                _menu["LaneClear"].GetValue<MenuSlider>("Mana").Value > Player.ManaPercent) return;

            if (!isInRStance && _r.IsReady() && _menu["LaneClear"].GetValue<MenuBool>("R").Enabled)
            {
                var allMinionsQ = GameObjects.GetJungles(_q.Range);

                var Rfarm = _r.GetCircularFarmLocation(allMinionsQ, _r.Range);
                if (Rfarm.MinionsHit >= _menu["LaneClear"].GetValue<MenuSlider>("RC").Value)
                    _r.Cast();
            }

            if (_menu["LaneClear"].GetValue<MenuBool>("Q").Enabled && _q.IsReady() &&
                Player.HealthPercent > _menu["LaneClear"].GetValue<MenuSlider>("WH").Value)
                _q.Cast();

            else if (_menu["LaneClear"].GetValue<MenuBool>("W").Enabled && !isInWStance && _w.IsReady() &&
                     Player.HealthPercent <= _menu["LaneClear"].GetValue<MenuSlider>("WH").Value) _w.Cast();
        }

        private void Feel()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var Feelbm = _menu["E"].GetValue<MenuBool>("FeelB").Enabled;
            if (_e.IsReady())
            {
                _chatsent = 0;
                _e.Cast();
            }

            if (_chatsent == 0 && Feelbm)
            {
                Game.Say("Later Bitch", true);
                _chatsent = 1;
            }
        }

        #endregion


        #region Extra Functions

        private static void Levelup()
        {
            var qLevel = _q.Level;
            var wLevel = _w.Level;
            var eLevel = _e.Level;
            var rLevel = _r.Level;

            if (qLevel + wLevel + eLevel + rLevel >= Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++)
                level[_SkillOrder[i] - 1] = level[_SkillOrder[i] - 1] + 1;

            if (qLevel < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);

            if (wLevel < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);

            if (eLevel < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rLevel < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
        }

        private static void Emote()
        {
            var b = _emotes.GetValue<MenuList>("selectitem").SelectedValue;
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

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static float PassvieBuffTimer(AIHeroClient Player)
        {
            var buffEndTimer = Player.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                .Where(buff => buff.Name == "UdyrMonkeyAgilityBuff")
                .Select(buff => buff.EndTime)
                .FirstOrDefault();
            return buffEndTimer;
        }

        #endregion
    }
}