using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using ShadowTracker;
using SharpDX;
using Color = System.Drawing.Color;

namespace PerfectWardReborn
{
    internal class PerfectWardTracker
    {
        public static Helper Helper;

        static readonly List<KeyValuePair<int, String>> _wards = new List<KeyValuePair<int, String>> //insertion order
        {
            new KeyValuePair<int, String>(3340, "Warding Totem Trinket"),
            new KeyValuePair<int, String>(2301, "Eye of the Watchers"),
            new KeyValuePair<int, String>(2302, "Eye of the Oasis"),
            new KeyValuePair<int, String>(2303, "Eye of the Equinox"),
            //new KeyValuePair<int, String>(3205, "Quill Coat"),
            new KeyValuePair<int, String>(3207, "Spirit Of The Ancient Golem"),
            new KeyValuePair<int, String>(3154, "Wriggle's Lantern"),
            new KeyValuePair<int, String>(2049, "Sight Stone"),
            new KeyValuePair<int, String>(2045, "Ruby Sightstone"),
            //new KeyValuePair<int, String>(3160, "Feral Flare"),
            new KeyValuePair<int, String>(2050, "Explorer's Ward"),
            new KeyValuePair<int, String>(2044, "Stealth Ward"),
            new KeyValuePair<int, String>(2055, "Control Ward"),

        };
        int _lastTimeWarded;

        private const int VK_LBUTTON = 1;
        private const int WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_CHAR = 0x0102, WM_SYSKEYDOWN = 0x0104, WM_SYSKEYUP = 0x0105, WM_MOUSEDOWN = 0x201;
        public static Menu Config;
        public static float lastuseward = 0;
        public class Wardspoting
        {
            public static WardSpot _PutSafeWard;
        }
        
        public PerfectWardTracker()
        {
            GameEvent.OnGameLoad += OnGameStart;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;


            //Create the menu
            Config = new Menu("PerfectWardReborn", "PerfectWardReborn", true);

            var wardkey = Config.Add(new Menu("WardKey", "WardKey"));
            wardkey.Add(new MenuKeyBind("placekey", "NormalWard Key",Keys.Z, KeyBindType.Press)).SetFontColor(Color.Green.ToSharpDxColor());
            wardkey.Add(new MenuKeyBind("placekeyconWard", "ControlWard Key",Keys.U, KeyBindType.Press)).SetFontColor(Color.DarkOrange.ToSharpDxColor());
            var drawings = Config.Add(new Menu("drawings", "Drawings"));
            drawings.Add(new MenuBool("drawplaces", "Draw ward places")); //System.Drawing.Color.FromArgb(100, 255, 0, 255)
            drawings.Add(new MenuSlider("drawDistance", "Don't draw if the distance >",2000, 1, 10000));
            var AutoBushRevealer = Config.Add(new Menu("AutoBushRevealer", "AutoBushRevealer"));
            AutoBushRevealer.Add(new MenuKeyBind("AutoBushKey", "Key",Keys.C, KeyBindType.Press));
            AutoBushRevealer.Add(new MenuBool("AutoBushEnabled", "Enabled").SetValue(true));
            Config.Attach();
            var autoBushWarfType = Config.Add(new Menu("Auto Bush Ward Type", "Auto Bush Ward Type"));
            foreach (var ward in _wards)
                autoBushWarfType.Add(new MenuBool("AutoBush" + ward.Key, ward.Value).SetValue(true));
            Game.OnUpdate += Game_OnGameUpdate;

        }

        private void OnGameStart()
        {
            Helper = new Helper();

            /*Game.Print(
                string.Format(
                    "{0} v{1} loaded.",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version
                )
            );*/
        }

        private void OnDraw(EventArgs args)
        {
            if (Config["drawings"]["drawplaces"].GetValue<MenuBool>().Enabled)
            {
                Ward.DrawWardSpots();
                Ward.DrawSafeWardSpots();
            }
        }

        InventorySlot GetWardSlot()
        {
            return _wards.Select(x => x.Key).Where(id => Config["Auto Bush Ward Type"]["AutoBush" + id].GetValue<MenuBool>().Enabled && Items.CanUseItem(ObjectManager.Player,id)).Select(wardId => ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId)).FirstOrDefault();
        }

