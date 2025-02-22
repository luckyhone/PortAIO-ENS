using ElUtilitySuite.Vendor.SFX;
using EnsoulSharp;
using LeagueSharpCommon;

namespace ElUtilitySuite.Trackers
{
    using System;
    using System.Drawing;
    using System.Linq;

    using EnsoulSharp.SDK.MenuUI;
    using SharpDX;
    using SharpDX.Direct3D9;

    using Color = System.Drawing.Color;
    using Font = SharpDX.Direct3D9.Font;

    internal class HealthTracker : IPlugin
    {
        #region Fields

        /// <summary>
        ///     The HP bar height
        /// </summary>
        private readonly int BarHeight = 10;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public EnsoulSharp.SDK.MenuUI.Menu Menu { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the font.
        /// </summary>
        /// <value>
        ///     The font.
        /// </value>
        private static Font Font { get; set; }

        /// <summary>
        ///     Gets the right offset of the HUD elements
        /// </summary>
        private int HudOffsetRight => this.Menu["HealthTracker.OffsetRight"].GetValue<MenuSlider>().Value;

        /// <summary>
        ///     Gets the right offset between text and healthbar
        /// </summary>
        private int HudOffsetText => this.Menu["HealthTracker.OffsetText"].GetValue<MenuSlider>().Value;

        /// <summary>
        ///     Gets the top offset between the HUD elements
        /// </summary>
        private int HudOffsetTop => this.Menu["HealthTracker.OffsetTop"].GetValue<MenuSlider>().Value;

        /// <summary>
        ///     Gets the spacing between HUD elements
        /// </summary>
        private int HudSpacing => this.Menu["HealthTracker.Spacing"].GetValue<MenuSlider>().Value;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        /// <returns></returns>
        public void CreateMenu(Menu rootMenu)
        {
            var predicate = new Func<Menu, bool>(x => x.Name == "Trackers");
            var menu = rootMenu.Components.All(x => x.Key == "Trackers")
                           ? rootMenu["healthbuilding"].Parent
                           : rootMenu.Add(new Menu("Trackers", "Trackers"));

            var enemySidebarMenu =
                menu.Add(new Menu("healthenemies","Health tracker"));
            enemySidebarMenu.SetFontColor(SharpDX.Color.Chartreuse);
            {
                enemySidebarMenu.Add(new MenuBool("DrawHealth_", "Activated").SetValue(true));
                enemySidebarMenu.Add(new MenuBool("DrawHealth_percent", "Champion health %").SetValue(true));
                enemySidebarMenu.Add(new MenuBool("DrawHealth_ultimate", "Champion ultimate").SetValue(true));
                enemySidebarMenu.Add(
                    new MenuSlider("HealthTracker.OffsetText", "Offset Text",30));
                enemySidebarMenu.Add(
                    new MenuSlider("HealthTracker.OffsetTop", "Offset Top",75, 0, 1500));
                enemySidebarMenu.Add(
                    new MenuSlider("HealthTracker.OffsetRight", "Offset Right",170, 0, 1500));
                enemySidebarMenu.Add(
                    new MenuSlider("HealthTracker.Spacing", "Spacing",10, 0, 30));
                enemySidebarMenu.Add(new MenuSlider("FontSize", "Font size",15, 13, 30));

                enemySidebarMenu.Add(new MenuList("Health.Version", "Display options: ",new[] { "Compact", "Clean", }));
            }

            this.Menu = menu;
        }

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            Font = new Font(
                       Drawing.Direct3DDevice9,
                       new FontDescription
                           {
                               FaceName = "Tahoma", Height = this.Menu["FontSize"].GetValue<MenuSlider>().Value,
                               OutputPrecision = FontPrecision.Default, Quality = FontQuality.Antialiased
                           });

            Drawing.OnEndScene += this.Drawing_OnEndScene;
            Drawing.OnPreReset += args => { Font.OnLostDevice(); };
            Drawing.OnPostReset += args => { Font.OnResetDevice(); };
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when the scene is completely rendered.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!this.Menu["DrawHealth_"].GetValue<MenuBool>().Enabled || Drawing.Direct3DDevice9.IsDisposed || Font.IsDisposed)
            {
                return;
            }

            float i = 0;

