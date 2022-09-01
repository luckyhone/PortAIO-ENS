using System;
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
    internal class Yorick
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static Menu _menu, _emotes;
        private static AIHeroClient Player => ObjectManager.Player;
        private static SystemColors _color;
        private static Font _berlinfont;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        private static bool HasfirstQ => Player.Spellbook.GetSpell(SpellSlot.Q).Name == "YorickQ";
        private static bool HasSecondQ => Player.Spellbook.GetSpell(SpellSlot.Q).Name == "YorickQ2";
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Yorick", "Yorick", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QM", "Mana for using Q in harass", 40)
            };
            _menu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
                new MenuSlider("WHM", "Only Use W in Harass When mana % > ", 40)
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
                new MenuBool("R", "Use R"),
                new MenuSlider("RC", "Use R When there are ",1,1,5),
            };
            _menu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuSliderButton("Q", "Q || Only Use when Mana % >",40),
                new MenuBool("QT","Use Q on Turrents")
            };
            _menu.Add(laneClearMenu);
            var LastHit = new Menu("LastHit", "LastHit")
            {
                new MenuBool("Q", "Q "),
            };
            _menu.Add(LastHit);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("E", "Use E")
            };
            _menu.Add(killSteal);
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 4, 0, 55),
                new MenuBool("autolevel", "Auto Level"),
                new MenuBool("UseW", "Auto W dasher"),
                new MenuBool("UseW2", "Auto W Interrupter")
            };
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Dorans Blade", "Corrupting Potion", "none"})
            };
            _menu.Add(itembuy);
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
                new MenuBool("DrawQ", "Draw Q",false),
                new MenuBool("DrawW", "Draw W",false),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            _menu.Add(drawMenu);


            _menu.Attach();
        }

        #endregion menu

        #region Gamestart

        public Yorick()
        {
            _spellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W,600);
            _w.SetSkillshot(1f,225,float.MaxValue,false,SpellType.Circle);
            _e = new Spell(SpellSlot.E, 700);
            _r = new Spell(SpellSlot.R, 600);
            _r.SetTargetted(0.50f, float.MaxValue);
            _e.SetSkillshot(0.33f, 100f, 500, false, SpellType.Line);
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
            Orbwalker.OnAfterAttack += OrbwalkerOnOnAction;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
            Dash.OnDash += (sender, args) =>
            {
                if (sender.IsEnemy && _w.IsReady() && _menu["Misc"].GetValue<MenuBool>("UseW").Enabled && _w.IsInRange(sender))
                {
                  var wpre = _w.GetPrediction(sender);
                  if (wpre.Hitchance == HitChance.Dash)
                  {
                      _w.Cast(args.EndPos);
                  }
                }
            };
            Interrupter.OnInterrupterSpell += (sender, args) =>
            {
                if (sender.IsEnemy && _w.IsReady() && _menu["Misc"].GetValue<MenuBool>("UseW2").Enabled  && _w.IsInRange(sender))
                {
                    var wpre = _w.GetPrediction(sender);
                    if (wpre.Hitchance >= HitChance.High)
                    {
                        _w.Cast(wpre.UnitPosition);
                    }
                }
            };
        }

       

        #endregion

        private void OrbwalkerOnOnAction(object sender, AfterAttackEventArgs args)
        {
            var target = TargetSelector.GetTarget(Player.GetRealAutoAttackRange() + 50,DamageType.Physical);
            var RManaCost = !_r.IsReady() ? 0 : 100;
            var QManaCost = _q.Level < 1 ? 0 : 25;
            if (_menu["Q"].GetValue<MenuBool>("QC").Enabled && Player.Mana - QManaCost > RManaCost)
            {
                CastQ(target);
            }
            
            
        }
        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "YorickQ")
            {
                Orbwalker.ResetAutoAttackTimer();
            }

            if (sender.IsMe && args.Target is AITurretClient && HasfirstQ && _menu["LaneClear"].GetValue<MenuBool>("QT").Enabled)
            {
                _q.Cast();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled && _q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Player.GetRealAutoAttackRange() + 50, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawW").Enabled && _w.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, _w.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE").Enabled && _e.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, _e.Range, Color.Violet);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR").Enabled && _r.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, _r.Range, Color.Firebrick);

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
            var gold = Player.Gold;
            var time = Game.Time / 60;
            var item = _menu["autoitem"].GetValue<MenuList>("selectitem").SelectedValue;

            if (item != "none" && Game.MapId == GameMapId.SummonersRift)
                switch (item)
                {
                    case "Dorans Blade":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(ItemId.Dorans_Blade)) Player.BuyItem(ItemId.Dorans_Blade);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
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

            var getskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = _menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled;
            if (skin && Player.SkinId != getskin) Player.SetSkin(getskin);
         
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
                    case OrbwalkerMode.LastHit:
                        LastHit();
                        break;
                }
            
                
            KillSteal();
            if (_menu["Misc"].GetValue<MenuBool>("autolevel").Enabled) Levelup();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1500,DamageType.Physical);
            var RManaCost = !_r.IsReady() ? 0 : 100;
            var QManaCost = _q.Level < 1 ? 0 : 25;
            var WManaCost = _w.Level < 1 ? 0 : 70;
            var EManaCost = _e.Level < 1 ? 0 : 50 +  5 * _e.Level;
            if (target == null) return;
            if (_menu["R"].GetValue<MenuBool>("R").Enabled) CastR(target);
            if (_menu["Q"].GetValue<MenuBool>("QC").Enabled && Player.Mana - QManaCost > RManaCost) { CastQ2(target); }
            if (_menu["W"].GetValue<MenuBool>("WC").Enabled && Player.Mana - WManaCost > RManaCost) CastW(target);
            if (_menu["E"].GetValue<MenuBool>("EC").Enabled && Player.Mana - EManaCost > RManaCost) CastE(target);
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_w.Range,DamageType.Physical);
            if (target == null) return;
            if (_menu["Q"].GetValue<MenuBool>("QH").Enabled &&
                _menu["Q"].GetValue<MenuSlider>("QM").Value < Player.ManaPercent) {CastQ(target); CastQ2(target); }
            if (_menu["W"].GetValue<MenuBool>("WH").Enabled &&
                _menu["W"].GetValue<MenuSlider>("WHM").Value < Player.ManaPercent) CastW(target);
            if (_menu["E"].GetValue<MenuBool>("EH").Enabled &&
                _menu["E"].GetValue<MenuSlider>("EM").Value < Player.ManaPercent) CastE(target);
        }

        private static void LaneClear()
        {
        if (!_menu["LaneClear"].GetValue<MenuSliderButton>("Q").Enabled ||
            Player.ManaPercent < _menu["LaneClear"].GetValue<MenuSliderButton>("Q").Value) return;
            var minons = GameObjects.GetMinions(Player.Position, Player.GetRealAutoAttackRange()+ 50)
                .Where(x => x.IsValid && !x.IsDead ).ToList();
            if (minons.Any())

                foreach (var minon in minons.OrderBy(x => x.DistanceToPlayer()))
                {
                    if ( _q.IsReady() &&
                        _q.IsInRange(minon) && _q.GetDamage(minon) >= minon.Health && HasfirstQ)
                    {
                        Orbwalker.Attack(minon);
                        _q.Cast();
                    }
                }
            
        }
        private static void LastHit()
        {
            if (!_menu["LastHit"].GetValue<MenuBool>("Q").Enabled) return;
            var minons = GameObjects.GetMinions(Player.Position, Player.GetRealAutoAttackRange()+ 50)
                .Where(x => x.IsValid && !x.IsDead ).ToList();
            if (minons.Any())

                foreach (var minon in minons.OrderBy(x => x.DistanceToPlayer()))
                {
                    if ( _q.IsReady() &&
                         _q.IsInRange(minon) && _q.GetDamage(minon) >= minon.Health && HasfirstQ)
                    {
                        Orbwalker.Attack(minon);
                        _q.Cast();
                    }
                }
            
        }

        #endregion


        #region Spell Functions

        private static void CastQ(AIHeroClient target)
        {
            if (!_q.IsReady() || target.DistanceToPlayer() > Player.GetRealAutoAttackRange() + 50 )
                return;
            if (HasfirstQ) _q.Cast();
        }

        private static void CastQ2(AIHeroClient target)
        {
            if (_q.IsReady() || !HasSecondQ || target.DistanceToPlayer() > 700 ) 
                return;
            _q.Cast();
        }
        private static void CastW(AIHeroClient target)
        {
            if (!_w.IsReady() || !_w.IsInRange(target))
                return;

            if (!target.IsMoving || target.IsWindingUp)
            {

                var a = _w.GetPrediction(target);
                if (a.Hitchance >= HitChance.High) _w.Cast(a.UnitPosition);
                return;
            }

            var wpre = _w.GetPrediction(target);
            if (wpre.Hitchance >= HitChance.High) _w.Cast(wpre.CastPosition);
        }

        private static void CastE(AIHeroClient target)
        {
            if (!_e.IsReady()) return;
            if (!target.IsMoving || target.IsWindingUp)
            {

                var a = _e.GetPrediction(target);
                if (a.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield)) _e.Cast(a.UnitPosition);
                return;
            }
            var Epre = _e.GetPrediction(target);
            if ( Epre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) && _e.IsInRange(target))
                _e.Cast(Epre.CastPosition);
        }

        private static void CastR(AIHeroClient target)
        {
            if (!_r.IsReady() || Player.CountEnemyHeroesInRange(1000) < _menu["R"].GetValue<MenuSlider>("RC").Value)
             return;
            if (_r.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield)) _r.Cast(target);
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_e.Range,DamageType.Physical);
            if (target == null) return;
            if (target.Health < _e.GetDamage(target) && !target.IsInvulnerable && _menu["KS"].GetValue<MenuBool>("E").Enabled)
            {
               CastE(target);
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