using EnsoulSharp;

namespace ElUtilitySuite.Vendor.SFX
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharpDX.Direct3D9;

    internal class MDrawing
    {
        private static readonly Dictionary<int, Font> Fonts = new Dictionary<int, Font>();
        private static readonly HashSet<Line> Lines = new HashSet<Line>();
        private static Sprite _sprite;
        private static bool _unloaded;

        static MDrawing()
        {
            try
            {
                Drawing.OnPreReset += OnDrawingPreReset;
                Drawing.OnPostReset += OnDrawingPostReset;
                Entry.OnUnload += OnUnload;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
        }

        private static void OnUnload(object sender, Entry.UnloadEventArgs unloadEventArgs)
        {
            try
            {
                _unloaded = true;

                if (_sprite != null && !_sprite.IsDisposed)
                {
                    _sprite.Dispose();
                }

                foreach (var font in Fonts.Where(font => font.Value != null && !font.Value.IsDisposed))
                {
                    font.Value.Dispose();
                }

                foreach (var line in Lines.Where(line => line != null && !line.IsDisposed))
                {
                    line.Dispose();
                }

                Drawing.OnPreReset -= OnDrawingPreReset;
                Drawing.OnPostReset -= OnDrawingPostReset;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
        }
        
        private static void OnDrawingPostReset(EventArgs args)
        {
            try
            { 
                if (_sprite != null && !_sprite.IsDisposed)
                {
                    _sprite.OnResetDevice();
                }

                foreach (var font in Fonts.Where(font => font.Value != null && !font.Value.IsDisposed))
                {
                    font.Value.OnResetDevice();
                }

                foreach (var line in Lines.Where(line => line != null && !line.IsDisposed))
                {
                    line.OnResetDevice();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
        }

        private static void OnDrawingPreReset(EventArgs args)
        {
            try
            {
                if (_sprite != null && !_sprite.IsDisposed)
                {
                    _sprite.OnLostDevice();
                }

                foreach (var font in Fonts.Where(font => font.Value != null && !font.Value.IsDisposed))
                {
                    font.Value.OnLostDevice();
                }

                foreach (var line in Lines.Where(line => line != null && !line.IsDisposed))
                {
                    line.OnLostDevice();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
        }

        public static Font GetFont(int fontSize)
        {
            Font font = null;
            try
            {
                if (!Fonts.TryGetValue(fontSize, out font))
                {
                    font = new Font(
                        Drawing.Direct3DDevice9,
                        new FontDescription
                        {
                            FaceName = "Tahoma",
                            Height = fontSize,
                            OutputPrecision = FontPrecision.Default,
                            Quality = FontQuality.Default
                        });
                    Fonts[fontSize] = font;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
            return font;
        }

        public static Line GetLine(int width = -1)
        {
            Line line = null;
            try
            {
                line = new Line(Drawing.Direct3DDevice9);
                if (width >= 0)
                {
                    line.Width = width;
                }
                Lines.Add(line);
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
            return line;
        }

        public static Sprite GetSprite()
        {
            try
            {
                _sprite = new Sprite(Drawing.Direct3DDevice9);
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
            return _sprite;
        }
    }
}