using EnsoulSharp;
using System.Net;
using System.Reflection;

namespace KDA_Akali
{
    class Update
    {
        public static void Check()
        {
            try
            {
                using (var wb = new WebClient())
                {
                    var raw = wb.DownloadString("https://raw.githubusercontent.com/011110001/EnsoulSharp/master/KDA_Akali/Version.txt");

                    System.Version Version = Assembly.GetExecutingAssembly().GetName().Version;

                    if (raw != Version.ToString())
                    {
                        Game.Print("<font color=\"#ff0000\">KDA_Akali is outdated! Please update to {0}!</font>", raw);
                    }
                }

            }
            catch
            {
                Game.Print("<font color=\"#ff0000\">Failed to verify script version!</font>");
            }
        }
    }
}