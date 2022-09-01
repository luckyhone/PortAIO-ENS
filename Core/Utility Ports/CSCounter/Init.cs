using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SharpDX;
using SharpDX.Direct3D9;

using LeagueSharpCommon;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace CS_Counter
{
    internal class Init
    {
        public static Menu Menu2;
        public static MenuBool Menuenable2;
        public static MenuBool Menuenable3;
        public static MenuBool Menuenable4;
        public static MenuBool Advanced;
        public static MenuBool Advanced_box;

        public static MenuSlider XPos;
        public static MenuSlider YPos;
        
        public static void PrepareMenu()
        {
            //Notifications.AddNotification("CS Counter loaded.", 100);

            Menu2 = new Menu("menu2","CS Counter", true);

            CsCounter.Line = new Line(Drawing.Direct3DDevice9);

            CsCounter.Textx = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Calibri",
                    Height = 13,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });

            Menuenable2 = new MenuBool("menu.drawings.enable2", "CS Count").SetValue(true);
            Menu2.Add(Menuenable2);
            Menuenable3 = new MenuBool("menu.drawings.enable3", "My CS Count").SetValue(true);
            Menu2.Add(Menuenable3);
            Menuenable4 = new MenuBool("menu.drawings.enable4", "Allies CS Count").SetValue(true);
            Menu2.Add(Menuenable4);
            Advanced = new MenuBool("menu.drawings.advanced", "Advanced Farminfo (for me)").SetValue(false);
            Menu2.Add(Advanced);
            Advanced_box = new MenuBool("menu.drawings.advanced_box", "Turn off Boxes").SetValue(false);
            Menu2.Add(Advanced_box);
            XPos = new MenuSlider("menu.Calc.calc5", "X - Position",0, -100);
            Menu2.Add(XPos);
            YPos = new MenuSlider("menu.Calc.calc6", "Y - Position",0, -100);
            Menu2.Add(YPos);


            Menu2.Attach();
        }
    }
}