using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;

namespace GangplankBuddy
{
    static class BarrelManager
    {
        public static void Init()
        {
            Drawing.OnDraw += Drawing_OnDraw;
            GameEvent.OnGameTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Barrels = ObjectManager.Get<AIBaseClient>().Where(a =>
                a.Name.ToLower().Contains("barrel") && a.Health > 1 && a.HasBuffFromMe("active")).ToList();
            Killablebarrels = ObjectManager.Get<AIBaseClient>().Where(a =>
                a.Name.ToLower().Contains("barrel") && a.Health == 1 &&
                a.HasBuffFromMe("active")).ToList();
        }

        public static List<AIBaseClient> Barrels = new List<AIBaseClient>();

        public static List<AIBaseClient> Killablebarrels = new List<AIBaseClient>();

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Program.DrawingMenu["drawUnKillable"].GetValue<MenuBool>().Enabled)
            {
                foreach (var gameObject in Barrels)
                {
                    CircleRender.Draw(gameObject.Position,350,Color.Wheat);
                }
            }
            if (Program.DrawingMenu["drawKillable"].GetValue<MenuBool>().Enabled)
            {
                foreach (var gameObject in Killablebarrels)
                {
                    CircleRender.Draw(gameObject.Position,350,Color.Red);
                }
            }
        }
        
        public static bool CanTriggerBarrel(Vector3 pos)
        {
            return Killablebarrels.Any(a => a.Distance(pos) < 350);
        }

        public static AIBaseClient KillableBarrelAroundUnit(AIBaseClient unit)
        {
            return Killablebarrels.FirstOrDefault(a => a.Distance(unit) < 350);
        }

        private static bool HasBuffFromMe(this AIBaseClient unit, string buff)
        {
            return unit.Buffs.Any(a => a.Name.ToLower().Contains(buff) && a.Caster.NetworkId == ObjectManager.Player.NetworkId);
        }
    }
}