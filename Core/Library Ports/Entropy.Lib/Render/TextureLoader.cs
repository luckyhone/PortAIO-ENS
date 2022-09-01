using EnsoulSharp;

namespace Entropy.Lib.Render
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using SharpDX.Direct3D9;

    /// <inheritdoc />
    /// <summary>
    ///     Safely load and convert bitmaps to textures which will automatically get disposed on AppDomain
    ///     unload/exit.
    ///     Primary advantage for this class is that it also reloads the textures on device resets.
    /// </summary>
    public class TextureLoader : IDisposable
    {
        private readonly Dictionary<string, Tuple<Bitmap, Texture>> Textures = new Dictionary<string, Tuple<Bitmap, Texture>>();

        public TextureLoader()
        {
            // Listen to reset events to reload the textures
            Drawing.OnPostReset += OnReset;

            // Listen to appdomain unloads or exits to make sure we dispose the textures
            AppDomain.CurrentDomain.DomainUnload += OnAppDomainUnload;
            AppDomain.CurrentDomain.ProcessExit  += OnAppDomainUnload;
        }

        /// <summary>
        ///     Returns the texture which is indexed by the given key
        /// </summary>
        /// <param name="key">The index key</param>
        /// <returns></returns>
        public Texture this[string key] => Textures[key].Item2;

        public void Dispose()
        {
            foreach (var entry in Textures.Values)
            {
                entry.Item1.Dispose();
                entry.Item2.Dispose();
            }

            Textures.Clear();

            AppDomain.CurrentDomain.DomainUnload -= OnAppDomainUnload;
            AppDomain.CurrentDomain.ProcessExit  -= OnAppDomainUnload;
        }

        public Texture Load(Bitmap bitmap, out string uniqueKey)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            string unique;
            do
            {
                unique = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            } while (Textures.ContainsKey(unique));

            uniqueKey = unique;
            return Load(unique, bitmap);
        }

        /// <summary>
        ///     Loads and converts the given bitmap to a texture
        /// </summary>
        /// <param name="key">The index key</param>
        /// <param name="bitmap">The bitmap to convert and load</param>
        /// <returns>The loaded texture</returns>
        public Texture Load(string key, Bitmap bitmap)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (Textures.ContainsKey(key))
            {
                throw new ArgumentException($"The given key '{key}' is already present!");
            }

            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            Textures[key] = new Tuple<Bitmap, Texture>(bitmap, BitmapToTexture(bitmap));

            return Textures[key].Item2;
        }

        /// <summary>
        ///     Unloads the texture which is associated with the index key from memory
        /// </summary>
        /// <param name="key">The index key</param>
        /// <returns></returns>
        public bool Unload(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (Textures.ContainsKey(key))
            {
                Textures[key].Item2.Dispose();
                Textures.Remove(key);
                return true;
            }

            return false;
        }

        private static Texture BitmapToTexture(Bitmap bitmap)
        {
            return Texture.FromMemory(Drawing.Direct3DDevice9,
                                      (byte[]) new ImageConverter().ConvertTo(bitmap, typeof(byte[])),
                                      bitmap.Width,
                                      bitmap.Height,
                                      0,
                                      Usage.None,
                                      Format.A1,
                                      Pool.Managed,
                                      Filter.Default,
                                      Filter.Default,
                                      0);
        }

        private void OnReset(EventArgs args)
        {
            foreach (var entry in Textures.ToList())
            {
                entry.Value.Item2.Dispose();
                Textures[entry.Key] = new Tuple<Bitmap, Texture>(entry.Value.Item1, BitmapToTexture(entry.Value.Item1));
            }
        }

        private void OnAppDomainUnload(object sender, EventArgs eventArgs)
        {
            Dispose();
        }
    }
}