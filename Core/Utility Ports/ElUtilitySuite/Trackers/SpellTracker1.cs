using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using LeagueSharpCommon;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace ElUtilitySuite.Trackers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Security.Permissions;

    using PortAIO.Properties;
    using ElUtilitySuite.Vendor.SFX;


    using SharpDX;
    using SharpDX.Direct3D9;

    using Font = SharpDX.Direct3D9.Font;
    using Rectangle = SharpDX.Rectangle;

    internal class SpellTracker : IPlugin
    {
        #region Constants
        // force commit kappa 123
        private const float TeleportCd = 300f;

        #endregion

        #region Fields

        private readonly Dictionary<int, List<SpellDataInst>> _spellDatas = new Dictionary<int, List<SpellDataInst>>();

        private readonly SpellSlot[] _spellSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };

        private readonly Dictionary<int, List<SpellDataInst>> _summonerDatas =
            new Dictionary<int, List<SpellDataInst>>();

        private readonly SpellSlot[] _summonerSlots = { SpellSlot.Summoner1, SpellSlot.Summoner2 };

        private readonly Dictionary<string, Texture> _summonerTextures = new Dictionary<string, Texture>();

        private readonly Dictionary<int, float> _teleports = new Dictionary<int, float>();

        private List<AIHeroClient> _heroes = new List<AIHeroClient>();

        private Texture _hudSelfTexture;

        private Texture _hudTexture;

        private Line _line;

        private Sprite _sprite;

        private Font _text;

        #endregion

        internal class ManualSpell
        {
            #region Constructors and Destructors

            public ManualSpell(string champ, string spell, SpellSlot slot, float[] cooldowns, float additional = 0)
            {
                this.Champ = champ;
                this.Spell = spell;
                this.Slot = slot;
                this.Cooldowns = cooldowns;
                this.Additional = additional;
            }

            #endregion

            #region Public Properties

            public float Additional { get; set; }

            public string Champ { get; private set; }

            public float Cooldown { get; set; }

            public float CooldownExpires { get; set; }

            public float[] Cooldowns { get; set; }

            public SpellSlot Slot { get; private set; }

            public string Spell { get; private set; }

            #endregion
        } // ReSharper disable StringLiteralTypo

        private readonly List<ManualSpell> _manualAllySpells = new List<ManualSpell>
                                                                   {
                                                                       new ManualSpell(
                                                                           "Lux",
                                                                           "LuxLightStrikeKugel",
                                                                           SpellSlot.E,
                                                                           new[] { 10f, 10f, 10f, 10f, 10f }),
                                                                       new ManualSpell(
                                                                           "Gragas",
                                                                           "GragasQ",
                                                                           SpellSlot.Q,
                                                                           new[] { 11f, 10f, 9f, 8f, 7f }),
                                                                       new ManualSpell(
                                                                           "Riven",
                                                                           "RivenFengShuiEngine",
                                                                           SpellSlot.R,
                                                                           new[] { 110f, 80f, 50f },
                                                                           15),
                                                                       new ManualSpell(
                                                                           "TwistedFate",
                                                                           "PickACard",
                                                                           SpellSlot.W,
                                                                           new[] { 6f, 6f, 6f, 6f, 6f }),
                                                                       new ManualSpell(
                                                                           "Velkoz",
                                                                           "VelkozQ",
                                                                           SpellSlot.Q,
                                                                           new[] { 7f, 7f, 7f, 7f, 7f },
                                                                           0.75f),
                                                                       new ManualSpell(
                                                                           "Xerath",
                                                                           "xeratharcanopulse2",
                                                                           SpellSlot.Q,
                                                                           new[] { 9f, 8f, 7f, 6f, 5f }),
                                                                       new ManualSpell(
                                                                           "Ziggs",
                                                                           "ZiggsW",
                                                                           SpellSlot.W,
                                                                           new[] { 26f, 24f, 22f, 20f, 18f }),
                                                                       new ManualSpell(
                                                                           "Rumble",
                                                                           "RumbleGrenade",
                                                                           SpellSlot.E,
                                                                           new[] { 10f, 10f, 10f, 10f, 10f }),
                                                                       new ManualSpell(
                                                                           "Riven",
                                                                           "RivenTriCleave",
                                                                           SpellSlot.Q,
                                                                           new[] { 13f, 13f, 13f, 13f, 13f }),
                                                                       new ManualSpell(
                                                                           "Fizz",
                                                                           "FizzJump",
                                                                           SpellSlot.E,
                                                                           new[] { 16f, 14f, 12f, 10f, 8f },
                                                                           0.75f)
                                                                   };

        private readonly List<ManualSpell> _manualEnemySpells = new List<ManualSpell>
                                                                    {
                                                                        new ManualSpell(
                                                                            "Lux",
                                                                            "LuxLightStrikeKugel",
                                                                            SpellSlot.E,
                                                                            new[] { 10f, 10f, 10f, 10f, 10f }),
                                                                        new ManualSpell(
                                                                            "Gragas",
                                                                            "GragasQ",
                                                                            SpellSlot.Q,
                                                                            new[] { 11f, 10f, 9f, 8f, 7f }),
                                                                        new ManualSpell(
                                                                            "Riven",
                                                                            "RivenFengShuiEngine",
                                                                            SpellSlot.R,
                                                                            new[] { 110f, 80f, 50f },
                                                                            15),
                                                                        new ManualSpell(
                                                                            "TwistedFate",
                                                                            "PickACard",
                                                                            SpellSlot.W,
                                                                            new[] { 6f, 6f, 6f, 6f, 6f }),
                                                                        new ManualSpell(
                                                                            "Velkoz",
                                                                            "VelkozQ",
                                                                            SpellSlot.Q,
                                                                            new[] { 7f, 7f, 7f, 7f, 7f },
                                                                            0.75f),
                                                                        new ManualSpell(
                                                                            "Xerath",
                                                                            "xeratharcanopulse2",
                                                                            SpellSlot.Q,
                                                                            new[] { 9f, 8f, 7f, 6f, 5f }),
                                                                        new ManualSpell(
                                                                            "Ziggs",
                                                                            "ZiggsW",
                                                                            SpellSlot.W,
                                                                            new[] { 26f, 24f, 22f, 20f, 18f }),
                                                                        new ManualSpell(
                                                                            "Rumble",
                                                                            "RumbleGrenade",
                                                                            SpellSlot.E,
                                                                            new[] { 10f, 10f, 10f, 10f, 10f }),
                                                                        new ManualSpell(
                                                                            "Riven",
                                                                            "RivenTriCleave",
                                                                            SpellSlot.Q,
                                                                            new[] { 13f, 13f, 13f, 13f, 13f }),
                                                                        new ManualSpell(
                                                                            "Fizz",
                                                                            "FizzJump",
                                                                            SpellSlot.E,
                                                                            new[] { 16f, 14f, 12f, 10f, 8f },
                                                                            0.75f)
                                                                    };

        public Menu Menu { get; set; }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        /// <returns></returns>
        public void CreateMenu(Menu rootMenu)
        {
            var predicate = new Func<Menu, bool>(x => x.Name == "Trackers");
            var menu = rootMenu.Components.All(x => x.Key != "Trackers")
                ? rootMenu.Add(new Menu("Trackers", "Trackers"))
                : rootMenu["Trackers"].Parent;

            var cooldownMenu = menu.Add(new Menu("cdddddtracker","Cooldown tracker"));
            {
                cooldownMenu.Add(
                    new MenuList("cooldown-tracker-TimeFormat", "Time Format",new[] { "mm:ss", "ss" }));
                cooldownMenu.Add(new MenuSlider("cooldown-tracker-FontSize", "Font Size",13, 3, 30));
                cooldownMenu.Add(new MenuBool("cooldown-tracker-Enemy", "Enemy").SetValue(true));
                cooldownMenu.Add(new MenuBool("cooldown-tracker-Ally", "Ally").SetValue(true));
                cooldownMenu.Add(new MenuBool("cooldown-tracker-Self", "Self").SetValue(true));

                cooldownMenu.Add(new MenuBool("cooldown-tracker-Enabled", "Enabled").SetValue(true));
            }

            this.Menu = cooldownMenu;

            this.Menu["cooldown-tracker-Enemy"].GetValue<MenuBool>().ValueChanged += delegate(MenuBool args, EventArgs item)
                {
                    if (_heroes == null)
                    {
                        return;
                    }
                    var ally = Menu["cooldown-tracker-Ally"].GetValue<MenuBool>().Enabled;
                    var enemy = args.GetValue<MenuBool>().Enabled;
                    _heroes = ally && enemy
                                  ? GameObjects.Heroes.ToList()
                                  : (ally ? GameObjects.AllyHeroes : (enemy ? GameObjects.EnemyHeroes : new List<AIHeroClient>()))
                                        .ToList();
                    if (Menu["cooldown-tracker-Self"].GetValue<MenuBool>().Enabled)
                    {
                        if (_heroes.All(h => h.NetworkId != ObjectManager.Player.NetworkId))
                        {
                            _heroes.Add(ObjectManager.Player);
                        }
                    }
                    else
                    {
                        _heroes.RemoveAll(h => h.NetworkId == ObjectManager.Player.NetworkId);
                    }
                };

            this.Menu["cooldown-tracker-Ally"].GetValue<MenuBool>().ValueChanged += delegate(MenuBool args, EventArgs item)
                {
                    if (_heroes == null)
                    {
                        return;
                    }
                    var ally = args.GetValue<MenuBool>().Enabled;
                    var enemy = Menu["cooldown-tracker-Enemy"].GetValue<MenuBool>().Enabled;
                    _heroes = ally && enemy
                                  ? GameObjects.Heroes.ToList()
                                  : (ally
                                         ? GameObjects.AllyHeroes
                                         : (enemy ? GameObjects.EnemyHeroes : new List<AIHeroClient>())).ToList();
                    if (Menu["cooldown-tracker-Self"].GetValue<MenuBool>().Enabled
                        && _heroes.All(h => h.NetworkId != ObjectManager.Player.NetworkId))
                    {
                        _heroes.Add(ObjectManager.Player);
                    }
                    if (Menu["cooldown-tracker-Self"].GetValue<MenuBool>().Enabled)
                    {
                        if (_heroes.All(h => h.NetworkId != ObjectManager.Player.NetworkId))
                        {
                            _heroes.Add(ObjectManager.Player);
                        }
                    }
                    else
                    {
                        _heroes.RemoveAll(h => h.NetworkId == ObjectManager.Player.NetworkId);
                    }
                };

            this.Menu["cooldown-tracker-Self"].GetValue<MenuBool>().ValueChanged += delegate(MenuBool args, EventArgs item)
                {
                    if (_heroes == null)
                    {
                        return;
                    }
                    var ally = Menu["cooldown-tracker-Ally"].GetValue<MenuBool>().Enabled;
                    var enemy = Menu["cooldown-tracker-Enemy"].GetValue<MenuBool>().Enabled;
                    _heroes = ally && enemy
                                  ? GameObjects.Heroes.ToList()
                                  : (ally ? GameObjects.AllyHeroes : (enemy ? GameObjects.EnemyHeroes : new List<AIHeroClient>()))
                                        .ToList();
                    if (args.GetValue<MenuBool>().Enabled)
                    {
                        if (_heroes.All(h => h.NetworkId != ObjectManager.Player.NetworkId))
                        {
                            _heroes.Add(ObjectManager.Player);
                        }
                    }
                    else
                    {
                        _heroes.RemoveAll(h => h.NetworkId == ObjectManager.Player.NetworkId);
                    }
                };
        }

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            try
            {
                this._hudTexture = Resources.CD_HudSelf.ToTexture();
                this._hudSelfTexture = Resources.CD_HudSelf.ToTexture();

                foreach (var enemy in GameObjects.Heroes)
                {
                    var lEnemy = enemy;
                    this._spellDatas.Add(
                        enemy.NetworkId,
                        this._spellSlots.Select(slot => lEnemy.GetSpell(slot)).ToList());
                    this._summonerDatas.Add(
                        enemy.NetworkId,
                        this._summonerSlots.Select(slot => lEnemy.GetSpell(slot)).ToList());
                }

                foreach (var sName in
                    GameObjects.Heroes.SelectMany(
                        h =>
                        this._summonerSlots.Select(summoner => h.Spellbook.GetSpell(summoner).Name.ToLower())
                            .Where(sName => !this._summonerTextures.ContainsKey(FixName(sName)))))
                {
                    this._summonerTextures[FixName(sName)] =
                        ((Bitmap)Resources.ResourceManager.GetObject(string.Format("CD_{0}", FixName(sName)))
                         ?? Resources.CD_SummonerBarrier).ToTexture();
                }

                this._heroes = this.Menu["cooldown-tracker-Ally"].GetValue<MenuBool>().Enabled
                               && this.Menu["cooldown-tracker-Enemy"].GetValue<MenuBool>().Enabled
                                   ? GameObjects.Heroes.ToList()
                                   : (this.Menu["cooldown-tracker-Ally"].GetValue<MenuBool>().Enabled
                                          ? GameObjects.AllyHeroes
                                          : (this.Menu["cooldown-tracker-Enemy"].GetValue<MenuBool>().Enabled
                                                 ? GameObjects.EnemyHeroes
                                                 : new List<AIHeroClient>())).ToList();

                if (!this.Menu["cooldown-tracker-Self"].GetValue<MenuBool>().Enabled)
                {
                    this._heroes.RemoveAll(h => h.NetworkId == ObjectManager.Player.NetworkId);
                }

                this._sprite = MDrawing.GetSprite();
                this._line = MDrawing.GetLine(4);
                this._text = MDrawing.GetFont(this.Menu["cooldown-tracker-FontSize"].GetValue<MenuSlider>().Value);

                Drawing.OnEndScene += this.OnDrawingEndScene;

                Drawing.OnPreReset += args =>
                    {
                        this._line.OnLostDevice();
                        this._sprite.OnLostDevice();
                        this._text.OnLostDevice();
                    };

                Drawing.OnPostReset += args =>
                    {
                        this._line.OnResetDevice();
                        this._sprite.OnResetDevice();
                        this._text.OnResetDevice();
                    };

                AIBaseClient.OnProcessSpellCast += this.OnObjAiBaseProcessSpellCast;
                Teleport.OnTeleport += this.OnObjAiBaseTeleport;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnObjAiBaseTeleport(AIBaseClient sender, Teleport.TeleportEventArgs packet)
        {
            try
            {
                if (!this.Menu["cooldown-tracker-Enabled"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                var unit = sender as AIHeroClient;

                if (unit == null || !unit.IsValid || unit.IsAlly)
                    return;

                if (packet.Type == Teleport.TeleportType.Teleport
                    && (packet.Status == Teleport.TeleportStatus.Finish
                        || packet.Status == Teleport.TeleportStatus.Abort))
                {
                    var time = Game.Time;
                    DelayAction.Add(
                        250,
                        delegate
                            {
                                var cd = packet.Status == Teleport.TeleportStatus.Finish ? 300 : 200;
                                _teleports[unit.NetworkId] = time + cd;
                            });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static string FixName(string name)
        {
            try
            {
                return name.ToLower().Contains("smite")
                           ? "summonersmite"
                           : (name.ToLower().Contains("teleport") ? "summonerteleport" : name.ToLower());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return name;
        }

        private void OnDrawingEndScene(EventArgs args)
        {
            try
            {

                if (!this.Menu["cooldown-tracker-Enabled"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
                {
                    return;
                }
                var totalSeconds = this.Menu["cooldown-tracker-TimeFormat"].GetValue<MenuList>().Index
                                   == 1;
                foreach (var hero in
                    this._heroes.Where(
                        hero => hero != null && hero.IsValid && hero.IsHPBarRendered && hero.Position.IsOnScreen()))
                {
                    try
                    {
                        var lHero = hero;
                        if (!hero.Position.IsValid() || !hero.HPBarPosition.IsValid())
                        {
                            return;
                        }

                        var x = (int)hero.HPBarPosition.X - (68);
                        var y = (int)hero.HPBarPosition.Y - (20);

                        this._sprite.Begin(SpriteFlags.AlphaBlend);
                        var summonerData = this._summonerDatas[hero.NetworkId];
                        for (var i = 0; i < summonerData.Count; i++)
                        {
                            var spell = summonerData[i];
                            if (spell != null)
                            {
                                var teleportCd = 0f;
                                if (spell.Name.Contains("Teleport") && this._teleports.ContainsKey(hero.NetworkId))
                                {
                                    this._teleports.TryGetValue(hero.NetworkId, out teleportCd);
                                }
                                var t = teleportCd > 0.1f
                                            ? teleportCd - Game.Time
                                            : (spell.IsReady() ? 0 : spell.CooldownExpires - Game.Time);
                                var sCd = teleportCd > 0.1f ? TeleportCd : spell.Cooldown;
                                var percent = Math.Abs(sCd) > float.Epsilon ? t / sCd : 1f;
                                var n = t > 0 ? (int)(19 * (1f - percent)) : 19;
                                if (t > 0)
                                {
                                    this._text.DrawTextCentered(
                                        t.FormatTime(totalSeconds),
                                        x - (-160),
                                        y + 7 + 13 * i,
                                        new ColorBGRA(255, 255, 255, 255));
                                }
                                if (this._summonerTextures.ContainsKey(FixName(spell.Name)))
                                {
                                    this._sprite.Draw(
                                        this._summonerTextures[FixName(spell.Name)],
                                        new ColorBGRA(255, 255, 255, 255),
                                        new Rectangle(0, 12 * n, 12, 12),
                                        new Vector3(-x - (132), -y - 1 - 13 * i, 0));
                                }
                            }
                        }

                        this._sprite.Draw(
                            hero.IsMe ? this._hudSelfTexture : this._hudTexture,
                            new ColorBGRA(255, 255, 255, 255),
                            null,
                            new Vector3(-x, -y, 0));

                        this._sprite.End();

                        var x2 = x + (24);
                        var y2 = y + 21;

                        this._line.Begin();
                        var spellData = this._spellDatas[hero.NetworkId];
                        foreach (var spell in spellData)
                        {
                            var lSpell = spell;
                            if (spell != null)
                            {
                                var spell1 = spell;
                                var manual = hero.IsAlly
                                                 ? this._manualAllySpells.FirstOrDefault(
                                                     m =>
                                                     m.Slot.Equals(lSpell.Slot)
                                                     && m.Champ.Equals(
                                                         lHero.CharacterName,
                                                         StringComparison.OrdinalIgnoreCase))
                                                 : this._manualEnemySpells.FirstOrDefault(
                                                     m =>
                                                     m.Slot.Equals(spell1.Slot)
                                                     && m.Champ.Equals(
                                                         lHero.CharacterName,
                                                         StringComparison.OrdinalIgnoreCase));

                                var t = (manual != null ? manual.CooldownExpires : spell.CooldownExpires) - Game.Time;
                                var spellCooldown = manual != null ? manual.Cooldown : spell.Cooldown;
                                var percent = t > 0 && Math.Abs(spellCooldown) > float.Epsilon
                                                  ? 1f - t / spellCooldown
                                                  : 1f;


                                if (t > 0 && t < 100)
                                {
                                    this._text.DrawTextCentered(
                                        t.FormatTime(totalSeconds),
                                        x2 + 27 / 2,
                                        y2 + 13,
                                        new ColorBGRA(255, 255, 255, 255));
                                }

                                if (spell.Level > 0)
                                {
                                    this._line.Draw(
                                        new[] { new Vector2(x2, y2), new Vector2(x2 + percent * 23, y2) },
                                        t > 0 ? new ColorBGRA(235, 137, 0, 255) : new ColorBGRA(0, 168, 25, 255));
                                }
                                x2 = x2 + 27;
                            }
                        }
                        this._line.End();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred: '{0}'", e);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private void OnObjAiBaseProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (!this.Menu["cooldown-tracker-Enabled"].GetValue<MenuBool>().Enabled)
                {
                    return;
                }

                var hero = sender as AIHeroClient;
                if (hero != null)
                {
                    var data = hero.IsAlly
                                   ? this._manualAllySpells.FirstOrDefault(
                                       m => m.Spell.Equals(args.SData.Name, StringComparison.OrdinalIgnoreCase))
                                   : this._manualEnemySpells.FirstOrDefault(
                                       m => m.Spell.Equals(args.SData.Name, StringComparison.OrdinalIgnoreCase));

                    if (data != null && data.CooldownExpires - Game.Time < 0.5)
                    {
                        var spell = hero.GetSpell(data.Slot);
                        if (spell != null)
                        {
                            var cooldown = data.Cooldowns[spell.Level - 1];
                            var cdr = hero.PercentCooldownMod * -1 * 100;
                            data.Cooldown = cooldown - cooldown / 100 * (cdr > 40 ? 40 : cdr) + data.Additional;
                            data.CooldownExpires = Game.Time + data.Cooldown;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}