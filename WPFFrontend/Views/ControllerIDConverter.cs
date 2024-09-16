using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFFrontend.Views;

[ValueConversion(typeof(bool), typeof(int))]
public class ControllerIDConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        parameter is string paramString && int.TryParse(paramString, out int paramInt)
            ? paramInt.Equals(value)
            : DependencyProperty.UnsetValue;

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool boolValue && boolValue ? parameter : DependencyProperty.UnsetValue;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
