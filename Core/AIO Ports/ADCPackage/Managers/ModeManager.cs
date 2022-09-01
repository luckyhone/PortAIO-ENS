using ADCPackage.Plugins;
using EnsoulSharp.SDK;

namespace ADCPackage
{
    class OrbwalkerSwitch
    {
        public static void Update()
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    switch (PluginLoader.Champname)
                    {
                        case "Jinx":
                            Jinx.Combo();
                            break;
                        case "Tristana":
                            //Tristana.Combo();
                            break;
                        case "Corki":
                            //Corki.Combo();
                            break;
                    }
                    break;
                case OrbwalkerMode.Harass:
                    switch (PluginLoader.Champname)
                    {
                        case "Jinx":
                            Jinx.Harass();
                            break;
                        case "Tristana":
                            //Tristana.Harass();
                            break;
                        case "Corki":
                            //Corki.Harass();
                            break;
                    }
                    break;
                case OrbwalkerMode.LaneClear:
                    switch (PluginLoader.Champname)
                    {
                        case "Jinx":
                            Jinx.LaneClear();
                            break;
                        case "Tristana":
                            //Tristana.LaneClear();
                            break;
                        case "Corki":
                            //Corki.LaneClear();
                            break;
                    }
                    break;
            }
        }
    }
}