using Avalonia.Data.Converters;
using EDVTrader.Common;
using System;
using System.Globalization;

namespace EDVTrader.Converters
{
    public class IntToDottedConverter : IValueConverter
    {
        public static readonly IntToDottedConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (!(value is int number))
                return $"UNK: {value}";

            return number.ToDotted();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (!(value is string str))
                return value;

            return int.Parse(str.Replace(".", "").Replace("UNK: ", ""));
        }
    }
}
