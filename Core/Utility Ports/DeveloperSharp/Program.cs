using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Color = System.Drawing.Color;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace DeveloperSharp
{
    static class Program
    {
        private static Menu Config;
        private static Menu Types;
        private static Menu Types1;
        private static Menu Types2;
        private static Menu Types3;
        private static Menu Types4;
        private static int _lastUpdateTick = 0;
        private static int _lastMovementTick = 0;

        public static void Loads()
        {
            InitMenu();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AIBaseClient.OnDoCast += OnProcessSpellCast;
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                //Chat.Print("Detected Spell Name: " + args.SData.Name + " Missile Name: " + args.SData.MissileBoneName + " Issued By: " + sender.BaseSkinName);
            }
        }

        private static void InitMenu()
        {
            Config = new Menu("developersharp", "Developer#", true);
            Config.Add(new MenuSlider("range", "Max object dist from cursor", 400, 100, 2000));
            Config.Add(new MenuBool("antiafk", "Anti-AFK").SetValue(false));
            Types = Config.Add(new Menu("typesinfo", "Show Info for: "));
            Types1 = Types.Add(new Menu("--- (1)", "firstList"));
            Types2 = Types.Add(new Menu("--- (2)", "secondList"));
            Types3 = Types.Add(new Menu("--- (3)", "thirdList"));
            Types4 = Types.Add(new Menu("--- (4)", "fourthList"));

            var gameObjectTypes = Enum.GetNames(typeof(GameObjectType));
            List<IEnumerable<string>> typesListsDivided = new List<IEnumerable<string>>();
            for (var i = 0; i < (double)gameObjectTypes.Length / 11; i++)
            {
                typesListsDivided.Add(gameObjectTypes.Skip(i * 11).Take(11));
            }

            var typeListNumber = 0;
            foreach (var types in typesListsDivided)
            {
                typeListNumber++;
                foreach (var type in types)
                {

                    if (typeListNumber == 1)
                    {
                        //var aa = new MenuBool(type.ToString(), type.ToString());
                        Types1.Add(new MenuBool(type.ToString(), type.ToString(), false));
                    }

                    if (typeListNumber == 2)
                    {
                        Types2.Add(new MenuBool(type.ToString(), type.ToString(), false));
                    }

                    if (typeListNumber == 3)
                    {
                        Types3.Add(new MenuBool(type.ToString(), type.ToString(), false));
                    }

                    if (typeListNumber == 4)
                    {
                        Types4.Add(new MenuBool(type.ToString(), type.ToString(), false));
                    }
                }
            }
            Config.Add(
                new MenuKeyBind("masteries", "Show Masteries", Keys.L, KeyBindType.Press));
            Config.Add(new MenuColor("color", "Text Color: ", Color.DarkTurquoise.ToSharpDxColor()));
            Config.Attach();
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Config["antiafk"].GetValue<MenuBool>().Enabled && Environment.TickCount - _lastMovementTick > 140000)
            {
                //ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                //    ObjectManager.Player.Position.Ra(-1000, 1000));
                //_lastMovementTick = Environment.TickCount;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config["masteries"].GetValue<MenuKeyBind>().Active)
            {
                /*var masteries = ObjectManager.Player.Ma;
                for (var i = 0; i < masteries.Length; i++)
                {
                    var mastery = masteries[i];
                    var text = "You have " + mastery.Points + " points in mastery #" + mastery.Id + " from page " + mastery.Page;
                    Drawing.DrawText(Drawing.Width / 2 - text.Length, Drawing.Height / 2 - masteries.Length + i * 10, Config.Item("color").GetValue<Color>(), text);
                }
                return;*/
            }

            foreach (var obj in ObjectManager.Get<GameObject>().Where(o =>
                         o.Position.Distance(Game.CursorPos) < Config["range"].GetValue<MenuSlider>().Value &&
                         !(o is AITurretClient) && o.Name != "missile" && !o.Name.Contains("MoveTo") &&
                         Types[Enum.GetName(typeof(GameObjectType), o.Type)].GetValue<MenuBool>().Enabled))
            {
                if (!obj.IsValid) return;
                var X = Drawing.WorldToScreen(obj.Position).X;
                var Y = Drawing.WorldToScreen(obj.Position).Y;
                Drawing.DrawText(X, Y, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                    (obj is AIHeroClient) ? ((AIHeroClient)obj).Name :
                    (obj is AIMinionClient) ? (obj as AIMinionClient).Name :
                    (obj is AITurretClient) ? (obj as AITurretClient).Name : obj.Name);
                Drawing.DrawText(X, Y + 12, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                    obj.Type.ToString());
                Drawing.DrawText(X, Y + 22, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                    "NetworkID: " + obj.NetworkId);
                Drawing.DrawText(X, Y + 32, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                    obj.Position.ToString());
                if (obj is AIBaseClient)
                {
                    var aiobj = obj as AIBaseClient;
                    Drawing.DrawText(X, Y + 42, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "Health: " + aiobj.Health + "/" + aiobj.MaxHealth + "(" + aiobj.HealthPercent + "%)");
                }

                if (obj is AIHeroClient)
                {
                    var hero = obj as AIHeroClient;
                    Drawing.DrawText(X, Y + 62, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(), "Spells:");
                    Drawing.DrawText(X, Y + 72, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(), "-------");
                    Drawing.DrawText(X, Y + 82, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "(Q): " + hero.Spellbook.Spells[0].Name);
                    Drawing.DrawText(X, Y + 92, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "(W): " + hero.Spellbook.Spells[1].Name);
                    Drawing.DrawText(X, Y + 102, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "(E): " + hero.Spellbook.Spells[2].Name);
                    Drawing.DrawText(X, Y + 112, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "(R): " + hero.Spellbook.Spells[3].Name);
                    Drawing.DrawText(X, Y + 122, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "(D): " + hero.Spellbook.Spells[4].Name);
                    Drawing.DrawText(X, Y + 132, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "(F): " + hero.Spellbook.Spells[5].Name);
                    var buffs = hero.Buffs;
                    if (buffs.Any())
                    {
                        Drawing.DrawText(X, Y + 152, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                            "Buffs:");
                        Drawing.DrawText(X, Y + 162, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                            "------");
                    }

                    for (var i = 0; i < buffs.Count() * 10; i += 10)
                    {
                        Drawing.DrawText(X, (Y + 172 + i), Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                            buffs[i / 10].Count + "x " + buffs[i / 10].Name);
                    }

                }

                if (obj is MissileClient && obj.Name != "missile")
                {
                    var missile = obj as MissileClient;
                    Drawing.DrawText(X, Y + 40, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "Missile Speed: " + missile.SData.MissileSpeed);
                    Drawing.DrawText(X, Y + 50, Config["color"].GetValue<MenuColor>().Color.ToSystemColor(),
                        "Cast Range: " + missile.SData.CastRange);
                }
            }
        }
    }
}