using System.Drawing;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace UniversalMinimapHack
{
    public class Menu
    {
        private readonly MenuSlider _iconOpacity;
        private readonly MenuSlider _slider;
        private readonly MenuItem _ssCircle;
        private readonly MenuItem _ssCircleColor;
        private readonly MenuItem _ssCircleSize;
        private readonly MenuItem _ssFallbackPing;
        private readonly MenuItem _ssTimerEnabler;
        private readonly MenuItem _ssTimerMin;
        private readonly MenuSlider _ssTimerMinPing;
        private readonly MenuSlider _ssTimerOffset;
        private readonly MenuSlider _ssTimerSize;
        
        public Menu() //: base("Universal MinimapHack", "UniversalMinimapHack", true)
        {
            var rootedMenu = new EnsoulSharp.SDK.MenuUI.Menu("UniversalMinimapHack","Universal MinimapHack", true);
            _slider = new MenuSlider("scale", "Icon Scale % (F5 to Reload)",20);
            _iconOpacity = new MenuSlider("opacity", "Icon Opacity % (F5 to Reload)",70);
            _ssTimerEnabler = new MenuBool("enableSS", "Enable").SetValue(true);
            _ssTimerSize = new MenuSlider("sizeSS", "SS Text Size (F5 to Reload)",15);
            _ssTimerOffset = new MenuSlider("offsetSS", "SS Text Height",15, -50, +50);
            _ssTimerMin = new MenuSlider("minSS", "Show after X seconds",30, 1, 180);
            _ssTimerMinPing = new MenuSlider("minPingSS", "Ping after X seconds",30, 5, 180);
            _ssFallbackPing = new MenuBool("fallbackSS", "Fallback ping (local)").SetValue(false);
            rootedMenu.Add(new MenuSeparator("3141324", "[Customize]"));
            rootedMenu.Add(_slider);
            rootedMenu.Add(_iconOpacity);
            var ssMenu = new EnsoulSharp.SDK.MenuUI.Menu("ssTimer","SS Timer");
            ssMenu.Add(_ssTimerEnabler);
            ssMenu.Add(new MenuSeparator("1", "--- [Extra] ---"));
            ssMenu.Add(_ssTimerMin);
            ssMenu.Add(_ssFallbackPing);
            ssMenu.Add(_ssTimerMinPing);
            ssMenu.Add(new MenuSeparator("2", "--- [Customize] ---"));
            ssMenu.Add(_ssTimerSize);
            ssMenu.Add(_ssTimerOffset);
            var ssCircleMenu = new EnsoulSharp.SDK.MenuUI.Menu("ccCircles","SS Circles");
            _ssCircle = new MenuBool("ssCircle", "Enable").SetValue(true);
            _ssCircleSize = new MenuSlider("ssCircleSize", "Max Circle Size",7000, 500, 15000);
            _ssCircleColor = new MenuColor("ssCircleColor", "Circle color",Color.Green.ToSharpDxColor());
            ssCircleMenu.Add(_ssCircle);
            ssCircleMenu.Add(_ssCircleSize);
            ssCircleMenu.Add(_ssCircleColor);
            rootedMenu.Add(ssMenu);
            rootedMenu.Add(ssCircleMenu);
            rootedMenu.Attach();
        }

        public float IconScale
        {
            get { return _slider.GetValue<MenuSlider>().Value / 100f; }
        }

        public float IconOpacity
        {
            get { return _iconOpacity.GetValue<MenuSlider>().Value / 100f; }
        }

        // ReSharper disable once InconsistentNaming
        public bool SSTimer
        {
            get { return _ssTimerEnabler.GetValue<MenuBool>().Enabled; }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerSize
        {
            get { return _ssTimerSize.GetValue<MenuSlider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerOffset
        {
            get { return _ssTimerOffset.GetValue<MenuSlider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerStart
        {
            get { return _ssTimerMin.GetValue<MenuSlider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public bool Ping
        {
            get { return _ssFallbackPing.GetValue<MenuBool>().Enabled; }
        }

        // ReSharper disable once InconsistentNaming
        public int MinPing
        {
            get { return _ssTimerMinPing.GetValue<MenuSlider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public bool SSCircle
        {
            get { return _ssCircle.GetValue<MenuBool>().Enabled; }
        }

        // ReSharper disable once InconsistentNaming
        public int SSCircleSize
        {
            get { return _ssCircleSize.GetValue<MenuSlider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public SharpDX.Color SSCircleColor
        {
            get { return _ssCircleColor.GetValue<MenuColor>().Color; }
        }
    }
}