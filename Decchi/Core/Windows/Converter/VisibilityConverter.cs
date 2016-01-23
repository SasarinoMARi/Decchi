using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class VisibilityConverter : IValueConverter
	{
		public object Visible	{ get; set; }
		public object Hidden	{ get; set; }
		public object Collapsed { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            var v = (Visibility)value;
            if (v == Visibility.Visible)   return this.Visible;
            if (v == Visibility.Hidden)    return this.Hidden;
            if (v == Visibility.Collapsed) return this.Collapsed;
            return null;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == this.Visible)   return Visibility.Visible;
            if (value == this.Hidden)    return Visibility.Hidden;
            if (value == this.Collapsed) return Visibility.Collapsed;
            return null;
		}
	}
}
