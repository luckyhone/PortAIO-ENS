using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Ashe
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
                ComboMenu.RBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.WSliderBool
            };
            
            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.WBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.WSliderBool,
                
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.WSliderBool,
            };

            var lastHitMenu = new Menu("lastHit", "Last Hit")
            {
                LastHitMenu.WSliderBool,
            };
            var GapCloserMenu = new Menu("GapCloserMenu","GapCloser")
            {
                Ashe.GapCloserMenu.RBool
            };
            var drawingMenu = new Menu("Drawing", "Drawing")
            {

                DrawingMenu.DrawQ,
                DrawingMenu.DrawW,
                DrawingMenu.DrawR
            };
            var StructureMenu = new Menu("StructureClear","Structure Clear")
            {
                StructureClearMenu.QSliderBool,
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
                GapCloserMenu,
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

            public static readonly MenuKeyBind RBool =
                new MenuKeyBind("combosemiR", "Semi R", Keys.T, KeyBindType.Press);
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("harassW", "Use W | If Mana >= x%", 50);

        }

        public static class KillStealMenu
        {
            public static readonly MenuBool WBool = new MenuBool("killStealW", "Use W");
        }

        public static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("jungleClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("jungleClearW", "Use W | If Mana >= x%", 50);
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("laneClearW", "Use W | If Mana >= x%", 50);

        }
        public static class GapCloserMenu
        {
            public static readonly MenuBool RBool = new MenuBool("enabled", "Use R on gapcloser");
        }
        public static class StructureClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("structClearQ", "Use Q | If Mana >= x%", 50);
        }


        public static class LastHitMenu
        {
            public static readonly MenuBool WSliderBool = new MenuBool("lastHitW", "Use W");
        }
        
        public static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q");
            public static readonly MenuBool DrawW = new MenuBool("DrawW", "Draw W");
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }
        

        #endregion

        #region Spells 

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, Player.GetRealAutoAttackRange());
            W = new Spell(SpellSlot.W,1250f);
            W.SetSkillshot(0.25f, 20f, 1500f, true, SpellType.Cone);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 4500f);
            R.SetSkillshot(0.25f, 130f, 1600f, false, SpellType.Line);
        }


        #endregion
        #region Gamestart
      
        public Ashe()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnAfterAttack += OrbwalkerOnOnAction;
            Drawing.OnEndScene += delegate(EventArgs args)
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            AntiGapcloser.OnGapcloser += GapcloserOnOnGapcloser;
            
        }
        

        #endregion

        #region args
        
        private void GapcloserOnOnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (sender == null || !sender.IsValid || !sender.IsEnemy)
            {
                return;
            }

            if (R.IsReady() && !sender.HasBuffOfType(BuffType.SpellShield))
            {
                if (!GapCloserMenu.RBool.Enabled) return;
                
                var rPred = R.GetPrediction(sender);
                
                if (!R.IsReady() || !sender.IsValidTarget(R.Range)) return;
                if (sender.IsMelee)
                    if (sender.IsValidTarget(sender.AttackRange + sender.BoundingRadius + 100))
                        if (rPred.Hitchance >= HitChance.VeryHigh) R.Cast((rPred.CastPosition));

                if (sender.IsDashing())
                    if (args.EndPosition.DistanceToPlayer() <= 250 ||
                        sender.PreviousPosition.DistanceToPlayer() <= 300)
                        if (rPred.Hitchance >= HitChance.VeryHigh) R.Cast((rPred.CastPosition));

                if (!sender.IsCastingImporantSpell()) return;
                if (!(sender.PreviousPosition.DistanceToPlayer() <= 300)) return;
                if (rPred.Hitchance >= HitChance.VeryHigh) R.Cast((rPred.CastPosition));
                R.Cast(sender);
            }
        }
        
        private static void OrbwalkerOnOnAction(object sender, AfterAttackEventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                {
                    if (args.Target == null || 
                        args.Target.Type != GameObjectType.AIHeroClient) return;

                    if (Q.IsReady() && ComboMenu.QBool.Enabled &&
                        args.Target.IsValidTarget(Player.GetRealAutoAttackRange()))
                    {
                        Q.Cast();
                    }
                    
                    if (W.IsReady() && ComboMenu.WBool.Enabled &&
                        args.Target.IsValidTarget(W.Range))
                    {
                        var target = args.Target as AIHeroClient;
                        if (W.GetPrediction(target).Hitchance >= HitChance.High) 
                            W.Cast(W.GetPrediction(target).UnitPosition);
                    }
                }
                    break;
                
                case OrbwalkerMode.LaneClear:
                {
                    if (!MainMenu.SpellFarm.Active) return;
                    //Jungle
                    if (args.Target.IsEnemy && args.Target.IsJungle())
                    {
                        var Jungle = GameObjects.GetJungles(Player.Position, W.Range)
                            .OrderByDescending(x => x.MaxHealth)
                            .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
                        if (Jungle == null) return;
                        if (JungleClearMenu.WSliderBool.Enabled &&
                            JungleClearMenu.WSliderBool.ActiveValue < Player.ManaPercent &&
                            W.IsReady())
                        {
                            W.Cast(Jungle);
                        }
                        if (JungleClearMenu.QSliderBool.Enabled &&
                            JungleClearMenu.QSliderBool.ActiveValue < Player.ManaPercent &&
                            Q.IsReady())
                        {
                            Q.Cast();
                        }
                    }
                    //Minions
                    if (args.Target.IsEnemy && args.Target.IsMinion())
                    {
                        var Minion = GameObjects.GetMinions(Player.Position, W.Range)
                            .OrderByDescending(x => x.MaxHealth)
                            .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
                        if (Minion == null) return;
                        if (LaneClearMenu.WSliderBool.Enabled &&
                            LaneClearMenu.WSliderBool.ActiveValue < Player.ManaPercent &&
                            W.IsReady())
                        {
                            W.Cast(Minion);
                        }
                        if (LaneClearMenu.QSliderBool.Enabled &&
                            LaneClearMenu.QSliderBool.ActiveValue < Player.ManaPercent &&
                            Q.IsReady())
                        {
                            Q.Cast();
                        }
                    }
                    //Turrets
                    if (args.Target is AITurretClient &&
                        StructureClearMenu.QSliderBool.Enabled &&
                        StructureClearMenu.QSliderBool.ActiveValue < Player.ManaPercent &&
                        Q.IsReady())
                    {
                        Q.Cast();
                    }
                }
                    break;
                case OrbwalkerMode.LastHit:
                {
                    if (!MainMenu.SpellFarm.Active) return;
                        //Turrets
                    if (args.Target is AITurretClient &&
                        StructureClearMenu.QSliderBool.Enabled &&
                        StructureClearMenu.QSliderBool.ActiveValue < Player.ManaPercent &&
                        Q.IsReady())
                    {
                        Q.Cast();
                    }
                }
                    break;
                case OrbwalkerMode.None:
                    break;
            }
            
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ.Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.Violet);
            if (DrawingMenu.DrawW.Enabled && W.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, W.Range, Color.DarkCyan);
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
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }
            
            KillSteal();

            if (ComboMenu.RBool.Active)
            {
                SemiR();
            }
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
           
        }
        
        private static void SemiR()
        {
            Orbwalker.Move(Game.CursorPos);
            var target = TargetSelector.SelectedTarget;
            if (target == null || !target.IsValidTarget(R.Range)) return;
            var rPred = R.GetPrediction(target);
            if (rPred.Hitchance >= HitChance.VeryHigh) R.Cast((rPred.CastPosition));
        }
            
        private static void Harass()
        {
            var target = TargetSelector.GetTarget((W.Range),DamageType.Physical);
            
            if (W.IsReady() && HarassMenu.WSliderBool.Enabled &&
                HarassMenu.WSliderBool.ActiveValue < Player.ManaPercent &&
                target.IsValidTarget(W.Range))
            {
                if (W.GetPrediction(target).Hitchance >= HitChance.High)
                    W.Cast(W.GetPrediction(target).UnitPosition);
            }
        }

        private static void LaneClear()
        {
            
        }

        private static void JungleClear()
        {
           
        }

        private static void LastHit()
        {
            if (!MainMenu.SpellFarm.Active || !LastHitMenu.WSliderBool.Enabled) return;
            //Minions
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, W.Range);
            foreach (var minion in allMinions.Where(minion =>
                minion.IsValidTarget(W.Range) && Player.Distance(minion) > Player.GetRealAutoAttackRange() &&
                minion.Health < Player.GetSpellDamage(minion, SpellSlot.W)))
            {
                W.Cast(minion);
                return;
            }
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

        private static bool Ignite => Player.Spellbook.CanUseSpell(Player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
        
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
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (W.IsReady()) Damage += W.GetDamage(target);
            if (E.IsReady()) Damage += Edmg(target);
            if (R.IsReady()) Damage += R.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        #endregion
    }
}