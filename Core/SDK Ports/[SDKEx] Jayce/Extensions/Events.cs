using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Jayce.Extensions
{
    #region

    using System;
    using System.Linq;

    using Jayce.Extensions;

    using static Config;

    using Jayce.Modes;


    using static Other;

    using SharpDX;

    using static Spells;

    #endregion

    /// <summary>
    ///     The events.
    /// </summary>
    internal class Events
    {
        #region Static Fields

        /// <summary>
        /// The indicator.
        /// </summary>
        private static readonly DamageBar Indicator = new DamageBar();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The initialize.
        /// </summary>
        public static void Initialize()
        {
            Game.OnUpdate += OnUpdate;
            AIBaseClient.OnDoCast += ProcessSpell;
            AIBaseClient.OnDoCast += OnSpellCast;
            Orbwalker.OnAfterAttack += OnAction;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnGapcloser += GapCloser;
            EnsoulSharp.SDK.Interrupter.OnInterrupterSpell += Interrupter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The gap closer.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void GapCloser(object sender, AntiGapcloser.GapcloserArgs args)
        {
            if (AGCM.Enabled)
                if (RangeForm())
                {
                    if (R.IsReady() && (HammerQ_CD_R == 0)) R.Cast();
                }
                else
                {
                    if (args.Target.IsValidTarget(E1.Range) && E1.IsReady()) E.Cast(args.Target);
                }
        }

        /// <summary>
        /// The interrupter.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Interrupter(object sender, Interrupter.InterruptSpellArgs args)
        {
            if (AGCM.Enabled)
                if (RangeForm())
                {
                    if (R.IsReady() && (HammerQ_CD_R == 0)) R.Cast();
                }
                else
                {
                    if (args.Sender.IsValidTarget(E1.Range) && E1.IsReady()) E.Cast(args.Sender);
                }
        }

        /// <summary>
        /// The on action.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void OnAction(object sender, AfterAttackEventArgs args)
        {
            if ((Orbwalker.ActiveMode == OrbwalkerMode.Combo)) if (RangeForm() && args.Target.IsEnemy) if (W.IsReady() && ComboCannonW.Enabled) W.Cast();
        }

        /// <summary>
        /// The on do cast.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (args.Slot == SpellSlot.Q))
                if (E.IsReady() && RangeForm())
                {
                    var Enemies = GameObjects.EnemyHeroes.Where(x => (x != null) && x.IsValidTarget(QE.Range));
                    GatePos = ObjectManager.Player.ServerPosition.Extend(args.To, 130 + Game.Ping / 2);
                    switch (Orbwalker.ActiveMode)
                    {
                        case OrbwalkerMode.Combo:
                            if (ComboCannonE.Enabled) E.Cast(GatePos);
                            break;
                        case OrbwalkerMode.Harass:
                            if (HarassCannonE.Enabled) E.Cast(GatePos);
                            break;
                        case OrbwalkerMode.LaneClear:
                            if (LaneCannonE.Enabled) E.Cast(GatePos);
                            break;
                    }

                    foreach (var Enemy in Enemies.Where(x => CannonQEDmg(x) > x.Health)) if (CannonEKS.Enabled) E.Cast(GatePos);
                }
        }

        /// <summary>
        /// The on draw.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void OnDraw(EventArgs args)
        {
            foreach (var Enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(2000)))
                if (DrawDmg.Enabled)
                {
                    Indicator.Unit = Enemy;
                    Indicator.DrawDmg((float)ComboDamage(Enemy), Color.DarkRed);
                }

            if (RangeForm())
            {
                if (CannonQERange.Enabled && QE.IsReady() && E.IsReady()) Render.Circle.DrawCircle(ObjectManager.Player.Position, QE.Range, System.Drawing.Color.Aqua, 2);
                if (CannonQRange.Enabled && Q.IsReady()) Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 2);
            }
            else
            {
                if (HammerQRange.Enabled && Q1.IsReady()) Render.Circle.DrawCircle(ObjectManager.Player.Position, Q1.Range, System.Drawing.Color.IndianRed, 2);
            }
        }

        /// <summary>
        /// The on update.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo.Execute();
                    break;
                case OrbwalkerMode.Harass:
                    Harass.Execute();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear.Execute();
                    JungleClear.Execute();
                    break;
            }

            KillSteal.Execute();
            CD();
            SkinChanger();
        }

        /// <summary>
        /// The process spell.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void ProcessSpell(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (args.SData.Name.ToLower() == "jayceshockblast") && RangeForm())
                if (E.IsReady())
                {
                    var Enemies = GameObjects.EnemyHeroes.Where(x => (x != null) && x.IsValidTarget(QE.Range));
                    GatePos = ObjectManager.Player.ServerPosition.Extend(args.To, 130 + Game.Ping / 2);
                    switch (Orbwalker.ActiveMode)
                    {
                        case OrbwalkerMode.Combo:
                            if (ComboCannonE.Enabled) E.Cast(GatePos);
                            break;
                        case OrbwalkerMode.Harass:
                            if (HarassCannonE.Enabled) E.Cast(GatePos);
                            break;
                        case OrbwalkerMode.LaneClear:
                            if (LaneCannonE.Enabled) E.Cast(GatePos);
                            break;
                    }

                    foreach (var Enemy in Enemies.Where(x => CannonQEDmg(x) > x.Health)) if (CannonEKS.Enabled) E.Cast(GatePos);
                }

            if (args.SData.Name == "JayceToTheSkies") HammerQ_CD = Game.Time + RealCD(HammerQ_TrueCD[Q1.Level - 1]);
            if (args.SData.Name == "JayceStaticField") HammerW_CD = Game.Time + RealCD(HammerW_TrueCD[W1.Level - 1]);
            if (args.SData.Name == "JayceThunderingBlow") HammerE_CD = Game.Time + RealCD(HammerE_TrueCD[E1.Level - 1]);

            if (args.SData.Name.ToLower() == "jayceshockblast") CannonQ_CD = Game.Time + RealCD(CannonQ_TrueCD[Q.Level - 1]);
            if (args.SData.Name.ToLower() == "jaycehypercharge") CannonW_CD = Game.Time + RealCD(CannonW_TrueCD[W.Level - 1]);
            if (args.SData.Name.ToLower() == "jayceaccelerationgate") CannonE_CD = Game.Time + RealCD(CannonE_TrueCD[E.Level - 1]);
        }

        #endregion
    }
}