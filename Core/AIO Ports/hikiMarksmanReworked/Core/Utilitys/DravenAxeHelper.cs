using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using Geometry = LeagueSharpCommon.Geometry.Geometry;

namespace hikiMarksmanRework.Core.Utilitys
{
    class Axe
    {
        public Axe(GameObject obj)
        {
            AxeObj = obj;
            EndTick = Environment.TickCount + 1200;
        }

        public int EndTick;
        public GameObject AxeObj;
    }

    class DravenAxeHelper
    {
        public static List<Axe> AxeSpots = new List<Axe>();
        public static int CurrentAxes;
        public static int LastAa;
        public static int LastQ;
        public static List<string> AxesList = new List<string>
        {
            "Draven_reticle.troy" ,
            "Draven_reticle.troy" ,
            "Draven_reticle.troy"
        };
        public static List<String> QBuffList = new List<string>()
        {
            "Draven_Base_Q_buf.troy",
            "Draven_Skin01_Q_buf.troy",
            "Draven_Skin02_Q_buf.troy",
            "Draven_Skin03_Q_buf.troy"
        };
        public static AIHeroClient Draven { get { return ObjectManager.Player; } }
        public static bool HasQBuff { get { return Draven.Buffs.Any(a => a.Name.ToLower().Contains("spinning")); } }

        public static int MidAirAxes
        {
            get { return AxeSpots.Count(a => a.AxeObj.IsValid && a.EndTick < Environment.TickCount); }
        }

        public static float RealAutoAttack(AIBaseClient target)
        {
            return (float)Draven.CalculateDamage(target, DamageType.Physical, (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod +
                (((DravenSpells.Q.Level) > 0 && HasQBuff ? new float[] { 45, 55, 65, 75, 85 }[DravenSpells.Q.Level - 1] : 0) / 100 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod))));
        }
        public static bool InCatchRadius(Axe a)
        {
            var mode = DravenMenu.Config["Miscellaneous"]["catchRadiusMode"].GetValue<MenuList>().Index;
            switch (mode)
            {
                case 1:
                    var b = new Geometry.Polygon.Sector(Draven.Position.ToVector2(), Game.CursorPos.ToVector2() - Draven.Position.ToVector2(), 100 * (float)Math.PI / 180
                        * (float)Math.PI / 180, 120).IsOutside(a.AxeObj.Position.Extend(Game.CursorPos, 30).ToVector2());

                    return !b;
                default:
                    return a.AxeObj.Position.Distance(Game.CursorPos) <
                           120;
            }
        }

        public static void AIHeroClient_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (ObjectManager.Player.Spellbook.IsAutoAttack)
            {
                LastAa = Environment.TickCount;
            }
            if (args.SData.Name == "dravenspinning")
            {
                LastQ = Environment.TickCount;
            }
        }

        public static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Draven_") && sender.Name.Contains("_Q_reticle_self") && sender.Position.Distance(ObjectManager.Player.Position) /
                ObjectManager.Player.MoveSpeed <= 2)
            {
                AxeSpots.Add(new Axe(sender));
            }
            if (QBuffList.Contains(sender.Name) && sender.Position.Distance(ObjectManager.Player.Position) < 100)
            {
                CurrentAxes += 1;
            }
        }

        public static void OnDelete(GameObject sender, EventArgs args)
        {
            for (var i = 0; i < AxeSpots.Count; i++)
            {
                if (AxeSpots[i].AxeObj.NetworkId == sender.NetworkId)
                {
                    AxeSpots.RemoveAt(i);
                    return;
                }
            }

            if ((QBuffList.Contains(sender.Name)) && sender.Position.Distance(ObjectManager.Player.Position) < 300)
            {
                if (CurrentAxes == 0)
                {
                    CurrentAxes = 0;
                }

                if (CurrentAxes <= 2)
                {
                    CurrentAxes = CurrentAxes - 1;
                }
                else
                {
                    CurrentAxes = CurrentAxes - 1;
                }
            }
        }
    }
}
