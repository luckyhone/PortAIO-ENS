using System;
using System.Collections.Generic;
using System.Linq;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

using SharpDX;

namespace OlympusAIO.Helpers
{
    internal class UtilityManager
    {
        public static readonly string[] JungleList =
{
            "SRU_Dragon_Air", "SRU_Dragon_Fire", "SRU_Dragon_Water",
            "SRU_Dragon_Earth", "SRU_Dragon_Elder", "SRU_Baron",
            "SRU_RiftHerald", "SRU_Red", "SRU_Blue", "SRU_Gromp",
            "Sru_Crab", "SRU_Krug", "SRU_Razorbeak", "SRU_Murkwolf"
        };
        public static readonly string[] Marksmans =
        {
            "Aphelios", "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Jhin", "Jinx",
            "KaiSa", "Kalista", "Kindred", "KogMaw", "Lucian", "MissFortune", "Senna",
            "Sivir", "Tristana", "Twitch", "Varus", "Vayne", "Xayah",
        };
        public static readonly HitChance[] HitchanceArray =
        {
            HitChance.Low,
            HitChance.Medium,
            HitChance.High,
            HitChance.VeryHigh,
        };
        public static readonly string[] HitchanceNameArray =
        {
            "Low",
            "Medium",
            "High",
            "Very High",
        };
        public static HitChance GetHitChance(MenuList menu)
        {
            var hitchance = HitChance.Medium;

            switch (menu.SelectedValue)
            {
                case "Low":
                    hitchance = HitChance.Low;
                    break;
                case "Medium":
                    hitchance = HitChance.Medium;
                    break;
                case "High":
                    hitchance = HitChance.High;
                    break;
                case "Very High":
                    hitchance = HitChance.VeryHigh;
                    break;
            }

            return hitchance;
        }
        public static float GetAngleByDegrees(float x)
        {
            return (float)(x * Math.PI / 180f);
        }
        public static float GetTargetHealthWithShield(AIBaseClient target)
        {
            return target.Health + target.AllShield;
        }
        public static List<Vector3> PointsAroundTarget(Vector3 pos, float dist, float prec = 15, float prec2 = 0)
        {
            if (pos.IsZero)
                return new List<Vector3>();

            List<Vector3> list = new List<Vector3>();

            if (dist > 205)
            {
                prec = 30; prec2 = 8;
            }
            if (dist > 805)
            {
                dist = (float)(dist * 1.5); prec = 45; prec2 = 10;
            }

            var angle = 360 / prec * Math.PI / 180.0f; var step = dist * 2 / prec2;

            for (int i = 0; i < prec; i++)
            {
                for (int ib = 0; ib < 6; ib++)
                {
                    list.Add(new Vector3(
                            pos.X + (float)(Math.Cos(angle * i) * (ib * step)),
                            pos.Y + (float)(Math.Sin(angle * i) * (ib * step)) - 90,
                            pos.Z));
                }
            }
            return list;
        }
        public static int GetWaitTimeForHumanizerByChampion(string slot)
        {
            switch (OlympusAIO.objPlayer.CharacterName)
            {
                case "AurelionSol":
                    if (slot == SpellSlot.Q.ToString())
                        return 50;
                    if (slot == SpellSlot.W.ToString())
                        return 50;
                    if (slot == SpellSlot.E.ToString())
                        return 50;
                    if (slot == SpellSlot.R.ToString())
                        return 50;
                    break;
                case "Evelynn":
                    if (slot == SpellSlot.Q.ToString())
                        return 50;
                    if (slot == SpellSlot.W.ToString())
                        return 50;
                    if (slot == SpellSlot.E.ToString())
                        return 50;
                    if (slot == SpellSlot.R.ToString())
                        return 50;
                    break;
                case "Heimerdinger":
                    if (slot == SpellSlot.Q.ToString())
                        return 50;
                    if (slot == SpellSlot.W.ToString())
                        return 50;
                    if (slot == SpellSlot.E.ToString())
                        return 50;
                    if (slot == SpellSlot.R.ToString())
                        return 50;
                    break;
                case "Lissandra":
                    if (slot == SpellSlot.Q.ToString())
                        return 50;
                    if (slot == SpellSlot.W.ToString())
                        return 50;
                    if (slot == SpellSlot.E.ToString())
                        return 50;
                    if (slot == SpellSlot.R.ToString())
                        return 50;
                    break;
                case "Poppy":
                    if (slot == SpellSlot.Q.ToString())
                        return 50;
                    if (slot == SpellSlot.W.ToString())
                        return 50;
                    if (slot == SpellSlot.E.ToString())
                        return 50;
                    if (slot == SpellSlot.R.ToString())
                        return 50;
                    break;
                case "Teemo":
                    if (slot == SpellSlot.Q.ToString())
                        return 50;
                    if (slot == SpellSlot.W.ToString())
                        return 50;
                    if (slot == SpellSlot.E.ToString())
                        return 50;
                    if (slot == SpellSlot.R.ToString())
                        return 50;
                    break;
            }

            if (OlympusAIO.SupportedChampions.All(x => !string.Equals(x, OlympusAIO.objPlayer.CharacterName, StringComparison.CurrentCultureIgnoreCase)))
            {
                if (slot == SpellSlot.Q.ToString())
                    return 50;
                if (slot == SpellSlot.W.ToString())
                    return 50;
                if (slot == SpellSlot.E.ToString())
                    return 50;
                if (slot == SpellSlot.R.ToString())
                    return 50;
            }

            return 50;
        }
        public static int GetMoveWaitTimeForHumanizer()
        {
            if (OlympusAIO.SupportedChampions.All(x => !string.Equals(x, OlympusAIO.objPlayer.CharacterName, StringComparison.CurrentCultureIgnoreCase)))
                return 50;

            switch (OlympusAIO.objPlayer.CharacterName)
            {
                case "AurelionSol":
                    return 50;
                case "Evelynn":
                    return 50;
                case "Heimerdinger":
                    return 50;
                case "Lissandra":
                    return 50;
                case "Poppy":
                    return 50;
                case "Teemo":
                    return 50;
            }

            return 0;
        }
    }
}
