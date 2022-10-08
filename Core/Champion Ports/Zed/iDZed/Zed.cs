using ADCPackage;
using Challenger_Series.Utils;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using iDZed.Utils;
using LeagueSharpCommon;
using SPrediction;
using zedisback;
using AssassinManager = iDZed.Utils.AssassinManager;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = EnsoulSharp.SDK.Render;
using VectorHelper = iDZed.Utils.VectorHelper;

namespace iDZed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;



    using SharpDX;

    using Color = System.Drawing.Color;
    public class Zed
    {
        public static Menu Menu;
        public static readonly SpellDataInst WShadowSpell = Player.Spellbook.GetSpell(SpellSlot.W);

        /// <summary>
        /// TODO The r shadow spell.
        /// </summary>
        private static readonly SpellDataInst RShadowSpell = Player.Spellbook.GetSpell(SpellSlot.R);

        /// <summary>
        /// TODO The e cast range.
        /// </summary>
        public static readonly float ERange = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange - 10;

        /// <summary>
        /// TODO The r cast range.
        /// </summary>
        public static readonly float RRange = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange;
        
        /// <summary>
        /// TODO The _spells.
        /// </summary>
        public static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            {
                SpellSlot.Q, 
                new Spell(SpellSlot.Q, 925f)
            }, 
            {
                SpellSlot.W, 
                new Spell(SpellSlot.W, 550f)
            }, 
            {
                SpellSlot.E, 
                new Spell(SpellSlot.E, ERange)
            }, 
            {
                SpellSlot.R, 
                new Spell(SpellSlot.R, RRange)
            }
        };
            
        private static AIHeroClient Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }
        
        /// <summary>
        /// TODO The _orbwalking modes dictionary.
        /// </summary>
        private static Dictionary<OrbwalkerMode, OnOrbwalkingMode> _orbwalkingModesDictionary;
        
        /// <summary>
        /// TODO The on orbwalking mode.
        /// </summary>
        private delegate void OnOrbwalkingMode();
        
        #region Public Methods and Operators

        /// <summary>
        /// TODO The on load.
        /// </summary>
        public static void OnLoad()
        {
            if (Player.CharacterName != "Zed")
            {
                return;
            }

            Game.Print("iDZed loaded!");
            ShadowManager.OnLoad();
            _orbwalkingModesDictionary = new Dictionary<OrbwalkerMode, OnOrbwalkingMode>
            {
                { OrbwalkerMode.Combo, Combo }, 
                { OrbwalkerMode.Harass, Harass }, 
                { OrbwalkerMode.LastHit, LastHit }, 
                { OrbwalkerMode.LaneClear, Laneclear }, 
                { OrbwalkerMode.None, () => { } }
            };
            InitMenu();
            InitSpells();
            InitEvents();
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// TODO The cast e.
        /// </summary>
        private static void CastE()
        {
            if (!_spells[SpellSlot.E].IsReady())
            {
                return;
            }

            if (
                GameObjects.EnemyHeroes.Count(
                    hero =>
                    hero.IsValidTarget()
                    && (hero.Distance(Player.ServerPosition) <= _spells[SpellSlot.E].Range
                        || (ShadowManager.WShadow.ShadowObject != null
                            && hero.Distance(ShadowManager.WShadow.Position) <= _spells[SpellSlot.E].Range)
                        || (ShadowManager.RShadow.ShadowObject != null
                            && hero.Distance(ShadowManager.RShadow.Position) <= _spells[SpellSlot.E].Range))) > 0)
            {
                _spells[SpellSlot.E].Cast();
            }
        }

        /// <summary>
        /// TODO The cast q.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        private static void CastQ(AIHeroClient target)
        {
            if (_spells[SpellSlot.Q].IsReady())
            {
                if (GetMarkedTarget() != null)
                {
                    target = GetMarkedTarget();
                }

                if (ShadowManager.WShadow.Exists
                    && ShadowManager.WShadow.ShadowObject.Distance(target.ServerPosition)
                    < Player.Distance(target.ServerPosition))
                {
                    _spells[SpellSlot.Q].UpdateSourcePosition(
                        ShadowManager.WShadow.Position, 
                        ShadowManager.WShadow.Position);
                    if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.useqpred"].GetValue<MenuBool>().Enabled)
                    {
                        var prediction = _spells[SpellSlot.Q].GetPrediction(target);
                        if (prediction.Hitchance >= GetHitchance())
                        {
                            if (ShadowManager.WShadow.ShadowObject.Distance(target) <= _spells[SpellSlot.Q].Range)
                            {
                                _spells[SpellSlot.Q].Cast(prediction.CastPosition);
                            }
                        }
                    }
                    else
                    {
                        if (ShadowManager.WShadow.ShadowObject.Distance(target) <= _spells[SpellSlot.Q].Range)
                        {
                            _spells[SpellSlot.Q].Cast(target.ServerPosition);
                        }
                    }
                }
                else if (ShadowManager.RShadow.Exists
                         && ShadowManager.RShadow.ShadowObject.Distance(target.ServerPosition)
                         < Player.Distance(target.ServerPosition))
                {
                    _spells[SpellSlot.Q].UpdateSourcePosition(
                        ShadowManager.RShadow.Position, 
                        ShadowManager.RShadow.Position);
                    if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.useqpred"].GetValue<MenuBool>().Enabled)
                    {
                        var prediction = _spells[SpellSlot.Q].GetPrediction(target);
                        if (prediction.Hitchance >= GetHitchance())
                        {
                            if (ShadowManager.RShadow.ShadowObject.Distance(target) <= _spells[SpellSlot.Q].Range)
                            {
                                _spells[SpellSlot.Q].Cast(prediction.CastPosition);
                            }
                        }
                    }
                    else
                    {
                        if (ShadowManager.RShadow.ShadowObject.Distance(target) <= _spells[SpellSlot.Q].Range)
                        {
                            _spells[SpellSlot.Q].Cast(target.ServerPosition);
                        }
                    }
                }
                else
                {
                    _spells[SpellSlot.Q].UpdateSourcePosition(Player.ServerPosition, Player.ServerPosition);
                    if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.useqpred"].GetValue<MenuBool>().Enabled)
                    {
                        var prediction = _spells[SpellSlot.Q].GetPrediction(target);
                        if (prediction.Hitchance >= GetHitchance())
                        {
                            if (Player.Distance(target) <= _spells[SpellSlot.Q].Range
                                && target.IsValidTarget(_spells[SpellSlot.Q].Range))
                            {
                                _spells[SpellSlot.Q].Cast(prediction.CastPosition);
                            }
                        }
                    }
                    else
                    {
                        if (Player.Distance(target) <= _spells[SpellSlot.Q].Range
                            && target.IsValidTarget(_spells[SpellSlot.Q].Range))
                        {
                            _spells[SpellSlot.Q].Cast(target.ServerPosition);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// TODO The cast w.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        private static void CastW(AIHeroClient target)
        {
            if (!HasEnergy(new[] { SpellSlot.W, SpellSlot.Q }))
            {
                return;
            }

            if (ShadowManager.WShadow.IsUsable)
            {
                if (_spells[SpellSlot.W].IsReady() && WShadowSpell.ToggleState == 0
                    && Environment.TickCount - _spells[SpellSlot.W].LastCastAttemptTime > 0)
                {
                    var position = Player.ServerPosition.ToVector2()
                        .Extend(target.ServerPosition.ToVector2(), _spells[SpellSlot.W].Range);
                    if (position.Distance(target) <= _spells[SpellSlot.Q].Range)
                    {
                        if (IsPassWall(Player.ServerPosition, target.ServerPosition))
                        {
                            return;
                        }

                        _spells[SpellSlot.W].Cast(position);
                        _spells[SpellSlot.W].LastCastAttemptTime = Environment.TickCount + 500;
                    }
                }
            }

            if (ShadowManager.CanGoToShadow(ShadowManager.WShadow) && WShadowSpell.ToggleState == (SpellToggleState)2)
            {
                if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.swapw"].GetValue<MenuBool>().Enabled
                    && ShadowManager.WShadow.ShadowObject.Distance(target.ServerPosition)
                    < Player.Distance(target.ServerPosition))
                {
                    _spells[SpellSlot.W].Cast();
                }
            }
        }

        /// <summary>
        /// TODO The combo.
        /// </summary>
        private static void Combo()
        {
            var target = GetAssasinationTarget();
            if(target == null ) return;

            switch (Menu["com.idz.zed.combo"]["com.idz.zed.combo.mode"].GetValue<MenuList>().Index)
            {
                case 0: // Line mode
                    if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.user"].GetValue<MenuBool>().Enabled && _spells[SpellSlot.R].IsReady()
                        && (target.Health + 20
                            >= _spells[SpellSlot.Q].GetDamage(target) + _spells[SpellSlot.E].GetDamage(target)
                            + ObjectManager.Player.GetAutoAttackDamage(target)))
                    {
                        if (!HasEnergy(new[] { SpellSlot.W, SpellSlot.R, SpellSlot.Q, SpellSlot.E }))
                        {
                            return;
                        }

                        if (ShadowManager.WShadow.Exists)
                        {
                            CastQ(target);
                            CastE();
                        }
                        else
                        {
                            DoLineCombo(target);
                        }
                    }
                    else
                    {
                        DoNormalCombo(target);
                    }

                    break;
                case 1: // triangle mode
                    if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.user"].GetValue<MenuBool>().Enabled && _spells[SpellSlot.R].IsReady()
                        && (target.Health + 20
                            >= _spells[SpellSlot.Q].GetDamage(target) + _spells[SpellSlot.E].GetDamage(target)
                            + ObjectManager.Player.GetAutoAttackDamage(target)))
                    {
                        if (!HasEnergy(new[] { SpellSlot.W, SpellSlot.R }))
                        {
                            return;
                        }

                        if (ShadowManager.WShadow.Exists)
                        {
                            CastQ(target);
                            CastE();
                        }
                        else
                        {
                            DoTriangleCombo(target);
                        }
                    }
                    else
                    {
                        DoNormalCombo(target);
                    }

                    break;
            }
        }

        /// <summary>
        /// TODO The do line combo.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        private static void DoLineCombo(AIHeroClient target)
        {
            if (ShadowManager.RShadow.IsUsable)
            {
                if (Menu["com.idz.zed.misc"]["checkQWE"].GetValue<MenuBool>().Enabled)
                {
                    if (_spells[SpellSlot.Q].IsReady() && _spells[SpellSlot.W].IsReady()
                        && _spells[SpellSlot.E].IsReady())
                    {
                        if (_spells[SpellSlot.R].IsReady() && _spells[SpellSlot.R].IsInRange(target))
                        {
                            _spells[SpellSlot.R].Cast(target);
                        }
                    }
                }
                else
                {
                    if (_spells[SpellSlot.R].IsReady() && _spells[SpellSlot.R].IsInRange(target))
                    {
                        _spells[SpellSlot.R].Cast(target);
                    }
                }
            }

            if (GetMarkedTarget() != null)
            {
                target = GetMarkedTarget();
            }

            Activator.ItemManager.UseDeathmarkItems();
            Activator.ItemManager.UseSummonerSpells();

            if (ShadowManager.RShadow.Exists && ShadowManager.WShadow.IsUsable)
            {
                var wCastLocation = Player.ServerPosition
                                    - Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 400;

                if (ShadowManager.WShadow.IsUsable && WShadowSpell.ToggleState == 0
                    && Environment.TickCount - _spells[SpellSlot.W].LastCastAttemptTime > 0)
                {
                    _spells[SpellSlot.W].Cast(wCastLocation);

                    // Maybe add a delay giving the target a chance to flash / zhonyas then it will place w at best location for more damage
                    _spells[SpellSlot.W].LastCastAttemptTime = Environment.TickCount + 500;
                }
            }

            if (ShadowManager.WShadow.Exists && ShadowManager.RShadow.Exists)
            {
                CastQ(target);
                CastE();
            }
            else if (ShadowManager.RShadow.Exists && !ShadowManager.WShadow.IsUsable && !ShadowManager.WShadow.Exists)
            {
                CastQ(target);
                CastE();
            }

            if (ShadowManager.CanGoToShadow(ShadowManager.WShadow) && WShadowSpell.ToggleState == (SpellToggleState)2)
            {
                // && !_deathmarkKilled)
                if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.swapw"].GetValue<MenuBool>().Enabled
                    && ShadowManager.WShadow.ShadowObject.Distance(target.ServerPosition)
                    < Player.Distance(target.ServerPosition))
                {
                    _spells[SpellSlot.W].Cast();
                }
            }
        }

        /// <summary>
        /// TODO The do normal combo.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        private static void DoNormalCombo(AIHeroClient target)
        {
            if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.usew"].GetValue<MenuBool>().Enabled
                && (_spells[SpellSlot.Q].IsReady() || _spells[SpellSlot.E].IsReady()))
            {
                CastW(target);
                if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.useq"].GetValue<MenuBool>().Enabled)
                {
                    DelayAction.Add(105, () => CastQ(target));
                }

                if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.usee"].GetValue<MenuBool>().Enabled)
                {
                    DelayAction.Add(105, CastE);
                }
            }
            else
            {
                CastQ(target);
                CastE();
            }
        }

        /// <summary>
        /// TODO The do triangle combo.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        private static void DoTriangleCombo(AIHeroClient target)
        {
            // I'm dumb, this triangular combo is only good for targets the Zhonyas, we can still use it for that i guess :^)
            if (ShadowManager.RShadow.IsUsable && !target.HasBuffOfType(BuffType.Invulnerability))
            {
                // Cast Ultimate m8 :S
                if (Menu["com.idz.zed.misc"]["checkQWE"].GetValue<MenuBool>().Enabled)
                {
                    if (_spells[SpellSlot.Q].IsReady() && _spells[SpellSlot.W].IsReady()
                        && _spells[SpellSlot.E].IsReady())
                    {
                        if (_spells[SpellSlot.R].IsReady() && _spells[SpellSlot.R].IsInRange(target))
                        {
                            _spells[SpellSlot.R].Cast(target);
                        }
                    }
                }
                else
                {
                    if (_spells[SpellSlot.R].IsReady() && _spells[SpellSlot.R].IsInRange(target))
                    {
                        _spells[SpellSlot.R].Cast(target);
                    }
                }
            }

            if (GetMarkedTarget() != null)
            {
                target = GetMarkedTarget();
            }

            Activator.ItemManager.UseDeathmarkItems();
            Activator.ItemManager.UseSummonerSpells();

            if (ShadowManager.RShadow.Exists && ShadowManager.WShadow.IsUsable)
            {
                var bestWPosition = VectorHelper.GetBestPosition(
                    target, 
                    VectorHelper.GetVertices(target)[0], 
                    VectorHelper.GetVertices(target)[1]);

                // Maybe add a delay giving the target a chance to flash / zhonyas then it will place w at best perpendicular location m8
                if (WShadowSpell.ToggleState == 0 && Environment.TickCount - _spells[SpellSlot.W].LastCastAttemptTime > 0)
                {
                    _spells[SpellSlot.W].Cast(bestWPosition);

                    // Allow half a second for the target to flash / zhonyas? :S
                    _spells[SpellSlot.W].LastCastAttemptTime = Environment.TickCount + 500;
                }
            }

            if (WShadowSpell.ToggleState == (SpellToggleState)2)
            {
                _spells[SpellSlot.W].Cast();
            }

            if (ShadowManager.WShadow.Exists && ShadowManager.RShadow.Exists)
            {
                CastQ(target);
                CastE();
            }
            else if (ShadowManager.RShadow.Exists && !ShadowManager.WShadow.IsUsable && !ShadowManager.WShadow.Exists)
            {
                CastQ(target);
                CastE();
            }
        }

        /// <summary>
        /// TODO The drawing_ on draw.
        /// </summary>
        /// <param name="args">
        /// TODO The args.
        /// </param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu["com.idz.zed.drawing"]["drawShadows"].GetValue<MenuBool>().Enabled)
            {
                foreach (var shadow in
                    ShadowManager._shadowsList.Where(sh => sh.State != ShadowState.NotActive && sh.ShadowObject != null)
                    )
                {
                    CircleRender.Draw(shadow.Position, 60f, Color.Orange.ToSharpDxColor());
                }
            }

            foreach (var spell in
                _spells.Where(
                    s => Menu["com.idz.zed.drawing"]["com.idz.zed.drawing.draw" + GetStringFromSpellSlot(s.Key)].GetValue<MenuBool>().Enabled)
                )
            {
                var value = Menu["com.idz.zed.drawing"]["com.idz.zed.drawing.draw" + GetStringFromSpellSlot(spell.Key)].GetValue<MenuBool>().Enabled;

                CircleRender.Draw(
                    Player.Position, 
                    spell.Value.Range, 
                    Color.Aqua.ToSharpDxColor());
            }
        }

        /// <summary>
        /// TODO The game_ on update.
        /// </summary>
        /// <param name="args">
        /// TODO The args.
        /// </param>
        private static void Game_OnUpdate(EventArgs args)
        {
            OnFlee();

            _orbwalkingModesDictionary[Orbwalker.ActiveMode]();
        }

        /// <summary>
        /// TODO The get assasination target.
        /// </summary>
        /// <param name="range">
        /// TODO The range.
        /// </param>
        /// <param name="damageType">
        /// TODO The damage type.
        /// </param>
        /// <returns>
        /// </returns>
        private static AIHeroClient GetAssasinationTarget(
            float range = 0, 
            DamageType damageType = DamageType.Physical)
        {
            if (Math.Abs(range) < 0.00001)
            {
                range = _spells[SpellSlot.R].IsReady()
                            ? _spells[SpellSlot.R].Range
                            : _spells[SpellSlot.W].Range + _spells[SpellSlot.Q].Range / 2f;
            }

            if (!Menu["MenuAssassin"]["AssassinActive"].GetValue<MenuBool>().Enabled)
            {
                return TargetSelector.GetTarget(range, damageType);
            }

            var assassinRange = Menu["MenuAssassin"]["AssassinSearchRange"].GetValue<MenuSlider>().Value;

            var vEnemy =
                GameObjects.EnemyHeroes.Where(
                    enemy =>
                    enemy.Team != Player.Team && !enemy.IsDead && enemy.IsVisible
                    && Menu["MenuAssassin"]["Assassin" + enemy.CharacterName] != null
                   && Menu["MenuAssassin"]["Assassin" + enemy.CharacterName].GetValue<MenuBool>().Enabled
                    && Player.Distance(enemy) < assassinRange);

            if (Menu["MenuAssassin"]["AssassinSelectOption"].GetValue<MenuList>().Index == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            var objAiHeroes = vEnemy as AIHeroClient[] ?? vEnemy.ToArray();

            var target = !objAiHeroes.Any() ? TargetSelector.GetTarget(range, damageType) : objAiHeroes[0];

            return target;
        }

        /// <summary>
        /// TODO The get hitchance.
        /// </summary>
        /// <returns>
        /// </returns>
        private static HitChance GetHitchance()
        {
            switch (Menu["com.idz.zed.misc"]["com.idz.zed.misc.hitchance"].GetValue<MenuList>().Index)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        /// <summary>
        /// TODO The get marked target.
        /// </summary>
        /// <returns>
        /// </returns>
        private static AIHeroClient GetMarkedTarget()
        {
            return
                GameObjects.EnemyHeroes.FirstOrDefault(
                    x =>
                    x.IsValidTarget(_spells[SpellSlot.W].Range + _spells[SpellSlot.Q].Range)
                    && x.HasBuff("zedulttargetmark") && x.IsVisible);
        }

        /// <summary>
        /// TODO The get string from spell slot.
        /// </summary>
        /// <param name="sp">
        /// TODO The sp.
        /// </param>
        /// <returns>
        /// </returns>
        private static string GetStringFromSpellSlot(SpellSlot sp)
        {
            switch (sp)
            {
                case SpellSlot.Q:
                    return "Q";
                case SpellSlot.W:
                    return "W";
                case SpellSlot.E:
                    return "E";
                case SpellSlot.R:
                    return "R";
                default:
                    return "unk";
            }
        }

        /// <summary>
        /// TODO The harass.
        /// </summary>
        private static void Harass()
        {
            if (!Menu["com.idz.zed.harass"]["com.idz.zed.harass.useHarass"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            var target = TargetSelector.GetTarget(
                _spells[SpellSlot.W].Range + _spells[SpellSlot.Q].Range, 
                DamageType.Physical);
            if(target == null) return;
            switch (Menu["com.idz.zed.harass"]["com.idz.zed.harass.harassMode"].GetValue<MenuList>().Index)
            {
                case 0: // "W-E-Q"
                    if (_spells[SpellSlot.W].IsReady() && ShadowManager.WShadow.IsUsable
                        && WShadowSpell.ToggleState == 0
                        && Environment.TickCount - _spells[SpellSlot.W].LastCastAttemptTime > 0
                        && Player.Distance(target) <= _spells[SpellSlot.W].Range + 300
                        && _spells[SpellSlot.Q].IsReady())
                    {
                        _spells[SpellSlot.W].Cast(target.ServerPosition);
                        _spells[SpellSlot.W].LastCastAttemptTime = Environment.TickCount + 500;
                    }
                    else if ((!_spells[SpellSlot.W].IsReady() || WShadowSpell.ToggleState != 0) && _spells[SpellSlot.Q].IsReady() && target.Distance(Player) < _spells[SpellSlot.Q].Range)
                    {
                        if (Menu["com.idz.zed.harass"]["fast.harass"].GetValue<MenuBool>().Enabled)
                        {
                            _spells[SpellSlot.Q].Cast(target.Position);
                        }
						else
						{
							if (_spells[SpellSlot.Q].GetPrediction(target).Hitchance < HitChance.High)
                            {
                                _spells[SpellSlot.Q].Cast(target.Position);
                            }
						}
                    }
                    else if (WShadowSpell.ToggleState != 0 && !_spells[SpellSlot.Q].IsReady() && _spells[SpellSlot.E].IsReady())
                    {
                        foreach (var shdw in ShadowManager._shadowsList.Where(x => x.Type == ShadowType.Normal))
                        {
                            _spells[SpellSlot.E].Cast();
                        }
                    }
                    

                    break;
            }
        }

        /// <summary>
        /// TODO The has energy.
        /// </summary>
        /// <param name="spells">
        /// TODO The spells.
        /// </param>
        /// <returns>
        /// </returns>
        private static bool HasEnergy(IEnumerable<SpellSlot> spells)
        {
            if (!Menu["com.idz.zed.misc"]["energyManagement"].GetValue<MenuBool>().Enabled)
            {
                return true;
            }

            var totalCost = spells.Sum(slot => Player.Spellbook.GetSpell(slot).ManaCost);
            return Player.Mana >= totalCost;
        }

        /// <summary>
        /// TODO The init events.
        /// </summary>
        private static void InitEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreateObject;
            AIBaseClient.OnDoCast += OnSpellCast;
        }

        /// <summary>
        /// TODO The init menu.
        /// </summary>
        private static void InitMenu()
        {
            Menu = new Menu("com.idz.zed","iDZed - Reworked", true);

            // ReSharper disable once ObjectCreationAsStatement
            new AssassinManager();

            var comboMenu = new Menu("com.idz.zed.combo",":: Combo");
            {
                comboMenu.Add(new MenuBool("com.idz.zed.combo.useq", "Use Q").SetValue(true));
                comboMenu.Add(
                    new MenuBool("com.idz.zed.combo.useqpred", "Q Prediction: On = slower, off = faster").SetValue(
                        false));
                comboMenu.Add(new MenuBool("com.idz.zed.combo.usew", "Use W").SetValue(true));
                comboMenu.Add(new MenuBool("com.idz.zed.combo.usee", "Use E").SetValue(true));
                comboMenu.Add(new MenuBool("com.idz.zed.combo.user", "Use R").SetValue(true));
                comboMenu.Add(new MenuBool("com.idz.zed.combo.swapw", "Swap W For Follow").SetValue(false));
                comboMenu.Add(new MenuBool("com.idz.zed.combo.swapr", "Swap R On kill").SetValue(true));
                comboMenu.Add(
                    new MenuList("com.idz.zed.combo.mode", "Combo Mode",new[] { "Line Mode", "Triangle Mode" }));
            }
            Menu.Add(comboMenu);

            var harassMenu = new Menu("com.idz.zed.harass",":: Harass");
            {
                harassMenu.Add(new MenuBool("com.idz.zed.harass.useHarass", "Use Harass").SetValue(true));
                harassMenu.Add(new MenuBool("fast.harass", "Q Prediction: On = slower, off = faster").SetValue(false));
                harassMenu.Add(
                    new MenuList("com.idz.zed.harass.harassMode", "Harass Mode",new[] { "W-E-Q" }));
            }

            Menu.Add(harassMenu);

            var lastHitMenu = new Menu("com.idz.zed.lasthit",":: LastHit");
            {
                lastHitMenu.Add(new MenuBool("com.idz.zed.lasthit.useQ", "Use Q in LastHit").SetValue(true));
                lastHitMenu.Add(new MenuBool("com.idz.zed.lasthit.useE", "Use E in LastHit").SetValue(true));
            }

            Menu.Add(lastHitMenu);

            var laneclearMenu = new Menu("com.idz.zed.laneclear",":: Laneclear");
            {
                laneclearMenu.Add(new MenuBool("com.idz.zed.laneclear.useQ", "Use Q in laneclear").SetValue(true));
                laneclearMenu.Add(
                    new MenuSlider("com.idz.zed.laneclear.qhit", "Min minions for Q",3, 1, 10));
                laneclearMenu.Add(new MenuBool("com.idz.zed.laneclear.useE", "Use E in laneclear").SetValue(true));
                laneclearMenu.Add(
                    new MenuSlider("com.idz.zed.laneclear.ehit", "Min minions for E",3, 1, 10));
            }

            Menu.Add(laneclearMenu);

            var drawMenu = new Menu("com.idz.zed.drawing",":: Drawing");
            {
                foreach (var slot in _spells.Select(entry => entry.Key))
                {
                    drawMenu.Add(
                        new MenuBool(
                            "com.idz.zed.drawing.draw" + GetStringFromSpellSlot(slot), 
                            "Draw " + GetStringFromSpellSlot(slot) + " Range"));
                }

                drawMenu.Add(new MenuBool("drawShadows", "Draw Shadows").SetValue(true));
            }

            Menu.Add(drawMenu);

            var fleeMenu = new Menu("com.idz.zed.flee",":: Flee");
            {
                fleeMenu.Add(
                    new MenuKeyBind("fleeActive", "Flee Key",Keys.P, KeyBindType.Press));
                fleeMenu.Add(new MenuBool("autoEFlee", "Auto E when fleeing").SetValue(true));
            }

            Menu.Add(fleeMenu);

            var miscMenu = new Menu("com.idz.zed.misc",":: Misc");
            {
                miscMenu.Add(new MenuBool("energyManagement", "Use Energy Management").SetValue(true));
                miscMenu.Add(new MenuBool("safetyChecks", "Check Safety for shadow swapping").SetValue(true));
                miscMenu.Add(
                    new MenuList("com.idz.zed.misc.hitchance", "Q Hitchance",new[] { "Low", "Medium", "High", "Very High" }, 2));
                miscMenu.Add(new MenuBool("checkQWE", "Check Other Spells before ult").SetValue(true));
            }

            Menu.Add(miscMenu);

            Activator.ItemManager.OnLoad(Menu);
            ZedEvader.OnLoad(Menu);

            Menu.Attach();
        }

        /// <summary>
        /// TODO The init spells.
        /// </summary>
        private static void InitSpells()
        {
            _spells[SpellSlot.Q].SetSkillshot(0.25F, 50F, 1600F, false, SpellType.Line);
            _spells[SpellSlot.W].SetSkillshot(0.75F, 75F, 1000F, false, SpellType.Circle);
        }

        /// <summary>
        /// TODO The is pass wall.
        /// </summary>
        /// <param name="start">
        /// TODO The start.
        /// </param>
        /// <param name="end">
        /// TODO The end.
        /// </param>
        /// <returns>
        /// </returns>
        private static bool IsPassWall(Vector3 start, Vector3 end)
        {
            double count = Vector3.Distance(start, end);
            for (uint i = 0; i <= count; i += 25)
            {
                var pos = start.ToVector2().Extend(Player.ServerPosition.ToVector2(), -i);
                if (Vector2Extensions.IsWall(pos))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// TODO The laneclear.
        /// </summary>
        private static void Laneclear()
        {
            var allMinionsQ = MinionManager.GetMinions(
                Player.ServerPosition, 
                _spells[SpellSlot.Q].Range, 
                MinionManager.MinionTypes.All, 
                MinionManager.MinionTeam.NotAlly);
            var allMinionsE = MinionManager.GetMinions(
                Player.ServerPosition, 
                _spells[SpellSlot.Q].Range, 
                MinionManager.MinionTypes.All, 
                MinionManager.MinionTeam.NotAlly);
            if (Menu["com.idz.zed.laneclear"]["com.idz.zed.laneclear.useQ"].GetValue<MenuBool>().Enabled && _spells[SpellSlot.Q].IsReady()
                && !Player.Spellbook.IsAutoAttack)
            {
                var bestPositionQ =
                    FarmPrediction.GetBestLineFarmLocation(
                        allMinionsQ.Select(x => x.ServerPosition.ToVector2()).ToList(), 
                        _spells[SpellSlot.Q].Width, 
                        _spells[SpellSlot.Q].Range);
                if (bestPositionQ.MinionsHit >= Menu["com.idz.zed.laneclear"]["com.idz.zed.laneclear.qhit"].GetValue<MenuSlider>().Value)
                {
                    _spells[SpellSlot.Q].Cast(bestPositionQ.Position);
                }
            }

            if (Menu["com.idz.zed.laneclear"]["com.idz.zed.laneclear.useE"].GetValue<MenuBool>().Enabled && _spells[SpellSlot.E].IsReady()
                && !Player.Spellbook.IsAutoAttack)
            {
                var eLocation =
                    FarmPrediction.GetBestLineFarmLocation(
                        allMinionsE.Select(x => x.ServerPosition.ToVector2()).ToList(), 
                        _spells[SpellSlot.E].Width, 
                        _spells[SpellSlot.E].Range);
                if (eLocation.MinionsHit >= Menu["com.idz.zed.laneclear"]["com.idz.zed.laneclear.ehit"].GetValue<MenuSlider>().Value)
                {
                    _spells[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        /// TODO The last hit.
        /// </summary>
        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, 1000f, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);
            if (Menu["com.idz.zed.lasthit"]["com.idz.zed.lasthit.useQ"].GetValue<MenuBool>().Enabled && _spells[SpellSlot.Q].IsReady())
            {
                var qMinion =
                    allMinions.FirstOrDefault(
                        x => _spells[SpellSlot.Q].IsInRange(x) && x.IsValidTarget(_spells[SpellSlot.Q].Range));

                if (qMinion != null && _spells[SpellSlot.Q].GetDamage(qMinion) > qMinion.Health
                    && !Player.InAutoAttackRange(qMinion))
                {
                    _spells[SpellSlot.Q].Cast(qMinion);
                }
            }

            if (Menu["com.idz.zed.lasthit"]["com.idz.zed.lasthit.useE"].GetValue<MenuBool>().Enabled && _spells[SpellSlot.E].IsReady())
            {
                var minions =
                    MinionManager.GetMinions(
                        Player.ServerPosition, 
                        _spells[SpellSlot.E].Range, 
                        MinionManager.MinionTypes.All, 
                        MinionManager.MinionTeam.NotAlly)
                        .FindAll(
                            minion =>
                            !Player.InAutoAttackRange(minion)
                            && minion.Health < 0.75 * _spells[SpellSlot.E].GetDamage(minion));
                if (minions.Count >= 1)
                {
                    _spells[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        /// TODO The on create object.
        /// </summary>
        /// <param name="sender">
        /// TODO The sender.
        /// </param>
        /// <param name="args">
        /// TODO The args.
        /// </param>
        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (!(sender is EffectEmitter))
            {
            }

            if (Menu["com.idz.zed.combo"]["com.idz.zed.combo.swapr"].GetValue<MenuBool>().Enabled)
            {
                if (sender.Name == "Zed_Base_R_buf_tell.troy")
                {
                    // _deathmarkKilled = true;
                    if (RShadowSpell.ToggleState == (SpellToggleState)2 && ShadowManager.CanGoToShadow(ShadowManager.RShadow))
                    {
                        _spells[SpellSlot.R].Cast();
                    }
                }
            }
        }

        /// <summary>
        /// TODO The on flee.
        /// </summary>
        private static void OnFlee()
        {
            if (!Menu["com.idz.zed.flee"]["fleeActive"].GetValue<MenuKeyBind>().Active)
            {
                return;
            }

            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (_spells[SpellSlot.W].IsReady() && ShadowManager.WShadow.IsUsable)
            {
                _spells[SpellSlot.W].Cast(Game.CursorPos);
            }

            if (ShadowManager.WShadow.Exists && ShadowManager.CanGoToShadow(ShadowManager.WShadow))
            {
                _spells[SpellSlot.W].Cast();
            }

            CastE();
        }

        /// <summary>
        /// TODO The on spell cast.
        /// </summary>
        /// <param name="sender1">
        /// TODO The sender 1.
        /// </param>
        /// <param name="args">
        /// TODO The args.
        /// </param>
        private static void OnSpellCast(AIBaseClient sender1, AIBaseClientProcessSpellCastEventArgs args)
        {
            var sender = sender1 as AIHeroClient;
            if (sender != null && sender.IsEnemy && sender.Team != Player.Team)
            {
                if (args.SData.Name == "ZhonyasHourglass" && sender.HasBuff("zedulttargetmark")
                    && Player.Distance(sender) < _spells[SpellSlot.W].Range - 20 * _spells[SpellSlot.W].Range - 20)
                {
                    var bestPosition = VectorHelper.GetBestPosition(
                        sender, 
                        VectorHelper.GetVertices(sender, true)[0], 
                        VectorHelper.GetVertices(sender, true)[1]);
                    if (_spells[SpellSlot.W].IsReady() && WShadowSpell.ToggleState == 0
                        && Environment.TickCount - _spells[SpellSlot.W].LastCastAttemptTime > 0)
                    {
                        _spells[SpellSlot.W].Cast(bestPosition);
                        _spells[SpellSlot.W].LastCastAttemptTime = Environment.TickCount + 500;
                    }
                }
            }
        }
        
        #endregion
    }
}