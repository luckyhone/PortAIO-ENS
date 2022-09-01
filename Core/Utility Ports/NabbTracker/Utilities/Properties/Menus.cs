using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace NabbTracker
{
    class Menus
    {
        /// <summary>
        /// Builds the general Menu.
        /// </summary>
        public static void Initialize()
        {
            Variables.Menu = new Menu($"{Variables.MainMenuName}", $"{Variables.MainMenuCodeName}", true);
            {
                Variables.Menu.Add(new MenuBool($"{Variables.MainMenuName}.allies", "Enable Allies"))
                    .SetFontColor(Color.Green);
                Variables.Menu.Add(new MenuBool($"{Variables.MainMenuName}.enemies", "Enable Enemies"))
                    .SetFontColor(Color.Red);
            }
            Variables.Menu.Attach();
        }
    }
}