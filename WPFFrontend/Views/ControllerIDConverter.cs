using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFFrontend.Views;

[ValueConversion(typeof(bool), typeof(int))]
public class ControllerIDConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return int.Parse((string)parameter).Equals(value);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return ((bool)value) == true ? parameter : DependencyProperty.UnsetValue;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
