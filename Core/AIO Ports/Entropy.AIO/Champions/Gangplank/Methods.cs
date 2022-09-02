using System;
using System.Collections.Generic;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Gangplank.Logics;
using Entropy.AIO.Gangplank.Misc;
using PortAIO.Library_Ports.Entropy.Lib;
using SharpDX;

namespace Entropy.AIO.Gangplank
{
    class Methods
    {

        public Methods()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Definitions.LastQCast = Environment.TickCount;
            Definitions.LastECast = Environment.TickCount;

            BarrelManager.CastedBarrels = new List<Vector3>();
            BarrelManager.Barrels       = new List<Barrel>();

            GameEvent.OnGameTick                       += Gangplank.OnTick;
            GameEvent.OnGameTick                 += Gangplank.OnCustomTick;
            GameObject.OnCreate               += Gangplank.OnCreate;
            AIBaseClient.OnDoCast += Gangplank.OnProcessSpellCast;
            AIBaseClient.OnDoCast += Gangplank.OnProcessBasicAttack;
            Drawing.OnDraw                 += Renderer_OnRender;

            //Custom Orb Options
            //Orbwalker.AddMode(new OrbwalkerMode("Combo To Mouse", WindowMessageWParam.A, null, ComboToMouse.Execute));
            //Orbwalker.AddMode(new OrbwalkerMode("Explode Nearest Barrel", WindowMessageWParam.T, null, ExplodeNearest.Execute));
            Orbwalker.OnBeforeAttack += delegate(object sender, BeforeAttackEventArgs args) {  };
            {
                if (Components.ComboMenu.ComboToMouse.Active)
                {
                    ComboToMouse.Execute();
                    Orbwalker.AttackEnabled = true;
                    Orbwalker.MoveEnabled   = true;
                }
                else if (Components.ComboMenu.ExplodeNearestBarrel.Active)
                {
                    ExplodeNearest.Execute();
                    Orbwalker.AttackEnabled = true;
                    Orbwalker.MoveEnabled   = true;
                }
            };
        }

        private static void Renderer_OnRender(EventArgs args)
        {
            //foreach (var barrel in BarrelManager.Barrels)
            //{
            //    Renderer.DrawCircularRangeIndicator(barrel.Position, Definitions.ChainRadius, Color.Pink);
            //    Renderer.DrawCircularRangeIndicator(barrel.Position, Definitions.ExplosionRadius, Color.Red);
            //}

            foreach (var barrel in BarrelManager.CastedBarrels)
            {
                Drawing.DrawCircleIndicator(barrel, Definitions.ChainRadius, Color.Pink.ToSystemColor());
                Drawing.DrawCircleIndicator(barrel,
                                                    Definitions.ExplosionRadius - Components.ComboMenu.ExtraTripleRange.Value,
                                                    Color.Yellow.ToSystemColor());
                Drawing.DrawCircleIndicator(barrel, Definitions.ExplosionRadius, Color.Red.ToSystemColor());
            }

            //foreach (var castedBarrel in BarrelManager.CastedBarrels)
            //{
            //    GameConsole.Print($"Casted: {castedBarrel.X}, {castedBarrel.Y}, {castedBarrel.Z}");
            //}
            //foreach (var barrel in BarrelManager.Barrels)
            //{
            //    GameConsole.Print($"Object: {barrel.Position.X}, {barrel.Position.Y}, {barrel.Position.Z} ({barrel.Object.Distance(BarrelManager.CastedBarrels[0])})");
            //}
        }
    }
}