
using System.Net;
using System.Reflection;
using EnsoulSharp;

namespace Easy_Sup
{
    class Update
    {
        public static void Check()
        {
            try
            {
                using (var wb = new WebClient())
                {
                    var raw = wb.DownloadString("https://raw.githubusercontent.com/011110001/EnsoulSharp/master/Easy_Sup/Version.txt");

                    System.Version Version = Assembly.GetExecutingAssembly().GetName().Version;

                    if (raw != Version.ToString())
                    {
                        Game.Print("<font color=\"#ff0000\">Easy_Sup is outdated! Please update to {0}!</font>", raw);
                    }
                    else
                        Game.Print("<font color=\"#ff0000\">Easy_Sup is updated! Version : {0}!</font>", Version.ToString());
                }

            }
            catch
            {
                Game.Print("<font color=\"#ff0000\">Failed to verify script version!</font>");
            }
        }
    }
}