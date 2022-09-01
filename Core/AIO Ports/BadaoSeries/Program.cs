using System;
using System.Linq;
using System.Reflection.Emit;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace BadaoSeries
{
    public class Program
    {
        public static Spell Q, Q2, W, W2, E, E2, R, R2;
        public static SpellSlot Smite, Ignite, Flash;
        public static Items.Item Bilgewater, BotRK, Youmuu, Tiamat, Hydra, Sheen, LichBane, IcebornGauntlet, TrinityForce, LudensEcho;
        public static bool enabled = true;
        public static Menu MainMenu;
        
        public static bool Enable
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
                if (MainMenu != null)
                {
                    MainMenu.GetValue<MenuBool>("Enable").SetValue(value);
                }
            }
        }
        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        public static void OnLoad()
        {
            var plugin = Type.GetType("BadaoSeries.Plugin." + Player.CharacterName);
            if (plugin == null)
            {
                AddUI.Notif(Player.CharacterName + ": Not Supported !", 10000);
                return;
            }
            AddUI.Notif(Player.CharacterName + ": Loaded !", 10000);
            //Bilgewater = new Items.Item(ItemId.Bilgewater_Cutlass.Id, 550);
            //BotRK = new Items.Item(ItemId.Blade_of_the_Ruined_King.Id, 550);
            Youmuu = new Items.Item(ItemId.Youmuus_Ghostblade, 0);
            //Hydra = new Items.Item(ItemId.Ravenous_Hydra_Melee_Only.Id, 400);
            Sheen = new Items.Item(ItemId.Sheen, 0);
            LichBane = new Items.Item(ItemId.Lich_Bane, 0);
            TrinityForce = new Items.Item(ItemId.Trinity_Force, 0);
            //IcebornGauntlet = new Items.Item(ItemId.Iceborn_Gauntlet.Id, 0);
            LudensEcho = new Items.Item(ItemId.Ludens_Tempest, 0);

            foreach (var spell in Player.Spellbook.Spells.Where(i =>
                         i.Name.ToLower().Contains("smite") &&
                         (i.Slot == SpellSlot.Summoner1 || i.Slot == SpellSlot.Summoner2)))
            {
                Smite = spell.Slot;
            }
            Ignite = Player.GetSpellSlot("summonerdot");
            Flash = Player.GetSpellSlot("summonerflash");
            
            MainMenu = new Menu("BadaoSeries", "BadaoSeries", true);
            AddUI.Bool(MainMenu, "Enable", Player.CharacterName + " Enable", true).ValueChanged += Program_ValueChanged;
            MainMenu.Attach();
            NewInstance(plugin);
        }

        private static void Program_ValueChanged(MenuBool menuitem, EventArgs args)
        {
            enabled = menuitem.Enabled;
            if (menuitem.Enabled)
            {
                AddUI.Notif(Player.CharacterName + ": Enabled",4000);
            }
            else
            {
                AddUI.Notif(Player.CharacterName + ": Disabled !", 4000);
            }
        }

        private static void NewInstance(Type type)
        {
            var target = type.GetConstructor(Type.EmptyTypes);
            var dynamic = new DynamicMethod(string.Empty, type, new Type[0], target.DeclaringType);
            var il = dynamic.GetILGenerator();
            il.DeclareLocal(target.DeclaringType);
            il.Emit(OpCodes.Newobj, target);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            ((Func<object>)dynamic.CreateDelegate(typeof(Func<object>)))();
        }
    }

    public class AddUI : Program
    {
        public static void Notif(string msg, int time)
        {
            var x = new Notification("BadaoSeries", msg);
        }

        public static MenuSeparator Separator(Menu subMenu, string name, string display)
        {
            return subMenu.Add(new MenuSeparator(name, display));
        }

        public static MenuBool Bool(Menu subMenu, string name, string display, bool state = true)
        {
            return subMenu.Add(new MenuBool(name, display).SetValue(state));
        }

        public static MenuKeyBind KeyBind(Menu subMenu, string name, string display, Keys key, KeyBindType type)
        {
            return subMenu.Add(new MenuKeyBind(name, display,key,type));
        }

        public static MenuList List(Menu subMenu, string name, string display, string[] array)
        {
            return subMenu.Add(new MenuList(name, display, array));
        }

        public static MenuSlider Slider(Menu subMenu, string name, string display, int cur, int min = 0, int max = 100)
        {
            return subMenu.Add(new MenuSlider(name, display, cur, min, max));
        }
    }
}