using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;

using SharpDX;

namespace PortAIO{

    internal class Program
    {
        public const string version = "1.0.0.5";
        
        public static void Main(string[] args)
        {
            GameEvent.OnGameLoad += Loading_OnLoadingComplete;
            
        }
        private static void Loading_OnLoadingComplete()
        {
            PortAIO.Init.Initialize();
        }
    }
}