using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace PortAIO
{
    class Misc
    {
        public static Menu menu, info;
        public static string[] champ = new string[] { };

        public static void Load()
        {
            info = new Menu("PAIOInfo", "[~] PortAIO - Info", true);
            info.Add(new MenuSeparator("aioBerb", "PortAIO - By Berb"));
            info.Add(new MenuSeparator("aioEweEwe", "Reported - By Eubb"));
            info.Add(new MenuSeparator("aioVersion", "Version : " + Program.version));
            info.Add(new MenuSeparator("aioNote", "Note : Make sure you're in Borderless!"));
            info.Attach();

            menu = new Menu("PAIOMisc", "[~] PortAIO - Ports", true);

            var dualPort = new Menu("DualPAIOPort", "Dual-Port");
            menu.Add(dualPort);

            var hasDualPort = true;

            switch (GameObjects.Player.CharacterName)
            {
                #region Dual-Ports
                case "Aatrox":
                    champ = new[]
                    {
                        "Entropy.AIO"
                    };
                    break;
                case "Ahri":
                    champ = new[]
                    {
                        "OKTW", "DZAhri", "EloFactory Ahri", "xSalice", "BadaoSeries", "AhriSharp",
                        "[SDK] Flowers' Series", "Babehri", "Entropy.AIO"
                    };
                    break;
                case "Akali":
                    champ = new[] { "KDA Akali", "MightyAIO" };
                    break;
                case "Alistar":
                    champ = new string[]
                        { "Easy_Sup" };
                    break;
                case "Annie":
                    champ = new string[]
                    {
                        "OKTW", "Flowers' Annie"
                    };
                    break;
                case "Ashe":
                    champ = new string[]
                    {
                        "OKTW", "SharpShooter", "[SDK] Flowers' Series", "Flowers' ADC Series"
                    };
                    break;
                case "Blitzcrank":
                    champ = new string[]
                    {
                        "[SDK] Flowers' Series", "Z.Aio", "Easy_Sup"
                    };
                    break;
                case "Brand":
                    champ = new string[]
                    {
                        "LyrdumAIO"
                    };
                    break;
                case "Caitlyn":
                    champ = new[]
                    {
                        "OKTW", "[SDK] ExorAIO", "SluttyCaitlyn", "Caitlyn Master Headshot"
                    };
                    break;
                case "Camille":
                    champ = new string[] { "hCamille", "Camille#", "Lord's Camille" };
                    break;
                case "Cassiopeia":
                    champ = new string[]
                    {
                        "[SDK] ExorAIO", "LyrdumAIO"
                    };
                    break;
                case "Chogath":
                    champ = new string[]
                        { "MightyAIO" };
                    break;
                case "Corki":
                    champ = new string[]
                    {
                        "OKTW", "SharpShooter", "[SDK] ExorAIO"
                    };
                    break;
                case "Darius":
                    champ = new string[]
                    {
                        "OKTW", "[SDK] ExorAIO", "[SDK] Flowers' Series"
                    };
                    break;
                case "Draven":
                    champ = new[]
                    {
                        "SharpShooter", "[SDK] ExorAIO", "[SDK] Tyler1.exe"
                    };
                    break;
                case "Ekko":
                    champ = new string[]
                    {
                        "OKTW"
                    };
                    break;
                case "Ezreal":
                    champ = new[] { "OKTW", "EzAIO", "SharpShooter", "MightyAIO" };
                    break;
                case "Fizz":
                    champ = new string[]
                    {
                        "MightyAIO"
                    };
                    break;
                case "Gangplank":
                    champ = new[]
                    {
                        "BadaoGangplank", "Bangplank", "BePlank", "e.Motion Gangplank", "GangplankBuddy", "Entropy.AIO"
                    };
                    break;
                case "Graves":
                    champ = new string[]
                    {
                        "OKTW", "SharpShooter", "[SDK] ExorAIO"
                    };
                    break;
                case "Illaoi":
                    champ = new[]
                    {
                        "Tentacle Kitty", "[SDK] Flowers' Series", "[SDK] Kraken Priestess", "IllaoiSOH"
                    };
                    break;
                case "Jax":
                    champ = new[]
                    {
                        "[SDK] ExorAIO", "NoobAIO"
                    };
                    break;
                case "Jayce":
                    champ = new[] { "OKTW", "[SDK] Shulepin Jayce" };
                    break;
                case "Jhin":
                    champ = new[]
                    {
                        "OKTW", "[SDK] ExorAIO", "EzAIO"
                    };
                    break;
                case "Jinx":
                    champ = new[]
                    {
                        "OKTW", "EzAIO", "ADCPackage", "GENESIS Jinx", "SharpShooter", "xSalice", "[SDK] ExorAIO",
                        "MightyAIO"
                    };
                    break;
                case "Kalista":
                    champ = new[]
                    {
                        "Hermes Kalista", "OKTW", "SharpShooter", "[SDK] ExorAIO", "EzAIO", "EnsoulSharp.Kalista"
                    };
                    break;
                case "Karthus":
                    champ = new string[]
                    {
                        "OKTW", "KimbaengKarthus", "Flowers' Karthus", "LyrdumAIO"
                    };
                    break;
                case "Katarina":
                    champ = new[] { "NoobAIO" };
                    break;
                case "Kayn":
                    champ = new[] { "NoobAIO" };
                    break;
                case "Kindred":
                    champ = new string[]
                    {
                        "OKTW"
                    };
                    break;
                case "Leblanc":
                    champ = new string[] { "Entropy.AIO" };
                    break;
                case "Leona":
                    champ = new string[]
                    {
                        "Z.Aio"
                    };
                    break;
                case "Lillia":
                    champ = new string[] { "MightyAIO" };
                    break;
                case "Lucian":
                    champ = new[]
                    {
                        "OKTW", "SharpShooter", "[SDK] ExorAIO", "Flowers' ADC Series", "EzAIO", "MightyAIO"
                    };
                    break;
                case "Lux":
                    champ = new[]
                    {
                        "OKTW", "M00N Lux", "LyrdumAIO", "Easy_Sup"
                    };
                    break;
                case "MasterYi":
                    champ = new string[]
                    {
                        "NoobAIO"
                    };
                    break;
                case "Morgana":
                    champ = new string[]
                    {
                        "Easy_Sup"
                    };
                    break;
                case "Neeko":
                    champ = new[]
                    {
                        "Entropy.AIO"
                    };
                    break;
                case "Pyke":
                    champ = new string[] { "Easy_Sup" };
                    break;
                case "Riven":
                    champ = new string[]
                    {
                        "MightyAIO"
                    };
                    break;
                case "Senna":
                    champ = new string[] { "MightyAIO" };
                    break;
                case "Shen":
                    champ = new string[]
                        { "MightyAIO" };
                    break;
                case "Shyvana":
                    champ = new string[]
                        { "NoobAIO" };
                    break;
                case "Sivir":
                    champ = new string[]
                    {
                        "OKTW", "SharpShooter", "[SDK] Flowers' Series"
                    };
                    break;
                case "Skarner":
                    champ = new string[] { "MightyAIO" };
                    break;
                case "Soraka":
                    champ = new string[]
                    {
                        "Easy_Sup"
                    };
                    break;
                case "Syndra":
                    champ = new string[]
                    {
                        "BadaoSeries", "OKTW"
                    };
                    break;
                case "Taliyah":
                    champ = new string[] { "[SDK] ExorAIO" };
                    break;
                case "Thresh":
                    champ = new[]
                    {
                        "OKTW", "SluttyThresh", "LyrdumAIO", "Easy_Sup"
                    };
                    break;
                case "TwistedFate":
                    champ = new string[]
                    {
                        "SharpShooter", "BadaoSeries", "NoobAIO"
                    };
                    break;
                case "Twitch":
                    champ = new string[]
                    {
                        "SharpShooter", "[SDK] ExorAIO", "NoobAIO"
                    };
                    break;
                case "Varus":
                    champ = new string[]
                    {
                        "ElVarus", "OKTW", "SharpShooter"
                    };
                    break;
                case "Vayne":
                    champ = new[]
                    {
                        "OKTW", "SharpShooter", "hi im gosu", "[SDK] ExorAIO", "[SDK] Flowers' Series", "EzAIO"
                    };
                    break;
                case "Viktor":
                    champ = new[]
                    {
                        "Badao's Viktor", "[SDK] Flowers' Series", "[SDK] Flowers' Viktor"
                    };
                    break;
                case "Xayah":
                    champ = new[] { "SharpShooter" };
                    break;
                case "Yasuo":
                    champ = new[]
                    {
                        "Flowers' Yasuo"
                    };
                    break;
                case "Yorick":
                    champ = new[] { "MightyAIO" };
                    break;
                case "Yuumi":
                    champ = new[] { "MightyAIO" };
                    break;
                case "Zac":
                    champ = new[] { "MightyAIO" };
                    break;
                case "Zed":
                    champ = new[]
                    {
                        "Korean Zed", "SharpyAIO", "Ze-D is Back"
                    };
                    break;
                case "Zoe":
                    champ = new[] { "MightyAIO" };
                    break;
                default:
                    hasDualPort = false;
                    dualPort.Add(new MenuSeparator("info1", "There are no dual-port for this champion."));
                    dualPort.Add(new MenuSeparator("info2", "Feel free to request one."));
                    break;

                #endregion
            }

            if (hasDualPort)
            {
                dualPort.Add(new MenuList(ObjectManager.Player.CharacterName.ToString(), "Which dual-port?", champ));
            }

            var dutility = new Menu("Utilitiesports", "Dual-Utilities");
            dutility.Add(new MenuBool("enableTracker", "Enable Tracker", false));
            dutility.Add(new MenuList("Tracker", "Which Tracker?", new[] { "NabbTracker", "Tracker#" }));
            dutility.Add(new MenuBool("enableEvade", "Enable Evade", false));
            dutility.Add(new MenuList("Evade", "Which Evade?", new[] { "Evade#" }));
            //dutility.Add(new MenuBool("enablePredictioner", "Enable Predictioner", false));
            /*dutility.Add(new MenuList("Predictioner", "Which Predictioner?",
                new[] {"SPredictioner", "OKTWPredictioner", "L#Predictioner"}));*/

            menu.Add(dutility);

            var utility = new Menu("PortAIOuTILITIESS", "Standalone Utilitie");
            utility.Add(new MenuBool("ShadowTracker", "Enable ShadowTracker", false));
            utility.Add(new MenuBool("BaseUlt3", "Enable BaseUlt3", false));
            utility.Add(new MenuBool("PerfectWardReborn", "Enable PerfectWardReborn", false));
            utility.Add(new MenuBool("UniversalRecallTracker", "Enable UniversalRecallTracker", false));
            utility.Add(new MenuBool("UniversalGankAlerter", "Enable UniversalGankAlerter", false));
            utility.Add(new MenuBool("UniversalMinimapHack", "Enable UniversalMinimapHack", false));
            utility.Add(new MenuBool("BasicChatBlock", "Enable BasicChatBlock", false));
            utility.Add(new MenuBool("CSCounter", "Enable CSCounter", false));
            utility.Add(new MenuBool("DeveloperSharp", "Enable DeveloperSharp", false));


            menu.Add(utility);

            menu.Add(new MenuBool("UtilityOnly", "Utility Only", false));
            menu.Add(new MenuBool("ChampsOnly", "Champs Only", false));
            menu.Attach();

        }
    }
}