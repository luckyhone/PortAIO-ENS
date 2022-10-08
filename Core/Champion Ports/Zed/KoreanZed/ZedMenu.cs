using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using KoreanZed.Enumerators;

namespace KoreanZed
{
    class ZedMenu
    {
        public Menu MainMenu;

        private readonly ZedSpells zedSpells;
        
        public ZedMenu(ZedSpells zedSpells)
        {
            MainMenu = new Menu("mainmenu","Korean Zed", true);
            this.zedSpells = zedSpells;

            //AddTargetSelector();
            //AddOrbwalker(out orbwalker);
            ComboMenu();
            HarassMenu();
            LaneClearMenu();
            LastHitMenu();
            MiscMenu();
            DrawingMenu();

            MainMenu.Attach();

            GetInitialSpellValues();
        }

        private void ComboMenu()
        {
            string prefix = "koreanzed.combomenu";
            Menu comboMenu = new Menu(prefix, "Combo");
            
            comboMenu.Add(new MenuBool(prefix + ".useq", "Use Q").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.Q.UseOnCombo = sender.GetValue<MenuBool>(prefix + ".useq").Enabled; };

            comboMenu.Add(new MenuBool(prefix + ".usew", "Use W").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.W.UseOnCombo = sender.GetValue<MenuBool>(prefix + ".usew").Enabled; };

            comboMenu.Add(new MenuBool(prefix + ".usee", "Use E").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.E.UseOnCombo = sender.GetValue<MenuBool>(prefix + ".usee").Enabled; };

            comboMenu.Add(new MenuBool(prefix + ".user", "Use R").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.R.UseOnCombo = sender.GetValue<MenuBool>(prefix + ".user").Enabled; };
            
            Menu rBlockSettings = new Menu(prefix + ".neverultmenu","Use R Against");
            string blockUltPrefix = prefix + ".blockult";
            foreach (var objAiHero in GameObjects.EnemyHeroes)
            {
                rBlockSettings.Add(
                    new MenuBool(
                        string.Format("{0}.{1}", blockUltPrefix, objAiHero.CharacterName.ToLowerInvariant()),
                        objAiHero.CharacterName).SetValue(true));
            }
            
            comboMenu.Add(new MenuBool("koreanzed.combo.ronselected", "Use R ONLY on Selected Target").SetValue(false));

            comboMenu.Add(new MenuSeparator(prefix + ".labelcombo1", "To switch the combo style:"));
            comboMenu.Add(new MenuSeparator(prefix + ".labelcombo2", "1 - Hold SHIFT; 2 - LEFT Click on Zed;"));
            comboMenu.Add(
                new MenuList(prefix + ".combostyle", "Combo Style",new string[] { "All Star", "The Line" }));

            //comboMenu.Add(useItems);
            comboMenu.Add(rBlockSettings);
            MainMenu.Add(comboMenu);
        }

        private void HarassMenu()
        {
            string prefix = "koreanzed.harasmenu";
            Menu harasMenu = new Menu(prefix,"Harass");
            
            harasMenu.Add(new MenuBool(prefix + ".useq", "Use Q").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.Q.UseOnHarass = sender.GetValue<MenuBool>(prefix + ".useq").Enabled; };

            harasMenu.Add(
                new MenuBool(prefix + ".checkcollisiononq", "Check Collision Before Using Q").SetValue(false));

            harasMenu.Add(new MenuBool(prefix + ".usee", "Use E").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.E.UseOnHarass = sender.GetValue<MenuBool>(prefix + ".usee").Enabled; };
            string eUsagePrefix = prefix + ".wusage";
            Menu eHarasUsage = new Menu(eUsagePrefix,"W Settings");
            
            eHarasUsage.Add(new MenuBool(prefix + ".usew", "Use W").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    zedSpells.W.UseOnHarass = sender.GetValue<MenuBool>(prefix + ".usew").Enabled;
                };
            eHarasUsage.Add(
                new MenuList(eUsagePrefix + ".trigger", "Trigger",new string[] { "Max Range", "Max Damage" }));
            eHarasUsage.Add(
                new MenuSlider(eUsagePrefix + ".dontuseagainst", "Don't Use if Laning Against X Enemies",6, 2, 6));
            eHarasUsage.Add(
                new MenuSlider(eUsagePrefix + ".dontuselowlife", "Don't Use if % HP Below",0, 0, 100));
            
            string blackListPrefix = prefix + ".blacklist";
            Menu blackListHaras = new Menu(blackListPrefix + "","Harass Target(s)");
            foreach (var objAiHero in GameObjects.EnemyHeroes)
            {
                blackListHaras.Add(
                    new MenuBool(
                        string.Format("{0}.{1}", blackListPrefix, objAiHero.CharacterName.ToLowerInvariant()),
                        objAiHero.CharacterName).SetValue(true));
            }
            
            harasMenu.Add(new MenuSlider(prefix + ".saveenergy", "Save Energy (%)",50, 0, 100));

            harasMenu.Add(blackListHaras);
            //harasMenu.Add(useItems);
            harasMenu.Add(eHarasUsage);
            MainMenu.Add(harasMenu);
        }

