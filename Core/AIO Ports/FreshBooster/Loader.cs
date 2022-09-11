using System;
using System.Reflection.Emit;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using static FreshBooster.FreshCommon;

namespace FreshBooster
{
    class Loader
    {
        public static void Load()
        {
            _MainMenu = new Menu("FreshBooster", "FreshBooster (" + ObjectManager.Player.CharacterName + ")", true);
            _MainMenu.SetFontColor(SharpDX.Color.Aqua);
            _MainMenu.Attach();
            Type t = Type.GetType("FreshBooster.Champion." + ObjectManager.Player.CharacterName);
            if (t != null)
            {
                //Object obj = Activator.CreateInstance(t);
                var target = t.GetConstructor(Type.EmptyTypes);
                var dynamic = new DynamicMethod(string.Empty, t, new Type[0], target.DeclaringType);
                var il = dynamic.GetILGenerator();
                il.DeclareLocal(target.DeclaringType);
                il.Emit(OpCodes.Newobj, target);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);
                var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
                method();
            }
            else
            {
                Game.Print("Can't Load FreshBooster. Please send Message to KorFresh, Error Code 99");
            }
        }
    }
}