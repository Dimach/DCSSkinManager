using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DCSSkinManager.Utils
{
    public class EnumToIconsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UnitType unitType)
            {
                var image = new BitmapImage(new Uri($"Icons/{unitType:G}.png", UriKind.Relative));
                return image;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string unitIconPath)
            {
                return Enum.GetValues(typeof(UnitType)).Cast<UnitType>().Single(
                    x => string.Equals(unitIconPath.Replace("Icons\\", String.Empty).Replace(".img", String.Empty),
                        x.ToString("G"))
                );
            }

            return null;
        }
    }
}
