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
    internal class Chogath
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static Menu _menu, _emotes, _items;
        private static AIHeroClient Player => ObjectManager.Player;
        private static SystemColors _color;
        private static Font _berlinfont;
        private static bool _postAttack;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Chogath", "Chogath", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QM", "Mana for using Q in harass", 40),
                new MenuBool("AQ", "Auto Q on Dasher")
            };
            _menu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
                new MenuSlider("WM", "Mana for using W in harass", 60),
                new MenuBool("AW", "Auto W on Interrupter")
            };
            _menu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EH", "Use E in Harass"),
                new MenuBool("EAA", "Only Use E PostAttack for AutoAttackReset"),
                new MenuSlider("EM", "Mana for using E in harass", 30)
            };
            _menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("RC", "Use R in Combo Only When target is Eatable"),
                new MenuKeyBind("RJ", "Simi R key Only UseAble for jgls", Keys.A, KeyBindType.Press)
            };
            var targets = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsEnemy
                select hero;
            foreach (var target in targets)
                rMenu.Add(new MenuBool(target.CharacterName, "Use R on " + target.CharacterName));
            _menu.Add(rMenu);
            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Q"),
                new MenuSlider("QLC", "Only Use Q if it can hit", 3, 1, 5),
                new MenuBool("W", "W"),
                new MenuBool("E", "E"),
                new MenuSlider("Mana", "Mana for Lane Clear", 40)
            };
            _menu.Add(laneClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W"),
                new MenuBool("E", "Use E")
            };
            _menu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 2, 0, 55),
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
            _items = new Menu("Items", "Items")
            {
                new MenuBool("Stone", "Use Stone Plate in Combo")
            };
            miscMenu.Add(_items);
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

        public Chogath()
        {
            _spellLevels = new[] {1, 3, 1, 2, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
            _q = new Spell(SpellSlot.Q, 950);
            _w = new Spell(SpellSlot.W, 650);
            _e = new Spell(SpellSlot.E, 500);
            _r = new Spell(SpellSlot.R, 175);
            _q.SetSkillshot(1.2f, 250f, float.MinValue, false, SpellType.Circle);
            _w.SetSkillshot(0.5f, 60f, int.MaxValue, false, SpellType.Cone);
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
            Orbwalker.OnAfterAttack += Orbwalker_OnAction;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnProcessSpellCast;
            AntiGapcloser.OnGapcloser += (sender, args) =>
            {
                if (sender.IsEnemy  && _menu["Q"].GetValue<MenuBool>("AQ").Enabled && _q.IsInRange(sender))
                    _q.Cast(args.EndPosition);
            };
            Interrupter.OnInterrupterSpell += (sender, args) =>
            {
                if (sender.IsEnemy && _menu["W"].GetValue<MenuBool>("AW").Enabled && _w.IsInRange(sender)) _w.Cast(sender);
            };
        }

        #endregion

        private void AIBaseClientOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "VorpalSpikes") Orbwalker.ResetAutoAttackTimer();
            }
            
            if (args.Target == null) return;
            if (sender.IsEnemy && args.Target.IsMe && args.Slot == SpellSlot.R &&
                args.SData.TargetingType == SpellDataTargetType.Target)
            {
                _w.Cast(sender);
            }
        }

        private void Orbwalker_OnAction(object sender, AfterAttackEventArgs args)
        {

            var orb = Orbwalker.GetTarget();
            if (orb != null) _postAttack = true;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled)
                Drawing.DrawCircleIndicator(Player.Position, _q.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE").Enabled)
                Drawing.DrawCircleIndicator(Player.Position, _e.Range, Color.Red);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR").Enabled)
                Drawing.DrawCircleIndicator(Player.Position, _r.Range + Player.BoundingRadius, Color.Firebrick);

            var drawKill = _menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities").Enabled;
            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
            {
                if (!enemyVisible.IsValidTarget()) continue;
                var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage);
                var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                if (!drawKill) continue;
                if (_q.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (_q.GetDamage(enemyVisible) + _w.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (_q.GetDamage(enemyVisible) + _w.GetDamage(enemyVisible) + _e.GetDamage(enemyVisible) >
                         enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W + E):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else if (_q.GetDamage(enemyVisible) + _w.GetDamage(enemyVisible) + _e.GetDamage(enemyVisible) +
                    _r.GetDamage(enemyVisible) > enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + W + E + R):",
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                else
                    DrawText(_berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                        (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
            }
        }

        #region gameupdate

        private void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling()) return;
            if (Player.ChampionsKilled > _mykills && _emotes.GetValue<MenuBool>("Kill").Enabled)
            {
                _mykills = Player.ChampionsKilled;
                Emote();
            }

            var getskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = _menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled;
            if (skin && Player.SkinId != getskin) Player.SetSkin(getskin);
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

            if (_menu["R"].GetValue<MenuKeyBind>("RJ").Active) Rdamage();
            KillSteal();
            if (_menu["Misc"].GetValue<MenuBool>("autolevel").Enabled) Levelup();
        }

        #endregion

        #region Orbwalker mod

        private void Combo()
        {
            var target = TargetSelector.GetTarget(_q.Range,DamageType.Mixed);
            var Rmana = !_r.IsReady() ? 0 : 100;
            if (target == null) return;
            // use Stone Plate 
            if (_items.GetValue<MenuBool>("Stone").Enabled && _r.IsReady() && Player.CanUseItem(3193) &&
                target.Health / 1.5 > _r.GetDamage(target) &&
                target.DistanceToPlayer() < _r.Range + Player.BoundingRadius) Player.UseItem(3193);
            if (_r.IsReady() && target.DistanceToPlayer() < _r.Range + Player.BoundingRadius && _r.GetDamage(target) >
                target.Health && !target.HasBuffOfType(BuffType.Invulnerability) &&
                !target.HasBuffOfType(BuffType.SpellShield) && _menu["R"].GetValue<MenuBool>(target.CharacterName).Enabled &&
                _menu["R"].GetValue<MenuBool>("RC").Enabled)
            {
                _r.Cast(target);
            }
            else
            {
                if (_menu["Q"].GetValue<MenuBool>("QC").Enabled && Player.Mana - 60 > Rmana) CastQ(target);
                if (_menu["W"].GetValue<MenuBool>("WC").Enabled &&
                    Player.Mana - 70 + 10 * (_w.Level - 1) > Rmana)
                    CastW(target);
                if (_menu["E"].GetValue<MenuBool>("EC").Enabled && Player.Mana - 30 > Rmana) CastE(target);
            }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(_q.Range,DamageType.Mixed);
            if (target == null) return;
            if (_menu["Q"].GetValue<MenuBool>("QH").Enabled &&
                _menu["Q"].GetValue<MenuSlider>("QM").Value < Player.ManaPercent) CastQ(target);
            if (_menu["W"].GetValue<MenuBool>("WH").Enabled && _menu["W"].GetValue<MenuSlider>("WM").Value < Player.ManaPercent)
                CastW(target);
            if (_menu["E"].GetValue<MenuBool>("EH").Enabled &&
                _menu["E"].GetValue<MenuSlider>("EM").Value < Player.ManaPercent) CastE(target);
        }

        private void LaneClear()
        {
            var minons = GameObjects.GetMinions(Player.Position, _q.Range).Where(x => x.IsValid && !x.IsDead).ToList();
            bool useQ = _menu["LaneClear"].GetValue<MenuBool>("Q").Enabled;
            bool useW = _menu["LaneClear"].GetValue<MenuBool>("W").Enabled;
            bool useE = _menu["LaneClear"].GetValue<MenuBool>("E").Enabled;
            var mana = _menu["LaneClear"].GetValue<MenuSlider>("Mana").Value;
            if (minons.Any())
                if (mana > Player.ManaPercent)
                    return;
            foreach (var minon in minons.OrderBy(x => x.DistanceToPlayer()))
            {
                var laneE = GameObjects.GetMinions(Player.Position, _q.Range + _q.Width);
                var efarmpos = _q.GetCircularFarmLocation(laneE);
                if (useQ && _q.IsReady() &&
                    efarmpos.MinionsHit >= _menu["LaneClear"].GetValue<MenuSlider>("QLC").Value &&
                    _q.IsInRange(minon)) _q.Cast(efarmpos.Position);

                if (useW && _w.IsReady() && _w.IsInRange(minon))
                {
                    var a = _w.GetCircularFarmLocation(minons);
                    if (a.MinionsHit >= 2)
                        _w.Cast(a.Position);
                }

                if (useE && _e.IsReady() && minon.DistanceToPlayer() < Player.GetRealAutoAttackRange()) _e.Cast();
                break;
            }

            var jgls = GameObjects.GetJungles(_q.Range).Where(x => x.IsValid && !x.IsDead).ToList();
            if (jgls.Any())
                foreach (var jgl in jgls.OrderBy(x => x.DistanceToPlayer()))
                {
                    if (useQ && _q.IsReady() && _q.IsInRange(jgl)) _q.Cast(jgl);

                    if (useW && _w.IsReady() && _w.IsInRange(jgl)) _w.Cast(jgl);

                    if (useE && _e.IsReady() && jgl.DistanceToPlayer() < Player.GetRealAutoAttackRange()) _e.Cast();
                    break;
                }
        }

        #endregion


        #region Spell Functions

        private static void CastQ(AIHeroClient target)
        {
            if (!_q.IsReady() || target.HasBuffOfType(BuffType.SpellShield))
                return;
            if (!target.IsMoving || target.IsWindingUp || !target.CanMove)
            {
                _q.Cast(target);
                return;
            }
            var t = _q.GetPrediction(target).CastPosition;
            var x = target.MoveSpeed;
            var y = x * 850 / 1000;
            var pos = target.Position;
            if (target.Distance(t) <= y) pos = t;
            if (target.Distance(t) > y) pos = target.Position.Extend(t, y);
            if (Player.Distance(pos) <= 949 && target.Distance(pos) >= 100) _q.Cast(pos);
            if (Player.Distance(target.Position) <=
                Player.BoundingRadius + Player.AttackRange + target.BoundingRadius) _q.Cast(pos);
            // credits MaddoPls for CastQ Prediction
        }

        private static void CastW(AIHeroClient target)
        {
            if (!_w.IsReady() || !_w.IsInRange(target)) return;
            _w.Cast(target);
        }

        private static void CastE(AIHeroClient target)
        {
            if (!_e.IsReady()) return;
            if (target.DistanceToPlayer() < Player.GetRealAutoAttackRange() && _postAttack)
                _e.Cast();
            _postAttack = false;
        }


        private void KillSteal()
        {
            var target = TargetSelector.GetTarget(_q.Range,DamageType.Mixed);
            if (target == null) return;
            if (target.Health < _q.GetDamage(target) && !target.IsInvulnerable && _menu["KS"].GetValue<MenuBool>("Q").Enabled)
                CastQ(target);
            if (target.Health < _w.GetDamage(target) && !target.IsInvulnerable && _menu["KS"].GetValue<MenuBool>("W").Enabled)
                CastW(target);
            if (target.Health < _e.GetDamage(target) && !target.IsInvulnerable && _menu["KS"].GetValue<MenuBool>("E").Enabled)
                CastE(target);
        }

        #endregion

        #region Extra functions

        private static void Rdamage()
        {
            var targets = GameObjects.GetJungles(Player.Position, _r.Range + Player.BoundingRadius);
            if (targets == null) return;
            foreach (var target in targets.Where(x => x.IsValid && x.MaxHealth > 3800))
                if (1000 + Player.TotalMagicalDamage * 0.5 + Player.BonusHealth * 0.10 > target.Health &&
                    _r.IsReady() &&
                    target.DistanceToPlayer() < _r.Range + Player.BoundingRadius)
                    _r.Cast(target);
        }

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void Levelup()
        {
            if (Math.Abs(Player.PercentCooldownMod) >= 0.8) return; // if it's urf Don't auto level 
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

        #endregion
    }
}