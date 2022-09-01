using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using LeagueSharpCommon;
using OneKeyToWin_AIO_Sebby.Core;
using SharpDX;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;
using Render = LeagueSharpCommon.Render;

namespace UniversalGankAlerter
{
    internal class Program
    {
        private static Program _instance;

        private readonly IDictionary<int, ChampionInfo> _championInfoById = new Dictionary<int, ChampionInfo>();
        private Menu _menu;
        private MenuItem _sliderRadius;
        private PreviewCircle _previewCircle;
        private MenuItem _sliderCooldown;
        private MenuItem _sliderLineDuration;
        private MenuItem _enemyJunglerOnly;
        private MenuItem _allyJunglerOnly;
        private MenuItem _showChampionNames;
        private MenuItem _drawMinimapLines;
        private MenuItem _dangerPing;
        private Menu _enemies;
        private Menu _allies;

        public int Radius
        {
            get { return _sliderRadius.GetValue<MenuSlider>().Value; }
        }

        public int Cooldown
        {
            get { return _sliderCooldown.GetValue<MenuSlider>().Value; }
        }

        public bool DangerPing
        {
            get { return _dangerPing.GetValue<MenuBool>().Enabled; }
        }

        public int LineDuration
        {
            get { return _sliderLineDuration.GetValue<MenuSlider>().Value; }
        }

        public bool EnemyJunglerOnly
        {
            get { return _enemyJunglerOnly.GetValue<MenuBool>().Enabled; }
        }

        public bool AllyJunglerOnly
        {
            get { return _allyJunglerOnly.GetValue<MenuBool>().Enabled; }
        }

        public bool ShowChampionNames
        {
            get { return _showChampionNames.GetValue<MenuBool>().Enabled; }
        }

        public bool DrawMinimapLines
        {
            get { return _drawMinimapLines.GetValue<MenuBool>().Enabled; }
        }

        public static void Loads()
        {
            _instance = new Program();
        }

        public static Program Instance()
        {
            return _instance;
        }

        private Program()
        {
            Game_OnGameLoad(new EventArgs());
        }
        
        private void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                _previewCircle = new PreviewCircle();

                _menu = new Menu("universalgankalerter", "Universal GankAlerter", true);
                _sliderRadius = new MenuSlider("range", "Trigger range", 3000, 500, 5000);
                _sliderRadius.GetValue<MenuSlider>().ValueChanged += SliderRadiusValueChanged;
                _sliderCooldown = new MenuSlider("cooldown", "Trigger cooldown (sec)", 10, 0, 60);
                _sliderLineDuration = new MenuSlider("lineduration", "Line duration (sec)", 10, 0, 20);
                _enemyJunglerOnly = new MenuBool("jungleronly", "Warn jungler only (smite)").SetValue(false);
                _allyJunglerOnly = new MenuBool("allyjungleronly", "Warn jungler only (smite)").SetValue(true);
                _showChampionNames = new MenuBool("shownames", "Show champion name").SetValue(true);
                _drawMinimapLines = new MenuBool("drawminimaplines", "Draw minimap lines").SetValue(false);
                _dangerPing = new MenuBool("dangerping", "Danger Ping (local)").SetValue(false);
                _enemies = new Menu("enemies","Enemies");
                _enemies.Add(_enemyJunglerOnly);

                _allies = new Menu("allies","Allies");
                _allies.Add(_allyJunglerOnly);

