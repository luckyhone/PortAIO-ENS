using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace Z.aio.Common
{
    internal abstract class Champion
    {
        public static AIHeroClient Player => ObjectManager.Player;
        internal static Menu ComboMenu { get; set; } = default(Menu);

        internal static Menu FarmMenu { get; set; } = default(Menu);
        internal static Menu EvadeMenu { get; set; }

        internal static Menu KillstealMenu { get; set; } = default(Menu);

        internal static Spell E { get; set; } = default(Spell);

        internal static Menu HarassMenu { get; set; } = default(Menu);

        internal static Menu DrawMenu { get; set; } = default(Menu);
        internal static Menu WhiteList { get; set; } = default(Menu);

        internal static Spell Q { get; set; } = default(Spell);

        internal static Spell R { get; set; } = default(Spell);
        internal static Spell W2 { get; set; } = default(Spell);
        internal static Menu RootMenu { get; set; } = default(Menu);

        internal static Spell W { get; set; } = default(Spell);
        internal static Spell Flash { get; set; } = default(Spell);

        internal void Initiate()
        {
            GameEvent.OnGameLoad += delegate
            {
                this.SetSpells();
                this.SetMenu();
                this.SetEvents();
            };
        }
        
        internal virtual void OnGameUpdate(EventArgs args)
        {
            if (Program.Player.IsDead || Player.IsRecalling()) return;
            Killsteal();
            SemiR();
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.None: break;
                case OrbwalkerMode.Combo:
                    this.Combo();
                    break;
                case OrbwalkerMode.Harass:
                    this.Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    this.Farming();
                    break;
                case OrbwalkerMode.LastHit: break;
            }      
        }


        internal virtual void SetEvents()
        {
            Game.OnUpdate += this.OnGameUpdate;
            EnsoulSharp.Drawing.OnDraw += Drawing;
            Orbwalker.OnAfterAttack += OnAfterAttack;
            Orbwalker.OnBeforeAttack += OnBeforeAttack;
            Spellbook.OnCastSpell += OnCastSpell;
            AIBaseClient.OnDoCast += OnProcessSpellCast;
            AntiGapcloser.OnGapcloser += OnGapcloser;
        }

        private void OnBeforeAttack(object sender, BeforeAttackEventArgs e)
        {
        }

        private void OnAfterAttack(object sender, AfterAttackEventArgs e)
        {
        }

        private void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            
        }


        internal virtual void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs e)
        {

        }

        internal virtual void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {

        }

        public void Drawing(EventArgs args)
        {
            Drawings();
        }


        protected abstract void Combo();

        protected abstract void SemiR();
        protected abstract void Farming();


        protected abstract void Drawings();
        protected abstract void Killsteal();
        protected abstract void Harass();

        protected abstract void SetMenu();

        protected abstract void SetSpells();
    }
}