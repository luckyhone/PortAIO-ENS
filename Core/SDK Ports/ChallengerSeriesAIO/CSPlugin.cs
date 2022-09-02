using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Color = System.Drawing.Color;

namespace Challenger_Series
{
    public abstract class CSPlugin
    {
        public Menu CrossAssemblySettings;
        public MenuBool DrawEnemyWaypoints;

        public MenuBool IsPerformanceChallengerEnabled;
        public MenuSlider TriggerOnUpdate;

        private int _lastOnUpdateTriggerT = 0;

        public CSPlugin()
        {
            MainMenu = new Menu("challengerseries", ObjectManager.Player.CharacterName + " To The Challenger", true);
            CrossAssemblySettings = MainMenu.Add(new Menu("crossassemblysettings", "Challenger Utils: "));
            DrawEnemyWaypoints =
                CrossAssemblySettings.Add(new MenuBool("drawenemywaypoints", "Draw Enemy Waypoints", true));
            this.IsPerformanceChallengerEnabled =
                this.CrossAssemblySettings.Add(
                    new MenuBool("performancechallengerx", "Use Performance Challenger", false));
            this.TriggerOnUpdate =
                this.CrossAssemblySettings.Add(
                    new MenuSlider("triggeronupdate", "Trigger OnUpdate X times a second", 26, 20, 33));
            Utils.Prediction.PredictionMode =
                this.CrossAssemblySettings.Add(new MenuList("predictiontouse", "Use Prediction: ", new[] {"Common", "SDK"}));
            
            Game.OnUpdate += this.DelayOnUpdate;

            Drawing.OnDraw += args =>
            {
                if (DrawEnemyWaypoints.Enabled)
                {
                    foreach (
                        var e in
                        ValidTargets.Where(
                            en => en.Distance(ObjectManager.Player) < 5000))
                    {
                        var ip = Drawing.WorldToScreen(e.Position); //start pos

                        var wp = e.Path.ToList();
                        var c = wp.Count - 1;
                        if (wp.Count() <= 1) break;

                        var w = Drawing.WorldToScreen(wp[c]); //endpos

                        Drawing.DrawLine(ip.X, ip.Y, w.X, w.Y, 2, Color.Red);
                        Drawing.DrawText(w.X, w.Y, Color.Red, e.Name);
                    }
                }
            };
        }
        
        #region Spells
        public Spell Q { get; set; }
        public Spell Q2 { get; set; }
        public Spell W { get; set; }
        public Spell W2 { get; set; }
        public Spell E { get; set; }
        public Spell E2 { get; set; }
        public Spell R { get; set; }
        public Spell R2 { get; set; }
        #endregion Spells

        public IEnumerable<AIHeroClient> ValidTargets { get {return GameObjects.EnemyHeroes.Where(enemy=>enemy.IsHPBarRendered);}}

        public Menu MainMenu { get; set; }

        public delegate void DelayedOnUpdateEH(EventArgs args);

        public event DelayedOnUpdateEH DelayedOnUpdate;
        
        public void DelayOnUpdate(EventArgs args)
        {
            if (this.DelayedOnUpdate != null)
            {
                if (this.IsPerformanceChallengerEnabled.Enabled)
                {
                    if (Variables.TickCount - this._lastOnUpdateTriggerT > 1000/this.TriggerOnUpdate.Value)
                    {
                        this._lastOnUpdateTriggerT = Variables.TickCount;
                        this.DelayedOnUpdate(args);
                    }
                }
                else
                {
                    this.DelayedOnUpdate(args);
                }
            }
        }

        public virtual void OnUpdate(EventArgs args) { }
        public virtual void OnProcessSpellCast(GameObject sender, AIBaseClientProcessSpellCastEventArgs args) { }
        public virtual void OnDraw(EventArgs args) { }
        public virtual void InitializeMenu() { }
    }
}