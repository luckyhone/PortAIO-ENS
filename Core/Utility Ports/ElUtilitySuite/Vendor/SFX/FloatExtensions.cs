namespace ElUtilitySuite.Vendor.SFX
{
    using System;

    public static class FloatExtensions
    {
        /// <exception cref="OverflowException">
        ///     <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or
        ///     greater than <see cref="F:System.TimeSpan.MaxValue" />.-or-<paramref name="value" /> is
        ///     <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="value" /> is
        ///     <see cref="F:System.Double.NegativeInfinity" />.
        /// </exception>
        public static string FormatTime(this float value, bool totalSeconds = false)
        {
            var ts = TimeSpan.FromSeconds(value);
            if (!totalSeconds && ts.TotalSeconds > 60)
            {
                return string.Format("{0}:{1:00}", (int)ts.TotalMinutes, ts.Seconds);
            }
            return string.Format("{0:0}", ts.TotalSeconds);
        }
    }
}