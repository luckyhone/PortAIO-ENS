using EnsoulSharp.SDK.MenuUI;
using Entropy.Awareness.Bases;

namespace Entropy.Awareness
{
    public class Menus
    {
        public Menus()
        {
            Initialize();
        }

        private static void Initialize()
        {
            var trackerMenu = new Menu("tracker", "Tracker")
            {
                MenuComponents.SpellTracker.TrackAllies,
                MenuComponents.SpellTracker.TrackEnemies,
                MenuComponents.SpellTracker.SpellLevels
            };

            var guiTrackerMenu = new Menu("guiTracker", "GUI Tracker")
            {
                MenuComponents.GuiTracker.TrackAllies,
                MenuComponents.GuiTracker.TrackEnemies,
                MenuComponents.GuiTracker.TrackerScale
            };

            var menuList = new[]
            {
                trackerMenu,
                guiTrackerMenu
            };

            foreach (var menu in menuList)
            {
                MenuBase.Root.Add(menu);
            }
        }
    }
}