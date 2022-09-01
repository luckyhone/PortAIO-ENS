using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace Entropy.Awareness.Bases
{
    static class MenuBase
    {
        public static Menu Root { get; private set; }

        public static void Initialize()
        {
            Root = new Menu("awareness", "Awareness", true);

            var a = new MenuSeparator("lksdfuhoiuhysdf", "Awareness");
            a.AddPermashow("Awareness", Color.Red);
            Root.Attach();
        }
    }
}