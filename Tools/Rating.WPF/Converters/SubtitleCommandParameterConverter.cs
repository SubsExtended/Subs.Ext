using System;
using System.Windows.Data;

namespace Rating.WPF.Converters
{
    public class SubtitleCommandParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;

            var position = values[0];
            var filePath = values[1];

            return new { Position = position, FilePath = filePath };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}