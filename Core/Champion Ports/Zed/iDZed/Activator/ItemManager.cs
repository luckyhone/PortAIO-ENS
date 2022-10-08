using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using iDZed.Activator.Spells;
using LeagueSharpCommon;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;

namespace iDZed.Activator
{
    internal class ItemManager
    {
        private static float _lastCheckTick;
        //TODO: List of Activator Features here:

        //TODO: Shield Module
        //TODO: Summoners Spells Implementation #Partially Done

        private static readonly List<DzItem> ItemList = new List<DzItem>
        {
            new DzItem
            {
                Id = 3142,
                Name = "Youmuu",
                Range = 600f,
                Class = ItemClass.Offensive,
                Mode = ItemMode.NoTarget
            },
        };

        private static readonly List<ISummonerSpell> SummonerSpellsList = new List<ISummonerSpell>
        {
            new Ignite(),
            new Heal()
        };

        public static void OnLoad(Menu menu)
        {
            //Create the menu here.
            var cName = ObjectManager.Player.CharacterName;
            var activatorMenu = new Menu("com.idz.zed.activator",":: Activator");

            //Offensive Menu
            var offensiveMenu = new Menu("com.idz.zed.activator.offensive","Activator - Offensive");
            var offensiveItems = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (var item in offensiveItems)
            {
                var itemMenu = new Menu(item.Name, cName + item.Id);
                itemMenu.Add(new MenuBool("com.idz.zed.activator." + item.Id + ".always", "Always").SetValue(true));
                itemMenu.Add(
                    new MenuSlider("com.idz.zed.activator." + item.Id + ".onmyhp", "On my HP < then %",30));
                itemMenu.Add(
                    new MenuSlider("com.idz.zed.activator." + item.Id + ".ontghpgreater", "On Target HP > then %",40));
                itemMenu.Add(
                    new MenuSlider("com.idz.zed.activator." + item.Id + ".ontghplesser", "On Target HP < then %",40));
                itemMenu.Add(
                    new MenuBool("com.idz.zed.activator." + item.Id + ".ontgkill", "On Target Killable").SetValue(true));
                itemMenu.Add(
                    new MenuBool("com.idz.zed.activator." + item.Id + ".displaydmg", "Display Damage").SetValue(true));
                offensiveMenu.Add(itemMenu);
            }
            activatorMenu.Add(offensiveMenu);

            var summonerSpellsMenu = new Menu("com.idz.zed.activator.summonerspells","Activator - Spells");
            foreach (var spell in SummonerSpellsList)
            {
                var tempMenu = new Menu(
                    "com.idz.zed.activator.summonerspells." + spell.GetName(),spell.GetDisplayName());
                spell.AddToMenu(tempMenu);
                tempMenu.Add(
                    new MenuBool("com.idz.zed.activator.summonerspells." + spell.GetName() + ".enabled", "Enabled")
                        .SetValue(true));
                summonerSpellsMenu.Add(tempMenu);
            }
            activatorMenu.Add(summonerSpellsMenu);

            //Defensive Menu
            AddHitChanceSelector(activatorMenu);

            activatorMenu.Add(
                new MenuSlider("com.idz.zed.activator.activatordelay", "Global Activator Delay",80, 0, 300));
            activatorMenu.Add(
                new MenuBool("com.idz.zed.activator.enabledalways", "Enabled Always?").SetValue(false));
            activatorMenu.Add(
                new MenuKeyBind("com.idz.zed.activator.enabledcombo", "Enabled On Press?",Keys.Space, KeyBindType.Press));
            activatorMenu.Add(
                new MenuBool("com.idz.zed.activator.afterDeathmark", "Use after deathmark").SetValue(true));
            menu.Add(activatorMenu);

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.enabledalways"].GetValue<MenuBool>().Enabled &&
                !Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.enabledcombo"].GetValue<MenuKeyBind>().Active)
            {
                return;
            }
            if (Environment.TickCount - _lastCheckTick <
                Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.activatordelay"].GetValue<MenuSlider>().Value)
            {
                return;
            }
            _lastCheckTick = Environment.TickCount;
            UseOffensive();
            UseSummonerSpells();
        }

        public static void UseDeathmarkItems()
        {
            var items = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (DzItem item in items)
            {
                AIHeroClient target = TargetSelector.GetTarget(item.Range, DamageType.True);
                if (!target.IsValidTarget())
                    return;

                if (Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.afterDeathmark"].GetValue<MenuBool>().Enabled &&
                    target.HasBuff("zedulttargetmark"))
                {
                    UseItem(target, item);
                }
            }
        }

        private static void UseOffensive()
        {
            var offensiveItems = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (DzItem item in offensiveItems)
            {
                AIBaseClient selectedTarget = TargetSelector.SelectedTarget as AIBaseClient ??
                                             TargetSelector.GetTarget(item.Range, DamageType.True);
                if (!selectedTarget.IsValidTarget(item.Range))
                {
                    return;
                }
                if (Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.offensive"]["com.idz.zed.activator." + item.Id + ".always"].GetValue<MenuBool>().Enabled)
                {
                    UseItem(selectedTarget, item);
                }
                if (ObjectManager.Player.HealthPercent <
                    Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.offensive"]["com.idz.zed.activator." + item.Id + ".onmyhp"].GetValue<MenuSlider>().Value)
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.HealthPercent <
                    Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.offensive"]["com.idz.zed.activator." + item.Id + ".ontghplesser"].GetValue<MenuSlider>().Value &&
                    !Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.offensive"]["com.idz.zed.activator." + item.Id + ".ontgkill"].GetValue<MenuBool>().Enabled)
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.HealthPercent >
                    Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.offensive"]["com.idz.zed.activator." + item.Id + ".ontghpgreater"].GetValue<MenuSlider>().Value)
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.Health < ObjectManager.Player.GetSpellDamage(selectedTarget, GetItemSpellSlot(item)) &&
                    Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.offensive"]["com.idz.zed.activator." + item.Id + ".ontgkill"].GetValue<MenuBool>().Enabled)
                {
                    UseItem(selectedTarget, item);
                }
            }
        }

        public static void UseSummonerSpells()
        {
            foreach (ISummonerSpell spell in SummonerSpellsList.Where(spell => spell.RunCondition()))
            {
                spell.Execute();
            }
        }

        private static void UseItem(AIBaseClient target, DzItem item)
        {
            if (!Items.HasItem(ObjectManager.Player,item.Id) || !Items.CanUseItem(ObjectManager.Player,item.Id))
            {
                return;
            }
            switch (item.Mode)
            {
                case ItemMode.Targeted:
                    Items.UseItem(ObjectManager.Player,item.Id, target);
                    break;
                case ItemMode.NoTarget:
                    Items.UseItem(ObjectManager.Player,item.Id, ObjectManager.Player);
                    break;
                case ItemMode.Skillshot:
                    if (item.CustomInput == null)
                    {
                        return;
                    }
                    PredictionOutput customPred = Prediction.GetPrediction(item.CustomInput);
                    if (customPred.Hitchance >= GetHitchance())
                    {
                        Items.UseItem(ObjectManager.Player,item.Id, customPred.CastPosition);
                    }
                    break;
            }
        }

        private static SpellSlot GetItemSpellSlot(DzItem item)
        {
            foreach (var it in ObjectManager.Player.InventoryItems.Where(it => (int) it.Id == item.Id))
            {
                return it.SpellSlot != SpellSlot.Unknown ? it.SpellSlot : SpellSlot.Unknown;
            }
            return SpellSlot.Unknown;
        }

        public static HitChance GetHitchance()
        {
            switch (Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.customhitchance"].GetValue<MenuList>().Index)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        public static void AddHitChanceSelector(Menu menu)
        {
            menu.Add(
                new MenuList("com.idz.zed.activator.customhitchance", "Hitchance",new[] { "Low", "Medium", "High", "Very High" }, 2));
        }

        internal static float GetItemsDamage(AIHeroClient target)
        {
            var items =
                ItemList.Where(
                    item =>
                        Items.HasItem(ObjectManager.Player,item.Id) && Items.CanUseItem(ObjectManager.Player,item.Id) &&
                        Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.offensive"]["com.idz.zed.activator." + item.Id + ".displaydmg"].GetValue<MenuBool>().Enabled);
            return items.Sum(item => (float) ObjectManager.Player.GetSpellDamage(target, GetItemSpellSlot(item)));
        }
    }

    internal class DzItem
    {
        public String Name { get; set; }
        public int Id { get; set; }
        public float Range { get; set; }
        public ItemClass Class { get; set; }
        public ItemMode Mode { get; set; }
        public PredictionInput CustomInput { get; set; }
    }

    internal enum ItemMode
    {
        Targeted,
        Skillshot,
        NoTarget
    }

    internal enum ItemClass
    {
        Offensive,
        Defensive
    }
}