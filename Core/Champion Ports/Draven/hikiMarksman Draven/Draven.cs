using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using hikiMarksmanRework.Core.Drawings;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;
using SharpDX;
using SharpDX.Direct3D9;
using SPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = EnsoulSharp.SDK.Render;

namespace hikiMarksmanRework.Champions
{

    class Draven
    {
        public Draven()
        {
            DravenOnLoad();
        }

        private static readonly LeagueSharpCommon.Render.Sprite HikiSprite = new LeagueSharpCommon.Render.Sprite(PortAIO.Properties.Resources.logo, new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
        private static void DravenOnLoad()
        {
            DravenMenu.Config =
                new Menu("hikiMarksman:AIO - Draven", "hikiMarksman:AIO - Draven", true);
            {
                DravenSpells.Init();
                DravenMenu.MenuInit();
            }
            DravenMenu.Config.SetFontColor(Color.Gold);

            Game.Print("<font color='#ff3232'>hikiMarksman:AIO - Draven: </font><font color='#00FF00'>loaded! You can rekt everyone with this assembly</font>", ObjectManager.Player.ChampionsKilled);
            Game.Print("<font color='#ff3232'>If you like this assembly feel free to upvote on Assembly Database</font>");
            
            HikiSprite.Layer = 0;
            //HikiSprite.Draw();
            DelayAction.Add(8000, () => HikiSprite.Dispose());

            Game.OnUpdate += DravenOnUpdate;
            AIBaseClient.OnProcessSpellCast += DravenAxeHelper.AIHeroClient_OnProcessSpellCast;
            GameObject.OnCreate += DravenAxeHelper.OnCreate;
            GameObject.OnDelete += DravenAxeHelper.OnDelete;
            Interrupter.OnInterrupterSpell += DravenOnInterruptableTarget;
            AntiGapcloser.OnGapcloser += DravenOnEnemyGapcloser;
            Drawing.OnDraw += DravenOnDraw;
        }

        private static void DravenOnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (DravenSpells.E.IsReady() && sender.IsValidTarget(DravenSpells.E.Range) && Helper.DEnabled("draven.e.antigapcloser"))
            {
                DravenSpells.E.Cast(sender);
            }
        }

        private static void DravenOnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!DravenMenu.Config["draven.e.interrupter"].GetValue<MenuBool>().Enabled || !sender.IsValidTarget()) return;
            Interrupter.DangerLevel a;
            switch (DravenMenu.Config["min.interrupter.danger.level"].GetValue<MenuList>().SelectedValue)
            {
                case "HIGH":
                    a = Interrupter.DangerLevel.High;
                    break;
                case "MEDIUM":
                    a = Interrupter.DangerLevel.Medium;
                    break;
                default:
                    a = Interrupter.DangerLevel.Low;
                    break;
            }

            if (args.DangerLevel == Interrupter.DangerLevel.High ||
                args.DangerLevel == Interrupter.DangerLevel.Medium && a != Interrupter.DangerLevel.High ||
                args.DangerLevel == Interrupter.DangerLevel.Medium && a != Interrupter.DangerLevel.Medium &&
                a != Interrupter.DangerLevel.High)
            {
                if (DravenSpells.E.IsReady() && sender.IsValidTarget(DravenSpells.E.Range))
                {
                    DravenSpells.E.Cast(sender);
                }
            }
        }

        private static void DravenOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    Jungle();
                    break;
            }
        }



        private static void Combo()
        {
            if (DravenSpells.Q.IsReady() && Helper.DEnabled("draven.q.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player))
                    && DravenAxeHelper.LastQ + 100 < Environment.TickCount && DravenAxeHelper.CurrentAxes < Helper.DSlider("draven.q.combo.axe.count")))
                {
                    DravenSpells.Q.Cast();
                }
            }
            if (DravenSpells.E.IsReady() && Helper.DEnabled("draven.e.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(DravenSpells.E.Range) &&
                    DravenSpells.E.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    DravenSpells.E.Cast(enemy);
                }
            }
            if (DravenSpells.R.IsReady() && Helper.DEnabled("draven.r.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(3000) && ObjectManager.Player.Distance(o.Position) > Helper.DSlider("draven.min.ult.distance") &&
                    ObjectManager.Player.Distance(o.Position) < Helper.DSlider("draven.max.ult.distance") && DravenSpells.R.GetPrediction(o).Hitchance >= HitChance.Medium &&
                    o.Health < DravenSpells.R.GetDamage(o)))
                {
                    DravenSpells.R.Cast(enemy);
                }
            }
        }
        private static void Clear()
        {
            try
            {
                if (ObjectManager.Player.ManaPercent < Helper.DSlider("draven.clear.mana"))
                {
                    return;
                }

                if (DravenSpells.Q.IsReady() &&
                    DravenMenu.Config["Clear Settings"]["draven.q.clear"].GetValue<MenuBool>().Enabled)
                {
                    var minions = MinionManager.GetMinions(ObjectManager.Player.Position, EzrealSpells.Q.Range,
                        MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);
                    if (minions.Count == 0) return;
                    if (minions.Count > DravenMenu.Config["Clear Settings"]["draven.q.minion.count"]
                            .GetValue<MenuSlider>().Value &&
                        DravenAxeHelper.CurrentAxes <
                        DravenMenu.Config["Clear Settings"]["draven.q.lane.clear.axe.count"].GetValue<MenuSlider>()
                            .Value)
                    {
                        DravenSpells.Q.Cast();
                    }
                }
            }
            catch (Exception e)
            {
                //
            }
        }
        private static void Jungle()
        {
            if (ObjectManager.Player.ManaPercent < Helper.DSlider("draven.jungle.mana") && MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth) == null ||
                 MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth).Count == 0)
            {
                return;
            }
            if (DravenSpells.Q.IsReady() && DravenAxeHelper.CurrentAxes < Helper.DSlider("draven.q.jungle.clear.axe.count") && Helper.DEnabled("draven.q.jungle"))
            {
                DravenSpells.Q.Cast();
            }
            if (DravenSpells.E.IsReady() && Helper.DEnabled("draven.e.jungle"))
            {
                DravenSpells.E.Cast(MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)[0]);
            }

        }
        private static void DravenOnDraw(EventArgs args)
        {
            DravenDrawing.Init();
        }
    }
}
