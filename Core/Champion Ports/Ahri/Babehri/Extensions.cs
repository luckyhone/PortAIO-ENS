using System;
using System.IO;
using System.Linq;
using System.Media;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace Babehri
{
    internal static class Extensions
    {
        public static bool IsActive(this OrbwalkerMode mode)
        {
            return mode != OrbwalkerMode.None;
        }

        public static SpellSlot GetSpellSlot(this AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            var instance = sender.Spellbook.Spells.FirstOrDefault(spell => spell.Name.Equals(args.SData.Name));
            return instance == null ? SpellSlot.Unknown : instance.Slot;
        }

        public static SpellDataInst[] GetMainSpells(this Spellbook spellbook)
        {
            return new[]
            {
                spellbook.GetSpell(SpellSlot.Q), spellbook.GetSpell(SpellSlot.W), spellbook.GetSpell(SpellSlot.E),
                spellbook.GetSpell(SpellSlot.R)
            };
        }

        public static bool IsSkillShot(this SpellDataTargetType type)
        {
            return type.Equals(SpellDataTargetType.Location)  ||
                   type.Equals(SpellDataTargetType.LocationClamped);
        }

        public static bool IsTargeted(this SpellDataTargetType type)
        {
            return type.Equals(SpellDataTargetType.Target) || type.Equals(SpellDataTargetType.Self);
        }

        public static ColorBGRA ToBGRA(this Color color)
        {
            return new ColorBGRA(color.R, color.G, color.B, color.A);
        }

        public static bool IsComboMode(this OrbwalkerMode mode)
        {
            return mode.Equals(OrbwalkerMode.Combo) || mode.Equals(OrbwalkerMode.Harass);
        }

        public static bool IsFarmMode(this OrbwalkerMode mode)
        {
            return mode.Equals(OrbwalkerMode.LaneClear) || mode.Equals(OrbwalkerMode.LastHit);
        }

        public static string GetModeString(this OrbwalkerMode mode)
        {
            return mode.Equals(OrbwalkerMode.Harass) ? "Harass" : mode.ToString();
        }

        public static HitChance GetHitChance(this MenuItem item)
        {
            return (HitChance) item.GetValue<MenuList>().Index + 3;
        }

        public static void AddList(this Menu menu, string name, string displayName, string[] list, int selectedIndex = 0)
        {
            menu.Add(new MenuList(name, displayName,list, selectedIndex));
        }

        public static void AddBool(this Menu menu, string name, string displayName, bool value = true)
        {
            menu.Add(new MenuBool(name, displayName).SetValue(value));
        }

        public static void AddHitChance(this Menu menu, string name, string displayName, HitChance defaultHitChance)
        {
            menu.Add(
                new MenuList(name, displayName,new[] { "Low", "Medium", "High", "Very High" }, (int) defaultHitChance - 3));
        }

        public static void AddSlider(this Menu menu,
            string name,
            string displayName,
            int value,
            int min = 0,
            int max = 100)
        {
            menu.Add(new MenuSlider(name, displayName,value, min, max));
        }
        
    }

    internal class SoundObject
    {
        public static float LastPlayed;
        private static SoundPlayer _sound;

        public SoundObject(Stream sound)
        {
            LastPlayed = 0;
            _sound = new SoundPlayer(sound);
        }

        public void Play()
        {
            if (Environment.TickCount - LastPlayed < 1500)
            {
                return;
            }
            _sound.Play();
            LastPlayed = Environment.TickCount;
        }
    }
}