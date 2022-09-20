using System.Globalization;

namespace EDVTrader.Common
{
    public static class Extensions
    {
        public static string ToDotted(this int value) => value.ToString("N0", new CultureInfo("en-US") { NumberFormat = { NumberGroupSeparator = "." } });

        public static string ToDotted(this long value) => value.ToString("N0", new CultureInfo("en-US") { NumberFormat = { NumberGroupSeparator = "." } });
    }
}