            foreach (var hero in GameObjects.EnemyHeroes.Where(x => !x.IsDead))
            {
                var champion = hero.CharacterName;
                if (champion.Length > 12)
                {
                    champion = champion.Remove(7) + "...";
                }

                var championInfo = this.Menu["DrawHealth_percent"].GetValue<MenuBool>().Enabled
                                       ? $"{champion} ({(int)hero.HealthPercent}%)"
                                       : champion;

                if (this.Menu["DrawHealth_ultimate"].GetValue<MenuBool>().Enabled)
                {
                    var timeR = hero.Spellbook.GetSpell(SpellSlot.R).CooldownExpires - Game.Time;
                    var ultText = timeR <= 0
                                      ? "READY"
                                      : (timeR < 10 ? timeR.ToString("N1") : ((int)timeR).ToString()) + "s";

                    if (hero.Spellbook.GetSpell(SpellSlot.R).Level == 0)
                    {
                        ultText = "N/A";
                    }

                    championInfo += $" - R: {ultText}";
                }

                if (this.Menu["Health.Version"].GetValue<MenuList>().Index == 1)
                {
                    const int Height = 25;

                    // Draws the rectangle
                    DrawRect(
                        Drawing.Width - this.HudOffsetRight,
                        this.HudOffsetTop + i,
                        200,
                        Height,
                        1,
                        Color.FromArgb(175, 51, 55, 51));

                    DrawRect(
                        Drawing.Width - this.HudOffsetRight + 2,
                        this.HudOffsetTop + i - -2,
                        hero.HealthPercent <= 0 ? 100 : (int)hero.HealthPercent * 2 - 4,
                        Height - 4,
                        1,
                        hero.HealthPercent < 30 && hero.HealthPercent > 0
                            ? Color.FromArgb(255, 250, 0, 23)
                            : hero.HealthPercent < 50
                                ? Color.FromArgb(255, 230, 169, 14)
                                : Color.FromArgb(255, 2, 157, 10));

                    // Draws the championnames
                    Font.DrawText(
                        null,
                        championInfo,
                        (int)(Drawing.Width - this.HudOffsetRight - Font.MeasureText(null, championInfo).Width / 2f)
                        + 200 / 2,
                        (int)(this.HudOffsetTop + i + 13 - Font.MeasureText(null, championInfo).Height / 2f),
                        new ColorBGRA(255, 255, 255, 175));
                }
                else
                {
                    // Draws the championnames
                    Font.DrawText(
                        null,
                        championInfo,
                        Drawing.Width - this.HudOffsetRight - this.HudOffsetText
                        - Font.MeasureText(null, championInfo).Width,
                        (int)(this.HudOffsetTop + i + 4 - Font.MeasureText(null, championInfo).Height / 2f),
                        hero.HealthPercent > 0 ? new ColorBGRA(255, 255, 255, 255) : new ColorBGRA(244, 8, 8, 255));

                    // Draws the rectangle
                    DrawRect(
                        Drawing.Width - this.HudOffsetRight,
                        this.HudOffsetTop + i,
                        100,
                        this.BarHeight,
                        1,
                        Color.FromArgb(255, 51, 55, 51));

                    // Fils the rectangle
                    DrawRect(
                        Drawing.Width - this.HudOffsetRight,
                        this.HudOffsetTop + i,
                        hero.HealthPercent <= 0 ? 100 : (int)hero.HealthPercent,
                        this.BarHeight,
                        1,
                        hero.HealthPercent < 30 && hero.HealthPercent > 0
                            ? Color.FromArgb(255, 250, 0, 23)
                            : hero.HealthPercent < 50
                                ? Color.FromArgb(255, 230, 169, 14)
                                : Color.FromArgb(255, 2, 157, 10));
                }

                i += 20f
                     + (this.Menu["Health.Version"].GetValue<MenuList>().Index == 1 ? 5 : this.HudSpacing);
            }
        }

        /// <summary>
        ///     Draws a rectangle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        private static void DrawRect(float x, float y, int width, float height, float thickness, Color color)
        {
            for (var i = 0; i < height; i++)
            {
                Drawing.DrawLine(x, y + i, x + width, y + i, thickness, color);
            }
        }

        #endregion
    }
}