using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Figurator.Views.Converters {
    // Конвертер оказался не применим на практике :/
    internal class SafeToColor: IValueConverter {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value == null) return null;
            // if (value is SafePoint @point) return @point.is_valid ? Brushes.Lime : Brushes.Pink;
            throw new NotSupportedException();
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
