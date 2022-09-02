using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Twitch
    {
        #region Basics

        private static Spell Q, W, E;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;
        private static int Mykills = 0 + Player.ChampionsKilled;
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
            };
            
            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.EBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.WSliderBool,
                LaneClearMenu.ESliderBool,
                new Menu("customization", "Customization")
                {
                    LaneClearMenu.WCountSliderBool
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.WSliderBool,
                JungleClearMenu.ESliderBool
            };

            var drawingMenu = new Menu("Drawing", "Drawing")
            {

                DrawingMenu.DrawQ,
                DrawingMenu.DrawW,
                DrawingMenu.DrawE,
                DrawingMenu.DrawR
            };
            var misc = new Menu("Misc","Misc")
            {
                MiscMenu.StealthBack
            };
            var menuList = new[]
            {
                comboMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                misc,
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
            public static readonly MenuBool QBool = new MenuBool("comboQ", "Use  after a kill");
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
        }

        private static class KillStealMenu
        {
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
        }

        private static class JungleClearMenu
        {
            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("jungleClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("jungleClearE", "Use E | If Mana >= x%", 50);
        }

        private static class LaneClearMenu
        {
            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("laneClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSlider WCountSliderBool =
                new MenuSlider("laneClearWCount", "Use W if hittable minions >= x", 3, 1, 5);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("laneClearE", "Use E | If Mana >= x%", 50);
        }
        private static class MiscMenu
        {
            public static readonly MenuKeyBind StealthBack = new MenuKeyBind("MiscBack", "Stealth Recall", Keys.B, KeyBindType.Press);
        }
        private static class DrawingMenu
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
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 950f);
            E = new Spell(SpellSlot.E, 1200f);
            
            W.SetSkillshot(0.25f, 75f, 1400f, false, SpellType.Circle);
        }


        #endregion
        #region Gamestart
      
        public Twitch()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
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
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            KillSteal();
            Stealthrecall();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (ComboMenu.QBool.Enabled)
            {
                CastQ();
            }
            if (ComboMenu.WBool.Enabled)
            {
                CastW();
            }
            if (ComboMenu.EBool.Enabled)
            {
                CastE();
            }
        }

        private static void Harass()
        {
           

        }

        private static void LaneClear()
        {
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, W.Range);

            if (!MainMenu.SpellFarm.Active) return;
            
            foreach (var minion in allMinions)
            {
                if (E.IsReady() && LaneClearMenu.ESliderBool.Enabled && LaneClearMenu.ESliderBool.ActiveValue < Player.ManaPercent &&
                    minion.IsValidTarget() && E.IsInRange(minion) && minion.Health < GetRealEDamage(minion))
                {
                    E.Cast(minion);
                }

                if (!W.IsReady() || !LaneClearMenu.WSliderBool.Enabled ||
                    !(LaneClearMenu.WSliderBool.ActiveValue < Player.ManaPercent) || !minion.IsValidTarget() ||
                    !W.IsInRange(minion)) continue;
                if (minion.IsValidTarget(W.Range) && W.GetLineFarmLocation(GameObjects.GetMinions(ObjectManager.Player.Position, Player.GetRealAutoAttackRange() + 100),
                    100).MinionsHit >= LaneClearMenu.WCountSliderBool.Value && GameObjects.GetMinions(ObjectManager.Player.Position, Player.GetRealAutoAttackRange() + 100).Count >= 3)
                {
                    W.Cast(minion);
                }
            }
        }

        private static void JungleClear()
        {
            var allJgl = GameObjects.GetJungles(ObjectManager.Player.Position, W.Range);

            if (!MainMenu.SpellFarm.Active) return;
            
            foreach (var jgl in allJgl)
            {
                if (E.IsReady() && jgl.IsValidTarget() && E.IsInRange(jgl) && JungleClearMenu.ESliderBool.Enabled && JungleClearMenu.ESliderBool.ActiveValue < Player.ManaPercent && jgl.Health < GetRealEDamage(jgl))
                {
                    E.Cast(jgl);
                }

                if (!W.IsReady() || !jgl.IsValidTarget() || !W.IsInRange(jgl) || !JungleClearMenu.WSliderBool.Enabled ||
                    !(JungleClearMenu.WSliderBool.ActiveValue < Player.ManaPercent)) continue;
                if (jgl.IsValidTarget(W.Range) && W.GetLineFarmLocation(GameObjects.GetJungles(ObjectManager.Player.Position, Player.GetRealAutoAttackRange() + 100),
                        100).MinionsHit >= LaneClearMenu.WCountSliderBool.Value && GameObjects.GetJungles(ObjectManager.Player.Position, Player.GetRealAutoAttackRange() + 100).Count >= LaneClearMenu.WCountSliderBool.Value)
                {
                    W.Cast(jgl);
                }
            }
        }

        private static void LastHit()
        {
          
        }

        private static void KillSteal()
        {
            if (KillStealMenu.EBool.Enabled)
            {
                CastE();
            }
        }
        
        private static void Stealthrecall()
        {
            if (!MiscMenu.StealthBack.Active || !Q.IsReady()) return;
            Q.Cast();
            Player.Spellbook.CastSpell(SpellSlot.Recall);
        }
        
        #endregion

        #region Spell Stage

      
        #endregion
        #region Spell Functions

        private static void CastQ()
        {
            if (Player.ChampionsKilled > Mykills)
            {
                Q.Cast();
                Mykills = Player.ChampionsKilled;
            }
        }
       
        private static void CastW()
        {
            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);
            if (W.GetPrediction(target).Hitchance >= HitChance.High && target.Health > GetRealEDamage(target) && GetEStackCount(target) < 6)
            {
                W.Cast(W.GetPrediction(target).CastPosition);
            }
        }

        private static void CastE()
        {
            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);

            if (target == null || target.IsInvulnerable || !target.IsValidTarget(E.Range) || !E.IsReady()) return;
            if (target.TrueHealth() < GetRealEDamage(target) - target.HPRegenRate)
            {
                E.Cast();
            }
        }


        #endregion

        #region damage 
        // Use it if some some damages aren't available by the sdk 
        
        private static double GetRealEDamage(AIBaseClient target)
        {
            if (target == null || target.IsDead || target.Buffs.All(b => b.Name.ToLower() != "twitchdeadlyvenom")) return 0d;
            if (target.HasBuff("KindredRNoDeathBuff"))
            {
                return 0;
            }

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
            {
                return 0;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return 0;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
            {
                return 0;
            }

            if (target.HasBuff("FioraW"))
            {
                return 0;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return 0;
            }

            if (target.HasBuff("SivirShield"))
            {
                return 0;
            }

            var damage = 0d;

            damage += E.IsReady() ? GetEdmgTwitch(target) : 0d;

            if (target.CharacterName == "Morderkaiser")
            {
                damage -= target.Mana;
            }

            if (Player.HasBuff("SummonerExhaust"))
            {
                damage *= 0.6f;
            }

            if (target.HasBuff("BlitzcrankManaBarrierCD") && target.HasBuff("ManaBarrier"))
            {
                damage -= target.Mana / 2f;
            }

            if (target.HasBuff("GarenW"))
            {
                damage *= 0.7f;
            }

            if (target.HasBuff("ferocioushowl"))
            {
                damage *= 0.7f;
            }

            return damage;

        }
        private static double GetEdmgTwitch(AIBaseClient target)
        {
            if (target == null || !target.IsValidTarget())
            {
                return 0;
            }

            if (!target.HasBuff("twitchdeadlyvenom"))
            {
                return 0;
            }

            var eLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level;
            if (eLevel <= 0)
            {
                return 0;
            }

            var buffCount = GetEStackCount(target);

            var baseDamage = new[] { 0, 20, 30, 40, 50, 60 }[eLevel];
            var extraDamage = new[] { 0, 15, 20, 25, 30, 35 }[eLevel] + 0.333f * ObjectManager.Player.TotalMagicalDamage +
                              0.35f * (ObjectManager.Player.TotalAttackDamage - ObjectManager.Player.BaseAttackDamage);
            var resultDamage =
                Player.CalculateDamage(target, DamageType.Physical, baseDamage + extraDamage * buffCount);
            if (Player.HasBuff("SummonerExhaust"))
            {
                resultDamage *= 0.6f;
            }

            return resultDamage;
        }
        private static int GetEStackCount(AIBaseClient target)
        {
            if (target == null || target.IsDead || !target.IsValidTarget() ||
                target.Type != GameObjectType.AIMinionClient && target.Type != GameObjectType.AIHeroClient)
            {
                return 0;
            }

            return target.GetBuffCount("twitchdeadlyvenom");
        }
        
        
   
        #endregion

        #region Extra functions

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            if (E.IsReady()) Damage += (float) GetRealEDamage(target);
            return Damage;
        }
        #endregion
    }
}