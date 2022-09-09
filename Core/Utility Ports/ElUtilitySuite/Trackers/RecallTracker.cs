using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using GameObjects = ElUtilitySuite.Vendor.SFX.GameObjects;

namespace ElUtilitySuite.Trackers
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;


    using SharpDX;
    using SharpDX.Direct3D9;

    using Color = System.Drawing.Color;
    using Font = SharpDX.Direct3D9.Font;

    #endregion

    internal class RecallTracker : IPlugin
    {
        #region Constants

        private const int BarHeight = 10;

        #endregion

        #region Fields

        public List<EnemyInfo> EnemyInfo = new List<EnemyInfo>();

        private readonly int SeperatorHeight = 5;

        private MapType Map;

        #endregion

        #region Public Properties

        public Menu Menu { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        private int BarWidth => (int)(Drawing.Width - 2 * this.BarX);

        /// <summary>
        /// </summary>
        private int BarX => (int)(Drawing.Width * 0.425f);

        /// <summary>
        /// </summary>
        private int BarY
            => (int)(Drawing.Height - 150f - this.Menu["RecallTracker.OffsetBottom"].GetValue<MenuSlider>().Value);

        private List<AIHeroClient> Enemies { get; set; }

        private List<AIHeroClient> Heroes { get; set; }

        /// <summary>
        /// </summary>
        private float Scale => (float)this.BarWidth / 8000;

        private Font Text { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        /// <returns></returns>
        public void CreateMenu(Menu rootMenu)
        {
            var predicate = new Func<Menu, bool>(x => x.Name == "Trackers");
            var menu = rootMenu.Components.All(x => x.Key != "Trackers")
                ? rootMenu.Add(new Menu("Trackers", "Trackers"))
                : rootMenu["Trackers"].Parent;

            var notificationsMenu = menu.Add(new Menu("Recall tracker", "Recall tracker"));
            {
                notificationsMenu.Add(new MenuBool("showRecalls", "Show Recalls").SetValue(true));
                notificationsMenu.Add(new MenuBool("notifRecFinished", "Recall finished").SetValue(true));
                notificationsMenu.Add(new MenuBool("notifRecAborted", "Recall aborted").SetValue(true));
                notificationsMenu.Add(
                    new MenuSlider("RecallTracker.OffsetBottom", "Offset bottom",52, 0, 1500));
                notificationsMenu.Add(
                    new MenuSlider("RecallTracker.FontSize", "Font size",13, 13, 30));
            }

            this.Menu = menu;
        }

        public void Load()
        {
            this.Heroes = ObjectManager.Get<AIHeroClient>().ToList();
            this.Enemies = GameObjects.Heroes.ToList();

            this.EnemyInfo = this.Enemies.Select(x => new EnemyInfo(x)).ToList();

            this.Map = EnsoulSharp.SDK.Utility.Map.GetMap().Type;

            this.Text = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                    {
                        FaceName = "Calibri", Height = this.Menu["RecallTracker.FontSize"].GetValue<MenuSlider>().Value,
                        Width = 6, OutputPrecision = FontPrecision.Default, Quality = FontQuality.Default
                    });

            Teleport.OnTeleport += this.Obj_AI_Base_OnTeleport;
            Drawing.OnDraw += this.Drawing_OnDraw;
            Drawing.OnPreReset += args => { this.Text.OnLostDevice(); };
            Drawing.OnPostReset += args => { this.Text.OnResetDevice(); };
            AppDomain.CurrentDomain.DomainUnload += this.CurrentDomainDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += this.CurrentDomainDomainUnload;
        }
        

        #endregion

        #region Methods

        private void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            this.Text.Dispose();
        }

        private void Obj_AI_Base_OnTeleport(AIBaseClient sender, Teleport.TeleportEventArgs args)
        {
            var unit = sender as AIHeroClient;

            if (unit == null || !unit.IsValid || unit.IsAlly)
            {
                return;
            }

            var recallinfo = new RecallInf(unit.NetworkId, args.Status, args.Type, args.Duration, args.Start);
            var enemyInfo = this.EnemyInfo.Find(x => x.Player.NetworkId == unit.NetworkId).RecallInfo.UpdateRecall(recallinfo);

            if (args.Type == Teleport.TeleportType.Recall)
            {
                switch (args.Status)
                {
                    case Teleport.TeleportStatus.Abort:
                        if (this.Menu["notifRecAborted"].GetValue<MenuBool>().Enabled)
                        {
                            this.ShowNotification(
                                enemyInfo.Player.CharacterName + ": Recall ABORTED",
                                Color.Orange,
                                4000);
                        }

                        break;
                    case Teleport.TeleportStatus.Finish:
                        if (this.Menu["notifRecFinished"].GetValue<MenuBool>().Enabled)
                        {
                            this.ShowNotification(
                                enemyInfo.Player.CharacterName + ": Recall FINISHED",
                                Color.White,
                                4000);
                        }
                        break;
                    case Teleport.TeleportStatus.Start:
                        if (this.Menu["notifRecFinished"].GetValue<MenuBool>().Enabled)
                        {
                            this.ShowNotification(
                                enemyInfo.Player.CharacterName + ": Recall STARTED",
                                Color.White,
                                4000);
                        }

                        break;
                }
            }
        }
        private void Drawing_OnDraw(EventArgs args)
        {
            if (!this.Menu["showRecalls"].GetValue<MenuBool>().Enabled || Drawing.Direct3DDevice9 == null
                || Drawing.Direct3DDevice9.IsDisposed)
            {
                return;
            }
            //

            var indicated = false;

            var fadeout = 1f;
            var count = 0;

            foreach (var enemyInfo in
                this.EnemyInfo.Where(
                    x =>
                    x.Player.IsValid && x.RecallInfo.ShouldDraw() && !x.Player.IsDead
                    && x.RecallInfo.GetRecallCountdown() > 0).OrderBy(x => x.RecallInfo.GetRecallCountdown()))
            {
                if (!indicated && (int)enemyInfo.RecallInfo.EstimatedShootT != 0)
                {
                    indicated = true;
                    this.DrawRect(
                        this.BarX + this.Scale * enemyInfo.RecallInfo.EstimatedShootT,
                        this.BarY + this.SeperatorHeight + BarHeight - 3,
                        0,
                        this.SeperatorHeight * 2,
                        2,
                        Color.White);
                }

                this.DrawRect(
                    this.BarX,
                    this.BarY,
                    (int)(this.Scale * enemyInfo.RecallInfo.GetRecallCountdown()),
                    BarHeight,
                    1,
                    Color.FromArgb(255, Color.DeepSkyBlue));

                this.DrawRect(
                    this.BarX + this.Scale * enemyInfo.RecallInfo.GetRecallCountdown() - 1,
                    this.BarY + this.SeperatorHeight + BarHeight - 3,
                    0,
                    this.SeperatorHeight + 1,
                    1,
                    Color.White);

                this.Text.DrawText(
                    null,
                    $"{enemyInfo.Player.CharacterName} ({(int)enemyInfo.Player.HealthPercent})%",
                    (int)this.BarX
                    + (int)
                      (this.Scale * enemyInfo.RecallInfo.GetRecallCountdown()
                       - (float)(enemyInfo.Player.CharacterName.Length * this.Text.Description.Width) / 2),
                    (int)this.BarY + this.SeperatorHeight + this.Text.Description.Height / 2,
                    new ColorBGRA(255, 255, 255, 255));

                count++;
            }

            if (count > 0)
            {
                if (count != 1)
                {
                    fadeout = 1f;
                }

                this.DrawRect(
                    this.BarX,
                    this.BarY,
                    this.BarWidth,
                    BarHeight,
                    1,
                    Color.FromArgb((int)(40f * fadeout), Color.White));

                this.DrawRect(
                    this.BarX - 1,
                    this.BarY + 1,
                    0,
                    BarHeight,
                    1,
                    Color.FromArgb((int)(255f * fadeout), Color.White));
                this.DrawRect(
                    this.BarX - 1,
                    this.BarY - 1,
                    this.BarWidth + 2,
                    1,
                    1,
                    Color.FromArgb((int)(255f * fadeout), Color.White));
                this.DrawRect(
                    this.BarX - 1,
                    this.BarY + BarHeight,
                    this.BarWidth + 2,
                    1,
                    1,
                    Color.FromArgb((int)(255f * fadeout), Color.White));
                this.DrawRect(
                    this.BarX + 1 + this.BarWidth,
                    this.BarY + 1,
                    0,
                    BarHeight,
                    1,
                    Color.FromArgb((int)(255f * fadeout), Color.White));
            }
        }

        private void DrawRect(float x, float y, int width, int height, float thickness, Color color)
        {
            for (var i = 0; i < height; i++)
            {
                Drawing.DrawLine(x, y + i, x + width, y + i, thickness, color);
            }
        }

        

        private void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            //Notification a = new Notification("Recall", message);
            //a.BodyTextColor = color.ToSharpDxColor();
        }

        #endregion
    }

    internal class EnemyInfo
    {
        #region Fields

        public AIHeroClient Player;

        public RecallInfo RecallInfo;

        #endregion

        #region Constructors and Destructors

        public EnemyInfo(AIHeroClient player)
        {
            this.Player = player;
            this.RecallInfo = new RecallInfo(this);
        }

        #endregion
    }

    internal class RecallInfo
    {
        #region Fields

        public float EstimatedShootT;

        public const int FadeoutTime = 3000;

        private readonly EnemyInfo enemyInfo;

        private RecallInf abortedRecall;

        private int abortedT;

        private RecallInf recall;

        #endregion

        #region Constructors and Destructors

        public RecallInfo(EnemyInfo enemyInfo)
        {
            this.enemyInfo = enemyInfo;
            this.recall = new RecallInf(
                this.enemyInfo.Player.NetworkId,
                Teleport.TeleportStatus.Unknown,
                Teleport.TeleportType.Unknown,
                0);
        }

        #endregion

        #region Public Methods and Operators

        public int GetDrawTime()
        {
            var drawtime = 0;

            if (this.WasAborted())
            {
                drawtime = FadeoutTime - (Environment.TickCount - this.abortedT);
            }
            else
            {
                drawtime = this.GetRecallCountdown();
            }

            return drawtime < 0 ? 0 : drawtime;
        }

        public int GetRecallCountdown()
        {
            int time = Variables.GameTimeTickCount;
            int countdown;

            if (time - this.abortedT < FadeoutTime)
                countdown = this.abortedRecall.Duration - (this.abortedT - this.abortedRecall.Start);
            else if (this.abortedT > 0)
                countdown = 0; //AbortedT = 0
            else
                countdown = this.recall.Start + this.recall.Duration - time;

            return countdown < 0 ? 0 : countdown;
        }

        public bool IsPorting()
        {
            return this.recall.Type == Teleport.TeleportType.Recall
                   && this.recall.Status == Teleport.TeleportStatus.Start;
        }

        public bool ShouldDraw()
        {
            return this.IsPorting() || (this.WasAborted() && this.GetDrawTime() > 0);
        }

        public override string ToString()
        {
            var drawtext = this.enemyInfo.Player.CharacterName + ": " + this.recall.Status;

            var countdown = this.GetRecallCountdown() / 1000f;

            if (countdown > 0)
            {
                drawtext += " (" + countdown.ToString("0.00", CultureInfo.InvariantCulture) + "s)";
            }

            return drawtext;
        }

        public EnemyInfo UpdateRecall(RecallInf newRecall)
        {
            this.EstimatedShootT = 0;

            if (newRecall.Type == Teleport.TeleportType.Recall && newRecall.Status == Teleport.TeleportStatus.Abort)
            {
                this.abortedRecall = this.recall;
                this.abortedT = Environment.TickCount;
            }
            else
            {
                this.abortedT = 0;
            }

            this.recall = newRecall;
            return this.enemyInfo;
        }

        public bool WasAborted()
        {
            return this.recall.Type == Teleport.TeleportType.Recall
                   && this.recall.Status == Teleport.TeleportStatus.Abort;
        }

        #endregion
    }

    public class RecallInf
    {
        public int NetworkID;
        public int Duration;
        public int Start;
        public Teleport.TeleportType Type;
        public Teleport.TeleportStatus Status;

        public RecallInf(int netid, Teleport.TeleportStatus stat, Teleport.TeleportType tpe, int dura, int star = 0)
        {
            NetworkID = netid;
            Status = stat;
            Type = tpe;
            Duration = dura;
            Start = star;
        }
    }
}