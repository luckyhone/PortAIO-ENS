using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using Pentakill_Cassiopeia.Controller;
using Pentakill_Cassiopeia.Util;
using SharpDX;
using Render = LeagueSharpCommon.Render;

namespace Pentakill_Cassiopeia
{
    class Program
    {
        public static MenuController menuController;
        public static AIHeroClient player;
        public static Spell q;
        public static Spell w;
        public static Spell e;
        public static Spell r;
        public static SpellSlot ignite;

        public static void OnGameLoad(EventArgs args)
        {
            //Assigning objects used in later parts
            menuController = new MenuController();
            player = ObjectManager.Player;

            //Check if our Champion is Cassiopeia
            if (player.CharacterName != "Cassiopeia")
            {
                return;
            }

            //Initiating spells
            q = new Spell(SpellSlot.Q, 850f);
            q.SetSkillshot(0.4f, 40f, float.MaxValue, false, SpellType.Circle);
            w = new Spell(SpellSlot.W, 850f);
            w.SetSkillshot(0.5f, 90f, 2500, false, SpellType.Circle);
            e = new Spell(SpellSlot.E, 700f);
            e.SetTargetted(0.2f, float.MaxValue);
            r = new Spell(SpellSlot.R, 825f);
            r.SetSkillshot(0.6f, (float)(80 * Math.PI / 180), float.MaxValue, false, SpellType.Cone);
            ignite = player.GetSpellSlot("summonerdot");

            //Add menu to main menu
            menuController.addToMainMenu();

            //Subscribe to OnGameUpdate and OnDraw method
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Game.Print("<font color ='#33FFFF'>Pentakill Cassiopeia</font> by <font color = '#FFFF00'>GoldenGates</font> loaded, enjoy!");
        }

        static void OnGameUpdate(EventArgs args)
        {
            //If we're dead do nothing
            if (player.IsDead)
                return;
            //Performs checks before orbwalking
            GameLogic.Checks();
            //Orbwalking handling with modes
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    GameLogic.performCombo();
                    break;
                case OrbwalkerMode.Harass:
                    if (menuController.getMenu()["harassManager"].GetValue<MenuSlider>().Value < player.ManaPercent)
                    {
                        GameLogic.performHarass();
                    }
                    break;
                case OrbwalkerMode.LastHit:
                    if (menuController.getMenu()["lastHitManager"].GetValue<MenuSlider>().Value < player.ManaPercent)
                    {
                        GameLogic.performLastHit();
                    }
                    break;
                case OrbwalkerMode.LaneClear:
                    if (menuController.getMenu()["laneClearManager"].GetValue<MenuSlider>().Value < player.ManaPercent)
                    {
                        GameLogic.performLaneClear();
                    }
                    break;
            }
        }

        #region Drawing
        static readonly Render.Text text = new Render.Text(
                                               0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");

        private static void OnDraw(EventArgs args)
        {
            if (Program.menuController.getMenu()["drawQW"].GetValue<MenuBool>().Enabled)
                Render.Circle.DrawCircle(Program.player.Position, Program.q.Range, System.Drawing.Color.Yellow);
            if (Program.menuController.getMenu()["drawE"].GetValue<MenuBool>().Enabled)
                Render.Circle.DrawCircle(Program.player.Position, Program.e.Range, System.Drawing.Color.Green);
            if (Program.menuController.getMenu()["drawR"].GetValue<MenuBool>().Enabled)
                Render.Circle.DrawCircle(Program.player.Position, Program.r.Range, System.Drawing.Color.IndianRed);
            if (Program.menuController.getMenu()["drawDmg"].GetValue<MenuBool>().Enabled)
                DrawHPBarDamage();

        }

        static void DrawHPBarDamage()
        {
            const int XOffset = 10;
            const int YOffset = 20;
            const int Width = 103;
            const int Height = 8;
            foreach (var unit in ObjectManager.Get<AIHeroClient>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy))
            {
                var barPos = unit.HPBarPosition;
                float damage = SpellDamage.getComboDamage(unit);
                float percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                float yPos = barPos.Y + YOffset;
                float xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                float xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health)
                {
                    text.X = (int)barPos.X + XOffset;
                    text.Y = (int)barPos.Y + YOffset - 13;
                    text.text = ((int)(unit.Health - damage)).ToString();
                    text.OnEndScene();
                }
                Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 2, System.Drawing.Color.Yellow);
            }
        }
        #endregion Drawing

        public static void Loads()
        {
            //Subscribes to OnGameLoad method
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }
    }
}