namespace PortAIO.Library_Ports.Entropy.Lib.Geometry
{
    using SharpDX;

    public static class RectangleEx
    {
        #region Public Methods and Operators

        public static Rectangle Add(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return new Rectangle(rectangle1.X      + rectangle2.X,
                                 rectangle1.Y      + rectangle2.Y,
                                 rectangle1.Width  + rectangle2.Width,
                                 rectangle1.Height + rectangle2.Height);
        }

        public static Rectangle Divide(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return new Rectangle(rectangle1.X      / rectangle2.X,
                                 rectangle1.Y      / rectangle2.Y,
                                 rectangle1.Width  / rectangle2.Width,
                                 rectangle1.Height / rectangle2.Height);
        }

        public static Rectangle Multiply(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return new Rectangle(rectangle1.X      * rectangle2.X,
                                 rectangle1.Y      * rectangle2.Y,
                                 rectangle1.Width  * rectangle2.Width,
                                 rectangle1.Height * rectangle2.Height);
        }

        public static Rectangle Negate(this Rectangle rectangle)
        {
            return new Rectangle(-rectangle.X, -rectangle.Y, -rectangle.Width, -rectangle.Height);
        }

        public static Rectangle Substract(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return new Rectangle(rectangle1.X      - rectangle2.X,
                                 rectangle1.Y      - rectangle2.Y,
                                 rectangle1.Width  - rectangle2.Width,
                                 rectangle1.Height - rectangle2.Height);
        }

        #endregion

        /*
        public static bool IsInside(this Rectangle rectangle, Vector2 position)
        {
            return position.X >= rectangle.X && position.Y >= rectangle.Y &&
                   position.X < rectangle.BottomRight.X && position.Y < rectangle.BottomRight.Y;
        }

        public static bool IsCompletlyInside(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return rectangle2.X >= rectangle1.X && rectangle2.Y >= rectangle1.Y &&
                   rectangle2.BottomRight.X <= rectangle1.BottomRight.X &&
                   rectangle2.BottomRight.Y <= rectangle1.BottomRight.Y;
        }

        public static bool IsPartialInside(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return rectangle2.X >= rectangle1.X && rectangle2.X <= rectangle1.BottomRight.X ||
                   rectangle2.Y >= rectangle1.Y && rectangle2.Y <= rectangle1.BottomRight.Y;
        }
        
        public static bool IsNear(this Rectangle rectangle, Vector2 position, int distance)
        {
            return
                new Rectangle(rectangle.X - distance, rectangle.Y - distance, rectangle.Width + distance,
                    rectangle.Height + distance).IsInside(position);
        }*/
    }
}