using System;
using System.Drawing;
using System.Net;
using System.Net.Cache;
using System.Security.Permissions;
using System.Security.Principal;
using EnsoulSharp;

namespace StormAIO
{
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    public static class Checker
    {
        private static readonly string ScriptVersion = "1.1";

        public static bool ServerStatus()
        {
            var Wc = new WebClient();
            try
            {
                Wc.CachePolicy =
                    new RequestCachePolicy(RequestCacheLevel.BypassCache);
                var Isonline =
                    Wc.DownloadString("https://raw.githubusercontent.com/noahdev2/Beta-tester/master/Server.txt");
                if (!Isonline.Contains("On"))
                {
                    Game.Print("Script Failed to Load Check Your Console");
                    Console.WriteLine("The Script is Disabled By Owner");
                    return false;
                }
            }
            catch
            {
                Game.Print("script failed to load Please Reload Reload Key {F5}");
                return false;
            }

            return true;
        }

        public static bool IsUpdatetoDate()
        {
            var Wc = new WebClient();
            try
            {
                Wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                var OnlineV =
                    Wc.DownloadString("https://raw.githubusercontent.com/noahdev2/Beta-tester/master/version.txt")
                        .Substring(0, 3);
                if (OnlineV != ScriptVersion)
                {
                    Game.Print("Script Failed to Load Check Your Console");
                    Console.WriteLine("The Script is outdated Please Update");
                    return false;
                }
            }
            catch
            {
                Game.Print("script failed to load Please Reload Reload Key {F5}");
                return false;
            }

            return true;
        }

        public static bool Hwid()
        {
            var securityIdentifier = WindowsIdentity.GetCurrent().User;
            if (securityIdentifier != null)
            {
            }

            var Wc = new WebClient();
            try
            {
                Wc.CachePolicy =
                    new RequestCachePolicy(RequestCacheLevel.BypassCache);
                var Isonline =
                    Wc.DownloadString("https://raw.githubusercontent.com/noahdev2/Beta-tester/master/hwid.txt");
                if (securityIdentifier != null && !Isonline.Contains(securityIdentifier.ToString()))
                {
                    Game.Print("ur Key doesn't exist in our database");
                    Console.WriteLine("ur Key doesn't exist in our database");
                    return false;
                }
            }
            catch
            {
                Game.Print("script failed to load Please Reload Reload Key {F5}");
                return false;
            }

            Game.Print("access granted", Color.LightGreen);
            return true;
        }
    }
}