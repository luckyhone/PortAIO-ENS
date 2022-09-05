using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = LeagueSharpCommon.Render;

namespace StormAIO.Champions
{
    internal class Vladimir
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
                HarassMenu.QBool,
                HarassMenu.QLastHit,
                HarassMenu.EBool
            };
            
            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.QBool,
                KillStealMenu.WBool,
                KillStealMenu.EBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QBool,
                LaneClearMenu.EBool,
                new Menu("customization", "Customization")
                {
                    LaneClearMenu.ECountSliderBool
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QBool,
                JungleClearMenu.EBool
            };

            var lastHitMenu = new Menu("lastHit", "Last Hit")
            {
                LastHitMenu.QBool,
            };

            var drawingMenu = new Menu("Drawing", "Drawing")
            {

                DrawingMenu.DrawQ,
                DrawingMenu.DrawE,
                DrawingMenu.DrawR
            };
            var Misc = new Menu("MiscMenu","MiscMenu")
            {
                Miscmenu.WBool,
                Miscmenu.WEvade,
            };
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                Misc,
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

        private static class ComboMenu
        {
            public static readonly MenuBool QBool = new MenuBool("comboQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
            public static readonly MenuSliderButton RBool = new MenuSliderButton("comboR", "Use R | If target Health <= x% ", 60);
            public static readonly MenuSliderButton RCount = new MenuSliderButton("RCount", "Use R | If you can hit X ", 2,1,5);
            
        }

        private static class HarassMenu
        {
            public static readonly MenuBool QBool = new MenuBool("harassQ", "Use Q ");
            public static readonly MenuBool QLastHit = new MenuBool("QLastHit", "Use Q to LastHit minions || Only when Target is out of range");
            public static readonly MenuBool EBool = new MenuBool("harassE", "Use E ");
            
        }

        private static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killStealQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("killStealW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
        }

        private static class JungleClearMenu
        {
            public static readonly MenuBool QBool =
                new MenuBool("jungleClearQ", "Use Q");
            
            public static readonly MenuBool EBool =
                new MenuBool("jungleClearE", "Use E ");
        }

        private static class LaneClearMenu
        {
            public static readonly MenuBool QBool =
                new MenuBool("laneClearQ", "Use Q ");
            
            public static readonly MenuBool EBool =
                new MenuBool("laneClearE", "Use E ");

            public static readonly MenuSlider ECountSliderBool =
                new MenuSlider("laneClearECount", "Use E if hittable minions >= x", 3, 1, 5);
        }
        private static class Miscmenu
        {
            public static readonly MenuBool WBool =
                new MenuBool("WTurrent", "Use W To Dodge Tower Shots");
            public static readonly MenuBool WEvade =
                new MenuBool("Evade", "Use W To Dodge targeted Spells ONLY UTL Spells");
            
        }


        private static class LastHitMenu
        {
            public static readonly MenuBool QBool = new MenuBool("lastHitQ", "Use Q");
        }
        
        private static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q");
            public static readonly MenuBool DrawE = new MenuBool("DrawE", "Draw E");
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }
        

        #endregion

        #region Spells 

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q,600);
            Q.SetTargetted(0.25f,1400f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 600)  { Delay = 0f};
            E.SetSkillshot(0f, 600f, float.MaxValue, false,  SpellType.Circle);
            E.SetCharged("VladimirE","VladimirE",50,590,1000f);
            R = new Spell(SpellSlot.R,625);
            R.SetSkillshot(0f,375f,2000f,false,SpellType.Circle);
        }


        #endregion
        #region Gamestart
      
        public Vladimir()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnBeforeAttack += OrbwalkerOnOnAction;
            Drawing.OnEndScene += delegate(EventArgs args)
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Magical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            AIBaseClient.OnDoCast += AIBaseClientOnOnProcessSpellCast;
            Spellbook.OnCastSpell += SpellbookOnOnCastSpell;
        }

        private void SpellbookOnOnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (E.IsCharging && args.Slot != SpellSlot.E)
            {
                args.Process = false;
            }
        }

        #endregion

        #region args

        private static void OrbwalkerOnOnAction(object sender, BeforeAttackEventArgs args)
        {
            if (E.IsCharging)
            {
                args.Process = false;
            }
        }
        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
           if (sender == null  || !W.IsReady() || args.Target == null) return;
           if (sender is AITurretClient && args.Target.IsMe && Miscmenu.WBool.Enabled)
           {
               DelayAction.Add((int) args.Start.DistanceToPlayer() / 4, Whelper);
           }

           if (sender.IsEnemy && args.Slot == SpellSlot.R && args.Target.IsMe &&
               args.SData.TargetingType == SpellDataTargetType.Target)
           {
               if (!W.IsReady() || !Miscmenu.WEvade.Enabled) return;
               Whelper();
           }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ.Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.Violet);
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
                    if (MainMenu.SpellFarm.Active) LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            KillSteal();
        }

         #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (ComboMenu.QBool.Enabled)CastQ();
            if (ComboMenu.EBool.Enabled) CastE();
            if (ComboMenu.RBool.Enabled) CastR();
            CastUlt();
        }

        private static void Harass()
        { 
            
            if (HarassMenu.QBool.Enabled) CastQ();
            if (HarassMenu.QLastHit.Enabled) CastQMinion();
            if (HarassMenu.EBool.Enabled) CastE();

        }

        private static void LaneClear()
        {
            if (Q.IsReady() && LaneClearMenu.QBool.Enabled && !E.IsCharging)
            {
                
                var minions = GameObjects.GetMinions(Player.Position, Q.Range)
                    .OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault(x => Qdmg(x) > x.Health);
                if (minions == null) return;
                Q.Cast(minions);
            }
            if (E.IsReady() && LaneClearMenu.EBool.Enabled)
            {
                var minions = GameObjects.GetMinions(Player.Position, E.Range);
                if (!minions.Any()) return;
                if (minions.Count <= 1) return;
                if (E.IsCharging)
                {
                    E.Cast(Game.CursorPos);
                }
                E.StartCharging();
            
            }
        }

        private static void JungleClear()
        {
            if (Q.IsReady() && JungleClearMenu.QBool.Enabled)
            {
                var mobs = GameObjects.GetJungles(Player.Position, Q.Range).OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (mobs == null) return;
                Q.Cast(mobs);
            }
            if (E.IsReady() && JungleClearMenu.EBool.Enabled)
            {
                var mobs = GameObjects.GetJungles(Player.Position, E.Range).OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (mobs == null) return;
                if (E.IsCharging)
                {
                    E.Cast(Game.CursorPos);
                }
                E.StartCharging();
            }
        }

        private static void LastHit()
        {
            if (!Q.IsReady() || !LastHitMenu.QBool.Enabled) return;
            var minions = GameObjects.GetMinions(Player.Position, Q.Range)
                .OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault(x => Qdmg(x) > x.Health);
            if (minions == null) return;
            Q.Cast(minions);
        }

        private static void KillSteal()
        {
            if (Q.IsReady() && KillStealMenu.QBool.Enabled)
            {
                var target = Q.GetTarget();
                if (target == null || Qdmg(target) < target.TrueHealth()) return;
                Q.Cast(target);
            }
            if (E.IsReady() && KillStealMenu.EBool.Enabled)
            {
                var target = E.GetTarget();
                if (target == null || E.GetDamage(target) < target.TrueHealth()) return;
                E.Cast();
            }

        }
        
        #endregion

        #region Spell Stage

      
        #endregion
        #region Spell Functions

        private static void CastQ()
        {
          if (Q.GetTarget() == null || !Q.IsReady()) return;
          Q.CastOnBestTarget();
        }  
        private static void CastQMinion()
        {
          if (Q.GetTarget() != null || !Q.IsReady()) return;
          var minions = GameObjects.GetMinions(Player.Position, Q.Range)
              .OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault(x => Qdmg(x) > x.Health);
          if (minions == null) return;
          Q.Cast(minions);
        }
        
        private static void CastE()
        {
           if (E.GetTarget() == null || !E.IsReady()) return;
           if (E.IsCharging)
           {
               E.Cast(Game.CursorPos);
           }
           E.StartCharging();
        }

        private static void CastR()
        {
          if (R.GetTarget() == null || !R.IsReady() || !ComboMenu.RBool.Enabled) return;
          if (R.GetTarget().HealthPercent >= ComboMenu.RBool.ActiveValue) return;
          var Rpre = R.GetPrediction(R.GetTarget());
          if (Rpre.Hitchance >= HitChance.High) R.Cast(Rpre.CastPosition);
        }

        private static void CastUlt()
        {
            if (R.GetTarget() == null || !ComboMenu.RCount.Enabled || !ComboMenu.RBool.Enabled ) return;
            if (R.GetTarget().HealthPercent >= ComboMenu.RBool.ActiveValue) return;
            if (R.GetPrediction(R.GetTarget()).AoeTargetsHitCount < ComboMenu.RCount.ActiveValue)
                R.Cast(R.GetTarget());
        }
        
        #endregion

        #region damage 
        // Use it if some some damages aren't available by the sdk 
        private static float Qdmg(AIBaseClient t)
        {
            double damage = 0;
            if (!Frenzy)
            {
                damage += 80 + 20 * (Q.Level - 1) + Player.TotalMagicalDamage * 0.6;
            }
            else
            {
                damage += 148 + 37 * (Q.Level - 1) + Player.TotalMagicalDamage * 1.11;
            }
           

            return (float) Player.CalculateMagicDamage(t,damage);
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
            if (W.IsReady()) Damage += W.GetDamage(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (R.IsReady()) Damage += R.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        private static bool Frenzy
        {
            get { return Player.HasBuff("vladimirqfrenzy"); }
        }

        private static void Whelper()
        {
            W.Cast();
        }
        #endregion
    }
}