        private void LaneClearMenu()
        {
            string prefix = "koreanzed.laneclearmenu";
            Menu laneClearMenu = new Menu(prefix,"Lane / Jungle Clear");
            
            laneClearMenu.Add(new MenuBool(prefix + ".useq", "Use Q").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    zedSpells.Q.UseOnLaneClear = sender.GetValue<MenuBool>(prefix + ".useq").Enabled;
                };

            laneClearMenu.Add(new MenuSlider(prefix + ".useqif", "Min. Minions to Use Q",3, 1, 6));

            laneClearMenu.Add(new MenuBool(prefix + ".usew", "Use W").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    zedSpells.W.UseOnLaneClear = sender.GetValue<MenuBool>(prefix + ".usew").Enabled;
                };
            laneClearMenu.Add(new MenuSlider(prefix + ".dontuseeif", "Don't Use if Laning Against X Enemies",6, 1, 6));

            laneClearMenu.Add(new MenuBool(prefix + ".usee", "Use E").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    zedSpells.E.UseOnLaneClear = sender.GetValue<MenuBool>(prefix + ".usee").Enabled;
                };

            laneClearMenu.Add(new MenuSlider(prefix + ".useeif", "Min. Minions to Use E",3, 1, 6));

            laneClearMenu.Add(new MenuSlider(prefix + ".saveenergy", "Save Energy (%)",40, 0, 100));
            
            MainMenu.Add(laneClearMenu);
        }
        
        private void LastHitMenu()
        {
            string prefix = "koreanzed.lasthitmenu";
            Menu lastHitMenu = new Menu(prefix,"Last Hit");

            lastHitMenu.Add(new MenuBool(prefix + ".useq", "Use Q").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    zedSpells.Q.UseOnLastHit = sender.GetValue<MenuBool>(prefix + ".useq").Enabled;
                };

            lastHitMenu.Add(new MenuBool(prefix + ".usee", "Use E").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    zedSpells.E.UseOnLastHit = sender.GetValue<MenuBool>(prefix + ".usee").Enabled;
                };

            lastHitMenu.Add(new MenuSlider(prefix + ".useeif", "Min. Minions to Use E",3, 1, 6));

            lastHitMenu.Add(new MenuSlider(prefix + ".saveenergy", "Save Energy (%)",0));

            MainMenu.Add(lastHitMenu);
        }
        

