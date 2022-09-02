using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using static StormAIO.utilities.SkinChanger;

namespace StormAIO.Champions
{
    internal class Zed
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;
        private static int lastW;
        private static int lastR;
        private static string El = "ASSETS/Perks/Styles/Domination/Electrocute/Electrocute.lua";
        #endregion

        #region Menu

        private static void CreateMenu()
        {
            ChampMenu = new Menu(Player.CharacterName, Player.CharacterName);
            
            var comboMenu = new Menu("combo", "Combo")
            {
                ComboMenu.QBool,
                ComboMenu.WBool,
                ComboMenu.WGapB,
                ComboMenu.Electrocute,
                ComboMenu.EBool,
                ComboMenu.RSliderBool,
                ComboMenu.OnlyUseR,
                ComboMenu.Ignite
                
                
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
                KillStealMenu.EBool
            };
            var Evademenu = new Menu("Evade", "Evade")
            {
                Evade.RBool,
                Evade.R2Bool,
                Evade.W2Bool
            };
            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.ESliderBool,
                new Menu("customization", "Customization")
                {
                    LaneClearMenu.QCountSliderBool,
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QBool,
                JungleClearMenu.WBool,
                JungleClearMenu.EBool
            };

            var lastHitMenu = new Menu("lastHit", "Last Hit")
            {
                LastHitMenu.QBool,
            };

            var drawingMenu = new Menu("Drawing", "Drawing")
            {

                DrawingMenu.DrawQ,
                DrawingMenu.DrawW,
                DrawingMenu.DrawE,
                DrawingMenu.DrawR,
            };
            var Fleemenumenu = new Menu("Flee","Flee")
            {
                FleeMenu.Flee,
                FleeMenu.UseW,
                FleeMenu.UseWWall
            };
         
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                Evademenu,
                laneClearMenu,
                jungleClearMenu,
                lastHitMenu,
                Fleemenumenu,
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
            public static readonly MenuBool WGapB = new MenuBool("comboG","Use ^ W to gapclose for utl");
            public static readonly MenuBool Electrocute = new MenuBool("Electrocute","Use ^ W to cast Electrocute proc");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
            public static readonly MenuSliderButton RSliderBool = new MenuSliderButton("comboR", "Use R |  If Energy >= x", 130,0,200);
            public static readonly MenuBool Ignite = new MenuBool("Ignite", "Use Ignite When Utling");
            public static readonly MenuBool OnlyUseR = new MenuBool("OnlyUseR", "OnlyUseR on selected target",false);
            
        }