        static public InventorySlot GetAnyWardSlot()
        {
            return _wards.Select(x => x.Key).Select(wardId => ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId)).FirstOrDefault();
        }
        
        AIBaseClient GetNearObject(String name, Vector3 pos, int maxDistance)
        {
            return ObjectManager.Get<AIBaseClient>().FirstOrDefault(x => x.Name == name && x.Distance(pos) <= maxDistance);
        }
        
        void Game_OnGameUpdate(EventArgs args)
        {
            int time = Environment.TickCount;
            if (Config["AutoBushRevealer"]["AutoBushEnabled"].GetValue<MenuBool>().Enabled && Config["AutoBushRevealer"]["AutoBushKey"].GetValue<MenuKeyBind>().Active)
                foreach (AIHeroClient enemy in Helper.EnemyInfo.Where(x =>
                            x.Player.IsValid &&
                            !x.Player.IsVisible &&
                            !x.Player.IsDead &&
                            x.Player.Distance(ObjectManager.Player.Position) < 1000 &&
                            time - x.LastSeen < 2500).Select(x => x.Player))
                {
                    var bestWardPos = GetWardPos(enemy.Position);
                    //           var bestWardPos = GetWardPos(enemy.ServerPosition, 165, 2);


                    if (bestWardPos != enemy.Position && bestWardPos != Vector3.Zero && bestWardPos.Distance(ObjectManager.Player.Position) <= 600)
                    {
                        int timedif = Environment.TickCount - _lastTimeWarded;

                        if (timedif > 1250 && !(timedif < 2500 && GetNearObject("SightWard", bestWardPos, 200) != null)) //no near wards
                        {
                            var wardSlot = GetWardSlot();

                            if (wardSlot != null && wardSlot.Id != ItemId.Unknown)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, bestWardPos);
                                _lastTimeWarded = Environment.TickCount;
                            }
                        }
                    }
                }

            InventorySlot wardSpellSlot = null;
            if (Config["WardKey"]["placekey"].GetValue<MenuKeyBind>().Active)
            {
                wardSpellSlot = Ward.GetWardSlot();
            }
            else if (Config["WardKey"]["placekeyconWard"].GetValue<MenuKeyBind>().Active)
            {
                wardSpellSlot = Ward.GetConWardSlot();
            }
            {
                if (wardSpellSlot == null || lastuseward + 1000 > Environment.TickCount)
                {
                    return;
                }
                Vector3? nearestWard = Ward.FindNearestWardSpot(Drawing.ScreenToWorld(Game.CursorPos.X, Game.CursorPos.Y));

                if (nearestWard != null)
                {
                    if (wardSpellSlot != null)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(wardSpellSlot.SpellSlot, (Vector3)nearestWard);
                        lastuseward = Environment.TickCount;
                    }
                }

                WardSpot nearestSafeWard = Ward.FindNearestSafeWardSpot(Drawing.ScreenToWorld(Game.CursorPos.X, Game.CursorPos.Y));

                if (nearestSafeWard != null)
                {
                    if (wardSpellSlot != null)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, nearestSafeWard.MovePosition);
                        Wardspoting._PutSafeWard = nearestSafeWard;
                    }
                }
            }


            if (Wardspoting._PutSafeWard != null && lastuseward + 1000 < Environment.TickCount)
            {
                wardSpellSlot = Items.GetWardSlot(ObjectManager.Player);
                if (Math.Sqrt(Math.Pow(Wardspoting._PutSafeWard.ClickPosition.X - ObjectManager.Player.Position.X, 2) + Math.Pow(Wardspoting._PutSafeWard.ClickPosition.Y - ObjectManager.Player.Position.Y, 2)) <= 640.0)
                {
                    if (Config["WardKey"]["placekey"].GetValue<MenuKeyBind>().Active)
                    {
                        wardSpellSlot = Ward.GetWardSlot();
                    }
                    else if (Config["WardKey"]["placekeyconWard"].GetValue<MenuKeyBind>().Active)
                    {
                        wardSpellSlot = Ward.GetConWardSlot();
                    }
                    if (wardSpellSlot != null)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(wardSpellSlot.SpellSlot, Wardspoting._PutSafeWard.ClickPosition);
                        lastuseward = Environment.TickCount;

                    }
                    Wardspoting._PutSafeWard = null;
                }
            }
        }
        
        Vector3 GetWardPos(Vector3 lastPos, int radius = 165, int precision = 3) //maybe reverse autobushward code from the bots?
        {
            //old: Vector3 wardPos = enemy.Position + Vector3.Normalize(enemy.Position - ObjectManager.Player.Position) * 150;

            var count = precision;

            while (count > 0)
            {
                var vertices = radius;

                var wardLocations = new WardLocation[vertices];
                var angle = 2 * Math.PI / vertices;

                for (var i = 0; i < vertices; i++)
                {
                    var th = angle * i;
                    var pos = new Vector3((float)(lastPos.X + radius * Math.Cos(th)), (float)(lastPos.Y + radius * Math.Sin(th)), 0);
                    wardLocations[i] = new WardLocation(pos, NavMesh.IsWallOfType(pos,CollisionFlags.Grass,5));
                }

                var grassLocations = new List<GrassLocation>();

                for (var i = 0; i < wardLocations.Length; i++)
                {
                    if (!wardLocations[i].Grass) continue;
                    if (i != 0 && wardLocations[i - 1].Grass)
                        grassLocations.Last().Count++;
                    else
                        grassLocations.Add(new GrassLocation(i, 1));
                }

                var grassLocation = grassLocations.OrderByDescending(x => x.Count).FirstOrDefault();

                if (grassLocation != null) //else: no pos found. increase/decrease radius?
                {
                    var midelement = (int)Math.Ceiling(grassLocation.Count / 2f);
                    lastPos = wardLocations[grassLocation.Index + midelement - 1].Pos;
                    radius = (int)Math.Floor(radius / 2f);
                }

                count--;
            }

            return lastPos;
        }

        class WardLocation
        {
            public readonly Vector3 Pos;
            public readonly bool Grass;

            public WardLocation(Vector3 pos, bool grass)
            {
                Pos = pos;
                Grass = grass;
            }
        }

        class GrassLocation
        {
            public readonly int Index;
            public int Count;

            public GrassLocation(int index, int count)
            {
                Index = index;
                Count = count;
            }
        }
    }
}