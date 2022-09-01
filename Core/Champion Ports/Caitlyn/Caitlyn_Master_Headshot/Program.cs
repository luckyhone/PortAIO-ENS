using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SebbyLib;
using ShadowTracker;
using SharpDX;
using Collision = SebbyLib.Collision;
using PredictionInput = EnsoulSharp.SDK.PredictionInput;
using PredictionOutput = EnsoulSharp.SDK.PredictionOutput;

namespace Caitlyn_Master_Headshot
{
    class Program
    {
        // Variable
        public static Menu myMenu,
            comboSettings,
            harassSettings,
            ultimateSettings,
            trapSettings,
            antiGapcloserSettings,
            killStealSettings,
            drawingSettings;
        public static AIHeroClient myHero;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static float lastTrap;
        private static Dictionary<int, GameObject> trapDict = new Dictionary<int, GameObject>();

        // Loader
        public static void Loads()
        {
            OnLoad(new EventArgs());
        }

        private static void OnLoad(EventArgs args)
        {
            initVariable();
            
            if (myHero.CharacterName != "Caitlyn")
                return;

            Game.Print("<font color=\"#FF001E\">Caitlyn Master Headshot- </font><font color=\"#FF980F\"> Loaded</font>");
            initMenu();

            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
            Game.OnUpdate += Update;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            AIBaseClient.OnBuffAdd += OnBuffAdd;
            Drawing.OnDraw += OnDraw;
        }
        
