using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using static StormAIO.MainMenu;

namespace StormAIO.utilities
{
    public class Emote
    {
        public Emote()
        {
            Game.OnNotify += GameOnOnNotify;
        }

        #region Args

        private void GameOnOnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnChampionDie && args.NetworkId == ObjectManager.Player.NetworkId &&
                Emotes.GetValue<MenuBool>("Kill").Enabled) RunEmote();
        }

        #endregion

        #region Functions

        private static void RunEmote()
        {
            var b = Emotes.GetValue<MenuList>("selectitem").SelectedValue;
            switch (b)
            {
                case "Mastery":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Mastery);
                    break;

                case "Center":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Center);
                    break;

                case "South":
                    Game.SendSummonerEmote(SummonerEmoteSlot.South);
                    break;

                case "West":
                    Game.SendSummonerEmote(SummonerEmoteSlot.West);
                    break;

                case "East":
                    Game.SendSummonerEmote(SummonerEmoteSlot.East);
                    break;

                case "North":
                    Game.SendSummonerEmote(SummonerEmoteSlot.North);
                    break;
            }
        }

        #endregion
    }
}