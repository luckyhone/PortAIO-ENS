using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;

namespace ElUtilitySuite
{
    using Color = SharpDX.Color;

    internal class Entry
    {
        #region Delegates

        /// <summary>
        /// Creates a new object of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        internal delegate T ObjectActivator<out T>(params object[] args);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the menu.
        /// </summary>
        /// <value>
        /// The menu.
        /// </value>
        public static Menu Menu { get; set; }

        /// <summary>
        ///     Gets script version
        /// </summary>
        /// <value>
        ///     The script version
        /// </value>
        public static string ScriptVersion => typeof(Entry).Assembly.GetName().Version.ToString();

        public static event EventHandler<UnloadEventArgs> OnUnload;
        public class UnloadEventArgs : EventArgs
        {
            public bool Final;

            public UnloadEventArgs(bool final = false)
            {
                Final = final;
            }
        }
        #endregion

        #region Public Methods and Operators

        //[PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
        {
            var paramsInfo = ctor.GetParameters();
            var param = Expression.Parameter(typeof(object[]), "args");
            var argsExp = new Expression[paramsInfo.Length];

            for (var i = 0; i < paramsInfo.Length; i++)
            {
                var paramCastExp = Expression.Convert(
                    Expression.ArrayIndex(param, Expression.Constant(i)),
                    paramsInfo[i].ParameterType);

                argsExp[i] = paramCastExp;
            }

            return
                (ObjectActivator<T>)
                Expression.Lambda(typeof(ObjectActivator<T>), Expression.New(ctor, argsExp), param).Compile();
        }


        public static void OnLoad()
        {
            try
            {
                var plugins =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(x => typeof(IPlugin).IsAssignableFrom(x) && !x.IsInterface)
                        .Select(x => GetActivator<IPlugin>(x.GetConstructors().First())(null));

                var menu = new Menu("ElUtilitySuite", "ElUtilitySuite", true);
                menu.SetFontColor(Color.GreenYellow);

                foreach (var plugin in plugins)
                {
                    plugin.CreateMenu(menu);
                    plugin.Load();
                }

                foreach (var ally in GameObjects.AllyHeroes)
                {
                    //IncomingDamageManager.AddChampion(ally);
                }

                menu.Add(new MenuSeparator("seperator1", " - "));
                menu.Add(new MenuKeyBind("usecombo", "Combo (Active)",Keys.Space,KeyBindType.Press));
                menu.Add(new MenuSeparator("seperator", " - "));
                menu.Add(new MenuSeparator("Versionnumber", $"Version: {ScriptVersion}"));
                menu.Add(new MenuSeparator("by.jQuery", "jQuery / ChewyMoon"));
                menu.Attach();

                Menu = menu;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
        }

        #endregion
    }
}