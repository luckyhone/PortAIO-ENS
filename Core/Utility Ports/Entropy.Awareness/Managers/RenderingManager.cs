using System;
using System.Collections.Generic;
using EnsoulSharp;
using EnsoulSharp.SDK.Utility;
using Entropy.Awareness.Bases;
using Entropy.Awareness.Trackers;

namespace Entropy.Awareness.Managers
{
    public static class RenderingManager
    {
     
        private static readonly List<ITracker> Trackers = new List<ITracker>
        {
            new SpellTracker(),
            //new HUDTracker()
        };
        
        private static bool deviceResetting;
        public static void Initialize()
        {
            Drawing.OnPreReset     += args => deviceResetting = true;
            Drawing.OnPostReset += args => deviceResetting = false;
            
            foreach (var tracker in Trackers)
            {
                try
                {
                    Console.WriteLine($"Initializing {tracker.Name} ...");
                    tracker.Initialize();
                    Console.WriteLine($"Initialized {tracker.Name} with success !");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to init {tracker.Name}");
                }
            }

            Drawing.OnDraw += RendererOnWorldRender;
            Drawing.OnEndScene += RendererOnOnEndScene;
        }

        private static void RendererOnWorldRender(EventArgs args)
        {
            foreach (var tracker in Trackers)
            {
                tracker.WorldRender();
            }
        }

        private static void RendererOnOnEndScene(EventArgs args)
        {
            if (deviceResetting)
            {
                return;
            }

            foreach (var tracker in Trackers)
            {
                tracker.Render();
            }
        }
        
        
    }
}