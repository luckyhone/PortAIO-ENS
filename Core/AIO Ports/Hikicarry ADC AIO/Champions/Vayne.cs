using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using LeagueSharpCommon;
using SharpDX;
using SPrediction;
using Dash = EnsoulSharp.SDK.Dash;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Utilities = HikiCarry.Core.Utilities.Utilities;

 namespace HikiCarry.Champions
{
    internal class Vayne
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;
        public static long LastCheck;
        public static List<Vector2> Points = new List<Vector2>();

        public Vayne()
        {

            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550f);
            E.SetTargetted(0.25f, 1600f);
            R = new Spell(SpellSlot.R);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));
                comboMenu.Add(new MenuBool("e.combo", "Use (E)", true).SetValue(true));
                comboMenu.Add(new MenuBool("r.combo", "Use (R)", true).SetValue(true));
                comboMenu.Add(new MenuSlider("combo.r.count", "R on x Enemy", 4, 1, 5));
                Initializer.Config.Add(comboMenu);
            }
            
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(true));
                harassMenu.Add(new MenuBool("e.harass", "Use (E)", true).SetValue(false));
                harassMenu.Add(new MenuSlider("harass.mana", "Harass Mana Percent", 30, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var junglemenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                junglemenu.Add(new MenuBool("q.jungle", "Use (Q)", true).SetValue(true));
                junglemenu.Add(new MenuBool("e.jungle", "Use (E)", true).SetValue(true));
                junglemenu.Add(new MenuSlider("jungle.mana", "Jungle Mana Percent", 30, 1, 99));
                Initializer.Config.Add(junglemenu);
            }

            var condemnmenu = new Menu("» Condemn Settings «", "Condemn Settings");
            {
                condemnmenu.Add(new MenuSlider("condemn.distance", "» Condemn Push Distance",410, 350, 420));
                condemnmenu.Add(new MenuSeparator("info.vayne.1", "                       Condemn Whitelist")).SetFontColor(SharpDX.Color.Yellow);
                foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(o => o.IsEnemy))
                {
                    condemnmenu.Add(new MenuBool("condemnset." + enemy.CharacterName,
                        $"Condemn: {enemy.CharacterName}",true).SetValue(true));
                }
                Initializer.Config.Add(condemnmenu);
            }

            Initializer.Config.Add(new MenuSeparator("masterracec0mb0", "                      Masterrace Settings"));
            Initializer.Config.Add(new MenuList("condemn.style", "Condemn Method",new[] { "Shine", "Asuna", "360" }));
            Initializer.Config.Add(new MenuList("condemn.x1", "Condemn Style",new[] { "Only Combo" }));
            Initializer.Config.Add(new MenuList("q.type", "Q Type",new[] { "Cursor Position", "Safe Position" }, 1));
            Initializer.Config.Add(new MenuList("combo.type", "Combo Type",new[] { "Burst", "Normal" }, 1));
            Initializer.Config.Add(new MenuList("harass.type", "Harass Type",new[] { "2 Silver Stack + Q", "2 Silver Stack + E" }));
            Initializer.Config.Add(new MenuSlider("q.stealth", "(Q) Stealth (ms) )",1000, 0, 1000));

            Game.OnUpdate += VayneOnUpdate;
            //Orbwalker.OnBeforeAttack += VayneBeforeAttack;
            AIBaseClient.OnProcessSpellCast += VayneOnSpellCast;
            AntiGapcloser.OnGapcloser += VayneAntiGapcloser;

        }

        private void VayneAntiGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (gapcloser.EndPosition.Distance(ObjectManager.Player.ServerPosition) <= 300 && Utilities.Enabled("anti.gap"))
            {
                E.Cast(gapcloser.EndPosition.Extend(ObjectManager.Player.ServerPosition, ObjectManager.Player.Distance(gapcloser.EndPosition) + E.Range));
            }
        }

        /*private void VayneBeforeAttack(object sender,BeforeAttackEventArgs args)
        {
            if (args.Target.Owner.IsMe && args.Target.IsEnemy && (args.Target as AIHeroClient).HasBuff("vaynetumblefade"))
            {
                var stealthtime = Utilities.Slider("q.stealth");
                var stealthbuff = (args.Target.Owner as AIHeroClient).GetBuff("vaynetumblefade");
                if (stealthbuff.EndTime - Game.Time > stealthbuff.EndTime - stealthbuff.StartTime - stealthtime / 1000)
                {
                    args.Process = false;
                }
            }
            else
            {
                args.Process = true;
            }
        }*/

        private void VayneOnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsMe && ObjectManager.Player.Spellbook.IsAutoAttack &&
                    args.Target.Type == GameObjectType.AIHeroClient)
                {
                    if (Initializer.Config["Combo Settings"]["q.combo"].GetValue<MenuBool>().Enabled && Q.IsReady() &&
                        !args.Target.IsDead &&
                        ((AIHeroClient)args.Target).IsValidTarget(ObjectManager.Player.AttackRange) &&
                        Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                    {
                        QCast(((AIHeroClient)args.Target));
                    }
                }
            }catch(Exception){
                //
            }
        }

        public static bool AsunasAllyFountain(Vector3 position)
        {
            float fountainRange = 750;
            var map = Map.GetMap();
            if (map != null && map.Type == MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => Vector2.Distance(position.ToVector2(), spawnPoint.Position.ToVector2()) < fountainRange);
        }

        public static void SelectedCondemn()
        {
            switch (Initializer.Config["condemn.style"].GetValue<MenuList>().Index)
            {
                case 0:
                    foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(E.Range)))
                    {
                        if (Utilities.Enabled("condemnset." + target.CharacterName))
                        {
                            var pushDistance = Utilities.Slider("condemn.distance");
                            var targetPosition = E.GetPrediction(target).UnitPosition;
                            var pushDirection = (targetPosition - ObjectManager.Player.ServerPosition).Normalized();
                            float checkDistance = pushDistance / 40f;
                            for (int i = 0; i < 40; i++)
                            {
                                Vector3 finalPosition = targetPosition + (pushDirection * checkDistance * i);
                                var collFlags = NavMesh.GetCollisionFlags(finalPosition);
                                if (collFlags.HasFlag(CollisionFlags.Wall) || collFlags.HasFlag(CollisionFlags.Building)) //not sure about building, I think its turrets, nexus etc
                                {
                                    E.Cast(target);
                                }
                            }
                        }

                    }
                    break;
                case 1:
                    foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)))
                    {
                        if (Utilities.Enabled("condemnset." + En.CharacterName))
                        {
                            var EPred = E.GetPrediction(En);
                            int pushDist = Utilities.Slider("condemn.distance");
                            var FinalPosition = EPred.UnitPosition.ToVector2().Extend(ObjectManager.Player.ServerPosition.ToVector2(), -pushDist).ToVector3();

                            for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                            {
                                Vector3 loc3 = EPred.UnitPosition.ToVector2().Extend(ObjectManager.Player.ServerPosition.ToVector2(), -i).ToVector3();

                                if (Vector3Extensions.IsWall(loc3) || AsunasAllyFountain(FinalPosition))
                                    E.Cast(En);
                            }
                        }
                    }
                    break;
                case 2:
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && !x.HasBuffOfType(BuffType.SpellShield) && !x.HasBuffOfType(BuffType.SpellImmunity) &&
                        IsCondemable(x)))
                    {
                        if (Utilities.Enabled("condemnset." + enemy.CharacterName))
                        {
                            E.Cast(enemy);
                        }
                    }
                    break;
            }
        }
        public static void ComboUltimateLogic()
        {
            if (ObjectManager.Player.CountEnemyHeroesInRange(1000) >= Utilities.Slider("combo.r.count"))
            {
                R.Cast();
            }
        }
        public void SilverStackE()
        {
            if (Initializer.Config["harass.type"].GetValue<MenuList>().Index == 1)
            {
                foreach (AIHeroClient qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(550)))
                {
                    if (qTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                    {
                        E.Cast(qTarget);
                    }
                }
            }

        }
        public void SilverStackQ()
        {
            if (Initializer.Config["harass.type"].GetValue<MenuList>().Index == 0)
            {
                foreach (AIHeroClient qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(550)))
                {
                    if (qTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }
            }
        }

        public static void QCast(AIHeroClient enemy)
        {
            switch (Initializer.Config["q.type"].GetValue<MenuList>().Index)
            {
                case 0:
                    Q.Cast(Game.CursorPos);
                    break;
                case 1:
                    Utilities.ECast(enemy,Q);
                    break;
            }
        }

        public static void QComboMethod()
        {
            switch (Initializer.Config["combo.type"].GetValue<MenuList>().Index)
            {
                case 0:
                    foreach (AIHeroClient qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(550)))
                    {
                        if (qTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                        {
                            QCast(qTarget);
                        }
                    }
                    break;
                case 1:
                    foreach (AIHeroClient qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(550)))
                    {
                        QCast(qTarget);
                    }
                    break;
            }
        }
        public static void CondemnJungleMobs()
        {
            foreach (var jungleMobs in ObjectManager.Get<AIMinionClient>().Where(o => o.IsValidTarget(E.Range) && o.Team == GameObjectTeam.Neutral && o.IsVisible && !o.IsDead))
            {
                if (jungleMobs.Name == "SRU_Razorbeak" || jungleMobs.Name == "SRU_Red" ||
                    jungleMobs.Name == "SRU_Blue" || jungleMobs.Name == "SRU_Dragon" ||
                    jungleMobs.Name == "SRU_Krug" || jungleMobs.Name == "SRU_Gromp" ||
                    jungleMobs.Name == "Sru_Crab")
                {
                    var pushDistance = Utilities.Slider("condemn.distance");
                    var targetPosition = E.GetPrediction(jungleMobs).UnitPosition;
                    var pushDirection = (targetPosition - ObjectManager.Player.ServerPosition).Normalized();
                    float checkDistance = pushDistance / 40f;
                    for (int i = 0; i < 40; i++)
                    {
                        Vector3 finalPosition = targetPosition + (pushDirection * checkDistance * i);
                        var collFlags = NavMesh.GetCollisionFlags(finalPosition);
                        if (collFlags.HasFlag(CollisionFlags.Wall) || collFlags.HasFlag(CollisionFlags.Building)) //not sure about building, I think its turrets, nexus etc
                        {
                            E.Cast(jungleMobs);
                        }
                    }
                }
            }
        }
        public static void JungleMobsQ()
        {
            var mob = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);
            if (mob == null || (mob.Count == 0))
                return;
            Q.Cast(Game.CursorPos);
        }
        public static bool IsCondemable(AIHeroClient unit, Vector2 pos = new Vector2())
        {
            if (unit.HasBuffOfType(BuffType.SpellImmunity) || unit.HasBuffOfType(BuffType.SpellShield) || LastCheck + 50 > Environment.TickCount || Dash.IsDashing(ObjectManager.Player)) return false;
            var prediction = E.GetPrediction(unit);
            var predictionsList = pos.IsValid() ? new List<Vector3>() { pos.ToVector3() } : new List<Vector3>
                        {
                            unit.ServerPosition,
                            unit.Position,
                            prediction.CastPosition,
                            prediction.UnitPosition
                        };

            var wallsFound = 0;
            Points = new List<Vector2>();
            foreach (var position in predictionsList)
            {
                for (var i = 0; i < Utilities.Slider("condemn.distance"); i += (int)unit.BoundingRadius) // 420 = push distance
                {
                    var cPos = ObjectManager.Player.Position.Extend(position, ObjectManager.Player.Distance(position) + i).ToVector2();
                    Points.Add(cPos);
                    if (NavMesh.GetCollisionFlags(cPos.ToVector3()).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(cPos.ToVector3()).HasFlag(CollisionFlags.Building))
                    {
                        wallsFound++;
                        break;
                    }
                }
            }
            if ((wallsFound / predictionsList.Count) >= 33 / 100f)
            {
                return true;
            }

            return false;
        }
        private void VayneOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    OnCombo();
                    break;
                case OrbwalkerMode.LaneClear:
                    OnJungle();
                    break;
            }
        }

        private static void OnCombo()
        {
            if (Utilities.Enabled("q.combo") && Q.IsReady())
            {
                QComboMethod();
            }
            if (Utilities.Enabled("e.combo") && E.IsReady())
            {
                SelectedCondemn();
            }
            if (Utilities.Enabled("r.combo") && R.IsReady())
            {
                ComboUltimateLogic();
            }

        }

        private static void OnJungle()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("jungle.mana"))
                return;

            if (Utilities.Enabled("q.jungle") && Q.IsReady())
            {
                JungleMobsQ();
            }
            if (Utilities.Enabled("e.jungle") && E.IsReady())
            {
                CondemnJungleMobs();
            }
        }
    }
}