                _menu.Add(_sliderRadius);
                _menu.Add(_sliderCooldown);
                _menu.Add(_sliderLineDuration);
                _menu.Add(_showChampionNames);
                _menu.Add(_drawMinimapLines);
                _menu.Add(_dangerPing);
                _menu.Add(_enemies);
                _menu.Add(_allies);
                foreach (AIHeroClient hero in ObjectManager.Get<AIHeroClient>())
                {
                    if (hero.NetworkId != ObjectManager.Player.NetworkId)
                    {
                        if (hero.IsEnemy)
                        {
                            _championInfoById[hero.NetworkId] = new ChampionInfo(hero, false);
                            _enemies.Add(new MenuBool("enemy" + hero.CharacterName, hero.CharacterName).SetValue(true));
                        }
                        else
                        {
                            _championInfoById[hero.NetworkId] = new ChampionInfo(hero, true);
                            _allies.Add(new MenuBool("ally" + hero.CharacterName, hero.CharacterName).SetValue(false));
                        }
                    }
                }

                _menu.Attach();
                Print("Loaded!");
            }catch(Exception){}
        }

        private void SliderRadiusValueChanged(MenuSlider menuitem, EventArgs e)
        {
            _previewCircle.SetRadius(menuitem.Value);
        }
        
        private static void Print(string msg)
        {
            Game.Print(
                "<font color='#ff3232'>Universal</font><font color='#d4d4d4'>GankAlerter:</font> <font color='#FFFFFF'>" +
                msg + "</font>");
        }

        public bool IsEnabled(AIHeroClient hero)
        {
            return hero.IsEnemy
                ? _enemies.GetValue<MenuBool>("enemy" + hero.CharacterName).Enabled
                : _allies.GetValue<MenuBool>("ally" + hero.CharacterName).Enabled;
        }
    }
    
    internal class PreviewCircle
    {
        private const int Delay = 2;

        private float _lastChanged;
        private readonly Render.Circle _mapCircle;
        private int _radius;

        public PreviewCircle()
        {
            try
            {
                Drawing.OnEndScene += Drawing_OnEndScene;

                AppDomain.CurrentDomain.ProcessExit += (sender, args) => this.Unload();

                Drawing.OnPostReset += Drawing_OnPostReset;
                Drawing.OnPreReset += Drawing_OnPreReset;

                GameEvent.OnGameEnd += Game_OnGameEnd;

                _mapCircle = new Render.Circle(ObjectManager.Player, 0, System.Drawing.Color.Red, 5);
                _mapCircle.Add(0);
                _mapCircle.VisibleCondition = sender => _lastChanged > 0 && Game.Time - _lastChanged < Delay;
            }catch(Exception e){}
        }

        private void Game_OnGameEnd()
        {
            _mapCircle.Dispose();
        }

        private void Drawing_OnPreReset(EventArgs args)
        {
            _mapCircle.OnPreReset();
        }

        private void Drawing_OnPostReset(EventArgs args)
        {
            _mapCircle.OnPostReset();
        }

        private void Unload()
        {
            _mapCircle.Dispose();
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (_lastChanged > 0 && Game.Time - _lastChanged < Delay)
            {
                CircleRender.Draw(ObjectManager.Player.Position, _radius, Color.Red, 2);
            }
        }

        public void SetRadius(int radius)
        {
            _radius = radius;
            _mapCircle.Radius = radius;
            _lastChanged = Game.Time;
        }
    
    }
    internal class ChampionInfo
    {
        private static int index = 0;

        private readonly AIHeroClient _hero;

        private event EventHandler OnEnterRange;

        private bool _visible;
        private float _distance;
        private float _lastEnter;
        private float _lineStart;
        private readonly Render.Line _line;

        public ChampionInfo(AIHeroClient hero, bool ally)
        {
            try
            {
                index++;
                int textoffset = index * 50;
                _hero = hero;
                Render.Text text = new Render.Text(
                    new Vector2(), _hero.CharacterName, 20,
                    ally
                        ? new Color {R = 205, G = 255, B = 205, A = 255}
                        : new Color {R = 255, G = 205, B = 205, A = 255})
                {
                    PositionUpdate =
                        () =>
                            Drawing.WorldToScreen(
                                ObjectManager.Player.Position.Extend((Vector2) _hero.Position, 300 + textoffset)),
                    VisibleCondition = delegate
                    {
                        float dist = _hero.Distance(ObjectManager.Player.Position);
                        return Program.Instance().ShowChampionNames && !_hero.IsDead &&
                               Game.Time - _lineStart < Program.Instance().LineDuration &&
                               (!_hero.IsVisible || !Render.OnScreen(Drawing.WorldToScreen(_hero.Position))) &&
                               dist < Program.Instance().Radius && dist > 300 + textoffset;
                    },
                    Centered = true,
                    OutLined = true,
                };
                text.Add(1);
                _line = new Render.Line(
                    new Vector2(), new Vector2(), 5,
                    ally ? new Color {R = 0, G = 255, B = 0, A = 125} : new Color {R = 255, G = 0, B = 0, A = 125})
                {
                    StartPositionUpdate = () => Drawing.WorldToScreen(ObjectManager.Player.Position),
                    EndPositionUpdate = () => Drawing.WorldToScreen(_hero.Position),
                    VisibleCondition =
                        delegate
                        {
                            return !_hero.IsDead && Game.Time - _lineStart < Program.Instance().LineDuration &&
                                   _hero.Distance(ObjectManager.Player.Position) < (Program.Instance().Radius + 1000);
                        }
                };
                _line.Add(0);
                Render.Line minimapLine = new Render.Line(
                    new Vector2(), new Vector2(), 2,
                    ally ? new Color {R = 0, G = 255, B = 0, A = 255} : new Color {R = 255, G = 0, B = 0, A = 255})
                {
                    StartPositionUpdate = () => Drawing.WorldToMinimap(ObjectManager.Player.Position),
                    EndPositionUpdate = () => Drawing.WorldToMinimap(_hero.Position),
                    VisibleCondition =
                        delegate
                        {
                            return Program.Instance().DrawMinimapLines && !_hero.IsDead &&
                                   Game.Time - _lineStart < Program.Instance().LineDuration;
                        }
                };
                minimapLine.Add(0);
                Game.OnUpdate += Game_OnGameUpdate;
                OnEnterRange += ChampionInfo_OnEnterRange;
            }catch(Exception){}
        }

        private void ChampionInfo_OnEnterRange(object sender, EventArgs e)
        {
            bool enabled = false;
            if (Program.Instance().EnemyJunglerOnly && _hero.IsEnemy)
            {
                enabled = IsJungler(_hero);
            }
            else if (Program.Instance().AllyJunglerOnly && _hero.IsAlly)
            {
                enabled = IsJungler(_hero);
            }
            else
            {
                enabled = Program.Instance().IsEnabled(_hero);
            }

            if (Game.Time - _lastEnter > Program.Instance().Cooldown && enabled)
            {
                _lineStart = Game.Time;
                if (Program.Instance().DangerPing && _hero.IsEnemy && !_hero.IsDead)
                {
                    Game.ShowPing(PingCategory.Danger,_hero, true);
                }
            }
            _lastEnter = Game.Time;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            try
            {
                if (ObjectManager.Player.IsDead)
                {
                    return;
                }

                float newDistance = _hero.Distance(ObjectManager.Player);

                if (Game.Time - _lineStart < Program.Instance().LineDuration)
                {
                    float percentage = newDistance / Program.Instance().Radius;
                    if (percentage <= 1)
                    {
                        _line.Width = (int) (2 + (percentage * 8));
                    }
                }

                if (newDistance < Program.Instance().Radius && _hero.IsVisible)
                {
                    if (_distance >= Program.Instance().Radius || !_visible)
                    {
                        if (OnEnterRange != null)
                        {
                            OnEnterRange(this, null);
                        }
                    }
                    else if (_distance < Program.Instance().Radius && _visible)
                    {
                        _lastEnter = Game.Time;
                    }
                }

                _distance = newDistance;
                _visible = _hero.IsVisible;
            }catch(Exception){}
        }

        private bool IsJungler(AIHeroClient hero)
        {
            return hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite"));
        }
    }
}