        private static void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsAlly && obj.Name == "Cupcake Trap")
            {
                trapDict.Add(obj.NetworkId, obj);
            }
               
        }

        private static void Obj_AI_Base_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.IsAlly && obj.Name == "Cupcake Trap")
            {
                if (trapDict.ContainsKey(obj.NetworkId))
                    trapDict.Remove(obj.NetworkId);
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && Q.IsReady() && sender.IsMe && (int)Args.Slot == 49)
            {
                var Target = TargetSelector.GetTarget(1200, DamageType.Physical);
                if (ValidTarget(Target) && Target.NetworkId == Args.Target.NetworkId)
                {
                    PredictionOutput qPred = Q.GetPrediction(Target);
                    if ((int)qPred.Hitchance > comboSettings.GetValue<MenuSlider>("qHitChance").Value)
                        Q.Cast(qPred.CastPosition);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!antiGapcloserSettings.GetValue<MenuBool>("AntiGapCloser").Enabled)
                return;

            if (E.IsReady())
            {
                PredictionOutput ePred = E.GetPrediction(sender);
                E.Cast(ePred.CastPosition);
            }
        }

        private static void OnBuffAdd(AIBaseClient sender, AIBaseClientBuffAddEventArgs Args)
        {
            if (!trapSettings.GetValue<MenuBool>("autoW").Enabled)
                return;

            if (W.IsReady() && sender.IsEnemy  && sender.Distance(myHero) < W.Range)
            {
                if (Args.Buff.Type == BuffType.Stun || Args.Buff.Type == BuffType.Taunt || Args.Buff.Type == BuffType.Knockup || Args.Buff.Type == BuffType.Snare)
                    W.Cast(sender.Position);
            }
        }

        private static void Update(EventArgs args)
        {
            if (myHero == null || myHero.IsDead)
                return;

            if (ultimateSettings.GetValue<MenuBool>("useR").Enabled)
                autoCastR();

            if (killStealSettings.GetValue<MenuBool>("qKillSteal").Enabled)
                qKillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
            } 
        }
        
        // Combo
        private static void Combo()
        {
            var Target = GetTarget();

            if (!ValidTarget(Target) || Target == null)
                return;

            wCastCombo(Target);
            eCastCombo(Target);      
        }

        private static void wCastCombo(AIBaseClient Target)
        {
            if (comboSettings.GetValue<MenuBool>("useW").Enabled && W.IsReady() && Game.Time - lastTrap > comboSettings.GetValue<MenuSlider>("wDelay").Value/1000)
            {
                PredictionOutput wPred = W.GetPrediction(Target);
                if (IsTrapNear(wPred.CastPosition, 100) == 0 && (int)wPred.Hitchance >= comboSettings.GetValue<MenuSlider>("wHitChance").Value)
                {
                    W.Cast(wPred.CastPosition);
                    lastTrap = Game.Time;
                }
            }

        }

        private static void eCastCombo(AIBaseClient Target)
        {
            if (comboSettings.GetValue<MenuBool>("useE").Enabled && E.IsReady())
            {
                PredictionOutput ePred = E.GetPrediction(Target);
                               
                if ((int)ePred.Hitchance >= comboSettings.GetValue<MenuSlider>("eHitChance").Value)
                    E.Cast(ePred.CastPosition);
            }
        }
        
        // Harass
        private static void Harass()
        {
            var Target = GetTarget();

            if (!ValidTarget(Target))
                return;

            qHarass(Target);
        }

        private static void qHarass(AIHeroClient unit)
        {
            if (Q.IsReady() && harassSettings.GetValue<MenuBool>("useQ.Harass").Enabled && myHero.ManaPercent > harassSettings.GetValue<MenuSlider>("qMana").Value)
            {
                PredictionOutput qPred = Q.GetPrediction(unit);
                if ((int)qPred.Hitchance >= harassSettings.GetValue<MenuSlider>("qHitChance.Harass").Value)
                    Q.Cast(qPred.CastPosition);
            }
        }
        
        // Utility
        private static bool ValidTarget(AIHeroClient unit)
        {
            return !(unit == null) && unit.IsValid && unit.IsTargetable && !unit.IsInvulnerable;
        }

        private static int IsTrapNear(Vector3 Position, int Range)
        {
            int trapNear = 0;
            foreach (var trap in trapDict)
            {
                if (Position.Distance(trap.Value.Position) < Range)
                    trapNear++;
            }           

            return trapNear;
        }

        private static int CountEnemyNear(Vector3 From, int Range)
        {
            int enemyNear = 0;
            foreach(var unit in GameObjects.EnemyHeroes)
            {
                if (From.Distance(unit.Position) < 500)
                    enemyNear++;
            }
            return enemyNear;
        }

        private static AIHeroClient GetTarget()
        {
            AIHeroClient Target = TargetSelector.GetTarget(myHero.AttackRange, DamageType.Physical);
            if(!ValidTarget(Target))
                Target = TargetSelector.GetTarget(1200, DamageType.Physical);

            return Target;
        }
        
        private static void autoCastR()
        {
            if (!R.IsReady() || (Orbwalker.ActiveMode == OrbwalkerMode.Combo && !ultimateSettings.GetValue<MenuBool>("rCombo").Enabled))
                return;

            foreach (var unit in GameObjects.EnemyHeroes)
            {
                if (ValidTarget(unit) && myHero.Distance(unit) > (myHero.AttackRange+500) && R.GetDamage(unit, 0) > unit.Health && CountEnemyNear(myHero.Position, 1500) == 0)
                {
                    PredictionInput predInput = new PredictionInput { From = myHero.Position, Radius = 1500, Range = 3000 };
                    predInput.CollisionObjects[0] = CollisionObjects.YasuoWall;
                    predInput.CollisionObjects[1] = CollisionObjects.Heroes;
               
                    IEnumerable<AIBaseClient> rCol = Collisions.GetCollision(new List<Vector3> { unit.Position },predInput);
                    IEnumerable<AIBaseClient> rObjCol = rCol as AIBaseClient[] ?? rCol.ToArray();

                    if (rObjCol.Count() == 0 && CountEnemyNear(unit.Position, 1000) == 1)
                        R.Cast(unit);
                }
            }
        }
        
        private static void qKillSteal()
        {
            foreach (var unit in GameObjects.EnemyHeroes)
            {
                if (ValidTarget(unit) && Q.GetDamage(unit, 0) > unit.Health && CountEnemyNear(myHero.Position, (int)myHero.AttackRange) == 0)
                {
                    PredictionOutput qPred = Q.GetPrediction(unit);
                    if ((int)qPred.Hitchance >= killStealSettings.GetValue<MenuSlider>("qKillSteal.Hitchance").Value && myHero.Distance(qPred.CastPosition) > (myHero.AttackRange + 100) && unit.MoveSpeed >= myHero.MoveSpeed)
                        Q.Cast(qPred.CastPosition);
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (myHero == null)
                return;
            if (myHero.Position.IsOnScreen() && Q.IsReady() && myMenu.GetValue<MenuBool>("qRange").Enabled)
                CircleRender.Draw(ObjectManager.Player.Position, Q.Range, SharpDX.Color.AliceBlue);

            if (myHero.Position.IsOnScreen() && W.IsReady() && myMenu.GetValue<MenuBool>("wRange").Enabled)
                CircleRender.Draw(ObjectManager.Player.Position, W.Range, SharpDX.Color.Aqua);

            if (myHero.Position.IsOnScreen() && E.IsReady() && myMenu.GetValue<MenuBool>("eRange").Enabled)
                CircleRender.Draw(ObjectManager.Player.Position, E.Range, SharpDX.Color.Aquamarine);
        }

        private static void initVariable()
        {
            Q = new Spell(SpellSlot.Q,1250 - 50);
            W = new Spell(SpellSlot.W,800f);
            E = new Spell(SpellSlot.E,750 - 50);
            R = new Spell(SpellSlot.R);

            lastTrap = Game.Time;
            myHero = ObjectManager.Player;
        }
        
        private static void initMenu()
        {
            myMenu = new Menu("CaitlynMasterHeadshot","Caitlyn - Master Headshot", true);

            comboSettings = myMenu.Add(new Menu("Combo","Combo Settings"));;
            comboSettings.Add(new MenuBool("useQ", "Use (Q)").SetValue(true));
            comboSettings.Add(new MenuSlider("qHitChance", "(Q) Hit Chance",3, 3, 6));
            comboSettings.Add(new MenuSeparator("infoW", " "));
            comboSettings.Add(new MenuBool("useW", "Use (W)").SetValue(true));
            comboSettings.Add(new MenuSlider("wDelay", "Delay Between Each Trap (ms)",1500, 0, 3000));
            comboSettings.Add(new MenuSlider("wHitChance", "(W) Hit Chance",5, 3, 6));
            comboSettings.Add(new MenuSeparator("infoE", " "));
            comboSettings.Add(new MenuBool("useE", "Use (E)").SetValue(true));
            comboSettings.Add(new MenuSlider("eHitChance", "(E) Hit Chance",5, 3, 6));

            harassSettings = myMenu.Add(new Menu("Harass","Harass Settings")); ;
            harassSettings.Add(new MenuBool("useQ.Harass", "Use (Q)").SetValue(true));
            harassSettings.Add(new MenuSlider("qHitChance.Harass", "(Q) Hit Chance",5, 3, 6));
            harassSettings.Add(new MenuSlider("qMana", "Mana Manger",80, 0, 100));

            ultimateSettings = myMenu.Add(new Menu("R","Ultimate Settings"));
                ultimateSettings.Add(new MenuBool("useR", "Auto Use (R)").SetValue(true));
                ultimateSettings.Add(new MenuBool("rCombo", "Use while Combo").SetValue(true));

            trapSettings = myMenu.Add(new Menu("Trap","Trap Settings"));
            trapSettings.Add(new MenuBool("autoW", "Auto (W)").SetValue(true));

            antiGapcloserSettings = myMenu.Add(new Menu("Anti GapCloser Settings", "AntiGapCloser"));
            antiGapcloserSettings.Add(new MenuBool("AntiGapCloser", "Auto (E) AntiGapCloser").SetValue(true));

            killStealSettings = myMenu.Add(new Menu("KillSteal","KillSteal Settings"));
            killStealSettings.Add(new MenuBool("qKillSteal", "Use (Q)").SetValue(true));
            killStealSettings.Add(new MenuSlider("qKillSteal.Hitchance", "(Q) Hit Chance",3, 3, 6));

            drawingSettings = myMenu.Add(new Menu("Draw","Drawing Settings"));
            drawingSettings.Add(new MenuBool("qRange", "Draw (Q)")); // AliceBlue
            drawingSettings.Add(new MenuBool("wRange", "Draw (W)")); // Aqua
            drawingSettings.Add(new MenuBool("eRange", "Draw (E)")); // Aquamarine

            myMenu.Add(new MenuSeparator("void", " "));
            myMenu.Add(new MenuSeparator("author", "Author: Rambe"));

            myMenu.Attach();
        }
    }
}