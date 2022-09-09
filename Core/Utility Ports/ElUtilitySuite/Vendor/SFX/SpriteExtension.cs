namespace ElUtilitySuite.Vendor.SFX
{
    using global::SharpDX;
    using global::SharpDX.Direct3D9;

    public static class SpriteExtension
    {
        public static void DrawCentered(this Sprite sprite,
            Texture texture,
            Vector2 position,
            Rectangle? rectangle = null)
        {
            var desc = texture.GetLevelDescription(0);
            sprite.Draw(
                texture, new ColorBGRA(255, 255, 255, 255), rectangle,
                new Vector3(-(position.X - desc.Width / 2f), -(position.Y - desc.Height / 2f), 0));
        }

        public static void DrawCentered(this Sprite sprite, Texture texture, int x, int y, Rectangle? rectangle = null)
        {
            DrawCentered(sprite, texture, new Vector2(x, y), rectangle);
        }
    }
}