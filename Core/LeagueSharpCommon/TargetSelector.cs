﻿using EnsoulSharp;
using EnsoulSharp.SDK;

namespace LeagueSharpCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharpDX;
    using Color = System.Drawing.Color;

    /*public class TargetSelector
    {
        #region Static Fields

        public static TargetingMode Mode = TargetingMode.AutoPriority;

        private static Menu _configMenu;

        private static int _focusTime;

        private static AIHeroClient _selectedTargetObjAiHero;

        private static string[] StackNames =
            {
                "kalistaexpungemarker", "vaynesilvereddebuff", "twitchdeadlyvenom",
                "ekkostacks", "dariushemo", "gnarwproc", "tahmkenchpdebuffcounter",
                "varuswdebuff",
            };

        private static bool UsingCustom;

        #endregion

        #region Delegates

        public delegate bool TargetSelectionConditionDelegate(AIHeroClient target);

        #endregion

        #region Enums

        public enum DamageType
        {
            Magical,

            Physical,

            True
        }

        public enum TargetingMode
        {
            AutoPriority,

            LowHP,

            MostAD,

            MostAP,

            Closest,

            NearMouse,

            LessAttack,

            LessCast,

            MostStack
        }

        #endregion

        #region Public Properties

        public static bool CustomTS
        {
            get
            {
                return UsingCustom;
            }
            set
            {
                UsingCustom = value;
                if (value)
                {
                    Drawing.OnDraw -= DrawingOnOnDraw;
                }
                else
                {
                    Drawing.OnDraw += DrawingOnOnDraw;
                }
            }
        }

        public static AIHeroClient SelectedTarget
        {
            get
            {
                return (_configMenu != null && _configMenu.Item("FocusSelected").GetValue<bool>()
                            ? _selectedTargetObjAiHero
                            : null);
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void AddToMenu(Menu config)
        {
            config.AddItem(new MenuItem("Alert", "----Use TS in Common Menu----"));
        }

        /// <summary>
        ///     Returns the priority of the hero
        /// </summary>
        public static float GetPriority(AIHeroClient hero)
        {
            var p = 1;
            if (_configMenu != null && _configMenu.Item("TargetSelector" + hero.CharacterName + "Priority") != null)
            {
                p = _configMenu.Item("TargetSelector" + hero.CharacterName + "Priority").GetValue<Slider>().Value;
            }

            switch (p)
            {
                case 2:
                    return 1.5f;
                case 3:
                    return 1.75f;
                case 4:
                    return 2f;
                case 5:
                    return 2.5f;
                default:
                    return 1f;
            }
        }

        public static AIHeroClient GetSelectedTarget()
        {
            return SelectedTarget;
        }

        public static AIHeroClient GetTarget(
            float range,
            DamageType damageType,
            bool ignoreShield = true,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null,
            TargetSelectionConditionDelegate conditions = null)
        {
            return GetTarget(
                ObjectManager.Player,
                range,
                damageType,
                ignoreShield,
                ignoredChamps,
                rangeCheckFrom,
                conditions);
        }

        public static AIHeroClient GetTarget(
            AIBaseClient champion,
            float range,
            DamageType type,
            bool ignoreShieldSpells = true,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null,
            TargetSelectionConditionDelegate conditions = null)
        {
            try
            {
                if (ignoredChamps == null)
                {
                    ignoredChamps = new List<AIHeroClient>();
                }

                var damageType = (DamageType)Enum.Parse(typeof(DamageType), type.ToString());

                if (_configMenu != null
                    && IsValidTarget(
                        SelectedTarget,
                        _configMenu.Item("ForceFocusSelected").GetValue<bool>() ? float.MaxValue : range,
                        type,
                        ignoreShieldSpells,
                        rangeCheckFrom))
                {
                    return SelectedTarget;
                }

                if (_configMenu != null
                    && IsValidTarget(
                        SelectedTarget,
                        _configMenu.Item("ForceFocusSelectedKeys").GetValue<bool>() ? float.MaxValue : range,
                        type,
                        ignoreShieldSpells,
                        rangeCheckFrom))
                {
                    if (_configMenu.Item("ForceFocusSelectedK").GetValue<KeyBind>().Active
                        || _configMenu.Item("ForceFocusSelectedK2").GetValue<KeyBind>().Active)
                    {
                        return SelectedTarget;
                    }
                }

                if (_configMenu != null && _configMenu.Item("TargetingMode") != null
                    && Mode == TargetingMode.AutoPriority)
                {
                    var menuItem = _configMenu.Item("TargetingMode").GetValue<StringList>();
                    Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out Mode);
                }

                var targets =
                    HeroManager.Enemies.FindAll(
                        hero =>
                        ignoredChamps.All(ignored => ignored.NetworkId != hero.NetworkId)
                        && IsValidTarget(hero, range, type, ignoreShieldSpells, rangeCheckFrom)
                        && (conditions == null || conditions(hero))
                        && (!Orbwalking.YasuoInGame || range > 0 || ObjectManager.Player.IsMelee || !Orbwalking.CollisionYasuo(ObjectManager.Player.Position, hero.Position)));
                switch (Mode)
                {
                    case TargetingMode.LowHP:
                        return targets.MinOrDefault(hero => hero.Health);

                    case TargetingMode.MostAD:
                        return targets.MaxOrDefault(hero => hero.BaseAttackDamage + hero.FlatPhysicalDamageMod);

                    case TargetingMode.MostAP:
                        return targets.MaxOrDefault(hero => hero.BaseAbilityDamage + hero.FlatMagicDamageMod);

                    case TargetingMode.Closest:
                        return
                            targets.MinOrDefault(
                                hero =>
                                (rangeCheckFrom.HasValue ? rangeCheckFrom.Value : champion.ServerPosition).Distance(
                                    hero.ServerPosition,
                                    true));

                    case TargetingMode.NearMouse:
                        return targets.MinOrDefault(hero => hero.Distance(Game.CursorPos));

                    case TargetingMode.AutoPriority:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                champion.CalculateDamage(hero, (EnsoulSharp.SDK.DamageType) damageType, 100) / (1 + hero.Health) * GetPriority(hero));

                    case TargetingMode.LessAttack:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                champion.CalculateDamage(hero, (EnsoulSharp.SDK.DamageType) DamageType.Physical, 100) / (1 + hero.Health)
                                * GetPriority(hero));

                    case TargetingMode.LessCast:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                champion.CalculateDamage(hero, (EnsoulSharp.SDK.DamageType) DamageType.Magical, 100) / (1 + hero.Health)
                                * GetPriority(hero));

                    case TargetingMode.MostStack:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                champion.CalculateDamage(hero, (EnsoulSharp.SDK.DamageType) damageType, 100) / (1 + hero.Health) * GetPriority(hero)
                                + (1 + hero.Buffs.Where(b => StackNames.Contains(b.Name.ToLower())).Sum(t => t.Count)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public static AIHeroClient GetTargetNoCollision(
            Spell spell,
            bool ignoreShield = true,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null)
        {
            var t = GetTarget(
                ObjectManager.Player,
                spell.Range,
                spell.DamageType,
                ignoreShield,
                ignoredChamps,
                rangeCheckFrom);

            if (spell.Collision && spell.GetPrediction(t).Hitchance != HitChance.Collision)
            {
                return t;
            }

            return null;
        }

        public static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += args =>
                {
                    var config = new Menu("Target Selector", "TargetSelector");

                    _configMenu = config;

                    var focusMenu = new Menu("Focus Target Settings", "FocusTargetSettings");

                    focusMenu.AddItem(new MenuItem("FocusSelected", "Focus selected target").SetShared().SetValue(true));
                    focusMenu.AddItem(
                        new MenuItem("SelTColor", "Focus selected target color").SetShared()
                            .SetValue(new Circle(true, Color.Red)));
                    focusMenu.AddItem(
                        new MenuItem("ForceFocusSelected", "Only attack selected target").SetShared().SetValue(false));
                    focusMenu.AddItem(new MenuItem("sep", ""));
                    focusMenu.AddItem(
                        new MenuItem("ForceFocusSelectedKeys", "Enable only attack selected Keys").SetShared()
                            .SetValue(false));
                    focusMenu.AddItem(new MenuItem("ForceFocusSelectedK", "Only attack selected Key"))
                        .SetValue(new KeyBind(32, KeyBindType.Press));
                    focusMenu.AddItem(new MenuItem("ForceFocusSelectedK2", "Only attack selected Key 2"))
                        .SetValue(new KeyBind(32, KeyBindType.Press));
                    focusMenu.AddItem(new MenuItem("ResetOnRelease", "Reset selected target upon release"))
                        .SetValue(false);

                    config.AddSubMenu(focusMenu);

                    var autoPriorityItem =
                        new MenuItem("AutoPriority", "Auto arrange priorities").SetShared()
                            .SetValue(true)
                            .SetTooltip("5 = Highest Priority");
                    autoPriorityItem.ValueChanged += autoPriorityItem_ValueChanged;

                    foreach (var enemy in HeroManager.Enemies)
                    {
                        config.AddItem(
                            new MenuItem("TargetSelector" + enemy.CharacterName + "Priority", enemy.CharacterName)
                                .SetShared()
                                .SetValue(
                                    new Slider(
                                        autoPriorityItem.GetValue<bool>() ? GetPriorityFromDb(enemy.CharacterName) : 1,
                                        5,
                                        1)));
                        if (autoPriorityItem.GetValue<bool>())
                        {
                            config.Item("TargetSelector" + enemy.CharacterName + "Priority")
                                .SetValue(
                                    new Slider(
                                        autoPriorityItem.GetValue<bool>() ? GetPriorityFromDb(enemy.CharacterName) : 1,
                                        5,
                                        1));
                        }
                    }
                    config.AddItem(autoPriorityItem);
                    config.AddItem(
                        new MenuItem("TargetingMode", "Target Mode").SetShared()
                            .SetValue(new StringList(Enum.GetNames(typeof(TargetingMode)))));

                    CommonMenu.Instance.AddSubMenu(config);
                    Game.OnWndProc += GameOnOnWndProc;

                    if (!CustomTS)
                    {
                        Drawing.OnDraw += DrawingOnOnDraw;
                    }
                };
        }

        public static bool IsInvulnerable(AIBaseClient target, DamageType damageType, bool ignoreShields = true)
        {
            var targetBuffs = new HashSet<string>(
                target.Buffs.Select(buff => buff.Name),
                StringComparer.OrdinalIgnoreCase);

            // Kindred's Lamb's Respite(R)
            if (targetBuffs.Contains("KindredRNoDeathBuff") && target.HealthPercent <= 10)
            {
                return true;
            }

            // Vladimir W
            if (targetBuffs.Contains("VladimirSanguinePool"))
            {
                return true;
            }

            // Tryndamere's Undying Rage (R)
            if (targetBuffs.Contains("UndyingRage") && target.Health <= target.MaxHealth * 0.10f)
            {
                return true;
            }

            // Kayle's Intervention (R)
            if (targetBuffs.Contains("JudicatorIntervention"))
            {
                return true;
            }

            if (ignoreShields)
            {
                return false;
            }

            // Morgana's Black Shield (E)
            if (targetBuffs.Contains("BlackShield") && damageType.Equals(DamageType.Magical))
            {
                return true;
            }

            // Banshee's Veil (PASSIVE)
            if (targetBuffs.Contains("bansheesveil") && damageType.Equals(DamageType.Magical))
            {
                return true;
            }

            // Sivir's Spell Shield (E)
            if (targetBuffs.Contains("SivirE") && damageType.Equals(DamageType.Magical))
            {
                return true;
            }

            // Nocturne's Shroud of Darkness (W)
            if (targetBuffs.Contains("NocturneShroudofDarkness") && damageType.Equals(DamageType.Magical))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Sets the priority of the hero
        /// </summary>
        public static void SetPriority(AIHeroClient hero, int newPriority)
        {
            if (_configMenu == null || _configMenu.Item("TargetSelector" + hero.CharacterName + "Priority") == null)
            {
                return;
            }
            var p = _configMenu.Item("TargetSelector" + hero.CharacterName + "Priority").GetValue<Slider>();
            p.Value = Math.Max(1, Math.Min(5, newPriority));
            _configMenu.Item("TargetSelector" + hero.CharacterName + "Priority").SetValue(p);
        }

        public static void SetTarget(AIHeroClient hero)
        {
            if (hero.IsValidTarget())
            {
                _selectedTargetObjAiHero = hero;
            }
        }

        public static void Shutdown()
        {
            Menu.Remove(_configMenu);

            Game.OnWndProc -= GameOnOnWndProc;

            if (!CustomTS)
            {
                Drawing.OnDraw -= DrawingOnOnDraw;
            }
        }

        #endregion

        #region Methods

        private static void autoPriorityItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (!e.GetNewValue<bool>())
            {
                return;
            }
            foreach (var enemy in HeroManager.Enemies)
            {
                _configMenu.Item("TargetSelector" + enemy.CharacterName + "Priority")
                    .SetValue(new Slider(GetPriorityFromDb(enemy.CharacterName), 5, 1));
            }
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (_selectedTargetObjAiHero.IsValidTarget() && _configMenu != null
                && _configMenu.Item("FocusSelected").GetValue<bool>()
                && _configMenu.Item("SelTColor").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(
                    _selectedTargetObjAiHero.Position,
                    150,
                    _configMenu.Item("SelTColor").GetValue<Circle>().Color,
                    7,
                    true);
            }

            var a = (_configMenu.Item("ForceFocusSelectedK").GetValue<KeyBind>().Active
                     || _configMenu.Item("ForceFocusSelectedK2").GetValue<KeyBind>().Active)
                    && _configMenu.Item("ForceFocusSelectedKeys").GetValue<bool>();

            _configMenu.Item("ForceFocusSelectedKeys").Permashow(SelectedTarget != null && a);
            _configMenu.Item("ForceFocusSelected").Permashow(_configMenu.Item("ForceFocusSelected").GetValue<bool>());

            if (!_configMenu.Item("ResetOnRelease").GetValue<bool>())
            {
                return;
            }

            if (SelectedTarget != null && !a)
            {
                if (!_configMenu.Item("ForceFocusSelected").GetValue<bool>()
                    && Utils.GameTimeTickCount - _focusTime < 150)
                {
                    if (!a)
                    {
                        _selectedTargetObjAiHero = null;
                    }
                }
            }
            else
            {
                if (a)
                {
                    _focusTime = Utils.GameTimeTickCount;
                }
            }
        }

        private static void GameOnOnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }
            _selectedTargetObjAiHero =
                HeroManager.Enemies.FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000)
                    // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
        }

        private static int GetPriorityFromDb(string championName)
        {
            return LeagueSharp.Data.Data.Get<ChampionPriorityData>().GetPriority(championName);
        }
        private static bool WasSet = false;
        private static bool IsSenna = false;
        private static bool IsShen = false;
        private static bool IsJax = false;
        private static bool IsSamira = false;
        private static bool IsTryndamere = false;

        private static bool IsValidTarget(
            AIBaseClient target,
            float range,
            DamageType damageType,
            bool ignoreShieldSpells = true,
            Vector3? rangeCheckFrom = null)
        {
            if (target.IsValidTarget()
                   && target.Distance(rangeCheckFrom ?? ObjectManager.Player.ServerPosition, true)
                   < Math.Pow(range <= 0 ? Orbwalking.GetRealAutoAttackRange(target) : range, 2)
                   && !IsInvulnerable(target, damageType, ignoreShieldSpells))
            {
                if (!WasSet)
                {
                    foreach (var enemy in HeroManager.Enemies)
                    {
                        switch(enemy.CharacterName)
                        {
                            case "Senna":
                            {
                                IsSenna = true;
                                break;
                            }
                            case "Shen":
                            {
                                IsShen = true;
                                break;
                            }
                            case "Jax":
                            {
                                IsJax = true;
                                break;
                            }
                            case "Samira":
                            {
                                IsSamira = true;
                                break;
                            }
                            case "Tryndamere":
                            {
                                IsTryndamere = true;
                                break;
                            }
                        }
                    }

                    WasSet = true;
                }

                if (range <= 0)
                {
                    if (IsSenna || IsJax || IsShen)
                    {
                        foreach (var buff in target.Buffs)
                        {
                            var name = buff.Name;

                            if (IsJax && name.Equals("JaxCounterStrike", StringComparison.InvariantCultureIgnoreCase))
                                return false;

                            if (IsSamira && name.Equals("SamiraW", StringComparison.InvariantCultureIgnoreCase))
                                return false;

                            if (IsSamira && target.Health < 100 && name.Equals("UndyingRage", StringComparison.InvariantCultureIgnoreCase))
                                return false;

                            if (IsSenna)
                            {
                                if (name.Equals("sennaecamo", StringComparison.InvariantCultureIgnoreCase) ||
                                    name.Equals("sennaewraithform", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (!target.IsHPBarRendered)
                                        return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        #endregion
    }

    /// <summary>
    ///     This TS attempts to always lock the same target, useful for people getting targets for each spell, or for champions
    ///     that have to burst 1 target.
    /// </summary>
    public class LockedTargetSelector
    {
        #region Static Fields

        public static AIHeroClient _lastTarget;

        private static TargetSelector.DamageType _lastDamageType;

        #endregion

        #region Public Methods and Operators

        public static void AddToMenu(Menu menu)
        {
            TargetSelector.AddToMenu(menu);
        }

        public static AIHeroClient GetTarget(
            float range,
            TargetSelector.DamageType damageType,
            bool ignoreShield = true,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null)
        {
            if (_lastTarget == null || !_lastTarget.IsValidTarget() || _lastDamageType != damageType)
            {
                var newTarget = TargetSelector.GetTarget(range, damageType, ignoreShield, ignoredChamps, rangeCheckFrom);

                _lastTarget = newTarget;
                _lastDamageType = damageType;

                return newTarget;
            }

            if (_lastTarget.IsValidTarget(range) && damageType == _lastDamageType)
            {
                return _lastTarget;
            }

            var newTarget2 = TargetSelector.GetTarget(range, damageType, ignoreShield, ignoredChamps, rangeCheckFrom);

            _lastTarget = newTarget2;
            _lastDamageType = damageType;

            return newTarget2;
        }

        /// <summary>
        ///     Unlocks the currently locked target.
        /// </summary>
        public static void UnlockTarget()
        {
            _lastTarget = null;
        }

        #endregion
    }*/
}