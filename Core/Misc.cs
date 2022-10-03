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
                    champ = new[] { "KDA Akali", "MightyAIO","StormAIO" };
                    break;
                case "Alistar":
                    champ = new string[]
                        { "Easy_Sup" };
                    break;
                case "Annie":
                    champ = new string[]
                    {
                        "OKTW", "Flowers' Annie","Olympus.Annie"
                    };
                    break;
                case "Ashe":
                    champ = new string[]
                    {
                        "OKTW", "SharpShooter", "[SDK] Flowers' Series", "Flowers' ADC Series","StormAIO","Hikicarry ADC"
                    };
                    break;
                case "AurelionSol":
                    champ = new string[]
                    {
                        "OlympusAIO","Zypppy AurelionSol"
                    };
                    break;
                /*case "Bard":
                    champ = new string[]
                    {
                        "FreshBooster"
                    };
                    break;*/
                case "Blitzcrank":
                    champ = new string[]
                    {
                        "[SDK] Flowers' Series", "Z.Aio", "Easy_Sup","FreshBooster"
                    };
                    break;
                case "Braum":
                    champ = new string[]
                    {
                        "FreshBooster"
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
                        "OKTW", "[SDK] ExorAIO", "SluttyCaitlyn", "Caitlyn Master Headshot","ChallengerSeriesAIO","Hikicarry ADC"
                    };
                    break;
                case "Camille":
                    champ = new string[] { "hCamille", "Camille#", "Lord's Camille","Entropy.AIO" };
                    break;
                case "Cassiopeia":
                    champ = new string[]
                    {
                        "[SDK] ExorAIO", "LyrdumAIO","Entropy.AIO","Pentakill Cassiopeia"
                    };
                    break;
                case "Chogath":
                    champ = new string[]
                        { "MightyAIO", "StormAIO" };
                    break;
                case "Corki":
                    champ = new string[]
                    {
                        "OKTW", "SharpShooter", "[SDK] ExorAIO","hikiMarksman"
                    };
                    break;
                case "Darius":
                    champ = new string[]
                    {
                        "OKTW", "[SDK] ExorAIO", "[SDK] Flowers' Series","Entropy.AIO"
                    };
                    break;
                case "Draven":
                    champ = new[]
                    {
                        "SharpShooter", "[SDK] ExorAIO", "[SDK] Tyler1.exe","Hikicarry ADC","hikiMarksman"
                    };
                    break;
                case "Ekko":
                    champ = new string[]
                    {
                        "OKTW"
                    };
                    break;
                case "Evelynn":
                    champ = new[] { "OlympusAIO" };
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
                        "BadaoGangplank", "Bangplank", "BePlank", "e.Motion Gangplank", "GangplankBuddy", "Entropy.AIO","Perplexed Gangplank"
                    };
                    break;
                case "Garen":
                    champ = new[]
                    {
                        "StormAIO"
                    };
                    break;
                case "Graves":
                    champ = new string[]
                    {
                        "OKTW", "SharpShooter", "[SDK] ExorAIO","hikiMarksman"
                    };
                    break;
                case "Heimerdinger":
                    champ = new string[]
                    {
                        "OlympusAIO"
                    };
                    break;
                case "Illaoi":
                    champ = new[]
                    {
                        "Tentacle Kitty", "[SDK] Flowers' Series", "[SDK] Kraken Priestess", "IllaoiSOH"
                    };
                    break;
                case "Irelia":
                    champ = new[]
                    {
                        "Entropy.AIO"
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
                        "OKTW", "[SDK] ExorAIO", "EzAIO","SharpShooter","Hikicarry ADC"
                    };
                    break;
                case "Jinx":
                    champ = new[]
                    {
                        "OKTW", "EzAIO", "ADCPackage", "GENESIS Jinx", "SharpShooter", "xSalice", "[SDK] ExorAIO",
                        "MightyAIO","Nicky.Jinx","Hikicarry ADC"
                    };
                    break;
                case "Kalista":
                    champ = new[]
                    {
                        "Hermes Kalista", "OKTW", "SharpShooter", "[SDK] ExorAIO", "EzAIO", "EnsoulSharp.Kalista","ChallengerSeriesAIO","Hikicarry ADC"
                    };
                    break;
                case "Karthus":
                    champ = new string[]
                    {
                        "OKTW", "KimbaengKarthus", "Flowers' Karthus", "LyrdumAIO","Olympus.Karthus"
                    };
                    break;
                case "Katarina":
                    champ = new[] { "NoobAIO"/*"StormAIO"*/,"Nicky.Katarina" };
                    break;
                case "Kindred":
                    champ = new string[]
                    {
                        "OKTW"
                    };
                    break;
                case "KogMaw":
                    champ = new string[]
                    {
                        "StormAIO","SharpShooter"
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
                /*case "Lillia":
                    champ = new string[] { "MightyAIO" };
                    break;*/
                case "Lissandra":
                    champ = new string[] { "OlympusAIO" };
                    break;
                case "Lucian":
                    champ = new[]
                    {
                        "OKTW", "SharpShooter", "[SDK] ExorAIO", "Flowers' ADC Series", "EzAIO", "MightyAIO","StormAIO","Hikicarry ADC","hikiMarksman"
                    };
                    break;
                case "Lux":
                    champ = new[]
                    {
                        "OKTW", "M00N Lux", "LyrdumAIO", "Easy_Sup"
                    };
                    break;
                case "Maokai":
                    champ = new[]
                    {
                        "StormAIO"
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
                        "Easy_Sup","Danz0r AIO"
                    };
                    break;
                case "Neeko":
                    champ = new[]
                    {
                        "Entropy.AIO"
                    };
                    break;
                case "Poppy":
                    champ = new[]
                    {
                        "OlympusAIO"
                    };
                    break;
                case "Quinn":
                    champ = new[]
                    {
                        "Hikicarry ADC"
                    };
                    break;
                case "Pyke":
                    champ = new string[] { "Easy_Sup" };
                    break;
                case "Rengar":
                    champ = new string[] { "StormAIO" };
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
                case "Singed":
                    champ = new[]
                    {
                        "ElSinged"
                    };
                    break;
                case "Sivir":
                    champ = new string[]
                    {
                        "OKTW", "SharpShooter", "[SDK] Flowers' Series","Hikicarry ADC","hikiMarksman"
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
                case "Teemo":
                    champ = new string[] { "OlympusAIO" };
                    break;
                case "Thresh":
                    champ = new[]
                    {
                        "OKTW", "SluttyThresh", "LyrdumAIO", "Easy_Sup"
                    };
                    break;
                case "Tristana":
                    champ = new[]
                    {
                        "Hikicarry ADC"
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
                        "SharpShooter", "[SDK] ExorAIO", "NoobAIO","StormAIO"
                    };
                    break;
                case "Urgot":
                    champ = new string[]
                    {
                        "StormAIO"
                    };
                    break;
                case "Varus":
                    champ = new string[]
                    {
                        "ElVarus", "OKTW", "SharpShooter","Hikicarry ADC"
                    };
                    break;
                case "Vayne":
                    champ = new[]
                    {
                        "OKTW", "SharpShooter", "hi im gosu", "[SDK] ExorAIO", "[SDK] Flowers' Series", "EzAIO","hi im gosu Reborn","Hikicarry ADC"
                    };
                    break;
                case "Veigar":
                    champ = new[]
                    {
                        "Olympus.Veigar"
                    };
                    break;
                case "Viktor":
                    champ = new[]
                    {
                        "Badao's Viktor", "[SDK] Flowers' Series", "[SDK] Flowers' Viktor"
                    };
                    break;
                case "Vladimir":
                    champ = new[]
                    {
                        "StormAIO"
                    };
                    break;
                case "Warwick":
                    champ = new[]
                    {
                        "StormAIO"
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
                case "Yone":
                    champ = new[]
                    {
                        "StormAIO"
                    };
                    break;
                case "Yorick":
                    champ = new[] { "MightyAIO","StormAIO" };
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
                        "Korean Zed", "SharpyAIO", "Ze-D is Back","StormAIO"
                    };
                    break;
                case "Zoe":
                    champ = new[] { "MightyAIO" };
                    break;
                default:
                    hasDualPort = false;
                    dualPort.Add(new MenuSeparator("info1", $"There are no dual-port for {ObjectManager.Player.CharacterName}."));
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
            dutility.Add(new MenuList("Tracker", "Which Tracker?", new[] { "NabbTracker", "Tracker#","ElUtilitySuite" }));
            dutility.Add(new MenuBool("enableEvade", "Enable Evade", false));
            dutility.Add(new MenuList("Evade", "Which Evade?", new[] { "Evade#" }));
            dutility.Add(new MenuBool("enableSkinChanger", "Enable Skin Changer", false));
            dutility.Add(new MenuList("SkinChanger", "Which Skin Changer?", new[] { "EnsoulSharp.SkinHack" }));
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