        private void GetInitialSpellValues()
        {
            zedSpells.Q.UseOnCombo = GetParamBool("koreanzed.combomenu.useq");
            zedSpells.W.UseOnCombo = GetParamBool("koreanzed.combomenu.usew");
            zedSpells.E.UseOnCombo = GetParamBool("koreanzed.combomenu.usee");
            zedSpells.R.UseOnCombo = GetParamBool("koreanzed.combomenu.user");

            zedSpells.Q.UseOnHarass = GetParamBool("koreanzed.harasmenu.useq");
            //zedSpells.W.UseOnHarass = GetParamBool("koreanzed.harasmenu.usew");
            zedSpells.W.UseOnHarass = MainMenu["koreanzed.harasmenu.wusage"].GetValue<MenuBool>("koreanzed.harasmenu.usew").Enabled;
            zedSpells.E.UseOnHarass = GetParamBool("koreanzed.harasmenu.usee");

            zedSpells.Q.UseOnLastHit = GetParamBool("koreanzed.lasthitmenu.useq");
            zedSpells.E.UseOnLastHit = GetParamBool("koreanzed.lasthitmenu.usee");

            zedSpells.Q.UseOnLaneClear = GetParamBool("koreanzed.laneclearmenu.useq");
            zedSpells.W.UseOnLaneClear = GetParamBool("koreanzed.laneclearmenu.usew");
            zedSpells.E.UseOnLaneClear = GetParamBool("koreanzed.laneclearmenu.usee");
        }

        private void MiscMenu()
        {
            string prefix = "koreanzed.miscmenu";
            Menu miscMenu = new Menu(prefix,"Miscellaneous");

            string gcPrefix = prefix + ".gc";
            Menu antiGapCloserMenu = new Menu(gcPrefix,"Gapcloser Options");
            antiGapCloserMenu.Add(new MenuBool(prefix + ".usewantigc", "Use W to Escape").SetValue(true));
            antiGapCloserMenu.Add(new MenuBool(prefix + ".useeantigc", "Use E to Slow").SetValue(true));

            string rDodgePrefix = prefix + ".rdodge";
            Menu rDodgeMenu = new Menu(rDodgePrefix,"Use R to Dodge");
            rDodgeMenu.Add(new MenuBool(rDodgePrefix + ".user", "Active").SetValue(true));
            rDodgeMenu.Add(new MenuSlider(rDodgePrefix + ".dodgeifhealf", "Only if % HP Below",90, 0, 100));
            rDodgeMenu.Add(new MenuSeparator(rDodgePrefix + ".label", "Dangerous Spells to Dodge:"));

            string[] neverDodge =
            {
                "shen", "karma", "poppy", "soraka", "janna", "nidalee", "zilean", "yorick",
                "mordekaiser", "vayne", "tryndamere", "trundle", "nasus", "lulu", "masteryi",
                "kennen", "anivia", "heimerdinger", "drmundo", "elise", "fiora", "jax", "kassadin",
                "khazix", "maokai", "fiddlesticks", "poppy", "shaco", "olaf", "alistar", "aatrox",
                "taric", "nunu", "katarina", "rammus", "singed", "twistedfate", "teemo", "sivir",
                "udyr"
            };
            
            foreach (
                AIHeroClient objAiHero in
                GameObjects.EnemyHeroes.Where(hero => !neverDodge.Contains(hero.CharacterName.ToLowerInvariant()))
                    .OrderBy(hero => hero.CharacterName))
            {
                foreach (
                    SpellDataInst spellDataInst in objAiHero.Spellbook.Spells.Where(spell => spell.Slot == SpellSlot.R))
                {
                    rDodgeMenu.Add(
                        new MenuBool(
                            rDodgePrefix + "." + spellDataInst.Name.ToLowerInvariant(),
                            objAiHero.CharacterName + " - " + spellDataInst.Name.Replace(objAiHero.CharacterName, "")).SetValue(
                            true));
                }
            }
            string potPrefix = prefix + ".pot";
            Menu usePotionMenu = new Menu(potPrefix,"Use Health Potion");
            usePotionMenu.Add(new MenuBool(potPrefix + ".active", "Active").SetValue(true));
            usePotionMenu.Add(new MenuSlider(potPrefix + ".when", "Use Potion at % HP",65));

            miscMenu.Add(new MenuKeyBind(prefix + ".flee", "Flee",Keys.G, KeyBindType.Press));

            miscMenu.Add(new MenuBool(prefix + ".autoe", "Auto E if any enemy is on range").SetValue(true));

            miscMenu.Add(new MenuBool(prefix + ".forceultimate", "Force R Using Mouse Buttons (Cursor Sprite)").SetValue(true));

            miscMenu.Add(antiGapCloserMenu);
            miscMenu.Add(rDodgeMenu);
            miscMenu.Add(usePotionMenu);
            MainMenu.Add(miscMenu);
        }

