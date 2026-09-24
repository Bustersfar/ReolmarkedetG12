using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReolmarkedetG12.UI.Converters
{
    // Viser et element, når værdien er "tom" (null, tom streng, tom collection, eller 0).
    // Sæt ConverterParameter="Invert" for at vende logikken om (vis når IKKE tom).
    public class EmptyToVisibilityConverter : IValueConverter
    {
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isEmpty = value switch
        {
            null => true,
            string s => string.IsNullOrEmpty(s),
            ICollection c => c.Count == 0,
            int i => i == 0,
            _ => false
        };

        bool invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        if (invert)
            isEmpty = !isEmpty;

        return isEmpty ? Visibility.Visible : Visibility.Collapsed;
    }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
