using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using EDVTrader.API;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace EDVTrader.Converters
{
    public class StationTypeToIconConverter : IValueConverter
    {
        public static readonly StationTypeToIconConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (!(value is StationType type))
                return "/Assets/QuestionMark.png";

            string rawUri = type switch
            {
                StationType.Coriolis => "/Assets/Stations/Coriolis.png",
                StationType.Outpost => "/Assets/Stations/Outpost.png",

                StationType.Orbis => "/Assets/Stations/Orbis.png",
                StationType.Ocellus => "/Assets/Stations/Ocellus.png",

                StationType.Asteroid => "/Assets/Stations/Asteroid.png",
                StationType.Megaship => "/Assets/Stations/Megaship.png",

                StationType.Fleet => "/Assets/Stations/FleetCarrier.png",
                StationType.Planetary => "/Assets/Stations/Planetary.png",

                StationType.Unknown => "/Assets/QuestionMark.png",
                _ => "/Assets/QuestionMark.png"
            };

            string? assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (assemblyName == null)
                return "/Assets/QuestionMark.png";

            IAssetLoader? assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            if (assets == null)
                return "/Assets/QuestionMark.png";

            Stream? asset = assets.Open(new Uri($"avares://{assemblyName}{rawUri}"));
            if (asset == null)
                return "/Assets/QuestionMark.png";

            return new Bitmap(asset);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
