using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using LeagueSharpCommon;
using SPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace ADCCOMMON
{
    using System;
    using System.Linq;

    public static class HumanizerManager
    {
        private static int randomTime, allTime;
        private static bool Enabled;
        private static Random random;
        private static Menu humanizerMenu;

        public static void AddToMenu(Menu mainMenu)
        {
            humanizerMenu = mainMenu;

            humanizerMenu.Add(new MenuBool("EnableHumanizer", "Enabled", true).SetValue(false));
            humanizerMenu.Add(
                new MenuSlider("AttackSpeed", "When Player AttackSpeed >= x(x/100)", 180, 150, 250));
            humanizerMenu.Add(
                new MenuSlider("MinRandomTime", "Min Random Move Time (x*100)", 1, 1, 10));
            humanizerMenu.Add(
                new MenuSlider("MaxRandomTime", "Max Random Move Time (x*100)", 2, 1, 10));

            allTime = Utils.TickCount;

            random = new Random();

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (!humanizerMenu["EnableHumanizer"].GetValue<MenuBool>().Enabled)
            {
                Orbwalker.MoveEnabled = true;
                return;
            }

            if (ObjectManager.Player.CharacterName == "Jhin")
            {
                Orbwalker.MoveEnabled = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name != "JhinRShot";
                return;
            }

            if (ObjectManager.Player.CharacterName == "Draven" || ObjectManager.Player.CharacterName == "Kalista")
            {
                Orbwalker.MoveEnabled = true;
                return;
            }

            var RealAttackSpeed = 1 / ObjectManager.Player.AttackDelay;
            var LimitSpeed = (float)humanizerMenu["AttackSpeed"].GetValue<MenuSlider>().Value / 100;
            var minRandomTime = humanizerMenu["MinRandomTime"].GetValue<MenuSlider>().Value;
            var maxRandomTime = humanizerMenu["MaxRandomTime"].GetValue<MenuSlider>().Value;

            Enabled = RealAttackSpeed >= LimitSpeed;
            randomTime = random.Next(minRandomTime, maxRandomTime) * 100;

            if (Move() && Enabled)
            {
                DelayAction.Add(1000, () => allTime = Utils.TickCount + randomTime);
            }

            Orbwalker.MoveEnabled = Move();
        }

        private static bool Move()
        {
            if (!Enabled)
            {
                return true;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                if (ObjectManager.Player.CountEnemyHeroesInRange(ObjectManager.Player.GetRealAutoAttackRange()) == 0)
                {
                    return true;
                }

                return Utils.TickCount - allTime > 0;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                var haveMinions =
                    MinionManager.GetMinions(ObjectManager.Player.Position,
                        ObjectManager.Player.GetRealAutoAttackRange()).Any();
                var haveMobs =
                    MinionManager.GetMinions(ObjectManager.Player.Position,
                        ObjectManager.Player.GetRealAutoAttackRange(), MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth).Any();
                var haveTurret =
                    ObjectManager.Get<AITurretClient>()
                        .Any(
                            x =>
                                !x.IsDead && x.IsEnemy &&
                                x.Distance(ObjectManager.Player) <=
                                ObjectManager.Player.GetRealAutoAttackRange());

                if (!haveMinions && !haveMobs && !haveTurret)
                {
                    return true;
                }

                return Utils.TickCount - allTime > 0;
            }

            return true;
        }
    }
}
