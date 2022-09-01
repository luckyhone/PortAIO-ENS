using EnsoulSharp.SDK.MenuUI;

namespace Entropy.Awareness
{
    public static class MenuComponents
    {
        public static class SpellTracker
        {
            public static readonly MenuBool TrackAllies = new MenuBool("trackAllies", "Track Allies");
            public static readonly MenuBool TrackEnemies = new MenuBool("trackEnemies", "Track Enemies", false);
            public static readonly MenuBool SpellLevels = new MenuBool("spellLevels", "Spell Levels", false);
        }

        public static class GuiTracker
        {
            public static readonly MenuBool TrackAllies = new MenuBool("guiTrackAllies", "Track allies");
            public static readonly MenuBool TrackEnemies = new MenuBool("guiTrackEnemies", "Track Enemies");
            public static readonly MenuSlider TrackerScale = new MenuSlider("guiTrackerScale", "Scale", 80);
        }

    }
}