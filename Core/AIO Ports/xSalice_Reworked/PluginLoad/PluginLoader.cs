using EnsoulSharp;
using LeagueSharpCommon.Notifications;
using xSalice_Reworked.Pluging;

namespace xSalice_Reworked.PluginLoad
{
    public class PluginLoader
    {
        private static bool _loaded;

        public PluginLoader()
        {
            if (!_loaded)
            {
                switch (ObjectManager.Player.CharacterName.ToLower())
                {
                    case "ahri":
                        var ahri = new Ahri();
                        _loaded = true;
                        break;
                    case "jinx":
                        var jinx = new Jinx();
                        _loaded = true;
                        break;
                    default:
                        Notifications.AddNotification(ObjectManager.Player.CharacterName + " not supported!!", 10000);
                        break;
                }
            }
        }
    }
}