        private static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool = new MenuSliderButton("harassQ", "Use Q |  If Energy >= x", 60,0,200);
            public static readonly MenuSliderButton WSliderBool = new MenuSliderButton("harassW", "Use W |  If Energy >= x", 110,0,200);
            public static readonly MenuSliderButton ESliderBool = new MenuSliderButton("harassE", "Use E |  If Energy >= x", 50,0,200);
            
        }

        private static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killStealQ", "Use Q");
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
        }
        

        private static class JungleClearMenu
        {
            public static readonly MenuBool QBool =
                new MenuBool("jungleClearQ", "Use Q");

            public static readonly MenuBool WBool =
                new MenuBool("jungleClearW", "Use W ");

            public static readonly MenuBool EBool =
                new MenuBool("jungleClearE", "Use E ");
        }

        private static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Energy >= x", 60,0,200);

            public static readonly MenuSlider QCountSliderBool =
                new MenuSlider("laneClearQCount", "Use Q if hittable minions >= x", 3, 1, 5);
            
            

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("laneClearE", "Use E |  If Energy >= x", 60,0,200);
            
        }
        
        private static class Evade
        {
            public static readonly MenuBool RBool = new MenuBool("EvadeR", "Use R To Evade");
            public static readonly MenuBool R2Bool = new MenuBool("EvadeR2", "Use R2 To Evade");
            public static readonly MenuBool W2Bool = new MenuBool("EvadeW", "Use W To Evade");
            
        }
        
        private static class LastHitMenu
        {
            public static readonly MenuBool QBool = new MenuBool("lastHitQ", "Use Q",false);
        }
        private static class FleeMenu
        {
            public static readonly MenuKeyBind Flee = new MenuKeyBind("Flee", "Flee",Keys.Z,KeyBindType.Press);
            public static readonly MenuBool UseW = new MenuBool("UseW", "Use W");
            public static readonly MenuBool UseWWall = new MenuBool("UseWWall", "^ Use W Only Through Walls");
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
            Q = new Spell(SpellSlot.Q, 900);
            Q.SetSkillshot(0.25f, 50f, 1700f, false, SpellType.Line);
            W = new Spell(SpellSlot.W, 650);
            W.SetSkillshot(0f, 50f, 2500, false, SpellType.Line);
            E = new Spell(SpellSlot.E, 290f) {Delay = 0f};
            R = new Spell(SpellSlot.R, 625);
            R.SetTargetted(0f, float.MaxValue);
        }


        #endregion
        #region Gamestart
      
        public Zed()
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
            AIBaseClient.OnDoCast += AIBaseClientOnOnProcessSpellCast;
            Interrupter.OnInterrupterSpell += InterrupterOnOnInterrupterSpell;
            GameObject.OnCreate += AIBaseClientOnOnMissileCreate;
            
           

        }
        
        private void AIBaseClientOnOnMissileCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "PerkSoulFeastVisualMissile" && sender.IsAlly) 
                Utlsafespot();
        }


        private void InterrupterOnOnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (sender.IsEnemy && args.DangerLevel == Interrupter.DangerLevel.High )
            {
                if (Evade.RBool.Enabled && R.IsReady())
                {
                    var target = TargetSelector.GetTarget(R.Range,DamageType.Physical);
                    if (target == null) return;
                    R.Cast(target);
                }
            }
        }


        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!Evade.R2Bool.Enabled && !Evade.W2Bool.Enabled && !Evade.RBool.Enabled) return;
            var hero = sender is AIHeroClient;
            if (!sender.IsEnemy || args.Target == null || !args.Target.IsMe) return;
            if (!hero) return;
            if ( args.Slot == SpellSlot.R && Evade.RBool.Enabled && R.IsReady() && _RStage == RStage.First && args.SData.TargetingType == SpellDataTargetType.Target )
            {
                var target = TargetSelector.GetTarget(R.Range,DamageType.Physical);
                if (target == null) return;
                R.Cast(target);
            }

            if (BlockSpellDataBase.DangerousList.Contains(args.SData.Name) && args.To.DistanceToPlayer() < 100 )
            {
                if (Evade.R2Bool.Enabled && _RStage == RStage.Second)
                {
                    if (args.To.Distance(RShadow) > args.SData.CastRadius)
                    {
                        R.Cast();
                    }
                    
                }

                if (Evade.W2Bool.Enabled && _wStage == WStage.Second && ShdowsafeSpot())
                {
                    if (args.To.Distance(WShadow) > args.SData.CastRadius)
                    {
                        W.Cast();
                    }
                  
                }
            }
            if (sender.CharacterName.Equals("Yone") && args.Slot == SpellSlot.R && args.To.DistanceToPlayer() < 100 )
            {
                if (Evade.R2Bool.Enabled && _RStage == RStage.Second)
                {
                    R.Cast();
                }

                if (Evade.W2Bool.Enabled && _wStage == WStage.Second && ShdowsafeSpot())
                {
                    W.Cast();
                }
            }
            
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

            KillSteal();
            if (FleeMenu.Flee.Active)
            {
                Orbwalker.Move(Game.CursorPos);
                if (FleeMenu.UseW.Enabled && FleeMenu.UseWWall.Enabled)
                {
                    if (!W.IsReady()) { 
                        return;
                    }

                    // dash endPos
                    var maxDashPos = Player.Position.Extend(Game.CursorPos, 650);
                    if (!maxDashPos.IsValid() || maxDashPos.IsWall()) {
                        return;
                    }
    
                    // check about the wall width
                    var wallwidth = Helper.GetWallWidth(Player.Position, maxDashPos, 650);
    
                    // if wall width == 0 or wall width > E Range, cant be dash
                    if (wallwidth <= 0f || wallwidth >= 650) { 
                        return;
                    }
                    if (Game.CursorPos.IsWall()) return;
                    // can walljump
                    W.Cast(Game.CursorPos);
                }
                if (FleeMenu.UseW.Enabled && !FleeMenu.UseWWall.Enabled)
                {
                    if (!W.IsReady()) { 
                        return;
                    }
                    
            
                    // can walljump
                    W.Cast(Game.CursorPos);
                }
            }
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (!R.IsReady() || _RStage == RStage.Second || !ComboMenu.RSliderBool.Enabled || ComboMenu.OnlyUseR.Enabled)
            {

                if (ComboMenu.QBool.Enabled)
                {
                    if(!W.IsReady() || _wStage == WStage.Second || !ComboMenu.WBool.Enabled) CastQ(GetEnemy);
                }
                if (ComboMenu.EBool.Enabled) CastE(GetEnemy);
                if (ComboMenu.WBool.Enabled && !ComboMenu.Electrocute.Enabled && ComboMenu.WGapB.Enabled) CastW();
                if (ComboMenu.WBool.Enabled && ComboMenu.Electrocute.Enabled ) CastW2();
                if (ComboMenu.WBool.Enabled && !ComboMenu.WGapB.Enabled) CastW3();
            }
            if (_RStage == RStage.First && ComboMenu.RSliderBool.Enabled  && Environment.TickCount > lastR )
            {
                if (!(Player.Mana >= ComboMenu.RSliderBool.ActiveValue)) return; 
                if (TargetSelector.SelectedTarget == null && ComboMenu.OnlyUseR.Enabled) return;
                var target = TargetSelector.GetTarget(R.Range + W.Range,DamageType.Physical);
                if (target == null) return;
                lastR = Environment.TickCount + 250;
                var igniteSlot = Player.GetSpellSlot("SummonerDot");
                var igniteTarget = TargetSelector.GetTarget(600,DamageType.Physical);
                if (Helper.Ignite && igniteTarget != null && ComboMenu.Ignite.Enabled) Player.Spellbook.CastSpell(igniteSlot, igniteTarget);
                if (W.IsReady() && ComboMenu.WGapB.Enabled)
                {
                    if (!R.IsInRange(target)) W.Cast(target.Position.Extend(Player.Position, -650));
                    DelayAction.Add(150,()=> R.Cast(target));
                    return;
                }
                R.Cast(target);

            }

        
            
        }

        private static void Harass()
        {
            if (HarassMenu.QSliderBool.Enabled && Player.Mana > HarassMenu.QSliderBool.ActiveValue)
            {
                if(!W.IsReady() || _wStage == WStage.Second || !HarassMenu.WSliderBool.Enabled)   CastQ(GetEnemy);
            }
            if (HarassMenu.WSliderBool.Enabled && Player.Mana > HarassMenu.WSliderBool.ActiveValue && Q.IsReady())
            {
                CastW();
            }
            if (HarassMenu.ESliderBool.Enabled && Player.Mana > HarassMenu.ESliderBool.ActiveValue)
            {
                CastE(GetEnemy);
            }
            
        }

        private static void LaneClear()
        {
            if (LaneClearMenu.QSliderBool.Enabled && Q.IsReady())
            {
                Q.UpdateSourcePosition(Player.Position,Player.Position);
                var minions = GameObjects.GetMinions(Player.Position, Q.Range);
                var minion = GameObjects.GetMinions(Player.Position, Q.Range).FirstOrDefault(x => x.IsValid && x.Health < Q.GetDamage(x));
                if (minion != null)
                {
                    Q.Cast(minion);
                    return;
                }
                if (!minions.Any()) return;
               var farmline = Q.GetLineFarmLocation(minions, 50f);
               if (farmline.MinionsHit >= LaneClearMenu.QCountSliderBool.Value)
               {
                   Q.Cast(farmline.Position);
               }
            }
            if (LaneClearMenu.ESliderBool.Enabled && E.IsReady())
            {
                E.UpdateSourcePosition(Player.Position,Player.Position);
                var minions = GameObjects.GetMinions(Player.Position, E.Range);
                if (minions == null) return;
                CastE(minions.FirstOrDefault());
            }
        }

        private static void JungleClear()
        {
            if (JungleClearMenu.QBool.Enabled && Q.IsReady() && (!W.IsReady() || _wStage == WStage.Second || !JungleClearMenu.WBool.Enabled))
            {
                var Jungles = GameObjects.GetJungles(Player.Position, Q.Range).OrderBy(x=> x.DistanceToPlayer()).FirstOrDefault();
                if (Jungles == null) return;
                Q.UpdateSourcePosition(Player.Position,Player.Position);
                    Q.Cast(Jungles.Position);
            }
            if (JungleClearMenu.EBool.Enabled && E.IsReady())
            {
                var jungles = GameObjects.GetJungles(Player.Position, E.Range);
                if (jungles == null) return;
                E.UpdateSourcePosition(Player.Position,Player.Position);
                CastE(jungles.FirstOrDefault());
            }
            if (JungleClearMenu.WBool.Enabled && W.IsReady() && _wStage == WStage.First && Environment.TickCount > lastW)
            {
                var jungles = GameObjects.GetJungles(Player.Position, W.Range + 200).FirstOrDefault();
                if (jungles == null) return;
                lastW = Environment.TickCount + 250;
                W.Cast(jungles.Position.Extend(Player.Position,-100));
            }
        }

        private static void LastHit()
        {
            if (Q.IsReady() && LastHitMenu.QBool.Enabled)
            {
                var minion = GameObjects.GetMinions(Player.Position, Q.Range)
                    .FirstOrDefault(x => Q.GetDamage(x) > x.Health);
                if (minion == null) return;
                Q.Cast(minion);
            }
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(20000,DamageType.Physical);
            if (target != null && !(Q.GetDamage(target) < target.TrueHealth()))
            {
                if (KillStealMenu.QBool.Enabled)
                {
                    CastQ(target);
                }
            }
            if (target != null && E.GetDamage(target) > target.TrueHealth())
            {
                if (KillStealMenu.EBool.Enabled)
                {
                    CastE(target);
                }
            }
        }
        
        #endregion

        #region Spell Stage

      
        #endregion
        #region Spell Functions

        private static void CastQ(AIBaseClient T = null)
        {

            var target = T;
            if (target == null) return;
            if (!Q.IsReady()) return;
            Q.Width = 35f;
           ShadowManager(T,Q);
            var qpre = Q.GetPrediction(target);
            if (qpre.Hitchance >= HitChance.High && Q.IsInRange(target))
            {
                Q.Cast(qpre.CastPosition);
            }
        }
       
        private static void CastW()
        {
            if (!W.IsReady()) return;
            var target = TargetSelector.GetTarget(W.Range + 100,DamageType.Physical);
            if (target == null) return;
            if (lastW > Environment.TickCount) return;
            if ( _wStage == WStage.Second ) return;
            lastW = Environment.TickCount + 250 + Game.Ping;
            W.Cast(target.Position.Extend(Player.Position, -100));
        }
        private static void CastW3()
        {
            if (!W.IsReady()) return;
            var target = TargetSelector.GetTarget(W.Range + 100,DamageType.Physical);
            if (target == null) return;
            if (lastW > Environment.TickCount) return;
            if ( _wStage == WStage.Second ) return;
            lastW = Environment.TickCount + 250 + Game.Ping;
            W.Cast(Player.Position.Extend(Game.CursorPos, 100));
        }

        private static void CastW2()
        {
            if (!W.IsReady()) return;
            var target = TargetSelector.GetTarget(W.Range + 100,DamageType.Physical);
            if (target == null) return;
            if (!Player.HasBuff(El) || !Q.IsReady() || Player.Mana < 60)
            {
                CastW();
                return;
            }
            W.Cast(target.Position.Extend(Player.Position, -25));
        }

        private static void CastE(AIBaseClient T)
        {
            if (!E.IsReady()) return;
            var target = T;
            if (target == null) return;
            ShadowManager(target,E);
            if ( E.IsInRange(target))
            {
                E.Cast();
            }
        }
        
        #endregion

        #region damage 
        // Use it if some some damages aren't available by the sdk 
        public static float passivedmg(AIBaseClient target)
        {
            var dmglevel = 6;
            if (Player.Level >= 7) dmglevel = 8;
            if (Player.Level >= 17) dmglevel = 10;

            return (float) (target.HealthPercent <= 50 && !target.HasBuff("zedpassivecd") ? Player.CalculateMagicDamage(target, target.MaxHealth / dmglevel) : 0);
        }
        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (R.IsReady()) Damage += R.GetDamage(target) + AllDamage2(target) * (float) new[] {0.25,0.40,0.60}[R.Level - 1 ];
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        private static float AllDamage2(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null)                                 return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100) 
                Damage += (float)Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }
        #endregion

      

    
        #region Shawdow postions

        private static AIMinionClient WShadow => GameObjects.Get<AIMinionClient>().FirstOrDefault(x => x.IsValid && x.IsAlly  && x.HasBuff("zedwshadowbuff")  && x.Name == "Shadow");
        private static AIMinionClient RShadow => GameObjects.Get<AIMinionClient>().FirstOrDefault(x => x.IsValid && x.IsAlly  && x.HasBuff("zedrshadowbuff")  && x.Name == "Shadow");
        

        #endregion
        #region Spell recasts

        private  enum WStage
        {
            First,
            Second,
            Cooldown
        }

        private static WStage _wStage
        {
            get
            {
                if (!W.IsReady()) return WStage.Cooldown;

                return W.Name == "ZedW" ? WStage.First : WStage.Second;
            }
        } 
        private  enum RStage
        {
            First,
            Second,
            Cooldown
        }

        private static RStage _RStage
        {
            get
            {
                if (!R.IsReady()) return RStage.Cooldown;

                return R.Name == "ZedR" ? RStage.First : RStage.Second;
            }
        } 

        #endregion
        
       
        #region Extra functions
        private static AIHeroClient GetEnemy
        {
            get
            {
                var t = TargetSelector.SelectedTarget == null
                    ? GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget() && (
                        x.Distance(Player.Position) < Q.Range || (WShadow != null && x.Distance(WShadow.Position) < Q.Range) ||  (RShadow != null &&
                            x.Distance(RShadow.Position) < Q.Range) ))
                    : TargetSelector.SelectedTarget;
                return t;
            }
        }

        private static void ShadowManager(AIBaseClient T,Spell spell)
        {
            var target = T;
            var localD = Player.Distance(target);
            var wShadowDistance = WShadow != null ? target.Distance(WShadow) : 16000000f;
            var rShadowDistance = RShadow != null ? target.Distance(RShadow) : 16000000f;
            var min = Math.Min(Math.Min(rShadowDistance, wShadowDistance), localD);
            if (Math.Abs(min - wShadowDistance) < Player.Distance(target))
            {
                if (WShadow == null) return;
                spell.UpdateSourcePosition(WShadow.Position,WShadow.Position);
            }
            else if (Math.Abs(min - rShadowDistance) < localD )
            {
                if (RShadow == null) return;
                spell.UpdateSourcePosition(RShadow.Position,RShadow.Position);
            }
            else
            {
                spell.UpdateSourcePosition(Player.Position, Player.Position);
            }
          
        }

        private static bool ShdowsafeSpot()
        {
            if (WShadow != null && _wStage == WStage.Second  && !WShadow.IsUnderEnemyTurret() &&
                WShadow.CountEnemyHeroesInRange(500) < Player.CountEnemyHeroesInRange(500))
            {
                return true;
            }
            if (RShadow != null && _RStage == RStage.Second && !RShadow.IsUnderEnemyTurret() &&
                WShadow.CountEnemyHeroesInRange(500) < Player.CountEnemyHeroesInRange(500))
            {
                return true;
            }
            return false;
        }
        private static void Utlsafespot()
        {
            if (WShadow != null && _wStage == WStage.Second && !WShadow.IsUnderEnemyTurret() &&
                WShadow.CountEnemyHeroesInRange(500) < Player.CountEnemyHeroesInRange(200) && _wStage == WStage.Second)
            {
                W.Cast();
                return;
            }
            if (RShadow != null && _RStage == RStage.Second &&!RShadow.IsUnderEnemyTurret() &&
                RShadow.CountEnemyHeroesInRange(200) < Player.CountEnemyHeroesInRange(500))
            {
                R.Cast();
            }
        }
        #endregion
    }
}