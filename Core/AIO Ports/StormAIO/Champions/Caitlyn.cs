using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Caitlyn
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            ChampMenu = new Menu(Player.CharacterName, Player.CharacterName);
            
            var comboMenu = new Menu("combo", "Combo")
            {
                ComboMenu.QBool,
                ComboMenu.WBool,
                ComboMenu.EBool,
                ComboMenu.RBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool,
                HarassMenu.WSliderBool,
                HarassMenu.ESliderBool
            };
            
            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.QBool,
                KillStealMenu.WBool,
                KillStealMenu.EBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.WSliderBool,
                LaneClearMenu.ESliderBool,
                new Menu("customization", "Customization")
                {
                    LaneClearMenu.QCountSliderBool,
                    LaneClearMenu.WCountSliderBool,
                    LaneClearMenu.ECountSliderBool
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.WSliderBool,
                JungleClearMenu.ESliderBool
            };

            var lastHitMenu = new Menu("lastHit", "Last Hit")
            {
                LastHitMenu.QSliderBool,
                LastHitMenu.WSliderBool,
                LastHitMenu.ESliderBool
            };

            var drawingMenu = new Menu("Drawing", "Drawing")
            {

                DrawingMenu.DrawQ,
                DrawingMenu.DrawW,
                DrawingMenu.DrawE,
                DrawingMenu.DrawR
            };
            var StructureMenu = new Menu("StructureClear","Structure Clear")
            {
                StructureClearMenu.QSliderBool,
                StructureClearMenu.WSliderBool,
                StructureClearMenu.ESliderBool
            };
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                StructureMenu,
                lastHitMenu,
                drawingMenu
            };

            foreach (var menu in menuList)
            {
                ChampMenu.Add(menu);
            }
            MainMenu.Main_Menu.Add(ChampMenu);
        }
        #endregion

        #region MenuHelper

        public static class ComboMenu
        {
            public static readonly MenuBool QBool = new MenuBool("comboQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
            public static readonly MenuBool RBool = new MenuBool("comboR", "Use R");
            public static readonly MenuKeyBind KeyBind = new MenuKeyBind("11", "Use E",Keys.Z,KeyBindType.Press);
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("harassQ", "Use Q | If Mana >= x%", 50);
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("harassW", "Use W | If Mana >= x%", 50);
            public static readonly MenuSliderButton ESliderBool = new MenuSliderButton("harassE", "Use E | If Mana >= x%", 50);
            
        }

        public static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killStealQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("killStealW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
        }

        public static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("jungleClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("jungleClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("jungleClearE", "Use E | If Mana >= x%", 50);
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSlider QCountSliderBool =
                new MenuSlider("laneClearQCount", "Use Q if hittable minions >= x", 3, 1, 5);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("laneClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSlider WCountSliderBool =
                new MenuSlider("laneClearWCount", "Use W if hittable minions >= x", 3, 1, 5);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("laneClearE", "Use E | If Mana >= x%", 50);

            public static readonly MenuSlider ECountSliderBool =
                new MenuSlider("laneClearECount", "Use E if hittable minions >= x", 3, 1, 5);
        }
        public static class StructureClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("structClearQ", "Use Q | If Mana >= x%", 50);
            
            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("structClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("structClearE", "Use E | If Mana >= x%", 50);
            
        }


        public static class LastHitMenu
        {
            public static readonly MenuBool QSliderBool = new MenuBool("lastHitQ", "Use Q");
            public static readonly MenuBool WSliderBool = new MenuBool("lastHitW", "Use W");
            public static readonly MenuBool ESliderBool = new MenuBool("lastHitE", "Use E");
        }
        
        public static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q");
            public static readonly MenuBool DrawW = new MenuBool("DrawW", "Draw W");
            public static readonly MenuBool DrawE = new MenuBool("DrawE", "Draw E");
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }
        

        #endregion

        #region Spells 

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, 1250f);
            Q.SetSkillshot(0.70f, 120f, 2200f, false, SpellType.Line);

            W = new Spell(SpellSlot.W, 800f);
            W.SetSkillshot(0.25f, 80f, 2000f, false, SpellType.Circle);

            E = new Spell(SpellSlot.E, 750f);
            E.SetSkillshot(0.15f, 60f, 1600f, true, SpellType.Line);

            R = new Spell(SpellSlot.R, 3500) {Delay = 0.4f};
        }


        #endregion
        #region Gamestart
      
        public Caitlyn()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += delegate(EventArgs args)
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            Dash.OnDash += delegate(AIBaseClient sender, Dash.DashArgs args)
            {
                if (sender.IsMe) Game.Print(args.EndPos.ToString());
            };
            
        }

        #endregion

        #region args
        
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ.Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.Violet);
            if (DrawingMenu.DrawW.Enabled && W.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, W.Range, Color.DarkCyan);
            if (DrawingMenu.DrawE.Enabled && E.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, E.Range, Color.DarkCyan);
            if (DrawingMenu.DrawR.Enabled && R.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, R.Range, Color.Violet);

        }

        #endregion

        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Helper.Checker()) return; 
            

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                  if(MainMenu.SpellFarm.Active) LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            if (ComboMenu.KeyBind.Active)
            {
                Flee();
            }
            KillSteal();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (!E.IsReady()) return;
            E.Cast(ObjectManager.Player.Position - (Game.CursorPos - ObjectManager.Player.Position));
          
        }

        private static void Harass()
        {
           

        }

        private static void LaneClear()
        {
            
        }

        private static void JungleClear()
        {
           
        }

        private static void LastHit()
        {
          
        }

        private static void KillSteal()
        {
           

        }
        #endregion

        #region Spell Stage

      
        #endregion
        #region Spell Functions

        private static void CastQ()
        {
          
        }
       
        private static void CastW()
        {
          
        }

        private static void CastE()
        {
           
        }

        private static void CastR()
        {
          
        }
        
        
        #endregion

        #region damage 
        // Use it if some some damages aren't available by the sdk 
        private static float Qdmg(AIBaseClient t)
        {
            var damage = 0;
            return damage;
        }
        private static float Wdmg(AIBaseClient t)
        {
            var damage = 0;
            return damage;
        }
        private static float Edmg(AIBaseClient t)
        {
            var damage = 0;
            return damage;
        }
        private static float Rdmg(AIHeroClient t)
        {
            var damage = 0;
            return damage;
        }
        
        #endregion

        #region Extra functions

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
                             Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Qdmg(target);
            if (W.IsReady()) Damage += Wdmg(target);
            if (E.IsReady()) Damage += Edmg(target);
            if (R.IsReady()) Damage += Rdmg(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
     

        private static void Flee() {
          
    
            Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position.Extend(Game.CursorPos, 150f));
        
            if (!E.IsReady()) { 
                return;
            }

            // dash endPos
            var maxDashPos = Player.Position.Extend(Game.CursorPos, 400);
            if (!maxDashPos.IsValid() || maxDashPos.IsWall()) {
                return;
            }
    
            // check about the wall width
            var wallwidth = Helper.GetWallWidth(Player.Position, maxDashPos, 400);
    
            // if wall width == 0 or wall width > E Range, cant be dash
            if (wallwidth <= 0f || wallwidth >= 400) { 
                return;
            }
            
            // can walljump
            E.Cast(Player.Position - (Game.CursorPos - Player.Position));
        }
        #endregion
    }
}