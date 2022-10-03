namespace Entropy.AIO.Irelia.Logics
{
    #region

    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    static class Lasthit
    {
        public static void ExecuteQ()
        {
            var minion = MinionManager.GetBestLasthitMinion;
            if (minion == null)
            {
                return;
            }

            if (!LaneClearMenu.QBool.Enabled)
            {
                return;
            }

            Q.CastOnUnit(minion);
        }
    }
}