using System;
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
    internal class Shen
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static Menu _menu, _emotes;
        private static AIHeroClient Player => ObjectManager.Player;
        private static Font _berlinfont;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Shen", "Shen", true); 
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
            };
            _menu.Add(qMenu);
            var wMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
            };
            _menu.Add(wMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EH", "Use E in Harass"),
                new MenuBool("EI", "Use E on interpreter", false),
                new MenuBool("EG", "Use E on gapcloser", false),
                new MenuKeyBind("FeelKey", "Feel Key", Keys.Z, KeyBindType.Press)
            };
            _menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Auto R"),
            };
            var targets = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsAlly
                select hero;
            foreach (var target in targets.Where(x=> x.CharacterName != "Shen"))
                rMenu.Add(new MenuBool(target.CharacterName, "Use R on " + target.CharacterName));
            _menu.Add(rMenu);
            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("QL", "Use Q for last hit"),
            };
            _menu.Add(laneClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("E", "Use E"),

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
            _menu.Add(miscMenu);
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            _menu.Add(drawMenu);


            _menu.Attach();
        }

        #endregion menu

        #region Gamestart

        public Shen()
        {
            _spellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
            _q = new Spell(SpellSlot.Q, 200);
            _w = new Spell(SpellSlot.W,300);
            _e = new Spell(SpellSlot.E, 610);
            _e.SetSkillshot(0f, 50, 500, false, SpellType.Line);
            _r = new Spell(SpellSlot.R, 20000);
            _r.SetTargetted(0.25f,2000);
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
            AIBaseClient.OnDoCast += AIBaseClientOnProcessSpellCast;
            AntiGapcloser.OnGapcloser += (sender, args) =>
            {
                if (_e.IsReady() && sender.IsValidTarget(_e.Range) && _menu["E"].GetValue<MenuBool>("EG").Enabled)
                    _e.Cast(args.EndPosition);
            };
            Interrupter.OnInterrupterSpell += (sender, args) =>
            {
                if (sender.IsEnemy && _e.IsReady() && sender.IsValidTarget(_e.Range) && _menu["E"].GetValue<MenuBool>("EI").Enabled)
                    _e.Cast(sender);
            };
        }

        #endregion

        private static void AIBaseClientOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
          
        }

      
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawE").Enabled)
                Drawing.DrawCircleIndicator(Player.Position, _e.Range, Color.Red);

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
                else if (_q.GetDamage(enemyVisible) + _e.GetDamage(enemyVisible) >
                         enemyVisible.Health)
                    DrawText(_berlinfont, "Killable Skills (Q + E):",
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
            if (_menu["E"].GetValue<MenuKeyBind>("FeelKey").Active) Feel();
           if (_menu["R"].GetValue<MenuBool>("R").Enabled) AutoR();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {

            var target = TargetSelector.GetTarget(_e.Range,DamageType.Physical);
            if (target == null) return;
            var predepos = _e.GetPrediction(target).CastPosition;
            if (_e.IsReady() && _e.IsInRange(target) && _menu["E"].GetValue<MenuBool>("EC").Enabled) _e.Cast(predepos);
            if (_q.IsReady() && _q.IsInRange(target) && _menu["Q"].GetValue<MenuBool>("QC").Enabled) _q.Cast();
            if (_w.IsReady() && Player.Distance(ShenWpos()) < _w.Range && target.Distance(ShenWpos()) < _w.Range && _menu["W"].GetValue<MenuBool>("WC").Enabled) _w.Cast();
        }


        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_e.Range,DamageType.Physical);
            if (target == null) return;
            var predepos = _e.GetPrediction(target).CastPosition;
            if (_e.IsReady() && _e.IsInRange(target) && _menu["E"].GetValue<MenuBool>("EH").Enabled) _e.Cast(predepos);
            if (_q.IsReady() && _q.IsInRange(target) && _menu["Q"].GetValue<MenuBool>("QH").Enabled) _q.Cast();
            if (_w.IsReady() && Player.Distance(ShenWpos()) < _w.Range && target.Distance(ShenWpos()) < _w.Range && _menu["W"].GetValue<MenuBool>("WH").Enabled) _w.Cast();
        }

        private static void LaneClear()
        {
            if (!_menu["LaneClear"].GetValue<MenuBool>("Q").Enabled) return;
            var minion = GameObjects.GetMinions(Player.Position, _q.Range).OrderBy(x => x.DistanceToPlayer())
                .FirstOrDefault();
            if (_q.IsReady() && minion.IsValidTarget(_q.Range)) _q.Cast();
            var jgl = GameObjects.GetJungles(Player.Position, _q.Range).OrderBy(x => x.DistanceToPlayer())
                .FirstOrDefault();
            if (_q.IsReady() && jgl.IsValidTarget(_q.Range)) _q.Cast();
        }
        private static void LastHit()
        {
            if (!_menu["Q"].GetValue<MenuBool>("QL").Enabled) return;
            var minion = GameObjects.GetMinions(Player.Position, _q.Range).OrderBy(x => x.DistanceToPlayer())
                .FirstOrDefault();
            if (minion != null && (_q.IsReady() && minion.IsValidTarget(_q.Range) && minion.Health <= _q.GetDamage(minion) + Player.TotalAttackDamage)) _q.Cast();
        }

        private static void Feel()
        {
            Orbwalker.Move(Game.CursorPos);
            _e.Cast(Game.CursorPos);
        }

        #endregion


        #region Spell Functions

        private static void AutoR()
        {
            if (!_r.IsReady()) return;
            var allies = GameObjects.AllyHeroes;
            if (Player.CountEnemyHeroesInRange(800) >= 1 || !(Player.HealthPercent >= 25)) return;
            foreach (var ally in allies.Where(x => x.IsValidTarget(_r.Range) && x.CharacterName != "Shen" && _menu["R"].GetValue<MenuBool>(x.CharacterName).Enabled))
            {
                if (ally.HealthPercent < 9 && ally.CountEnemyHeroesInRange(650) >= 1 && ally.CountEnemyHeroesInRange(1300) < 3)
                    _r.Cast(ally);
            }

        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_e.Range,DamageType.Physical);
            if (target == null) return;
            if (!(target.Health < _e.GetDamage(target))) return;
            var predepos = _e.GetPrediction(target).CastPosition;
            if (_e.IsReady() && _e.IsInRange(target) && _menu["KS"].GetValue<MenuBool>("E").Enabled) _e.Cast(predepos);
        }

        #endregion

        #region Extra functions

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static Vector3 ShenWpos()
        {
            var a = GameObjects.AllGameObjects.FirstOrDefault(x => x.Name == "ShenSpiritUnit") ?? Player;
            return a.Position;
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

        #endregion
    }
}