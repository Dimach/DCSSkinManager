using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DCSSkinManager.Utils
{
    public class EnumToIconsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UnitType unitType)
            {
                var path = $"Icons/{unitType:G}.png";
                var uri = new Uri(path, UriKind.Relative);
                var image = new BitmapImage(uri);
                return image;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
