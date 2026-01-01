using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartSystemPDV.Converters;

/// <summary>
/// Converte valores null/não-null para Visibility
/// null = Collapsed, não-null = Visible
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter != null && parameter.ToString() == "Inverse")
        {
            // Inverso: null = Visible, não-null = Collapsed
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        // Padrão: null = Collapsed, não-null = Visible
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}