using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace ElUtilitySuite.Trackers
{
    using System;
    using System.Linq;

    using ElUtilitySuite.Logging;

    using SharpDX;
    using SharpDX.Direct3D9;

    internal class BuildingTracker : IPlugin
    {
        private IPlugin _pluginImplementation;
        #region Public Properties

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public Menu Menu { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the font.
        /// </summary>
        /// <value>
        ///     The font.
        /// </value>
        private static Font Font { get; set; }

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
            var menu = rootMenu.Components.All(x => x.Key != "Trackers")
                ? rootMenu.Add(new Menu("Trackers", "Trackers"))
                           : rootMenu["Trackers"].Parent;

            var buildingMenu = menu.Add(new Menu("healthbuilding","Tower and Inhib tracker"));
            {
                buildingMenu.Add(new MenuBool("DrawHealth", "Activated").SetValue(true));
                buildingMenu.Add(new MenuBool("DrawTurrets", "Turrets").SetValue(true));
                buildingMenu.Add(new MenuBool("DrawInhibs", "Inhibitors").SetValue(true));
                buildingMenu.Add(new MenuSlider("Turret.FontSize", "Tower Font size",13, 13, 30));
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
                        FaceName = "Tahoma", Height = this.Menu["Turret.FontSize"].GetValue<MenuSlider>().Value,
                        OutputPrecision = FontPrecision.Default, Quality = FontQuality.Default
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
            try
            {
                if (!this.Menu["DrawHealth"].GetValue<MenuBool>().Enabled || Drawing.Direct3DDevice9.IsDisposed || Font.IsDisposed)
                {
                    return;
                }

                if (this.Menu["DrawTurrets"].GetValue<MenuBool>().Enabled)
                {
                    foreach (var turret in
                        ObjectManager.Get<AITurretClient>()
                            .Where(x => x != null && x.IsValid && !x.IsDead & x.HealthPercent <= 75))
                    {
                        var turretPosition = Drawing.WorldToMinimap(turret.Position);
                        var healthPercent = $"{(int)turret.HealthPercent}%";

                        Font.DrawText(
                            null,
                            healthPercent,
                            (int)
                            (turretPosition.X - Font.MeasureText(null, healthPercent, FontDrawFlags.Center).Right / 2f),
                            (int)
                            (turretPosition.Y - Font.MeasureText(null, healthPercent, FontDrawFlags.Center).Top / 2f),
                            new ColorBGRA(255, 255, 255, 255));
                    }
                }

                if (this.Menu["DrawInhibs"].GetValue<MenuBool>().Enabled)
                {
                    foreach (var inhibitor in
                        ObjectManager.Get<BarracksDampenerClient>()
                            .Where(x => x.IsValid && !x.IsDead && x.Health > 1 && x.HealthPercent <= 75))
                    {
                        var turretPosition = Drawing.WorldToMinimap(inhibitor.Position);
                        var healthPercent = $"{(int)inhibitor.HealthPercent}%";

                        Font.DrawText(
                            null,
                            healthPercent,
                            (int)
                            (turretPosition.X - Font.MeasureText(null, healthPercent, FontDrawFlags.Center).Right / 2f),
                            (int)
                            (turretPosition.Y - Font.MeasureText(null, healthPercent, FontDrawFlags.Center).Top / 2f),
                            new ColorBGRA(255, 255, 255, 255));
                    }
                }
            }
            catch (Exception e)
            {
                Logging.AddEntry(LoggingEntryType.Error, "BuildingTracker.cs: An error occurred: {0}", e);
            }
        }

        #endregion
    }
}