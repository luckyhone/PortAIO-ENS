using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using SPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = LeagueSharpCommon.Render;

namespace hikiMarksmanRework.Champions
{
    public class Graves
    {
        public Graves()
        {
            GravesOnLoad();
        }
        private static readonly Render.Sprite HikiSprite = new Render.Sprite(PortAIO.Properties.Resources.logo, new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
        private static void GravesOnLoad()
        {
            GravesMenu.Config =
                new Menu("hikiMarksman:AIO - Graves", "hikiMarksman:AIO - Graves", true);
            {
                GravesSpells.Init();
                GravesMenu.MenuInit();
            }
            GravesMenu.Config.SetFontColor(Color.Gold);

            Game.Print("<font color='#ff3232'>hikiMarksman:AIO - Graves: </font><font color='#00FF00'>loaded! You can rekt everyone with this assembly</font>", ObjectManager.Player.CharacterName);
            Game.Print(string.Format("<font color='#ff3232'>hikiMarksman:AIO - </font><font color='#00FF00'>Assembly Version: </font><font color='#ff3232'><b>{0}</b></font> ", typeof(Program).Assembly.GetName().Version));
            Game.Print("<font color='#ff3232'>If you like this assembly feel free to upvote on Assembly Database</font>");


            HikiSprite.Add(0);
            HikiSprite.OnDraw();
            DelayAction.Add(8000, () => HikiSprite.Remove());

            /*Notification notif = new Notification("hikiMarksman:AIO",
                ObjectManager.Player.CharacterName+" - Loaded!");*/

            Game.OnUpdate += GravesOnUpdate;
            AIBaseClient.OnProcessSpellCast += GravesOnProcessSpellCast;
            Orbwalker.OnAfterAttack += AfterAttack;
            Drawing.OnDraw += GravesOnDraw;
        }

        private static void GravesOnDraw(EventArgs args)
        {
            GravesDrawing.Init();
        }

        private static void AfterAttack(object unit, AfterAttackEventArgs target)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && Helper.Enabled("graves.e.combo") && GravesSpells.E.IsReady()
                && target.Target.IsValidTarget(GravesSpells.Q.Range))
            {
                GravesSpells.E.Cast(Game.CursorPos);
            }
        }

        private static void GravesOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (GravesMenu.Config["Miscellaneous"]["Anti-Gapclose Settings"]["graves.e.gapclosex"].GetValue<MenuList>().Index == 0)
            {
                Helper.GravesAntiGapcloser(sender, args);
            }
        }
        private static void GravesOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    Jungle();
                    break;
            }
            if (GravesMenu.Config["Harass Settings"]["graves.q.harass"].GetValue<MenuBool>().Enabled && GravesSpells.Q.IsReady() && ObjectManager.Player.ManaPercent > GravesMenu.Config["Harass Settings"]["graves.harass.mana"].GetValue<MenuSlider>().Value)
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(GravesSpells.Q.Range) && GravesMenu.Config["Harass Settings"]["Q Toggle"]["graves.q.toggle."+x.CharacterName].GetValue<MenuBool>().Enabled &&
                    GravesSpells.Q.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    GravesSpells.Q.Cast(enemy);
                }
            }
        }

        private static void Combo()
        {
            if (GravesSpells.Q.IsReady() && Helper.Enabled("graves.q.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(GravesSpells.Q.Range) && 
                    GravesSpells.Q.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    GravesSpells.Q.Cast(enemy);
                }
            }
            if (GravesSpells.W.IsReady() && Helper.Enabled("graves.w.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(GravesSpells.W.Range) &&
                    GravesSpells.W.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    GravesSpells.W.Cast(enemy);
                }
            }
            if (GravesSpells.R.IsReady() && Helper.Enabled("graves.r.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(GravesSpells.R.Range) &&
                    GravesSpells.R.GetPrediction(x).Hitchance >= HitChance.High && GravesSpells.R.GetDamage(x) > x.Health))
                {
                    GravesSpells.R.Cast(enemy);
                }
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Helper.Slider("graves.harass.mana"))
            {
                return;
            }
            if (GravesSpells.Q.IsReady() && Helper.Enabled("graves.q.harass"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(GravesSpells.Q.Range) &&
                    GravesSpells.Q.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    GravesSpells.Q.Cast(enemy);
                }
            }
            if (GravesSpells.W.IsReady() && Helper.Enabled("graves.w.harass"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(GravesSpells.W.Range) &&
                    GravesSpells.W.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    GravesSpells.W.Cast(enemy);
                }
            }
        }

        private static void Clear()
        {
            if (ObjectManager.Player.ManaPercent < Helper.Slider("graves.clear.mana"))
            {
                return;
            }

            if (GravesSpells.Q.IsReady() && Helper.Enabled("graves.q.clear") && MinionManager.GetMinions(ObjectManager.Player.Position, GravesSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly).Count >= Helper.Slider("graves.q.minion.hit.count")
                && GravesSpells.Q.GetLineFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, GravesSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).MinionsHit >= Helper.Slider("graves.q.minion.hit.count"))
            {
                GravesSpells.Q.Cast(GravesSpells.Q.GetLineFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, GravesSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).Position);
            }

        }

        private static void Jungle()
        {
            if (ObjectManager.Player.ManaPercent < Helper.Slider("graves.jungle.mana") && MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth) == null ||
                MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth).Count == 0)
            {
                return;
            }
            if (GravesSpells.Q.IsReady() && Helper.Enabled("graves.q.jungle"))
            {
                GravesSpells.Q.Cast(
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)[0]);
            }
            if (GravesSpells.W.IsReady() && Helper.Enabled("graves.w.jungle"))
            {
                GravesSpells.W.Cast(
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)[0].Position);
            }
        }
    }
}
