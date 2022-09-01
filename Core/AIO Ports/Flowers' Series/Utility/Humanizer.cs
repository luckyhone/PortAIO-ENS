 /*using EnsoulSharp;
 using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using EnsoulSharp.SDK.Utility;
 using LeagueSharpCommon;
 using SPrediction;
 using Menu = EnsoulSharp.SDK.MenuUI.Menu;

 namespace Flowers_ADC_Series.Utility
{
    using System;
    using System.Linq;

    internal class Humanizer : Logic
    {
        private static int randomTime, allTime;
        private static bool Enabled;
        private static Random random;
        private static readonly Menu Menu = Utilitymenu;

        internal static void Init()
        {
            var HumanizerMenu = Menu.Add(new Menu("Humanizer", "Humanizer"));
            {
                HumanizerMenu.Add(new MenuBool("EnableHumanizer", "Enabled", true).SetValue(false));
                HumanizerMenu.Add(
                    new MenuSlider("AttackSpeed", "When Player AttackSpeed >= x(x/100)", 180, 150, 250));
                HumanizerMenu.Add(
                    new MenuSlider("MinRandomTime", "Min Random Move Time (x*100)", 1, 1, 10));
                HumanizerMenu.Add(
                    new MenuSlider("MaxRandomTime", "Max Random Move Time (x*100)", 2, 1, 10));
            }
            allTime = Utils.TickCount;

            random = new Random();

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (!Menu["Humanizer"]["EnableHumanizer"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            if (Me.CharacterName == "Jhin")
            {
                Orbwalker.MoveEnabled = (Me.Spellbook.GetSpell(SpellSlot.R).Name != "JhinRShot");
                return;
            }

            if (Me.CharacterName == "Draven" || Me.CharacterName == "Kalista")
            {
                Orbwalker.MoveEnabled =(true);
                return;
            }

            var RealAttackSpeed = 1/Me.AttackDelay;
            var LimitSpeed = (float)Menu["Humanizer"]["AttackSpeed"].GetValue<MenuSlider>().Value/100;
            var minRandomTime = Menu["Humanizer"]["MinRandomTime"].GetValue<MenuSlider>().Value;
            var maxRandomTime = Menu["Humanizer"]["MaxRandomTime"].GetValue<MenuSlider>().Value;

            Enabled = RealAttackSpeed >= LimitSpeed;
            randomTime = random.Next(minRandomTime, maxRandomTime)*100;

            if (Move() && Enabled)
            {
                DelayAction.Add(1000,() => allTime = Utils.TickCount + randomTime);
            }

            Orbwalker.MoveEnabled = (Move());
        }

        private static bool Move()
        {
            if (!Enabled)
            {
                return true;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (Me.CountEnemyHeroesInRange(Me.GetRealAutoAttackRange()) == 0)
                    {
                        return true;
                    }

                    return Utils.TickCount - allTime > 0;
                case OrbwalkerMode.LaneClear:
                    var haveMinions = MinionManager.GetMinions(Me.Position, Me.GetRealAutoAttackRange()).Any();
                    var haveMobs =
                        MinionManager.GetMinions(Me.Position, Me.GetRealAutoAttackRange(), MinionManager.MinionTypes.All,
                            MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth).Any();
                    var haveTurret =
                        ObjectManager.Get<AITurretClient>()
                            .Any(x => !x.IsDead && x.IsEnemy && x.Distance(Me) <= Me.GetRealAutoAttackRange());
                    

                    if (!haveMinions && !haveMobs && !haveTurret)
                    {
                        return true;
                    }

                    return Utils.TickCount - allTime > 0;
            }

            return true;
        }
    }
}*/