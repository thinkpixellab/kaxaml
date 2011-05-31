using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KaxamlPlugins
{
    public class RoundConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val;
            if (value != null && double.TryParse(value.ToString(), out val))
            {
                return Math.Round(val, 2);
            }
            return DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}