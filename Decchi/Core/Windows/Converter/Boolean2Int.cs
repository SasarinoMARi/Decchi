using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
    public sealed class Boolean2Int : IValueConverter
    {
        public int True { get; set; }
        public int False { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && (bool)value ? this.True : this.False;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int && (int)value == this.True ? true : false;
        }
    }
}
