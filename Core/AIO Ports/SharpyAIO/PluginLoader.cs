using System;
using EnsoulSharp;

namespace Sharpy_AIO
{
    class PluginLoader
    {
        internal static bool LoadPlugin(string PluginName)
        {
            if (CanLoadPlugin(PluginName))
            {
                DynamicInitializer.NewInstance(Type.GetType("Sharpy_AIO.Plugins." + ObjectManager.Player.CharacterName));
                return true;
            }

            return false;
        }
        internal static bool CanLoadPlugin(string PluginName)
        {
            return Type.GetType("Sharpy_AIO.Plugins." + ObjectManager.Player.CharacterName) != null;
        }
    }
}