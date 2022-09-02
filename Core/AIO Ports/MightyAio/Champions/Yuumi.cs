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
    internal class Yuumi
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu _menu, _emotes;
        private static AIHeroClient Player => ObjectManager.Player;
        private static int _mykills = 0 + Player.ChampionsKilled;
        private static int[] _spellLevels;
        private static Font Berlinfont;
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            _menu = new Menu("Yuumi", "Yuumi", true);
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("Q", "Use Q in Combo"),
                new MenuBool("QA", "Use Q in Auto Play")
            };
            _menu.Add(qMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuSliderButton("E", "Use E || Only when my ally is below", 50)
            };
            _menu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuSliderButton("R", "Use R || if it will hit", 1, 1, 5)
            };
            _menu.Add(rMenu);

            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuKeyBind("AP", "Auto Play", Keys.M,KeyBindType.Toggle),
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 13, 0, 55),
                new MenuBool("autolevel", "Auto Level")
            };
            var allies = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsAlly
                select hero;
            foreach (var ally in allies.Where(x => !x.IsMe))
                miscMenu.Add(new MenuSlider(ally.CharacterName, "Give weight " + ally.CharacterName, 1, 1, 5));
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Spellthiefs Edge", "none"})
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
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("DrawPlay", "Draw Auto Play status")
            };
            _menu.Add(drawMenu);


            _menu.Attach();
        }

        #endregion menu
        

        public Yuumi()
        {
            
            _spellLevels = new[] {1, 3, 3, 1, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2};
            Q = new Spell(SpellSlot.Q, 1150f);
            Q.SetSkillshot(0f, 35f, 1000f, true, SpellType.Line);
            W = new Spell(SpellSlot.W, 700);
            W.SetTargetted(0f, 1200f + 100 * (W.Level - 1));
            E = new Spell(SpellSlot.E) {Delay = 0f};
            R = new Spell(SpellSlot.R, 1100);
            R.SetSkillshot(0.25f, 220f, 1900f, false, SpellType.Line);
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
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnDoCast += AIBaseClientOnOnProcessSpellCast;
            // += OrbwalkerOnOnAction;
        }
        

        /*private void OrbwalkerOnOnAction(object sender, AttackingEventArgs args)
        {
            if (Wativce()) args. = false;
        }

        #endregion*/


        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            var myally = GameObjects.AllyHeroes.FirstOrDefault(x => x.IsValid && x.HasBuff("YuumiWAttach"));
            if (myally == null) return;
            if (sender is AITurretClient && args.Target == myally && E.IsReady()) E.Cast();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_menu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.DarkCyan);
            if (_menu["Drawing"].GetValue<MenuBool>("DrawR").Enabled && R.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, R.Range, Color.Firebrick);
            var drawS = _menu["Drawing"].GetValue<MenuBool>("DrawPlay").Enabled;
            if (drawS)
            {
                if (_menu["Misc"].GetValue<MenuKeyBind>("AP").Active)
                    DrawText(Berlinfont, "Auto Play On",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
                if (!_menu["Misc"].GetValue<MenuKeyBind>("AP").Active)
                    DrawText(Berlinfont, "Auto Play Off",
                        (int) Drawing.WorldToScreen(Player.Position)[0] - 58,
                        (int) Drawing.WorldToScreen(Player.Position)[1] + 30, SharpDX.Color.White);
            }
        }

        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling()) return;
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
                    case "Spellthiefs Edge":
                    {
                        if (time < 1 && Player.InShop())
                        {
                            if (gold >= 500 && !Player.HasItem(3850)) Player.BuyItem((ItemId) 3850);
                            if (gold >= 50 && !Player.HasItem(ItemId.Health_Potion))
                                Player.BuyItem(ItemId.Health_Potion);
                        }

                        break;
                    }
                }

            var getskin = _menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = _menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled;
            if (skin && Player.SkinId != getskin) Player.SetSkin(getskin);

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
            }


            if (_menu["Misc"].GetValue<MenuKeyBind>("AP").Active)
            {
                Hacks.AntiAFK = true;
                AutoPlay();
            }

            if (_menu["Misc"].GetValue<MenuBool>("autolevel").Enabled) Levelup();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (_menu["Q"].GetValue<MenuBool>("Q").Enabled) CastQ();
            CastE();
            CastR();
        }

        private static void AutoPlay()
        {
            if (Wativce())
            { 
                if (!Myally().IsRecalling()) return;
                if (_menu["Q"].GetValue<MenuBool>("QA").Enabled) CastQ();
                 CastE();
                 CastR();
            }

            if (Wativce()) return;
            if (Player.InShop() && Player.ManaPercent <= 95 && Player.HealthPercent <= 95) return;
            var allies = GameObjects.AllyHeroes.Where(x => x.IsValid && !x.IsDead && !x.IsMe).ToList();
            if (!allies.Any()) return;
            foreach (var ally in allies.OrderByDescending(
                x => _menu["Misc"].GetValue<MenuSlider>(x.CharacterName).Value))
            {
                if (W.IsInRange(ally))
                {
                    CastW(ally);
                    return;
                }

                Orbwalker.Move(ally.Position);
                return;
            }
        }

        #endregion


        #region Spell Functions

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            if (target == null) return;
            if (!Q.IsReady()) return;
            var qpre = Q.GetPrediction(target);
            if (qpre.Hitchance >= HitChance.High) Q.Cast(target);
        }


        private static void CastW(AIHeroClient ally)
        {
            if (!W.IsReady() || !W.IsInRange(ally))
                return;
            W.Cast(ally);
        }

        private static void CastE()
        {
            if (!E.IsReady() || !Wativce() || Myally() == null ||
                !_menu["E"].GetValue<MenuSliderButton>("E").Enabled) return;
            if (Myally().HealthPercent <= _menu["E"].GetValue<MenuSliderButton>("E").Value) E.Cast();
        }


        private static void CastR()
        {
            var target = TargetSelector.GetTarget(R.Range,DamageType.Magical);
            if (target == null) return;
            if (!R.IsReady() || !_menu["R"].GetValue<MenuSliderButton>("R").Enabled)
                return;
            var a = _menu["R"].GetValue<MenuSliderButton>("R").Value == 1
                ? -1
                : _menu["R"].GetValue<MenuSliderButton>("R").Value - 1;
            if (R.IsInRange(target) && !target.HasBuffOfType(BuffType.SpellShield))
                R.Cast(target,
                    false, false,  a);
        }

        #endregion

        #region Extra functions

        private static bool Wativce()
        {
            var a = GameObjects.AllyHeroes.Where(x => x.IsValid && x.HasBuff("YuumiWAttach")).ToList();
            if (a.Any()) return true;
            return false;
        }

        private static void Levelup()
        {
            if (Math.Abs(Player.PercentCooldownMod) >= 0.8) return; // if it's urf Don't auto level 
            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

            if (qLevel + wLevel - 1 + eLevel + rLevel >= Player.Level || Player.Level > 18) return;

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
            Game.SendEmote(EmoteId.Laugh);
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

        private static AIHeroClient Myally()
        {
            return GameObjects.AllyHeroes.FirstOrDefault(x => x.IsValid && x.HasBuff("YuumiWAttach"));
        }

        #endregion
    }
}