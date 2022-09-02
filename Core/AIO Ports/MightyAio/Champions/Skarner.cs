﻿using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace MightyAio.Champions
{
    internal class Skarner
    {
        #region Basics

        private static Spell _q, _w, _e, _r, Flash;
        private static Menu _menu, _emotes, _Rmenu, _FlashR;
        private static AIHeroClient Player => ObjectManager.Player;
        private static SystemColors _color;
        private static Font _berlinfont;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        private static bool AutoW => _menu["W"].GetValue<MenuBool>("AW").Enabled;
        private static bool Rbuff => Player.HasBuff("skarnerimpalebuff");
        private static int OnWayPonit;
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Skarner", "Skarner", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QM", "Mana for using Q in harass", 40)
            };
            _menu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("AW", "Auto W"),
                new MenuBool("WC", "Use W when target can be stunned for Gapclose", false),
                new MenuSlider("WM", "Mana for using Auto W ", 20)
            };
            _menu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EH", "Use E in Harass"),
                new MenuSlider("EM", "Mana for using E in harass", 30)
            };
            _menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Use R when E isn't Ready"),
                new MenuBool("RT", "When target is underTower"),
                new MenuKeyBind("SimiR", "Simi R Key", Keys.T, KeyBindType.Press)
            };
            _Rmenu = new Menu("UseRon", "Use R On");

            var targets = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsEnemy
                select hero;
            foreach (var target in targets)
                _Rmenu.Add(new MenuBool(target.CharacterName, "Use R on " + target.CharacterName));
            rMenu.Add(_Rmenu);
            _FlashR = new Menu("FlashR", "Flash R On")
            {
                new MenuKeyBind("FlashKey", "Flash R Key", Keys.ControlKey, KeyBindType.Press)
            };

            var ftargets = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsEnemy
                select hero;
            foreach (var target in ftargets)
                _FlashR.Add(new MenuBool(target.CharacterName, " Flash R on " + target.CharacterName));
            rMenu.Add(_FlashR);
            _menu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("E", "E")
            };
            _menu.Add(laneClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("E", "Use E")
            };
            _menu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 4, 0, 55),
                new MenuBool("autolevel", "Auto Level")
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
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            _menu.Add(drawMenu);


            _menu.Attach();
        }

        #endregion menu

        #region Gamestart

        public Skarner()
        {
            _spellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
            _q = new Spell(SpellSlot.Q, 350);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 1000);
            _r = new Spell(SpellSlot.R, 350);
            _r.SetTargetted(0.25f, int.MaxValue);
            _e.SetSkillshot(0.25f, 70f, 1500, false, SpellType.Line);
            CreateMenu();
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
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnDoCast += AIBaseClientOnOnProcessSpellCast;
            AntiGapcloser.OnGapcloser += (sender, args) =>
            {
                // something will be added if needed it 
            };
            Interrupter.OnInterrupterSpell += (sender, args) =>
            {
                // something will be added if needed it 
            };
        }
        #endregion


        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender == null || !sender.IsValid || args.Target == null) return;
            if (sender is AITurretClient && args.Target.IsMe && AutoW && _w.IsReady()) _w.Cast();
            if (sender.IsEnemy && sender is AIHeroClient && args.Target.IsMe) _w.Cast();
            if (AutoW && _w.IsReady() && args.Target.IsMe && sender.IsJungle() && args.Target.IsMe) // For jgl Clear
                _w.Cast();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled)
                Drawing.DrawCircleIndicator(Player.Position, _q.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE").Enabled)
                Drawing.DrawCircleIndicator(Player.Position, _e.Range, Color.Red);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR").Enabled)
                Drawing.DrawCircleIndicator(Player.Position, _r.Range, Color.Firebrick);

            var drawKill = _menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities");
            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
            {
                if (!enemyVisible.IsValidTarget()) continue;
                var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage);
                var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                if (!drawKill.Enabled) continue;
                if (_q.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (_q.GetDamage(enemyVisible) + _w.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (_q.GetDamage(enemyVisible) + _e.GetDamage(enemyVisible) +
                    _r.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + E + R):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else
                    DrawText(_berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
            }
        }

        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.ChampionsKilled > _mykills && _emotes.GetValue<MenuBool>("Kill").Enabled)
            {
                _mykills = Player.ChampionsKilled;
                Emote();
            }

            var getskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = _menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled;
            if (skin && Player.SkinId != getskin) Player.SetSkin(getskin);
            if (Rbuff)
            {
                Orbwalker.AttackEnabled = false;
            }

            if (!Rbuff)
            {
                Orbwalker.AttackEnabled = true;
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
                        break;
                }
            }

            if (_menu["R"].GetValue<MenuKeyBind>("SimiR").Active) SimiR();
            if (_FlashR.GetValue<MenuKeyBind>("FlashKey").Active) FlashR();
            KillSteal();
            if (_menu["Misc"].GetValue<MenuBool>("autolevel").Enabled) Levelup();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(_e.Range,DamageType.Mixed);
            var RManaCost = Player.Level < 6 ? 0 : 100;
            var QManaCost = _q.Level < 1 ? 0 : new[] {10, 11, 12, 13, 14}[_q.Level - 1];
            if (target == null) return;

            if (_menu["R"].GetValue<MenuBool>("R").Enabled) CastR(target);
            if (_menu["Q"].GetValue<MenuBool>("QC").Enabled && Player.Mana - QManaCost > RManaCost) CastQ(target);
            if (_menu["E"].GetValue<MenuBool>("EC").Enabled && Player.Mana - 55 > RManaCost) CastE(target);
            if (haspassivebuff(target) && target.DistanceToPlayer() > 300 &&
                _menu["W"].GetValue<MenuBool>("WC").Enabled) _w.Cast();
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_e.Range,DamageType.Mixed);
            if (target == null) return;
            if (_menu["Q"].GetValue<MenuBool>("QH").Enabled &&
                _menu["Q"].GetValue<MenuSlider>("QM").Value < Player.ManaPercent) CastQ(target);
            if (_menu["E"].GetValue<MenuBool>("EH").Enabled &&
                _menu["E"].GetValue<MenuSlider>("EM").Value < Player.ManaPercent) CastE(target);
        }
        
        private static void LaneClear()
        {
            var minons = GameObjects.GetMinions(Player.Position, _q.Range).Where(x => x.IsValid && !x.IsDead).ToList();
            bool useQ = _menu["LaneClear"].GetValue<MenuBool>("Q").Enabled;
            bool useE = _menu["LaneClear"].GetValue<MenuBool>("E").Enabled;
            if (minons.Any())

                foreach (var minon in minons.OrderBy(x => x.DistanceToPlayer()))
                {
                    if (useQ && _q.IsReady() &&
                        _q.IsInRange(minon)) _q.Cast();


                    if (useE && _e.IsReady() && _e.IsInRange(minon)) _e.Cast(minon);
                    break;
                }

            var jgls = GameObjects.GetJungles(_q.Range).Where(x => x.IsValid && !x.IsDead).ToList();
            if (jgls.Any())
                foreach (var jgl in jgls.OrderBy(x => x.DistanceToPlayer()))
                {
                    if (useQ && _q.IsReady() &&
                        _q.IsInRange(jgl)) _q.Cast();


                    if (useE && _e.IsReady() && _e.IsInRange(jgl)) _e.Cast(jgl);
                    break;
                }
        }

        #endregion


        #region Spell Functions

        private static void CastQ(AIHeroClient target)
        {
            if (!_q.IsReady())
                return;
            if (_q.IsInRange(target)) _q.Cast();
        }

        private static void CastE(AIHeroClient target)
        {
            if (!_e.IsReady()) return;
            var Epre = _e.GetPrediction(target);
            if ( Epre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) && _e.IsInRange(target))
                _e.Cast(Epre.CastPosition);
        }

        private static void CastR(AIHeroClient target)
        {
            if (!_r.IsReady() || haspassivebuff(target))
                return;
            if (_r.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield) &&
                _Rmenu.GetValue<MenuBool>(target.CharacterName).Enabled && target.IsUnderEnemyTurret()) _r.Cast(target);
            if (_r.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield) &&
                _Rmenu.GetValue<MenuBool>(target.CharacterName).Enabled && !haspassivebuff(target)) _r.Cast(target);
        }

        private static void SimiR()
        {
            var target = TargetSelector.GetTarget(750,DamageType.Mixed);
            if (!_r.IsReady() || target == null)
                return;
            if (_r.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield)) _r.Cast(target);
        }

        private static void FlashR()
        {
            Orbwalker.Move(Game.CursorPos);
            var target = TargetSelector.GetTarget(950,DamageType.Mixed);
            var flash = Player.GetSpellSlot("summonerflash");
            if (!_r.IsReady() || target == null || Player.Spellbook.CanUseSpell(flash) != SpellState.Ready ||
                !_FlashR.GetValue<MenuBool>(target.CharacterName).Enabled)
                return;
            if (target.DistanceToPlayer() < 750 && !_r.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield))
            {
                Player.Spellbook.CastSpell(flash, target.Position);
                _r.Cast(target);
            }
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_e.Range,DamageType.Mixed);
            if (target == null) return;
            if (target.Health < _e.GetDamage(target) && !target.IsInvulnerable && _menu["KS"].GetValue<MenuBool>("E").Enabled)
            {
                var Epre = _e.GetPrediction(target);
                if (Epre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield))
                    _e.Cast(Epre.CastPosition);
            }
        }

        #endregion

        #region Extra functions

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void Levelup()
        {
            var qLevel = _q.Level;
            var wLevel = _w.Level;
            var eLevel = _e.Level;
            var rLevel = _r.Level;

            if (qLevel + wLevel + eLevel + rLevel >= Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++) level[_spellLevels[i] - 1] = level[_spellLevels[i] - 1] + 1;

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


        private static bool haspassivebuff(AIHeroClient target)
        {
            return target.HasBuff("skarnerpassivebuff");
        }

        #endregion
    }
}