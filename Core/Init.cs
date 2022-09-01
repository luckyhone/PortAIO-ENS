using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace PortAIO
{
    public static class Init
    {
        public static bool loaded = false;
        public static int moduleNum = 1;

        public static void Initialize()
        {

            Console.WriteLine("[PortAIO] Core loading : Module " + moduleNum + " - Common Loaded");
            moduleNum++;

            Misc.Load();
            Console.WriteLine("[PortAIO] Core loading : Module " + moduleNum + " - Misc Loaded");
            moduleNum++;
            if (!Misc.menu.GetValue<MenuBool>("UtilityOnly").Enabled)
            {
                LoadChampion();
                Console.WriteLine("[PortAIO] Core loading : Module " + moduleNum + " - Champion Script Loaded");
                moduleNum++;
                Game.OnUpdate += Game_OnUpdate;
                moduleNum++;
            }

            if (!Misc.menu.GetValue<MenuBool>("ChampsOnly").Enabled)
            {
                LoadUtility();
                Console.WriteLine("[PortAIO] Core loading : Module " + moduleNum + " - Utilities Loaded");
                moduleNum++;
            }

            Console.WriteLine("[PortAIO] Core loaded.");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Orbwalker.AttackEnabled = true;
            Orbwalker.MoveEnabled = true;
        }

        public static void PortAIOMsg(string msg)
        {
            Game.Print("<font color=\"#43ddaa\">[PortAIO] </font><font color=\"#ff9999\">" + msg + "</font>");
        }

        #region LOADUTILITY

        public static void LoadUtility()
        {
            if (Misc.menu["Utilitiesports"].GetValue<MenuBool>("enableTracker").Enabled)
            {
                switch (Misc.menu["Utilitiesports"].GetValue<MenuList>("Tracker").Index)
                {
                    case 0: //NabbTracker
                        NabbTracker.Program.Loads();
                        break;
                    case 1: // Tracker#
                        Tracker.Program.Loads();
                        break;
                    case 2: // Entropy.Awareness
                        //Entropy.Awareness.Program.Loads();
                        break;
                }
            }

            if (Misc.menu["enableEvade"].GetValue<MenuBool>().Enabled)
            {
                switch (Misc.menu["Evade"].GetValue<MenuList>().Index)
                {

                    case 0: // EzEvade - Done
                        //ezEvade.Program.Main();
                        break;
                    case 1: // Evade# - Done
                        Evade.Program.Loads();
                        break;
                    case 2: // vEvade
                        //vEvade.Program.Main();
                        break;
                }
            }

            /*if (Misc.menu["Utilitiesports"].GetValue<MenuBool>("enablePredictioner").Enabled)
            {
                switch (Misc.menu["Utilitiesports"].GetValue<MenuList>("Predictioner").Index)
                {
                    case 0: // SPredictioner
                        //SPredictioner.SPredictioner.Initialize();
                        break;
                    case 1: // OKTW Predictioner
                        //OKTWPredictioner.OKTWPredictioner.Initialize();
                        break;
                }
            }*/

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("ShadowTracker").Enabled)
            {
                ShadowTracker.Program.Game_OnGameLoad();
            }

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("PerfectWardReborn").Enabled)
            {
                PerfectWardReborn.Program.Loads();
            }

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("UniversalGankAlerter").Enabled)
            {
                UniversalGankAlerter.Program.Loads();
            }

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("UniversalRecallTracker").Enabled)
            {
                UniversalRecallTracker.Program.Loads();
            }

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("UniversalMinimapHack").Enabled)
            {
                UniversalMinimapHack.Program.Loads();
            }

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("BaseUlt3").Enabled)
            {
                BaseUlt3.Program.Loads();
            }

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("BasicChatBlock").Enabled)
            {
                BasicChatBlock.Program.Loads();
            }

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("CSCounter").Enabled)
            {
                CS_Counter.CsCounter.Loads();
            }

            if (Misc.menu["PortAIOuTILITIESS"].GetValue<MenuBool>("DeveloperSharp").Enabled)
            {
                DeveloperSharp.Program.Loads();
            }
        }

        #endregion

        #region LOADCHAMPION

        public static void LoadChampion()
        {
            switch (ObjectManager.Player.CharacterName)
            {
                case "Ahri":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // DZAhri
                            DZAhri.Program.Game_OnGameLoad();
                            break;
                        case 2: // EloFactory
                            EloFactory_Ahri.Program.Game_OnGameLoad();
                            break;
                        case 3: // Kappa Series
                            //KappaSeries.Program.OnGameLoad();
                            break;
                        case 4: // xSalice
                            xSalice_Reworked.Program.LoadReligion();
                            break;
                        case 5: // BadaoSeries
                            BadaoSeries.Program.OnLoad();
                            break;
                        case 6: // M1D 0R F33D
                            //Mid_or_Feed.Program.Main();
                            break;
                        case 7: // AhriSharp
                            AhriSharp.Program.Game_OnGameLoad();
                            break;
                        case 8: // Flowers' Series
                            Flowers_Series.Program.Loads();
                            break;
                        case 9: // Babehri
                            Babehri.Program.Game_OnGameLoad();
                            break;
                        case 10: // EasyAhri
                            //EasyAhri.Program.Main();
                            break;
                        case 11: // SenseAhri
                            //Sense_Ahri.Program.Game_OnGameLoad();
                            break;
                        case 12: // Entropy.AIO
                            Entropy.AIO.Program.Loads();
                            break;
                    }

                    break;
                case "Akali":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // KDA Akali
                            KDA_Akali.Program.OnLoad();
                            break;
                        case 1: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Alistar":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // ElAlistar
                            //ElAlistarReborn.Alistar.OnGameLoad();
                            break;
                        case 1: // Support is Easy
                            //Support.Program.Main();
                            break;
                        case 2: // FreshBooster
                            //FreshBooster.Program.Game_OnGameLoad();
                            break;
                        case 3: // vSeries
                            //vSupport_Series.Program.Game_OnGameLoad();
                            break;
                        case 4: // SkyAlstar
                            //AlistarBySky97.Program.Game_OnGameLoad();
                            break;
                        case 5: // Easy_Sup
                            Easy_Sup.Program.Loads();
                            break;
                    }

                    break;
                case "Annie":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // Korean Annie
                            //KoreanAnnie.Program.Game_OnGameLoad();
                            break;
                        case 2: // SharpyAIO
                            //Sharpy_AIO.Program.Game_OnGameLoad();
                            break;
                        case 3: // Support is Easy
                            //Support.Program.Main();
                            break;
                        case 4: // EloFactory Annie
                            //EloFactory_Annie.Program.Game_OnGameLoad();
                            break;
                        case 5: // Flower's Annie
                            Flowers__Annie.Program.Game_OnGameLoad();
                            break;
                        case 6: // OAnnie
                            //OAnnie.Program.Main();
                            break;
                        case 7: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 8: // MySeries
                            //myAnnie.Program.Main();
                            break;
                    }

                    break;
                case "Ashe":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // ProSeries
                            //ProSeries.Program.GameOnOnGameLoad();
                            break;
                        case 2: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 3: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 4: // SurvivorSeries
                            //SurvivorAshe.Program.Game_OnGameLoad();
                            break;
                        case 5: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 6: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 7: // [SBTW] Ashe
                            //Flowers_Ashe.Program.Main();
                            break;
                        case 8: // ChallengerSeries
                            //Challenger_Series.Program.Main();
                            break;
                        case 9: // Dicaste's Ashe
                            //DicasteAshe.Program.Main();
                            break;
                        case 10: // ExorAIO
                            //ExorAIO.Program.Main();
                            break;
                        case 11: // Flowers' Series
                            Flowers_Series.Program.Loads();
                            break;
                        case 12: // xcsoft's Ashe
                            //xcAshe.Program.Main();
                            break;
                        case 13: // Ashe#
                            //AsheSharp.Source.Program.Game_OnGameLoad();
                            break;
                        case 14: // CarryAshe
                            //CarryAshe.Program.Game_OnGameLoad();
                            break;
                        case 15: // SNAshe
                            //SNAshe.Program.Game_OnGameLoad();
                            break;
                        case 16: // ProjectGeass
                            //_Project_Geass.Program.Main();
                            break;
                        case 17: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 18: // Flowers' ADC Series
                            Flowers_ADC_Series.Program.Loads();
                            break;
                        case 19: //SurvivorSeries AIO
                            //SurvivorSeriesAIO.Program.Main();
                            break;
                    }

                    break;
                case "Blitzcrank":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // FreshBooster
                            //FreshBooster.Program.Game_OnGameLoad();
                            break;
                        case 2: // KurisuBlitz
                            //Blitzcrank.Program.Game_OnGameLoad();
                            break;
                        case 3: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 4: // SharpShooter
                            //SharpShooter.Program.Game_OnGameLoad();
                            break;
                        case 5: // ShineAIO
                            //ShineSharp.Program.Game_OnGameLoad();
                            break;
                        case 6: // Support is Easy
                            //Support.Program.Main();
                            break;
                        case 7: // vSeries
                            //vSupport_Series.Program.Game_OnGameLoad();
                            break;
                        case 8: // Flowers' Series
                            //Flowers_Series.Program.Main();
                            break;
                        case 9: // xcsoft's Blitzcrank
                            //xcBlitzcrank.Program.Main();
                            break;
                        case 10: // JustBlitzcrank
                            //JustBlitz.Program.Main();
                            break;
                        case 11: // SluttyBlitz
                            //Slutty_Blitz.Program.Main();
                            break;
                        case 12: // sAIO
                            //sAIO.Program.Main();
                            break;
                        case 13: // Z.Aio
                            Z.aio.Program.Loads();
                            break;
                        case 14: // Easy_Sup
                            Easy_Sup.Program.Loads();
                            break;
                    }

                    break;
                case "Brand":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // The Brand
                            //TheBrand.Program.Main();
                            break;
                        case 1: // Hikicarry Brand
                            //HikiCarry_Brand.Program.Game_OnGameLoad();
                            break;
                        case 2: // OKTW
                            //OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 3: // SurvivorSeries
                            //SurvivorBrand.Program.Game_OnGameLoad();
                            break;
                        case 4: // yol0 Brand
                            //yol0Brand.Program.Game_OnGameLoad();
                            break;
                        case 5: // DevBrand
                            //DevBrand.Program.Main();
                            break;
                        case 6: // Flower's Brand
                            //Flowers_Brand.Program.Main();
                            break;
                        case 7: // Kimbaeng Brand
                            //Kimbaeng_Brand.Program.Main();
                            break;
                        case 8: // sBrand
                            //sBrand.Program.Main();
                            break;
                        case 9: // SNBrand
                            //SNBrand.Program.Main();
                            break;
                        case 10: //SurvivorSeries AIO
                            //SurvivorSeriesAIO.Program.Main();
                            break;
                        case 11: // SergixBrand
                            //SergixBrand.Program.Main();
                            break;
                        case 12: // Shulepin's Brand
                            //Brand.Load.Main();
                            break;
                        case 13: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 14: // LyrdumAIO
                            LyrdumAIO.Program.Loads();
                            break;
                    }

                    break;

                case "Caitlyn":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 2: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 3: // ChallengerSeriesAIO
                            //Challenger_Series.Program.Main();
                            break;
                        case 4: // ExorAIO
                            //ExorAIO.Program.Main();
                            break;
                        case 5: // Slutty Caitlyn
                            Slutty_Caitlyn.Program.Loads();
                            break;
                        case 6: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 7: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                        case 8: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 9: // Caitlyn Master Headshot
                            Caitlyn_Master_Headshot.Program.Loads();
                            break;
                    }

                    break;
                case "Camille":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // hCamille
                            hCamille.Program.Loads();
                            break;
                        case 1: // Camille#
                            CamilleSharp.Program.Loads();
                            break;
                        case 2: // Lord's Camille
                            LordsCamille.Program.Loads();
                            break;
                    }

                    break;

                case "Cassiopeia":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 1: // SFXChallenger
                            //SFXChallenger.Program.Main();
                            break;
                        case 2: // SharpyAIO
                            //Sharpy_AIO.Program.Game_OnGameLoad();
                            break;
                        case 3: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 4: // TheCassiopeia
                            //TheCassiopeia.Program.Main();
                            break;
                        case 5: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 6: // Eat my Cass
                            //Eat_My_Cass.Program.Main();
                            break;
                        case 7: // mztikks Cass
                            //mztikksCassiopeia.Program.Main();
                            break;
                        case 8: // RiseofThePython
                            //riseofthepython.Program.Main();
                            break;
                        case 9: // sAIO
                            //sAIO.Program.Main();
                            break;
                        case 10: // DevCassio2
                            //DevCassio.Program.Main();
                            break;
                        case 11: // LyrdumAIO
                            LyrdumAIO.Program.Loads();
                            break;
                    }

                    break;
                case "Chogath":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // UnderratedAIO
                            //UnderratedAIO.Program.OnGameLoad();
                            break;
                        case 1: // Windwalker Cho'Gath
                            //WindWalker_Cho._._.gath.Program.Game_OnGameLoad();
                            break;
                        case 2: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 3: // Troop Gath
                            //TroopChogath.Program.Main();
                            break;
                        case 4: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Corki":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // ElCorki
                            //ElCorki.Corki.Game_OnGameLoad();
                            break;
                        case 1: // ADCPackage
                            //ADCPackage.Program.Game_OnGameLoad();
                            break;
                        case 2: // D-Corki
                            //D_Corki.Program.Game_OnGameLoad();
                            break;
                        case 3: // hikiMarksman
                            //hikiMarksmanRework.Program.Game_OnGameLoad();
                            break;
                        case 4: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 5: // ProSeries
                            //ProSeries.Program.GameOnOnGameLoad();
                            break;
                        case 6: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 7: // SharpShooter 
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 8: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 9: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 10: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 11: // EasyCorki
                            //EasyCorki.EasyCorki.Main();
                            break;
                        case 12: // jhkCorki
                            //jhkCorki.Program.Game_OnGameLoad();
                            break;
                        case 13: // LeCorki
                            //LeCorki.Program.Main();
                            break;
                        case 14: // PewPewCorki
                            //PewPewCorki.Program.Main();
                            break;
                        case 15: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                    }

                    break;
                case "Darius":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // ElEasy
                            //ElEasy.Entry.OnLoad();
                            break;
                        case 2: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 3: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 4: // Flowers' Series
                            Flowers_Series.Program.Loads();
                            break;
                        case 5: // Darius#
                            //DariusSharp.Program.Main();
                            break;
                        case 6: // KurisuDarius
                            //KurisuDarius.Program.Main();
                            break;
                        case 7: // ODarius
                            //ODarius.Program.Main();
                            break;
                        case 8: // PerfectDarius
                            //PerfectDarius.Program.Main();
                            break;
                        case 9: // sAIO
                            //sAIO.Program.Main();
                            break;
                        case 10: // Flowers' Darius
                            //Flowers_Darius.Program.Main();
                            break;
                    }

                    break;
                case "Draven":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            //OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // hikiMarksman
                            //hikiMarksmanRework.Program.Game_OnGameLoad();
                            break;
                        case 2: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 3: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 4: // M00N Draven
                            //MoonDraven.Program.GameOnOnGameLoad();
                            break;
                        case 5: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 6: // Tyler1
                            Tyler1.Program.Loads();
                            break;
                        case 7: // BadaoDraven
                            //BadaoDraven.Program.Main();
                            break;
                        case 8: // myWorld AIO
                            //myWorld.Program.Main();
                            break;
                        case 9: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 10: // Flowers' ADC_Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                    }

                    break;
                case "Ekko":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // EloFactory Ekko
                            //EloFactory_Ekko.Program.Game_OnGameLoad();
                            break;
                        case 2: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 3: // Ekko Master of Time
                            //Ekko_master_of_time.Program.Main();
                            break;
                        case 4: // Ekko The Boy Who Shattered Time
                            //Ekko_the_Boy_Who_Shattered_Time.Bootstrap.Main();
                            break;
                        case 5: // EkkoGod
                            //EkkoGod.Program.Main();
                            break;
                        case 6: // ElEkko
                            //ElEkko.Program.Main();
                            break;
                        case 7: // Hikicarry Ekko
                            //HikiCarry_Ekko.Program.Main();
                            break;
                        case 8: // TheEkko
                            //TheEkko.Program.Main();
                            break;
                    }

                    break;
                case "Jax":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // xQx Jax
                            //JaxQx.Program.Game_OnGameLoad();
                            break;
                        case 1: // BrianSharp
                            //BrianSharp.Program.Main();
                            break;
                        case 2: // NoobJaxReloaded
                            //NoobJaxReloaded.Program.Game_OnGameLoad();
                            break;
                        case 3: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 4: // UnderratedAIO
                            //UnderratedAIO.Program.OnGameLoad();
                            break;
                        case 5: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 6: // SkyLv JAx
                            //SkyLv_Jax.Initialiser.Main();
                            break;
                        case 7: // NoobAIO
                            NoobAIO.Program.Loads();
                            break;
                    }
                    break;
                case "Jayce":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // Hikicarry Jayce
                            //HikiCarry_Jayce___Hammer_of_Justice.Program.OnGameLoad();
                            break;
                        case 2: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 3: // AJayce
                            //AJayce.Program.Main();
                            break;
                        case 4: // JayceSharpV2
                            //JayceSharpV2.Program.Main();
                            break;
                        case 5: // Shulepin's Jayce
                            Jayce.Load.Loads();
                            break;
                        case 6: // SharpShooter
                            //SharpShooter.MyLoader.Loads();
                            break;
                    }
                    break;
                case "Jhin":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // Hikigaya's Jhin
                            //Jhin___The_Virtuoso.Program.JhinOnGameLoad();
                            break;
                        case 2: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 3: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 4: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 5: // hJhin
                            //hJhin.Program.Main();
                            break;
                        case 6: // Jhin As The Virtuoso
                            //Jhin_As_The_Virtuoso.Program.Main();
                            break;
                        case 7: // BadaoJhin
                            //BadaoKingdom.Program.Main();
                            break;
                        case 8: // Tc_SDKexAIO
                            //Tc_SDKexAIO.PlaySharp.Main();
                            break;
                        case 9: // Flowers' Jhin
                            //Flowers_Jhin.Program.Main();
                            break;
                        case 10: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 11: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                        case 12:
                            EzAIO.Program.Loads();
                            break;
                    }
                    break;
                case "Jinx":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: //OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // EzAIO
                            EzAIO.Program.Loads();
                            break;
                        case 2: // ADCPackage
                            ADCPackage.Program.Game_OnGameLoad();
                            break;
                        case 3: // GenesisJinx
                            Jinx_Genesis.Program.Game_OnGameLoad();
                            break;
                        case 4: // iSeriesReborn
                            //iSeriesReborn.Program.OnGameLoad();
                            break;
                        case 5: // ProSeries
                            //ProSeries.Program.GameOnOnGameLoad();
                            break;
                        case 6: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 7: // xSalice
                            xSalice_Reworked.Program.LoadReligion();
                            break;
                        case 8: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 9: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 10: // CJShu Jinx
                            //CjShuJinx.Program.Main();
                            break;
                        case 11: // EasyJinx
                            //EasyJinx.EasyJinx.Main();
                            break;
                        case 12: // EloFactory Jinx
                            //EloFactory_Jinx.Program.Main();
                            break;
                        case 13: // GenerationJinx
                            //GenerationJinx.Program.Main();
                            break;
                        case 14: // PennyJinx Reborn
                            //PennyJinxReborn.Program.Main();
                            break;
                        case 15: // myWorld AIO
                            //myWorld.Program.Main();
                            break;
                        case 16: // Tc_SDKex AIO
                            //Tc_SDKexAIO.PlaySharp.Main();
                            break;
                        case 17: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 18: // Flowers' ADC_Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                        case 19: // MigthyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Ezreal":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: //OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: //EzAIO
                            EzAIO.Program.Loads();
                            break;
                        case 2: //Sharpshooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 3: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Kalista":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // S+Class
                            //S_Plus_Class_Kalista.Program.OnLoad();
                            break;
                        case 1: // HERMES Kalista
                            HERMES_Kalista.Program.Loads();
                            break;
                        case 2: // Hikicarry Kalista
                            //HikiCarry_Kalista.Program.Game_OnGameLoad();
                            break;
                        case 3: // iSeriesReborn
                            //iSeriesReborn.Program.OnGameLoad();
                            break;
                        case 4: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 5: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 6: // SFXChallenger
                            //SFXChallenger.Program.Main();
                            break;
                        case 7: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 8: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 9: // ChallengerSeries
                            //Challenger_Series.Program.Main();
                            break;
                        case 10: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 11: // xcsoft's Kalista
                            //xcKalista.Program.Main();
                            break;
                        case 12: // DonghuKalista
                            //DonguKalista.Program.Main();
                            break;
                        case 13: // EasyKalista
                            break;
                        case 14: // ElKalista
                            //ElKalista.Program.Main();
                            break;
                        case 15: // iKalista
                            //IKalista.Program.Main();
                            break;
                        case 16: // iKalista:Reborn
                            //iKalistaReborn.Program.Main();
                            break;
                        case 17: // Kalima
                            //Kalima.Kalista.Main();
                            break;
                        case 18: // KAPPALISTAXD
                            //KAPPALISTAXD.Program.Main();
                            break;
                        case 19: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 20: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                        case 21: // EzAIO
                            EzAIO.Program.Loads();
                            break;
                        case 22: // EnsoulSharp.Kalista
                            EnsoulSharp.Kalista.Program.Loads();
                            break;
                    }

                    break;
                case "Karthus":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // SharpShooter
                            //SharpShooter.Program.Game_OnGameLoad();
                            break;
                        case 2: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 3: // RAREKarthus
                            //RAREKarthus.Program.Main();
                            break;
                        case 4: // Karthus#
                            //KarthusSharp.Program.Main();
                            break;
                        case 5: // KimbaengKarthus
                            Kimbaeng_KarThus.Program.Loads();
                            break;
                        case 6: // SNKarthus
                            //SNKarthus.Program.Main();
                            break;
                        case 7: // XDSharpAIO
                            //XDSharp.Program.Main();
                            break;
                        case 8: // ExorAIO
                            //ExorAIO.Program.Main();
                            break;
                        case 9: // Flowers' Karthus
                            Flowers_Karthus.Program.Loads();
                            break;
                        case 10: //AlqoholicKarthus
                            //AlqoholicKarthus.Program.Main();
                            break;
                        case 11: // LyrdumAIO
                            LyrdumAIO.Program.Loads();
                            break;
                    }

                    break;
                case "Katarina":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // NoobAIO
                            NoobAIO.Program.Loads();
                            break;
                    }

                    break;
                case "Kayn":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // NoobAIO
                            NoobAIO.Program.Loads();
                            break;
                    }

                    break;
                case "Kindred":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Yin & Yang
                            //Kindred___YinYang.Program.Game_OnGameLoad();
                            break;
                        case 1: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 2: // SharpShooter
                            //SharpShooter.Program.Game_OnGameLoad();
                            break;
                        case 3: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 4: // KindredSpirits
                            //KindredSpirits.Program.Main();
                            break;
                        case 5: // Slutty Kindred
                            //Slutty_Kindred.Program.Main();
                            break;
                        case 6: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                    }

                    break;
                case "Vayne":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // VayneHunterReborn
                            //VayneHunter_Reborn.Program.Main();
                            break;
                        case 1: // hikiMarksman
                            //hikiMarksmanRework.Program.Game_OnGameLoad();
                            break;
                        case 2: // iSeriesReborn
                            //iSeriesReborn.Program.OnGameLoad();
                            break;
                        case 3: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 4: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 5: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 6: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 7: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 8: // hi im gosu
                            hi_im_gosu.Vayne.Game_OnGameLoad();
                            break;
                        case 9: // ChallengerSeries
                            //Challenger_Series.Program.Main();
                            break;
                        case 10: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 11: // Flowers' Series
                            Flowers_Series.Program.Loads();
                            break;
                        case 12: // hVayne
                            //hVayne.Program.Main();
                            break;
                        case 13: // Hikicarry Vayne Masterrace
                            //HikiCarry_Vayne_Masterrace.Program.Main();
                            break;
                        case 14: // PRADA Vayne
                            //PRADA_Vayne_Old.Program.Main();
                            break;
                        case 15: // SOLO Vayne
                            //SoloVayne.Program.Main();
                            break;
                        case 16: // VayneGodMode
                            //GodModeOn_Vayne.Program.Main();
                            break;
                        case 17: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 18: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                        case 19: // hi_im_gosy reborn
                            //hi_im_gosu_Reborn.Vayne.Main();
                            break;
                        case 20: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 21: // Lord's Vayne
                            //Lord_s_Vayne.Program.Main();
                            break;
                        case 22: // PradaVayneReborn
                            //PRADA_Vayne.Program.Main();
                            break;
                        case 23: // EzAIO
                            EzAIO.Program.Loads();
                            break;

                    }

                    break;
                case "Viktor":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // TRUSt in my Viktor
                            //Viktor.Program.Game_OnGameLoad();
                            break;
                        case 1: // Hikicarry Viktor
                            //HikiCarry_Viktor.Program.Game_OnGameLoad();
                            break;
                        case 2: // Perplexed Viktor
                            //PerplexedViktor.Program.Game_OnGameLoad();
                            break;
                        case 3: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 4: // SFXChallenger
                            //SFXChallenger.Program.Main();
                            break;
                        case 5: // Badao's Viktor
                            ViktorBadao.Program.Game_OnGameLoad();
                            break;
                        case 6: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 7: // Flowers' Series
                            Flowers_Series.Program.Loads();
                            break;
                        case 8: // Flowers' Viktor
                            Flowers_Viktor.Program.Loads();
                            break;
                        case 9: // TRUStInMyViktor SDK
                            //TRUStInMyViktor.Program.Main();
                            break;
                        case 10: // King Lazer
                            //HamViktor.Program.Main();
                            break;
                        case 11: // MySeries
                            //myViktor.Program.Main();
                            break;
                    }

                    break;
                case "Fizz":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Math Fizz
                            //MathFizz.Program.Game_OnGameLoad();
                            break;
                        case 1: // ElFizz
                            //ElFizz.Fizz.OnGameLoad();
                            break;
                        case 2: // UnderratedAIO
                            //UnderratedAIO.Program.OnGameLoad();
                            break;
                        case 3: // HeavenStrikeFizz
                            //HeavenStrikeFizz.Program.Main();
                            break;
                        case 4: // NoobFizz
                            //NoobFizz.Program.Main();
                            break;
                        case 5: // OneKeyToFish
                            //OneKeyToFish.Program.Main();
                            break;
                        case 6: // Lord's Fizz
                            //LordsFizz.Program.Main();
                            break;
                        case 7: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Gangplank":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // UnderratedAIO
                            //UnderratedAIO.Program.OnGameLoad();
                            break;
                        case 1: // Badao Gangplank
                            BadaoGP.Program.Loads();
                            break;
                        case 2: // BangPlank
                            Bangplank.Program.Loads();
                            break;
                        case 3: // BePlank
                            BePlank.Program.Loads();
                            break;
                        case 4: // e.Motion Gangplank
                            e.Motion_Gangplank.Program.OnLoad();
                            break;
                        case 5: // GangplankBuddy
                            GangplankBuddy.Program.Loads();
                            break;
                        case 6: // Entropy.AIO
                            Entropy.AIO.Program.Loads();
                            break;
                    }

                    break;
                case "Graves":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // D-Graves
                            //D_Graves.Program.Game_OnGameLoad();
                            break;
                        case 2: // Hikimarksman
                            //hikiMarksmanRework.Program.Game_OnGameLoad();
                            break;
                        case 3: // KurisuGraves
                            //KurisuGraves.Program.Game_OnLoad();
                            break;
                        case 4: // SFXChallenger
                            //SFXChallenger.Program.Main();
                            break;
                        case 5: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 6: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 7: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 8: // Flowers' Series
                            //Flowers_Series.Program.Main();
                            break;
                        case 9: // VSTGraves
                            //VST_Auto_Carry_Standalone_Graves.Program.Main();
                            break;
                        case 10: // EasyGraves
                            //EasyGraves.EasyGraves.Main();
                            break;
                        case 11: // BadaoGraves
                            //BadaoKingdom.Program.Main();
                            break;
                        case 12: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                    }

                    break;
                case "Lillia":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Lucian":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // LCS Lucian
                            //LCS_Lucian.Program.Main();
                            break;
                        case 1: // BrianSharp
                            //BrianSharp.Program.Main();
                            break;
                        case 2: // hikiMarksman
                            //hikiMarksmanRework.Program.Game_OnGameLoad();
                            break;
                        case 3: // HoolaLucian
                            //HoolaLucian.Program.OnGameLoad();
                            break;
                        case 4: // iLucian
                            //iLucian.LucianBootstrap.OnGameLoad();
                            break;
                        case 5: // iSeriesReborn
                            //iSeriesReborn.Program.OnGameLoad();
                            break;
                        case 6: // Korean Lucian
                            //KoreanLucian.Program.Game_OnGameLoad();
                            break;
                        case 7: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 8: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 9: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 10: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 11: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 12: // ChallengerSeries
                            //Challenger_Series.Program.Main();
                            break;
                        case 13: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 14: // D_Lucian
                            //D_Lucian.Program.Main();
                            break;
                        case 15: // FuckingLucianReborn
                            //FuckingLucianReborn.Program.Main();
                            break;
                        case 16: // Slutty Lucian
                            //Slutty_Lucian.Program.Main();
                            break;
                        case 17: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 18: // Flowers' ADC Series
                            Flowers_ADC_Series.Program.Loads();
                            break;
                        case 19: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 20: // Lord's Lucian
                            //Lord_s_Lucian.Program.Main();
                            break;
                        case 21: // EzAIO
                            EzAIO.Program.Loads();
                            break;
                        case 22: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Lux":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // vSeries
                            //vSupport_Series.Program.Game_OnGameLoad();
                            break;
                        case 2: // M1D 0R F33D
                            //Mid_or_Feed.Program.Main();
                            break;
                        case 3: // M00N Lux
                            MoonLux.Program.Loads();
                            break;
                        case 4: // ExorAIO
                            //ExorAIO.Program.Main();
                            break;
                        case 5: // Cheerleader Lux
                            //CheerleaderLux.Loader.Main();
                            break;
                        case 6: // ElLux
                            //ElLux.Program.Main();
                            break;
                        case 7: // Hikigaya Lux
                            //Hikigaya_Lux.Program.Main();
                            break;
                        case 8: // SephLux
                            //SephLux.Program.Loads();
                            break;
                        case 9: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 10: // LyrdumAIO
                            LyrdumAIO.Program.Loads();
                            break;
                        case 11: // Easy_Sup
                            Easy_Sup.Program.Loads();
                            break;
                    }

                    break;
                case "Leblanc":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // LeBlanc II
                            //Leblanc.Leblanc.Game_OnGameLoad();
                            break;
                        case 1: // FreshBooster
                            //FreshBooster.Program.Game_OnGameLoad();
                            break;
                        case 2: // LCS Leblanc
                            //LCS_LeBlanc.Program.OnLoad();
                            break;
                        case 3: // M1D 0R F33D
                            //Mid_or_Feed.Program.Main();
                            break;
                        case 4: // BadaoLeblanc
                            //BadaoLeblanc.Program.Main();
                            break;
                        case 5: // PopBlanc
                            //PopBlanc.Program.Main();
                            break;
                        case 6: // Entropy.AIO
                            Entropy.AIO.Program.Loads();
                            break;
                    }
                    break;
                case "Leona":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // ElEasy
                            //ElEasy.Entry.OnLoad();
                            break;
                        case 1: // Support Is Easy
                            //Support.Program.Main();
                            break;
                        case 2: // vSeries
                            //vSupport_Series.Program.Game_OnGameLoad();
                            break;
                        case 3: // SethLeona
                            //SethLeona.Program.Main();
                            break;
                        case 4: // Troopeona
                            //Troopeona.Program.Main();
                            break;
                        case 5: // sAIO
                            //sAIO.Program.Main();
                            break;
                        case 6: // Kuroko's Leona
                            //Kuroko_s_Leona.Program.Main();
                            break;
                        case 7: // Z.Aio
                            Z.aio.Program.Loads();
                            break;
                    }

                    break;
                case "MasterYi":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // MasterSharp
                            //MasterSharp.Program.Main();
                            break;
                        case 1: // Hoola Yi
                            //HoolaMasterYi.Program.OnGameLoad();
                            break;
                        case 2: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 3: // MasterYi by Prunes
                            //MasterYiByPrunes.Program.Main();
                            break;
                        case 4: // xQx Yi
                            //MasterYiQx.Program.Main();
                            break;
                        case 5: // Yi by Crisdmc
                            //crisMasterYi.Program.Main();
                            break;
                        case 6: // ChallengerYi
                            //ChallengerYi.Program.Main();
                            break;
                        case 7: // NoobAIO
                            NoobAIO.Program.Loads();
                            break;
                    }

                    break;
                case "Morgana":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // KurisuMorgana
                            //KurisuMorgana.Program.Game_OnGameLoad();
                            break;
                        case 1: // FreshBooster
                            //FreshBooster.Program.Game_OnGameLoad();
                            break;
                        case 2: // OKTW
                            //OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 3: // ShineAIO
                            //ShineSharp.Program.Game_OnGameLoad();
                            break;
                        case 4: // Support Is Easy
                            //Support.Program.Main();
                            break;
                        case 5: // vSeries
                            //vSupport_Series.Program.Game_OnGameLoad();
                            break;
                        case 6: // Flowers' Series
                            //Flowers_Series.Program.Main();
                            break;
                        case 7: // Easy_Sup
                            Easy_Sup.Program.Loads();
                            break;
                    }

                    break;
                case "Thresh":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Chain Warden
                            //Thresh___The_Chain_Warden.Program.Game_OnGameLoad();
                            break;
                        case 1: // FreshBooster
                            //FreshBooster.Program.Game_OnGameLoad();
                            break;
                        case 2: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 3: // Support is Easy
                            //Support.Program.Main();
                            break;
                        case 4: // vSeries
                            //vSupport_Series.Program.Game_OnGameLoad();
                            break;
                        case 5: // Dark Star Thresh
                            //Dark_Star_Thresh.Program.OnLoad();
                            break;
                        case 6: // Slutty Thresh
                            Slutty_Thresh.Program.Loads();
                            break;
                        case 7: // Thresh as the Chain Warden
                            //ThreshAsTheChainWarden.Program.Main();
                            break;
                        case 8: // Thresh - Catch Fish
                            //ThreshCatchFish.Program.Main();
                            break;
                        case 9: // Thresh Ruler of the Soul
                            //ThreshTherulerofthesoul.Program.Main();
                            break;
                        case 10: // Thresh the Flay Maker
                            //ThreshFlayMaker.Program.Main();
                            break;
                        case 11: // yol0 Thresh
                            //yol0Thresh.Program.Main();
                            break;
                        case 12: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 13: // LyrdumAIO
                            LyrdumAIO.Program.Loads();
                            break;
                        case 14: // Easy_Sup
                            Easy_Sup.Program.Loads();
                            break;
                    }

                    break;
                case "TwistedFate":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Kortatu Twistedfate
                            //TwistedFate.Program.Game_OnGameLoad();
                            break;
                        case 1: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 2: // BadaoSeries
                            BadaoSeries.Program.OnLoad();
                            break;
                        case 3: // EloFactor TF
                            //EloFactory_TwistedFate.Program.Game_OnGameLoad();
                            break;
                        case 4: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 5: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 6: // SFXChallenger
                            //SFXChallenger.Program.Main();
                            break;
                        case 7: // TwistedFate-Danz
                            //Twisted_Fate___Its_all_in_the_cards.Program.Game_OnGameLoad();
                            break;
                        case 8: // Flowers' Series
                            Flowers_Series.Program.Loads();
                            break;
                        case 9: // RARETwistedFate
                            //RARETwistedFate.Program.Main();
                            break;
                        case 10: // Diabath's TwistedFate
                            //D_TwistedFate.Program.Main();
                            break;
                        case 11: // Flower's Twisted Fate
                            //FlowersTwistedFate.Program.Main();
                            break;
                        case 12: // mztikks Twisted Fate
                            //mztikksTwistedFate.Program.Main();
                            break;
                        case 13: // Gross Gore - Fate
                            //GrossGoreTwistedFate.Program.Main();
                            break;
                        case 14: // NoobAIO
                            NoobAIO.Program.Loads();
                            break;
                    }

                    break;
                case "Twitch":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            //OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // iSeriesReborn
                            //iSeriesReborn.Program.OnGameLoad();
                            break;
                        /*
                    case 2: // iTwitch2.0
                        iTwitch.Program.Main();
                        break;
                        */
                        case 2: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 3: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 4: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 5: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 6: // Flowers' Series
                            //Flowers_Series.Program.Main();
                            break;
                        case 7: // InfectedTwitch
                            //Infected_Twitch.Program.Main();
                            break;
                        case 8: // Flower's Twitch
                            //Flowers_Twitch.Program.Main();
                            break;
                        case 9: // NechritoTwitch
                            //Nechrito_Twitch.Program.Main();
                            break;
                        case 10: // SNTwitch
                            //SNTwitch.Program.Main();
                            break;
                        case 11: // theobjops's Twitch
                            //Twiitch.Twitch.Main();
                            break;
                        case 12: // TheTwitch
                            //TheTwitch.Program.Main();
                            break;
                        case 13: // Twitch#
                            //TwitchSharp.Program.Main();
                            break;
                        case 14: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                        case 15: // NoobAIO
                            NoobAIO.Program.Loads();
                            break;
                    }

                    break;
                case "Udyr":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // BrianSharp
                            //BrianSharp.Program.Main();
                            break;
                        case 1: // D-Udyr
                            //D_Udyr.Program.Game_OnGameLoad();
                            break;
                        case 2: // EloFactory Udyr
                            //EloFactory_Udyr.Program.Game_OnGameLoad();
                            break;
                        case 3: // LCS Udyr
                            //LCS_Udyr.Program.OnGameLoad();
                            break;
                        case 4: // UnderratedAIO
                            //UnderratedAIO.Program.OnGameLoad();
                            break;
                        case 5: // ExorAIO
                            //ExorAIO.Program.Main();
                            break;
                        case 6: // NoodUdyr
                            //NoobUdyr.Program.Main();
                            break;
                        case 7: // yetAnotherUdyr
                            //yetAnotherUdyr.Program.Main();
                            break;
                        case 8: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Varus":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // ElVarus
                            ElVarus.Varus.Game_OnGameLoad();
                            break;
                        case 1: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 2: // SFXChallenger
                            //SFXChallenger.Program.Main();
                            break;
                        case 3: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 4: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 5: // VarusGod
                            //Varus_God.Program.Main();
                            break;
                        case 6: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 7: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                        case 8: // ElVarusRevamped
                            //ElVarusRevamped.Program.Main();
                            break;
                    }

                    break;

                case "Pyke":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Easy_Sup
                            Easy_Sup.Program.Loads();
                            break;
                        case 1:
                            break;
                    }

                    break;
                case "Riven":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // KurisuRiven
                            //KurisuRiven.Program.Game_OnGameLoad();
                            break;
                        case 1: // HoolaRiven
                            //HoolaRiven.Program.OnGameLoad();
                            break;
                        case 2: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 3: // NechritoRiven
                            //NechritoRiven.Program.Main();
                            break;
                        case 4: // Flowers' Series
                            //Flowers_Series.Program.Main();
                            break;
                        case 5: // ReforgedRiven
                            //Reforged_Riven.Program.Main();
                            break;
                        case 6: // EasyPeasyRivenSqueezy
                            //EasyPeasyRivenSqueezy.Program.Main();
                            break;
                        case 7: // EloFactory Riven
                            //EloFactory_Riven.Program.Main();
                            break;
                        case 8: // Flower's Riven
                            //Flowers_Riven_Reborn.Program.Main();
                            break;
                        case 9: // HeavenStrikeRiven
                            //HeavenStrikeRiven.Program.Main();
                            break;
                        case 10: // yol0Riven
                            //yol0Riven.Program.Main();
                            break;
                        case 11: // RivenToTheChallenger  
                            //RivenToTheChallenger.Program.Main();
                            break;
                        case 12: // MoonRiven
                            //MoonRiven.Program.Main();
                            break;
                        case 13: // MigthyAIO
                            MightyAio.Program.Loads();
                            break;

                    }

                    break;
                case "Senna":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Shen":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // UnderratedAIO
                            //UnderratedAIO.Program.OnGameLoad();
                            break;
                        case 1: // BrianSharp
                            //BrianSharp.Program.Main();
                            break;
                        case 2: // Kimbaeng Shen
                            //Kimbaeng_Shen.Program.Game_OnGameLoad();
                            break;
                        case 3: // BadaoShen
                            //BadaoShen.Program.Main();
                            break;
                        case 4: // xQx Shen
                            //Shen.Program.Main();
                            break;
                        case 5: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Shyvana":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // D-Shyvana
                            //D_Shyvana.Program.Game_OnGameLoad();
                            break;
                        case 1: // HeavenStrike Shyvana
                            //HeavenStrikeShyvana.Program.Game_OnGameLoad();
                            break;
                        case 2: //  JustShyvana
                            //JustShyvana.Program.OnLoad();
                            break;
                        case 3: // SAutoCarry
                            //SAutoCarry.Program.Game_OnGameLoad();
                            break;
                        case 4: // NoobAIO
                            NoobAIO.Program.Loads();
                            break;
                    }

                    break;
                case "Illaoi":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Tentacle Kitty
                            Illaoi_Tentacle_Kitty.Program.Game_OnGameLoad();
                            break;
                        case 1: // SharpShooter
                            //SharpShooter.Program.Game_OnGameLoad();
                            break;
                        case 2: // Flowers' Series
                            Flowers_Series.Program.Loads();
                            break;
                        case 3: // Kraken Priestess
                            Flowers__Illaoi.Program.Loads();
                            break;
                        case 4: // IllaoiSOH
                            IllaoiSOH.Program.Game_OnGameLoad();
                            break;
                        case 5: // TentacleBabeIllaoi
                            //TentacleBabeIllaoi.Program.Main();
                            break;
                    }

                    break;
                case "Sivir":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 1: // hikiMarksman
                            //hikiMarksmanRework.Program.Game_OnGameLoad();
                            break;
                        case 2: // Proseries
                            //ProSeries.Program.GameOnOnGameLoad();
                            break;
                        case 3: // SFXChallenger
                            //SFXChallenger.Program.Main();
                            break;
                        case 4: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                        case 5: // ShineAIO
                            //ShineSharp.Program.Game_OnGameLoad();
                            break;
                        case 6: // Marksman#
                            //Marksman.Program.Game_OnGameLoad();
                            break;
                        case 7: // ExorAIO
                            //ExorAIO.Program.Main();
                            break;
                        case 8: // Flowers' Series
                            Flowers_Series.Program.Loads();
                            break;
                        case 9: // xcsoft's Sivir
                            //xcSivir.Program.Main();
                            break;
                        case 10: // HeavenStrikeSivir
                            //HeavenStrikeSivir.Program.Main();
                            break;
                        /*
                    case 11: // iSivir
                        iSivir.Program.Main();
                        break;
                        */
                        case 11: // JustSivir
                            //JustSivir.Program.Main();
                            break;
                        case 12: // KurisuSivir
                            //KurisuSivir.Program.Main();
                            break;
                        case 13: // Hikicarry ADC
                            //HikiCarry.Program.Main();
                            break;
                        case 14: // Flowers' ADC Series
                            //Flowers_ADC_Series.Program.Main();
                            break;
                    }

                    break;
                case "Skarner":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // UnderratedAIO
                            //UnderratedAIO.Program.OnGameLoad();
                            break;
                        case 1: // kSkarner
                            //kSkarner.Program.Main();
                            break;
                        case 2: // SneakySkarner
                            //SneakySkarner.Program.Main();
                            break;
                        case 3: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Soraka":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Seph Soraka
                            //SephSoraka.Soraka.Main();
                            break;
                        case 1: // FreshBooster
                            //FreshBooster.Program.Game_OnGameLoad();
                            break;
                        case 2: // Heal-Bot
                            //Soraka_HealBot.Program.OnGameLoad();
                            break;
                        case 3: // MLG Soraka
                            //MLGSORAKA.Program.OnLoad();
                            break;
                        case 4: // Support is Easy
                            //Support.Program.Main();
                            break;
                        case 5: // vSeries
                            //vSupport_Series.Program.Game_OnGameLoad();
                            break;
                        case 6: // ChallengerSeries
                            //Challenger_Series.Program.Main();
                            break;
                        case 7: // Sophie's Soraka
                            //Sophies_Soraka.Program.Main();
                            break;
                        case 8: // Soraka#
                            //SorakaSharp.Source.Program.Main();
                            break;
                        case 9: // SorakaToTheChallenger
                            //SorakaToTheChallenger.Program.Load();
                            break;
                        case 10: // Easy_Sup
                            Easy_Sup.Program.Loads();
                            break;
                    }

                    break;
                case "Syndra":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Kortatu's Syndra
                            //Syndra.Program.Game_OnGameLoad();
                            break;
                        case 1: // BadaoSeries
                            BadaoSeries.Program.OnLoad();
                            break;
                        case 2: // Hikigaya Syndra
                            //Hikigaya_Syndra.Program.OnLoad();
                            break;
                        case 3: // OKTW
                            OneKeyToWin_AIO_Sebby.Program.GameOnOnGameLoad();
                            break;
                        case 4: // Syndra by L33T
                            //SyndraL33T.Bootstrap.Main();
                            break;
                        case 5: // vSeries
                            //vSupport_Series.Program.Game_OnGameLoad();
                            break;
                        case 6: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                        case 7: // SephSyndra
                            //SephSyndra.Syndra.Main();
                            break;
                        case 8: // Syndra - The Dark Sovereign
                            //Syndra______The_Dark_Sovereign.Program.Main();
                            break;
                        case 9: // DarkMage
                            //DarkMage.Program.Main();
                            break;
                        case 10: // Lord's Syndra
                            //LordsSyndra.Program.Main();
                            break;
                        case 11: // SyndraRevamped
                            //SyndraRevamped.Program.Main();
                            break;
                    }

                    break;
                case "Taliyah":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // TophSharp
                            //TophSharp.Taliyah.OnLoad();
                            break;
                        case 1: // ExorAIO
                            ExorAIO.Program.Loads();
                            break;
                        case 2: // Stoneweaver
                            //Taliyah.Program.Main();
                            break;
                        case 3: // Hinata's Taliyah
                            //Hinata_s_Taliyah.Program.Main();
                            break;
                    }
                    break;
                case "Yasuo":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // YasuoPro
                            //new YasuoPro.Yasuo().OnLoad();
                            break;
                        case 1: // BrianSharp
                            //BrianSharp.Program.Main();
                            break;
                        case 2: // GosuMechanics
                            //GosuMechanicsYasuo.Program.Game_OnGameLoad();
                            break;
                        case 3: // YasuoSharpv2
                            //YasuoSharpV2.Program.Main();
                            break;
                        case 4: // MasterOfWinds
                            //MasterOfWind.Program.Main();
                            break;
                        case 5: // M1D 0R F33D
                            //Mid_or_Feed.Program.Main();
                            break;
                        case 6: //YasuoMemeBender
                            //YasuoTheLastMemebender.Program.Game_OnGameLoad();
                            break;
                        case 7: // Valvrave#
                            //Valvrave_Sharp.Program.Main();
                            break;
                        case 8: // Badaos Yasuo
                            //BadaoYasuo.Program.Main();
                            break;
                        case 9: // hYasuo
                            //hYasuo.Program.Main();
                            break;
                        case 10: // ReformedAIO
                            //ReformedAIO.Program.Main();
                            break;
                        case 11: // Flowers' Yasuo
                            Flowers_Yasuo.MyLoader.Loads();
                            break;
                        case 12: // GosuMechanicsYasuo Rebirth
                            //FloraGosYasuo.Program.Main();
                            break;
                    }

                    break;
                case "Xayah":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // SharpShooter
                            SharpShooter.MyLoader.Loads();
                            break;
                    }

                    break;
                case "Yorick":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Yuumi":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // MightyuAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Zac":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // UnderratedAIO
                            //UnderratedAIO.Program.OnGameLoad();
                            break;
                        case 1: // The Secret Flubber
                            //Zac_The_Secret_Flubber.Program.Main();
                            break;
                        case 2: //MightyAIO
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
                case "Zed":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Korean Zed
                            KoreanZed.Program.Game_OnGameLoad();
                            break;
                        case 1: // SharpyAIO
                            Sharpy_AIO.Program.Game_OnGameLoad();
                            break;
                        case 2: // Valvrave#
                            //Valvrave_Sharp.Program.Main();
                            break;
                        case 3: // iDZed
                            //iDZed.Program.Main();
                            break;
                        case 4: // Ze-D Is Back
                            zedisback.Program.Loads();
                            break;
                        case 5: // xSalice
                            //xSaliceResurrected_Rework.Program.LoadReligion();
                            break;
                    }

                    break;
                case "Zoe":
                    switch (Misc.menu[ObjectManager.Player.CharacterName.ToString()].GetValue<MenuList>().Index)
                    {
                        case 0: // Korean Zed
                            MightyAio.Program.Loads();
                            break;
                    }

                    break;
            }
        }

        #endregion

    }
}