using Avalonia.Data.Converters;
using System.Globalization;

namespace QArantine.Code.QArantineGUI.Converters
{
    public class IdFormattedStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return $"{value}: ";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
