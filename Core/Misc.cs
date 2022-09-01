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

            //string[] champ = new string[] { };
            switch (GameObjects.Player.CharacterName)
            {
                #region Dual-Ports

                case "Ahri":
                    champ = new[]
                    {
                        "OKTW [Listo]", "DZAhri [Listo]", "EloFactory Ahri [Listo]", "KappaSeries", "xSalice [Listo]",
                        "BadaoSeries [Listo]", "M1D 0R F33D", "AhriSharp [Listo]", "[SDK] Flowers' Series [Listo]",
                        "Babehri [Listo]", "EasyAhri", "SenseAhri","Entropy.AIO [Listo]"
                    };
                    break;
                case "Akali":
                    champ = new[] {"KDA Akali [Listo]", "MightyAIO [Listo]"};
                    break;
                case "Alistar":
                    champ = new string[]
                        {"ElAlistar", "Support Is Easy", "FreshBooster", "vSeries", "SkyAlistar", "Easy_Sup [Listo]"};
                    break;
                case "Annie":
                    champ = new string[]
                    {
                        "OKTW [Listo]", "Korean Annie", "SharpyAIO", "Support is Easy", "EloFactory Annie",
                        "Flowers' Annie [Listo]", "OAnnie", "ReformedAIO", "mySeries"
                    };
                    break;
                case "Ashe":
                    champ = new string[]
                    {
                        "OKTW [Listo]", "ProSeries", "ReformedAIO", "SharpShooter [Listo]", "[SA] SurvivorSeries",
                        "xSalice", "Marksman#", "[SDK] The Queen of the Ice", "[SDK] ChallengerSeriesAIO",
                        "[SDK] Dicaste's Ashe", "[SDK] ExorAIO", "[SDK] Flowers' Series [Listo]", "[SDK] xcsoft's Ashe",
                        "Ashe#", "CarryAshe", "SNAshe", "ProjectGeass", "Hikicarry ADC", "Flowers' ADC Series [Listo]",
                        "SurvivorSeriesAIO"
                    };
                    break;
                case "Blitzcrank":
                    champ = new string[]
                    {
                        "OKTW", "FreshBooster", "KurisuBlitz", "SAutoCarry", "SharpShooter", "ShineAIO",
                        "Support is Easy", "vSeries", "[SDK] Flowers' Series [Listo]", "[SDK] xcsoft's Blitzcrank",
                        "JustBlitzcrank", "SluttyBlitz", "sAIO", "Z.Aio [Listo]", "Easy_Sup [Listo]"
                    };
                    break;
                case "Brand":
                    champ = new string[]
                    {
                        "The Brand", "Hikicarry Brand", "OKTW", "[SA] SurvivorSeries", "yol0 Brand", "DevBrand",
                        "Flowers' Brand", "Kimbaeng Brand", "sBrand", "SN Brand", "SurvivorSeriesAIO", "SergixBrand",
                        "Shulepin's Brand", "ReformedAIO", "LyrdumAIO [Listo]"
                    };
                    break;
                case "Caitlyn":
                    champ = new[]
                    {
                        "OKTW [Listo]", "SharpShooter [Listo]", "Marksman#", "[SDK] ChallengerSeriesAIO",
                        "[SDK] ExorAIO [Listo]", "SluttyCaitlyn [Listo]", "Hikicarry ADC", "Flowers' ADC Series", "ReformedAIO",
                        "Caitlyn Master Headshot [Listo]"
                    };
                    break;
                case "Camille":
                    champ = new string[] {"hCamille [Listo]", "Camille# [Listo]", "Lord's Camille [Listo]"};
                    break;
                case "Cassiopeia":
                    champ = new string[]
                    {
                        "SAutoCarry", "SFXChallenger", "SharpyAIO", "xSalice", "TheCassiopeia", "[SDK] ExorAIO [Listo]",
                        "Eat My Cass", "mztikk's Cass", "RiseOfThePython", "sAIO", "DevCassio2", "LyrdumAIO [Listo]"
                    };
                    break;
                case "Chogath":
                    champ = new string[]
                        {"UnderratedAIO", "Windwalker Cho'Gath", "xSalice", "Troop'Gath", "MightyAIO [Listo]"};
                    break;
                case "Corki":
                    champ = new string[]
                    {
                        "El Corki", "ADCPackage", "D-Corki", "hikiMarksman", "OKTW [Listo]", "ProSeries", "SAutoCarry",
                        "SharpShooter [Listo]", "xSalice", "Marksman#", "[SDK] ExorAIO [Listo]", "EasyCorki", "jhkCorki",
                        "LeCorki", "PewPewCorki", "Flowers' ADC Series"
                    };
                    break;
                case "Darius":
                    champ = new string[]
                    {
                        "OKTW [Listo]", "ElEasy", "SAutoCarry", "[SDK] ExorAIO [Listo]", "[SDK] Flowers' Series [Listo]",
                        "Darius#", "KurisuDarius", "ODarius", "PerfectDarius", "sAIO", "Flower's Darius"
                    };
                    break;
                case "Draven":
                    champ = new[]
                    {
                        "OKTW", "hikiMarksman", "SharpShooter [Listo]", "Marksman#", "M00N Draven", "[SDK] ExorAIO [Listo]",
                        "[SDK] Tyler1.exe [Listo]", "badaoDraven", "myWorld AIO", "Hikicarry ADC", "Flowers' ADC Series"
                    };
                    break;
                case "Ekko":
                    champ = new string[]
                    {
                        "OKTW [Listo]", "EloFactory Ekko", "xSalice", "Ekko Master of Time",
                        "Ekko The Boy Who Shattered Time", "EkkoGod", "ElEkko", "HikiCarry Ekko", "TheEkko"
                    };
                    break;
                case "Ezreal":
                    champ = new[] {"OKTW [Listo]", "EzAIO [Listo]", "SharpShooter [Listo]", "MightyAIO [Listo]"};
                    break;
                case "Fizz":
                    champ = new string[]
                    {
                        "Math Fizz", "ElFizz", "UnderratedAIO", "HeavenStrikeFizz", "NoobFizz", "OneKeyToFish",
                        "Lord's Fizz", "MightyAIO [Listo]"
                    };
                    break;
                case "Gangplank":
                    champ = new[]
                    {
                        "UnderratedAIO", "BadaoGangplank [Listo]", "Bangplank [Listo]", "BePlank [Listo]",
                        "e.Motion Gangplank [Listo]", "GangplankBuddy [Listo]","Entropy.AIO [Listo]"
                    };
                    break;
                case "Graves":
                    champ = new string[]
                    {
                        "OKTW [Listo]", "D-Graves", "hikiMarksman", "Kurisu Graves", "SFXChallenger", "SharpShooter [Listo]",
                        "Marksman#", "[SDK] ExorAIO [Listo]", "[SDK] Flowers' Series", "[SDK] VSTGraves", "EasyGraves",
                        "BadaoGraves", "Flowers' ADC Series"
                    };
                    break;
                case "Illaoi":
                    champ = new[]
                    {
                        "Tentacle Kitty [Listo]", "SharpShooter", "[SDK] Flowers' Series [Listo]",
                        "[SDK] Kraken Priestess [Listo]", "IllaoiSOH [Listo]", "TentacleBabeIllaoi"
                    };
                    break;
                case "Jax":
                    champ = new[]
                    {
                        "xQx Jax", "BrianSharp", "NoobJaxReloaded", "SAutoCarry", "UnderratedAIO", "[SDK] ExorAIO [Listo]",
                        "SkyLv Jax", "NoobAIO [Listo]"
                    };
                    break;
                case "Jayce":
                    champ = new[] { "OKTW [Listo]", "Hikicarry Jayce", "xSalice", "AJayce", "JayceSharpV2", "[SDK] Shulepin Jayce [Listo]","SharpShooter" };
                    break;
                case "Jhin":
                    champ = new[]
                    {
                        "OKTW [Listo]", "Hikigaya's Jhin", "SAutoCarry", "Marksman#", "[SDK] ExorAIO [Listo]", "[SDK] hJhin",
                        "Jhin as the Virtuoso", "BadaoJhin", "[SDK] Tc_SDKexAIO", "Flowers' Jhin", "Hikicarry ADC",
                        "Flowers' ADC Series", "EzAIO [Listo]",   
                    };
                    break;
                case "Jinx":
                    champ = new[]
                    {
                        "OKTW [Listo]", "EzAIO [Listo]", "ADCPackage [Listo]", "GENESIS Jinx [Listo]", "iSeriesReborn",
                        "ProSeries", "SharpShooter [Listo]", "xSalice [Listo]", "Marksman#", "[SDK] ExorAIO [Listo]", "CjShu Jinx",
                        "EasyJinx", "EloFactory Jinx", "GenerationJinx", "PennyJinxReborn", "myWorld AIO",
                        "[SDK] Tc_SDKexAIO", "Hikicarry ADC", "Flowers' ADC Series", "MightyAIO [Listo]"
                    };
                    break;
                case "Kalista":
                    champ = new[]
                    {
                        "S+ Class Kalista", "HERMES Kalista [Listo]", "Hikicarry Kalista", "iSeriesReborn",
                        "OKTW [Listo]", "SAutoCarry", "SFXChallenger", "SharpShooter [Listo]", "Marksman#",
                        "[SDK] ChallengerSeriesAIO", "[SDK] ExorAIO [Listo]", "[SDK] xcsoft's Kalista", "DonguKalista",
                        "EasyKalista", "ElKalista", "iKalista", "iKalista:Reborn", "Kalima", "KAPPALISTAXD",
                        "Hikicarry ADC", "Flowers' ADC Series", "EzAIO [Listo]", "EnsoulSharp.Kalista [Listo]"
                    };
                    break;
                case "Karthus":
                    champ = new string[]
                    {
                        "OKTW [Listo]", "SharpShooter", "xSalice", "[SDK] RAREKarthus", "Karthus#",
                        "KimbaengKarthus [Listo]", "SNKarthus", "XDSharpAIO", "[SDK] ExorAIO",
                        "Flowers' Karthus [Listo]", "AlqoholicKarthus", "LyrdumAIO [Listo]"
                    };
                    break;
                case "Katarina":
                    champ = new[] {"NoobAIO [Listo]"};
                    break;
                case "Kayn":
                    champ = new[] {"NoobAIO [Listo]"};
                    break;
                case "Kindred":
                    champ = new string[]
                    {
                        "Yin & Yang", "OKTW [Listo]", "SharpShooter", "Marksman#", "KindredSpirits", "SluttyKindred",
                        "Flowers' ADC Series"
                    };
                    break;
                case "Leblanc":
                    champ = new string[] { "Leblanc II", "FreshBooster", "LCS Leblanc", "M1D 0R F33D", "BadaoLeBlanc", "PopBlanc","Entropy.AIO [Listo]" };
                    break;
                case "Leona":
                    champ = new string[]
                    {
                        "ElEasy", "Support is Easy", "vSeries", "SethLeona", "Troopeona", "sAIO", "Kuroko's Leona",
                        "Z.Aio [Listo]"
                    };
                    break;
                case "Lillia":
                    champ = new string[] {"MightyAIO [Listo]"};
                    break;
                case "Lucian":
                    champ = new[]
                    {
                        "LCS Lucian", "BrianSharp", "hikiMarksman", "Hoola Lucian", "iLucian", "iSeriesReborn",
                        "KoreanLucian", "OKTW [Listo]", "SAutoCarry", "SharpShooter [Listo]", "xSalice", "Marksman#",
                        "[SDK] ChallengerSeriesAIO", "[SDK] ExorAIO [Listo]", "D_Lucian", "FuckingLucianReborn", "SluttyLucian",
                        "Hikicarry ADC", "Flowers' ADC Series [Listo]", "ReformedAIO", "Lord's Lucian", "EzAIO [Listo]",
                        "MightyAIO [Listo]"
                    };
                    break;
                case "Lux":
                    champ = new[]
                    {
                        "OKTW [Listo]", "vSeries", "M1D 0R F33D", "M00N Lux [Listo]", "[SDK] ExorAIO", "CheerleaderLux",
                        "ElLux", "Hikigaya Lux", "SephLux", "ReformedAIO", "LyrdumAIO [Listo]", "Easy_Sup [Listo]"
                    };
                    break;
                case "MasterYi":
                    champ = new string[]
                    {
                        "MasterSharp", "Hoola Yi", "SAutoCarry", "MasterYi by Prunes", "xQx Yi", "Yi by Crisdmc",
                        "[SDK] ChallengerYi", "NoobAIO [Listo]"
                    };
                    break;
                case "Morgana":
                    champ = new string[]
                    {
                        "Kurisu Morgana", "FreshBooster", "OKTW", "ShineAIO", "Support is Easy", "vSeries",
                        "[SDK] Flowers' Series", "Easy_Sup [Listo]"
                    };
                    break;
                case "Pyke":
                    champ = new string[] {"Easy_Sup [Listo]"};
                    break;
                case "Riven":
                    champ = new string[]
                    {
                        "KurisuRiven", "Hoola Riven", "SAutoCarry", "Nechrito Riven", "[SDK] Flowers' Series",
                        "[SDK] ReforgedRiven", "EasyPeasyRivenSqueezy", "EloFactory Riven", "Flowers' Riven",
                        "HeavenStrikeRiven", "yol0Riven", "RivenToTheChallenger", "MoonRiven", "MightyAIO [Listo]"
                    };
                    break;
                case "Senna":
                    champ = new string[] {"MightyAIO [Listo]"};
                    break;
                case "Shen":
                    champ = new string[]
                        {"UnderratedAIO", "BrianSharp", "Kimbaeng Shen", "Badao Shen", "xQx Shen", "MightyAIO [Listo]"};
                    break;
                case "Shyvana":
                    champ = new string[]
                        {"D-Shyvana", "HeavenStrike Shyvana", "JustShyvana", "SAutoCarry", "NoobAIO [Listo]"};
                    break;
                case "Sivir":
                    champ = new string[]
                    {
                        "OKTW [Listo]", "hikiMarksman", "ProSeries", "SFXChallenger", "SharpShooter [Listo]",
                        "ShineAIO",
                        "Marksman#", "[SDK] ExorAIO", "[SDK] Flowers' Series [Listo]", "[SDK] xcsoft's Sivir",
                        "HeavenStrikeSivir", /*"iSivir",*/ "JustSivir", "KurisuSivir", "Hikicarry ADC",
                        "Flowers' ADC Series"
                    };
                    break;
                case "Skarner":
                    champ = new string[] {"UnderratedAIO", "kSkarner", "SneakySkarner", "MightyAIO [Listo]"};
                    break;
                case "Soraka":
                    champ = new string[]
                    {
                        "SephSoraka", "FreshBooster", "Heal-Bot", "MLG Soraka", "Support is Easy", "vSeries",
                        "[SDK] ChallengerSeriesAIO", "Sophie's Soraka", "Soraka#", "SorakaToTheChallenger",
                        "Easy_Sup [Listo]"
                    };
                    break;
                case "Syndra":
                    champ = new string[]
                    {
                        "Syndra by Kortatu", "BadaoSeries [Listo]", "Hikigaya Syndra", "OKTW [Listo]", "Syndra by L33T",
                        "vSeries", "xSalice", "SephSyndra", "Syndra - The Dark Sovereign", "Syndra - Dark Mage",
                        "Lord's Syndra", "SyndraRevamped"
                    };
                    break;
                case "Taliyah":
                    champ = new string[] {"Toph#", "[SDK] ExorAIO [Listo]", "[SDK] StoneWeaver", "Hinata's Taliyah"};
                    break;
                case "Thresh":
                    champ = new[]
                    {
                        "Danz - Chain Warden", "FreshBooster", "OKTW [Listo]", "Support is Easy", "vSeries",
                        "Dark Star Thresh", "SluttyThresh [Listo]", "Thresh as The Chain Warden", "Thresh - Catch Fish",
                        "Thresh - the Ruler of the Soul", "Thresh the Flay Maker", "yol0 Thresh", "ReformedAIO",
                        "LyrdumAIO [Listo]", "Easy_Sup [Listo]"
                    };
                    break;
                case "TwistedFate":
                    champ = new string[]
                    {
                        "TwistedFate by Kortatu", "SharpShooter [Listo]", "BadaoSeries [Listo]", "EloFactory TF",
                        "OKTW", "SAutoCarry", "SFXChallenger", "Twisted Fate - Danz", "[SDK] Flowers' Series",
                        "[SDK] RARETwistedFate", "Diabath's TwistedFate", "Flowers' TwistedFate",
                        "mztikk's TwistedFate", "GrossGore - Fate", "NoobAIO [Listo]"
                    };
                    break;
                case "Twitch":
                    champ = new string[]
                    {
                        "OKTW", "iSeriesReborn", /*"iTwitch 2.0",*/ "SAutoCarry", "SharpShooter [Listo]", "Marksman#",
                        "[SDK] ExorAIO [Listo]", "[SDK] Flowers' Series", "[SDK] InfectedTwitch", "Flowers' Twitch",
                        "NechritoTwitch", "SNTwitch", "theobjops's Twitch", "TheTwitch", "Twitch#",
                        "Flowers' ADC Series", "NoobAIO [Listo]"
                    };
                    break;
                case "Udyr":
                    champ = new string[]
                    {
                        "BrianSharp", "D-Udyr", "EloFactory Udyr", "LCS Udyr", "UnderratedAIO", "[SDK] ExorAIO",
                        "NoobUdyr", "yetAnotherUdyr", "MightyAIO [Listo]"
                    };
                    break;
                case "Varus":
                    champ = new string[]
                    {
                        "ElVarus [Listo]", "OKTW [Listo]", "SFXChallenger", "SharpShooter [Listo]", "Marksman#",
                        "Varus God", "Hikicarry ADC", "Flowers' ADC Series", "ElVarus Revamped"
                    };
                    break;
                case "Vayne":
                    champ = new[]
                    {
                        "VayneHunterReborn", "hikiMarksman", "iSeriesReborn", "OKTW [Listo]", "SAutoCarry",
                        "SharpShooter [Listo]", "xSalice", "Marksman#", "hi im gosu [Listo]",
                        "[SDK] ChallengerSeriesAIO", "[SDK] ExorAIO [Listo]", "[SDK] Flowers' Series [Listo]", "[SDK] hVayne",
                        "HikiCarry Vayne Masterrace", "PRADA Vayne", "SOLO Vayne", "VayneGodMode", "Hikicarry ADC",
                        "Flowers' ADC Series", "hi_im_gosu reborn", "ReformedAIO", "Lord's Vayne", "PRADA Vayne Reborn",
                        "EzAIO [Listo]"
                    };
                    break;
                case "Viktor":
                    champ = new[]
                    {
                        "TRUSt in my Viktor", "Hikicarry Viktor", "Perplexed Viktor", "SAutoCarry", "SFXChallenger",
                        "Badao's Viktor [Listo]", "xSalice", "[SDK] Flowers' Series [Listo]",
                        "[SDK] Flowers' Viktor [Listo]", "[SDK] TRUSt in my Viktor", "King Lazer", "mySeries"
                    };
                    break;
                case "Xayah":
                    champ = new[] {"SharpShooter [Listo]"};
                    break;
                case "Yasuo":
                    champ = new[]
                    {
                        "YasuoPro", "BrianSharp", "GosuMechanics", "YasuoSharpv2", "[Yasuo] Master of Wind",
                        "M1D 0R F33D", "YasuoMemeBender", "[SDK] Valvrave#", "BadaoYasuo", "hYasuo", "ReformedAIO",
                        "Flowers' Yasuo [Listo]", "GosuMechanicsYasuo_Rebirth"
                    };
                    break;
                case "Yorick":
                    champ = new[] {"MightyAIO [Listo]"};
                    break;
                case "Yuumi":
                    champ = new[] {"MightyAIO [Listo]"};
                    break;
                case "Zed":
                    champ = new[]
                    {
                        "Korean Zed [Listo]", "SharpyAIO [Listo]", "[SDK] Valvrave#", "iDZed", "Ze-D is Back [Listo]",
                        "xSalice"
                    };
                    break;
                case "Zoe":
                    champ = new[] {"MightyAIO [Listo]"};
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
            dutility.Add(new MenuList("Tracker", "Which Tracker?", new[] {"NabbTracker", "Tracker#"}));
            dutility.Add(new MenuBool("enableEvade", "Enable Evade", false));
            dutility.Add(new MenuList("Evade", "Which Evade?", new[] {"EzEvade", "Evade# [Listo]}", "vEvade"}));
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