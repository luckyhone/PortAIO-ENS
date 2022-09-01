using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace BadaoSeries.CustomOrbwalker
{
    internal static class FakeClicks
    {
        #region Static Fields

        public static bool Enabled
        {
            get { return root.GetValue<MenuBool>("Enable").Enabled; }
        }
        /// <summary>
        ///     If the user is attacking
        ///     Currently used for the second style of fake clicks
        /// </summary>
        private static bool attacking;

        /// <summary>
        ///     The delta t for click frequency
        /// </summary>
        private static readonly float deltaT = .2f;

        /// <summary>
        ///     The last direction of the player
        /// </summary>
        private static Vector3 direction;

        /// <summary>
        ///     The last endpoint the player was moving to.
        /// </summary>
        private static Vector3 lastEndpoint;

        /// <summary>
        ///     The last order the player had.
        /// </summary>
        private static GameObjectOrder lastOrder;

        /// <summary>
        ///     The time of the last order the player had.
        /// </summary>
        private static float lastOrderTime;

        /// <summary>
        ///     The last time a click was done.
        /// </summary>
        private static float lastTime;

        /// <summary>
        ///     The Player.
        /// </summary>
        private static AIHeroClient player;

        /// <summary>
        ///     The Random number generator
        /// </summary>
        private static readonly Random r = new Random();

        /// <summary>
        ///     The root menu.
        /// </summary>
        private static readonly Menu root = new Menu("FakeClicks", "Fake Clicks");

        #endregion

        #region Methods

        /// <summary>
        ///     The move fake click after attacking
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <param name="target">
        ///     The target.
        /// </param>
        private static void AfterAttack(object sender, AfterAttackEventArgs args)
        {
            attacking = false;
            var t = args.Target as AIHeroClient;
            if (t != null && args.Target.IsMe)
            {
                ShowClick(RandomizePosition(t.Position), ClickType.Move);
            }
        }

        /// <summary>
        ///     The before attack fake click.
        ///     Currently used for the second style of fake clicks
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void BeforeAttackFake(object sender, BeforeAttackEventArgs args)
        {
            if (root.GetValue<MenuList>("Click Mode").Index == 1)
            {
                ShowClick(RandomizePosition(args.Target.Position), ClickType.Attack);
                attacking = true;
            }
        }

        /// <summary>
        ///     The fake click before you cast a spell
        /// </summary>
        /// <param name="s">
        ///     The Spell Book.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void BeforeSpellCast(Spellbook s, SpellbookCastSpellEventArgs args)
        {
            var target = args.Target;

            if (target == null)
            {
                return;
            }

            if (target.Position.Distance(player.Position) >= 5f)
            {
                ShowClick(args.Target.Position, ClickType.Attack);
            }
        }

        /// <summary>
        ///     The on new path fake.
        ///     Currently used for the second style of fake clicks
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void DrawFake(AIBaseClient sender, AIBaseClientNewPathEventArgs args)
        {
            if (sender.IsMe && lastTime + deltaT < Game.Time && args.Path.LastOrDefault() != lastEndpoint &&
                args.Path.LastOrDefault().Distance(player.Position) >= 5f &&
                root.GetValue<MenuBool>("Enable").Enabled &&
                root.GetValue<MenuList>("Click Mode").Index == 1)
            {
                lastEndpoint = args.Path.LastOrDefault();
                if (!attacking)
                {
                    ShowClick(Game.CursorPos, ClickType.Move);
                }
                else
                {
                    ShowClick(Game.CursorPos, ClickType.Attack);
                }

                lastTime = Game.Time;
            }
        }

        /// <summary>
        ///     The OnIssueOrder event delegate.
        ///     Currently used for the first style of fake clicks
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void OnIssueOrder(AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
        {
            if (sender.IsMe &&
                (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.AttackUnit ||
                 args.Order == GameObjectOrder.AttackUnit) &&
                lastOrderTime + r.NextFloat(deltaT, deltaT + .2f) < Game.Time &&
                root.GetValue<MenuBool>("Enable").Enabled &&
                root.GetValue<MenuList>("Click Mode").Index == 0)
            {
                var vect = args.TargetPosition;
                vect.Z = player.Position.Z;
                if (args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackUnit)
                {
                    ShowClick(RandomizePosition(vect), ClickType.Attack);
                }
                else
                {
                    ShowClick(vect, ClickType.Move);
                }

                lastOrderTime = Game.Time;
            }
        }

        public static void Initiate()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            root.Add(new MenuBool("Enable", "Enable").SetValue(true));
            root.Add(new MenuList("Click Mode", "Click Mode",
                new[] {"Evade, No Cursor Position", "Cursor Position, No Evade"}));

            BadaoSeries.Program.MainMenu.Add(root);

            player = ObjectManager.Player;

            AIBaseClient.OnNewPath += DrawFake;
            Orbwalker.OnBeforeAttack += BeforeAttackFake;
            Spellbook.OnCastSpell += BeforeSpellCast;
            Orbwalker.OnAfterAttack += AfterAttack;
            AIBaseClient.OnIssueOrder += OnIssueOrder;
        }
        

        private static void ShowClick(Vector3 position, ClickType type)
        {
            if (!Enabled)
            {
                return;
            }

            Hud.ShowClick(type, position);
        }

        /// <summary>
        ///     The RandomizePosition function to randomize click location.
        /// </summary>
        /// <param name="input">
        ///     The input Vector3.
        /// </param>
        /// <returns>
        ///     A Vector within 100 units of the unit
        /// </returns>
        private static Vector3 RandomizePosition(Vector3 input)
        {
            if (r.Next(2) == 0)
            {
                input.X += r.Next(100);
            }
            else
            {
                input.Y += r.Next(100);
            }

            return input;
        }

        #endregion
    }
}