        private void DrawingMenu()
        {
            string prefix = "koreanzed.drawing";
            Menu drawingMenu = new Menu(prefix,"Drawings");

            drawingMenu.Add(new MenuBool(prefix + ".damageindicator", "Damage Indicator").SetValue(true));
            drawingMenu.Add(
                new MenuList(prefix + ".damageindicatorcolor", "Color Scheme",new string[] { "Normal", "Colorblind", "Sexy (Beta)" }));
            drawingMenu.Add(new MenuBool(prefix + ".killableindicator", "Killable Indicator").SetValue(true));
            drawingMenu.Add(new MenuBool(prefix + ".skillranges", "Skill Ranges").SetValue(true));

            MainMenu.Add(drawingMenu);
        }
        
        public List<AIHeroClient> GetBlockList(BlockListType blockListType)
        {
            List<AIHeroClient> blackList = new List<AIHeroClient>();

            switch (blockListType)
            {
                case BlockListType.Harass:
                    foreach (AIHeroClient objAiHero in GameObjects.EnemyHeroes)
                    {
                        if (!MainMenu["koreanzed.harasmenu.blacklist"]
                                .GetValue<MenuBool>("koreanzed.harasmenu.blacklist." +
                                                    objAiHero.CharacterName.ToLowerInvariant()).Enabled)
                        {
                            blackList.Add(objAiHero);
                        }

                    }
                    break;

                case BlockListType.Ultimate:
                    foreach (AIHeroClient objAiHero in GameObjects.EnemyHeroes)
                    {
                        if (!MainMenu["koreanzed.combomenu.blockult"]
                                .GetValue<MenuBool>("koreanzed.combomenu.blockult." +
                                                    objAiHero.CharacterName.ToLowerInvariant()).Enabled)
                        {
                            blackList.Add(objAiHero);
                        }


                    }
                    break;
            }

            return blackList;
        }
        
        
        public bool GetParamKeyBind(string paramName)
        {
            return MainMenu.GetValue<MenuKeyBind>(paramName).Active;
        }

        public int GetParamSlider(string paramName)
        {
            return MainMenu.GetValue<MenuSlider>(paramName).Value;
        }

        public bool GetParamBool(string paramName)
        {
            return MainMenu.GetValue<MenuBool>(paramName).Enabled;
        }

        public int GetParamStringList(string paramName)
        {
            return MainMenu.GetValue<MenuList>(paramName).Index;
        }

        public bool GetPotActive()
        {
            return MainMenu["koreanzed.miscmenu"].GetValue<MenuBool>("koreanzed.miscmenu.pot.active").Enabled;
        }
        public bool Getuseeantigc()
        {
            return MainMenu["koreanzed.miscmenu"].GetValue<MenuBool>("koreanzed.miscmenu.useeantigc").Enabled;
        }
        public int GetPotActiveWhen()
        {
            return MainMenu["koreanzed.miscmenu"].GetValue<MenuSlider>("koreanzed.miscmenu.pot.when").Value;
        }

        public int GetDontUseLowLife()
        {
            return MainMenu["koreanzed.harasmenu.wusage"]
                .GetValue<MenuSlider>("koreanzed.harasmenu.wusage.dontuselowlife").Value;
        }

        public int Getdontuseagainst()
        {
            return MainMenu["koreanzed.harasmenu.wusage"]
                .GetValue<MenuSlider>("koreanzed.harasmenu.wusage.dontuseagainst").Value;
        }

        public int Gettrigger()
        {
            return MainMenu["koreanzed.harasmenu.wusage"].GetValue<MenuList>("koreanzed.harasmenu.wusage.trigger")
                .Index;
        }
        
        public ComboType GetCombo()
        {
            return (ComboType)GetParamStringList("koreanzed.combomenu.combostyle");
        }

        public void SetCombo(ComboType comboStyle)
        {
            var teste =
                MainMenu.GetValue<MenuList>("koreanzed.combomenu.combostyle")
                    .SetValue((int) comboStyle);
        }
        

    }
}