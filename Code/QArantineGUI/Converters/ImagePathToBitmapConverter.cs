using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Data;
using System.Globalization;

namespace QArantine.Code.QArantineGUI.Converters
{
    public class ImagePathToBitmapConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                try
                {
                    var uri = new Uri($"avares://{typeof(ImagePathToBitmapConverter).Assembly.GetName().Name}{path}");
                    using (var asset = AssetLoader.Open(uri))
                    {
                        return new Bitmap(asset);
                    }
                }
                catch (Exception)
                {
                    return BindingOperations.DoNothing;
                }
            }
            return BindingOperations.DoNothing;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}