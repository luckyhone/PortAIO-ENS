using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Drawings;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;
using SharpDX;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = LeagueSharpCommon.Render;

namespace hikiMarksmanRework.Champions
{
    class Vayne
    {
        public Vayne()
        {
            VayneOnLoad();
        }
        private static readonly Render.Sprite HikiSprite = new Render.Sprite(PortAIO.Properties.Resources.logo, new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
        private static void VayneOnLoad()
        {
            VayneMenu.Config =
                new Menu("hikiMarksman:AIO - Vayne", "hikiMarksman:AIO - Vayne", true);
            {
                VayneSpells.Init();
                VayneMenu.MenuInit();
            }
            VayneMenu.Config.SetFontColor(Color.Gold);

            Game.Print("<font color='#ff3232'>hikiMarksman:AIO - Lucian: </font><font color='#00FF00'>loaded! You can rekt everyone with this assembly</font>", ObjectManager.Player.CharacterName);
            Game.Print(string.Format("<font color='#ff3232'>hikiMarksman:AIO - </font><font color='#00FF00'>Assembly Version: </font><font color='#ff3232'><b>{0}</b></font> ", typeof(Program).Assembly.GetName().Version));
            Game.Print("<font color='#ff3232'>If you like this assembly feel free to upvote on Assembly Database</font>");
            

            Game.OnUpdate += VayneOnUpdate;
            AIHeroClient.OnDoCast += VayneOnProcessSpellCast;
            AIBaseClient.OnProcessSpellCast += VayneOnSpellCast;
            Drawing.OnDraw += VayneOnDraw;
        }

        private static void VayneOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    break; 
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
            }
        }

        private static void Clear()
        {
            if (VayneSpells.E.IsReady() && Helper.VEnabled("vayne.condemn.jungle.mobs"))
            {
                foreach (var junglemobs in ObjectManager.Get<AIMinionClient>().Where(x=> x.IsValidTarget(VayneSpells.E.Range) && x.Team == GameObjectTeam.Neutral &&
                    (x.Name == "SRU_Razorbeak" || x.Name == "SRU_Red" ||
                     x.Name == "SRU_Blue" || x.Name == "SRU_Gromp" ||
                     x.Name == "SRU_Krug" || x.Name == "SRU_Murkwolf" ||
                     x.Name == "Sru_Crab")))
                {
                    VayneHelper.VhrBasicJungleCondemn(junglemobs);
                }
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Helper.VSlider("vayne.harass.mana"))
            {
                return;
            }

            if (VayneMenu.Config["harass.type"].GetValue<MenuList>().Index == 0 && VayneSpells.Q.IsReady())
            {
                foreach (var qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(ObjectManager.Player.AttackRange)
                    && x.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2)))
                {
                    VayneHelper.TumbleCast();
                }
            }
            if (VayneMenu.Config["harass.type"].GetValue<MenuList>().Index == 1 && VayneSpells.E.IsReady())
            {
                foreach (var etarget in HeroManager.Enemies.Where(x => x.IsValidTarget(ObjectManager.Player.AttackRange)
                    && x.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2)))
                {
                    VayneSpells.E.CastOnUnit(etarget);
                }
            }
        }

        private static void Combo()
        {
            if (VayneSpells.E.IsReady() && Helper.VEnabled("vayne.e.combo"))
            {
                VayneHelper.CondemnCast();
            }

            if (VayneSpells.R.IsReady() && Helper.VEnabled("vayne.r.combo") 
                && ObjectManager.Player.CountEnemyHeroesInRange(Helper.VSlider("vayne.auto.r.search.range")) >= Helper.VSlider("vayne.auto.r.enemy.count")
                && ObjectManager.Player.HealthPercent <= Helper.VSlider("vayne.auto.r.minimum.health"))
            {
                VayneSpells.R.Cast();
            }
        }

        private static void VayneOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (VayneSpells.E.IsReady())
            {
                Helper.VayneAntiGapcloser(sender,args);
            }
        }

        private static void VayneOnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Orbwalker.IsAutoAttack(args.SData.Name) && args.Target is AIHeroClient && args.Target.IsValid 
                && Orbwalker.ActiveMode == OrbwalkerMode.Combo && Helper.VEnabled("vayne.q.combo")
                && Helper.VEnabled("vayne.q.after.aa"))
            {
                VayneHelper.TumbleCast();
            }
            if (sender.IsMe && Orbwalker.IsAutoAttack(args.SData.Name) && args.Target is AIHeroClient && args.Target.IsValid
                && Orbwalker.ActiveMode == OrbwalkerMode.Combo && Helper.VEnabled("vayne.q.combo")
                && Helper.VEnabled("vayne.auto.q.if.enemy.has.2.stack") && ((AIHeroClient)args.Target).GetBuffCount("vaynesilvereddebuff") == 2)
            {
                VayneHelper.TumbleCast();
            }

            if (sender.IsMe && Orbwalker.IsAutoAttack(args.SData.Name) && args.Target is AIMinionClient && args.Target.IsValid &&
                Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && Helper.VEnabled("vayne.q.combo")
                && Helper.VEnabled("vayne.tumble.jungle.mobs") && ((AIMinionClient)args.Target).Team == GameObjectTeam.Neutral
                && (((AIMinionClient)args.Target).Name == "SRU_Razorbeak" ||((AIMinionClient)args.Target).Name == "SRU_Red" ||
                ((AIMinionClient)args.Target).Name == "SRU_Blue" || ((AIMinionClient)args.Target).Name == "SRU_Gromp" ||
                ((AIMinionClient)args.Target).Name == "SRU_Krug" || ((AIMinionClient)args.Target).Name == "SRU_Murkwolf" ||
                ((AIMinionClient)args.Target).Name == "Sru_Crab"))
            {
                VayneHelper.TumbleCast();
            }

        }

        private static void VayneOnDraw(EventArgs args)
        {
            VayneDrawing.Init();
        }
        
    }
}